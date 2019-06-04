using MetroLog;
using PetStoreClientDataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetStoreClientBackgroundApplication
{
    class WorkersManager
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<WorkersManager>();
        public HubDiscoveryWorker HubDiscoveryWorker { get; private set; }
        public SensorsWorker SensorsWorker { get; private set; }
        public SubscriptionWorker SubscriptionWorker { get; private set; }
        public InfluxDbWorker InfluxDbWorker { get; private set; }
        private Config config;
        public Config Config
        {
            get {
                if (config == null)
                {
                    config = new Config();
                    config.Load(LocalSettingsConfigProvider.Instance);
                }
                return config;
            }
            private set
            {
                config = value;
            }
        }
        private bool running;
        private SubscriptionStatus lastStatus = SubscriptionStatus.None;


        private static WorkersManager instance;

        public static WorkersManager GetWorkersManager()
        {
            if(instance == null)
            {
                instance = new WorkersManager();
            }
            return instance;
        }

        private WorkersManager()
        {
            
        }

        public async Task Start()
        {
            if (!running)
            {
                Log.Trace("WorkersManager:Start");
                OnStatusChanged("Starting..");
                await StartSensorsReading();

                if (Config.HubUrl != null)
                {
                    StartSubscriptionWorker(Config.HubUrl);
                }
                else
                {
                    StartHubDiscovery();
                }
                if (Config.IsDbConfigAvailable())
                {
                    StartDbWorker();
                }
                Log.Trace("WorkersManager:Started");
                running = true;
            }
        }

        public async Task Stop()
        {
            if (running)
            {
                Log.Trace("WorkersManager:Stop");
                OnStatusChanged("Stopping workers");
                StopDbWorker();
                StopSubscriptionWorker();
                StopHubDiscovery();
                StopSensorsReading();
                await Task.Run(() =>
                {
                    var stop = false;
                    while (!stop)
                    {
                        Task.Delay(1000);
                        stop = (HubDiscoveryWorker == null || !HubDiscoveryWorker.Running)
                                && (SensorsWorker == null || !SensorsWorker.Running)
                                && (SubscriptionWorker == null || !SubscriptionWorker.Running)
                                && (InfluxDbWorker == null || !InfluxDbWorker.Running);

                    }
                });
                HubDiscoveryWorker = null;
                SensorsWorker = null;
                SubscriptionWorker = null;
                InfluxDbWorker = null;
                OnStatusChanged("Stopped workers");
                Log.Trace("WorkersManager:Stopped");
                running = false;
            }
        }

        public async Task Restart()
        {
            Log.Trace("WorkersManager:Restart");
            await Stop();
            await Start();
            Log.Trace("WorkersManager:Restarted");
        }

        private void StartHubDiscovery()
        {
            if (HubDiscoveryWorker == null)
            {
                Log.Trace("WorkersManager:StartHubDiscovery");
                HubDiscoveryWorker = new HubDiscoveryWorker();
                HubDiscoveryWorker.DiscoveryCompleted += HubDiscoveryWorker_DiscoveryCompleted;
                OnStatusChanged("Discovering hub");
                HubDiscoveryWorker.Start();
            }
        }

        private void StopHubDiscovery()
        {
            if (HubDiscoveryWorker != null)
            {
                Log.Trace("WorkersManager:StopHubDiscovery");
                HubDiscoveryWorker.DiscoveryCompleted -= HubDiscoveryWorker_DiscoveryCompleted;
                HubDiscoveryWorker.Stop();
                
            }
        }

        private void HubDiscoveryWorker_DiscoveryCompleted(object sender, HubDisoveryCompletedEventArgs e)
        {
            if (e.Result.HubUrl != null)
            {
                Log.Info($"WorkersManager:DiscoveryCompleted: {e.Result.HubUrl}");
                var hubUrl = e.Result.HubUrl;
                OnStatusChanged($"Discoverered hub url: {e.Result.HubUrl}");
                StartSubscriptionWorker(hubUrl);
            }
            if(e.Result.Error != null)
            {
                OnStatusChanged($"Discovery error: {e.Result.Error.Message}");
            }
        }

        private async Task StartSensorsReading()
        {
            if (SensorsWorker == null)
            {
                Log.Trace("WorkersManager:StartSensorsReading");
                SensorsWorker = new SensorsWorker(Config.SensorsPeriod);
                SensorsWorker.StatusChanged += Worker_StatusChanged;
                await SensorsWorker.Start();
            }
        }

        private void StopSensorsReading()
        {
            if (SensorsWorker != null)
            {
                Log.Trace("WorkersManager:StopSensorsReading");
                SensorsWorker.StatusChanged -= Worker_StatusChanged;
                SensorsWorker.Stop();
                
            }
        }

        private void Worker_StatusChanged(object sender, StatusUpdatedEventArgs e)
        {
            OnStatusChanged(e.Status);
        }

        private void StartSubscriptionWorker(string hubUrl)
        {
            if(SubscriptionWorker == null)
            {
                Log.Trace("WorkersManager:StartSubscriptionWorker");
                SubscriptionWorker = new SubscriptionWorker(hubUrl, Config.SubscriptionCheckPeriod);
                SubscriptionWorker.SubscriptionStatusChanged += SubscriptionWorker_SubscriptionStatusChanged;
                SubscriptionWorker.Start();
            }

        }

        private void StopSubscriptionWorker()
        {
            if (SubscriptionWorker != null)
            {
                Log.Trace("WorkersManager:StopSubscriptionWorker");
                SubscriptionWorker.SubscriptionStatusChanged -= SubscriptionWorker_SubscriptionStatusChanged;
                SubscriptionWorker.Stop();
               
            }

        }

        private void StartDbWorker()
        {
            if(InfluxDbWorker == null)
            {
                Log.Trace("WorkersManager:StartDbWorker");
                InfluxDbWorker = new InfluxDbWorker(Config.DbReadWritePeriod);
                InfluxDbWorker.StatusChanged += Worker_StatusChanged;
                InfluxDbWorker.Start();
            }
        }

        private void StopDbWorker()
        {
            if (InfluxDbWorker != null)
            {
                Log.Trace("WorkersManager:StopDbWorker");
                InfluxDbWorker.StatusChanged -= Worker_StatusChanged;
                InfluxDbWorker.Stop();
                
            }
        }

        private void SubscriptionWorker_SubscriptionStatusChanged(object sender, SubscriptionStatusUpdatedEventArgs e)
        {
            if (e.Status != SubscriptionStatus.Subscribed)
            {
                OnStatusChanged(SubscriptionWorker.SubscriptionStatusToString(e.Status));
            } else
            {
                OnStatusChanged("");
            }
            if(e.Status == SubscriptionStatus.Accepted || e.Status == SubscriptionStatus.Subscribed)
            {
                if(!Config.IsDbConfigAvailable())
                {
                    OnStatusChanged("Logic error: subscribed, but without config");
                }
                else
                {
                    StartDbWorker();
                }
            } 
            if(e.Status == SubscriptionStatus.WaitingForAuthorization && lastStatus != SubscriptionStatus.WaitingForAuthorization)
            {
                Log.Info("Deauthorization, clearing DB config and restarting");
                Config.ResetDbSettings();
                Config.Save(LocalSettingsConfigProvider.Instance);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Restart();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            }
            lastStatus = e.Status;
        }



        protected void OnStatusChanged(string status)
        {
            OverviewData.GetOverviewData().Status = status;
        }

    }
}
