using RestSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace PetStoreUWPClient
{
    public enum SubscriptionStatus
    {
        WaitingForAuthorization,
        Accepted,
        Subscribed,
        Error
    };

    public class SubscriptionWorker
    {
        private BackgroundWorker subscriptionWorker;
        private string hubUrl;
        private int delay;

        public string ErrorString { get; private set; }

        public SubscriptionWorker(string hubUrl, int delay = 15000)
        {
            this.hubUrl = hubUrl;
            this.delay = delay;
            subscriptionWorker = new BackgroundWorker();
            subscriptionWorker.WorkerSupportsCancellation = true;
            subscriptionWorker.RunWorkerCompleted += ReadingWorker_RunWorkerCompleted;
            subscriptionWorker.DoWork += ReadingWorker_DoWork;
        }

        public void Start()
        {
            subscriptionWorker.RunWorkerAsync();
        }

        private void ReadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while(!subscriptionWorker.CancellationPending)
            {
                Subscribe();
                Thread.Sleep(delay);
            }
            if(subscriptionWorker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void ReadingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void Subscribe()
        {
            var status = SubscriptionStatus.Error;
            //todo: url to ui
            RestClient hubClient = new RestClient(hubUrl);


            var request = new RestRequest("register/{id}", Method.GET);
            //equest.AddUrlSegment("id", "1234-5678-9012-3456"); // replaces matching token in request.Resource
            request.AddUrlSegment("id", GetSerialNumber());

            hubClient.UserAgent = "Win10IoTCore.PetStore";

            var response = hubClient.Execute(request);
            if (response.ErrorException != null)
            {
                status = SubscriptionStatus.Error;
                ErrorString = "Error: " + response.ErrorException.Message;
                Debug.WriteLine("SubscriptionWorker:Subscribe:Error: " + ErrorString);
            }
            else
            {
                Debug.WriteLine("SubscriptionWorker:Subscribe:statusCode: " + (int)response.StatusCode);
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        status = SubscriptionStatus.Accepted;// "Device accepted";
                        DbConfig.GetInstance().InitFromJson(response.Content);
                        DbConfig.GetInstance().Save();
                        break;
                    case System.Net.HttpStatusCode.Created:
                        status = SubscriptionStatus.WaitingForAuthorization;// "Waiting for device authorization";
                        break;
                    case System.Net.HttpStatusCode.NoContent:
                        status = SubscriptionStatus.Subscribed;
                        break;
                    default:
                        status = SubscriptionStatus.Error;
                        ErrorString = "Uknown response: " + response.StatusCode.ToString();
                        break;

                }

            }
            OnSubscriptionStatusChanged(status);
        }

        public string SubscriptionStatusToString(SubscriptionStatus status)
        {
            var text = "Unknown";
            switch(status)
            {
                case SubscriptionStatus.Accepted:
                    text = "Device accepted";
                    break;
                case SubscriptionStatus.Error:
                    text = ErrorString;
                    break;
                case SubscriptionStatus.Subscribed:
                    text = "Already subscribed";
                    break;
                case SubscriptionStatus.WaitingForAuthorization:
                    text = "Waiting for device authorization";
                    break;
            }
            return text;
        }

        //"B827 EBD4 17DE"
        private static string SerialNumber;
        private static string GetSerialNumber()
        {
            if (SerialNumber == null)
            {
                var macAddr = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                               where nic.OperationalStatus == OperationalStatus.Up
                               select nic.GetPhysicalAddress().ToString()
                            ).FirstOrDefault();
                SerialNumber = string.Format("{0}-{1}-{2}-0000", macAddr.Substring(0, 4), macAddr.Substring(4, 4), macAddr.Substring(8, 4));
            }
            return SerialNumber;
        }
        protected void OnSubscriptionStatusChanged(SubscriptionStatus status)
        {
            SubscriptionStatusChanged?.Invoke(this, new SubscriptionStatusUpdatedEventArgs(status));
        }

        public event EventHandler<SubscriptionStatusUpdatedEventArgs> SubscriptionStatusChanged;

    }

    public class SubscriptionStatusUpdatedEventArgs : EventArgs
    {
        public SubscriptionStatus Status { get; set; }
        public SubscriptionStatusUpdatedEventArgs(SubscriptionStatus status)
        {
            Status = status;
        }

    }
}
