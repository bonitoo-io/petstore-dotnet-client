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

        private static WorkersManager instance;
        private string hubUrl;

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
            await StartSensorsReading();
            if(!DbConfig.GetInstance().IsDbConfigAvailable())
            {
                StartHubDiscovery();
            } else
            {
                StartDbWorker();
            }
        }

        private void StartHubDiscovery()
        {
            if (HubDiscoveryWorker == null)
            {
                Debug.WriteLine("WorkersManager:StartHubDiscovery");
                HubDiscoveryWorker = new HubDiscoveryWorker();
                HubDiscoveryWorker.DiscoveryCompleted += HubDiscoveryWorker_DiscoveryCompleted;
                HubDiscoveryWorker.Start();
            }
        }

        private void HubDiscoveryWorker_DiscoveryCompleted(object sender, HubDisoveryCompletedEventArgs e)
        {
            if(e.Result.HubUrl != null)
            {
                Debug.WriteLine("WorkersManager:DiscoveryCompleted: " + e.Result.HubUrl);
                hubUrl = e.Result.HubUrl;
                StartSubscriptionWorker(hubUrl);
            }
        }

        private async Task StartSensorsReading()
        {
            if (SensorsWorker == null)
            {
                Debug.WriteLine("WorkersManager:StartSensorsReading");
                SensorsWorker = new SensorsWorker(15000);
                await SensorsWorker.Start();
            }
        }

        public void StartSubscriptionWorker(string hubUrl)
        {
            if(SubscriptionWorker == null)
            {
                Debug.WriteLine("WorkersManager:StartSubscriptionWorker");
                SubscriptionWorker = new SubscriptionWorker(hubUrl);
                SubscriptionWorker.SubscriptionStatusChanged += SubscriptionWorker_SubscriptionStatusChanged;
                SubscriptionWorker.Start();
            }

        }

        public void StartDbWorker()
        {
            if(InfluxDbWorker == null)
            {
                Debug.WriteLine("WorkersManager:StartDbWorker");
                InfluxDbWorker = new InfluxDbWorker();
                InfluxDbWorker.Start();
            }
        }

        private void SubscriptionWorker_SubscriptionStatusChanged(object sender, SubscriptionStatusUpdatedEventArgs e)
        {
            BasicData.GetBasicData().Status = SubscriptionWorker.SubscriptionStatusToString(e.Status);
            if(e.Status == SubscriptionStatus.Accepted || e.Status == SubscriptionStatus.Subscribed)
            {
                if(!DbConfig.GetInstance().IsDbConfigAvailable())
                {
                    BasicData.GetBasicData().Status = "Logic error: subscribed, but without config";
                }
                else
                {
                    StartDbWorker();
                }
            } 
        }

    }
}
