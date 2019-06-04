using MetroLog;
using PetStoreClientDataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetStoreUWPClient
{
    class DataManager
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<DataManager>();
        public static DataManager Instance { get; } = new DataManager();
        private BackgroundWorker backroundWorker;


        private DataManager()
        {
            backroundWorker = new BackgroundWorker();
            backroundWorker.WorkerSupportsCancellation = true;
            backroundWorker.DoWork += Run;
        }

        private void Run(object sender, DoWorkEventArgs e)
        {
            Log.Trace("Started");
            while(!backroundWorker.CancellationPending)
            {
                try
                {
                    SensorsDataViewModel.GetSensorsDataViewModel().Update(BackgroundJobClient.GetMeasuredData());
                    OverviewDataViewModel.GetOverviewDataViewModel().Update(BackgroundJobClient.GetOverviewData());
                }
                catch (Exception ex)
                {
                    Log.Error("Error", ex);
                }
                Thread.Sleep(15000);
            }
            if(backroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }
            Log.Trace("Stopped");

        }

        

        public void Start()
        {
            backroundWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            backroundWorker.CancelAsync();
        }
    }
}
