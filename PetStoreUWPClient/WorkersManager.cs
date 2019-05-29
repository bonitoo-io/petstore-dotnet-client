using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetStoreUWPClient
{
    class WorkersManager
    {
        public HubDiscoveryWorker HubDiscoveryWorker { get; private set; }
        public SensorsWorker SensorsWorker { get; private set; }
        public SubscriptionWorker SubscriptionWorker { get; private set; }
        public InfluxDbWorker InfluxDbWorker { get; private set; }
        private bool running;

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
                OnStatusChanged("Starting..");
                await StartSensorsReading();
                if (!Config.GetInstance().IsDbConfigAvailable())
                {
                    if (Config.GetInstance().hubUrl != null)
                    {
                        StartSubscriptionWorker(Config.GetInstance().hubUrl);
                    }
                    else
                    {
                        StartHubDiscovery();
                    }
                }
                else
                {
                    StartDbWorker();
                }
                running = true;
            }
        }

        public async Task Stop()
        {
            if (running)
            {
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
                running = false;
            }
        }

        private void StartHubDiscovery()
        {
            if (HubDiscoveryWorker == null)
            {
                Debug.WriteLine("WorkersManager:StartHubDiscovery");
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
                Debug.WriteLine("WorkersManager:StopHubDiscovery");
                HubDiscoveryWorker.DiscoveryCompleted -= HubDiscoveryWorker_DiscoveryCompleted;
                HubDiscoveryWorker.Stop();
                
            }
        }

        private void HubDiscoveryWorker_DiscoveryCompleted(object sender, HubDisoveryCompletedEventArgs e)
        {
            if (e.Result.HubUrl != null)
            {
                Debug.WriteLine($"WorkersManager:DiscoveryCompleted: {e.Result.HubUrl}");
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
                Debug.WriteLine("WorkersManager:StartSensorsReading");
                SensorsWorker = new SensorsWorker(15000);
                SensorsWorker.StatusChanged += Worker_StatusChanged;
                await SensorsWorker.Start();
            }
        }

        private void StopSensorsReading()
        {
            if (SensorsWorker != null)
            {
                Debug.WriteLine("WorkersManager:StopSensorsReading");
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
                Debug.WriteLine("WorkersManager:StartSubscriptionWorker");
                SubscriptionWorker = new SubscriptionWorker(hubUrl);
                SubscriptionWorker.SubscriptionStatusChanged += SubscriptionWorker_SubscriptionStatusChanged;
                SubscriptionWorker.Start();
            }

        }

        private void StopSubscriptionWorker()
        {
            if (SubscriptionWorker != null)
            {
                Debug.WriteLine("WorkersManager:StopSubscriptionWorker");
                SubscriptionWorker.SubscriptionStatusChanged -= SubscriptionWorker_SubscriptionStatusChanged;
                SubscriptionWorker.Stop();
               
            }

        }

        private void StartDbWorker()
        {
            if(InfluxDbWorker == null)
            {
                Debug.WriteLine("WorkersManager:StartDbWorker");
                InfluxDbWorker = new InfluxDbWorker();
                InfluxDbWorker.StatusChanged += Worker_StatusChanged;
                InfluxDbWorker.Start();
            }
        }

        private void StopDbWorker()
        {
            if (InfluxDbWorker != null)
            {
                Debug.WriteLine("WorkersManager:StopDbWorker");
                InfluxDbWorker.StatusChanged -= Worker_StatusChanged;
                InfluxDbWorker.Stop();
                
            }
        }

        private void SubscriptionWorker_SubscriptionStatusChanged(object sender, SubscriptionStatusUpdatedEventArgs e)
        {
            OnStatusChanged(SubscriptionWorker.SubscriptionStatusToString(e.Status));
            if(e.Status == SubscriptionStatus.Accepted || e.Status == SubscriptionStatus.Subscribed)
            {
                if(!Config.GetInstance().IsDbConfigAvailable())
                {
                    BasicData.GetBasicData().Status = "Logic error: subscribed, but without config";
                }
                else
                {
                    StartDbWorker();
                }
            } 
        }

        protected void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, new StatusUpdatedEventArgs(status));
        }

        public event EventHandler<StatusUpdatedEventArgs> StatusChanged;

    }
}
