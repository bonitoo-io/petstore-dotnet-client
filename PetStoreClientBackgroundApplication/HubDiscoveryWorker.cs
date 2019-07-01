using MetroLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PetStoreClientBackgroundApplication
{
    class HubDiscoreryResult
    {
        public string HubUrl { get; private set; }
        public Exception Error { get; private set; }

        public HubDiscoreryResult(string hubUrl)
        {
            this.HubUrl = hubUrl;
            this.Error = null;
        }

        public HubDiscoreryResult(Exception error)
        {
            this.HubUrl = null;
            this.Error = error;
        }
    }

    class HubDiscoveryWorker
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<HubDiscoveryWorker>();
        private BackgroundWorker hubDiscoveryWorker;
        private const int DiscoveryListenPort = 4567;
        private const string PetStoreHubUrlPrefix = "[petstore.hubUrl=";
        private static IPAddress GroupAddress = IPAddress.Parse("239.0.0.2");

        public bool Running { get; private set; }

        public HubDiscoveryWorker()
        {
            hubDiscoveryWorker = new BackgroundWorker();
            hubDiscoveryWorker.WorkerSupportsCancellation = true;
            hubDiscoveryWorker.WorkerReportsProgress = false;
            hubDiscoveryWorker.DoWork += HubDiscoveryWorker_DoWork;
            hubDiscoveryWorker.RunWorkerCompleted += HubDiscoveryWorker_RunWorkerCompleted;
        }

        public void Start()
        {
            hubDiscoveryWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            hubDiscoveryWorker.CancelAsync();
        }

        private void HubDiscoveryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log.Info("Started");
            Running = true;
            Socket socket = null;
            var buffer = new byte[1024];
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                var ipAddress = IPAddress.Any;
                //var ipAddress = IPAddress.Parse("192.168.1.56");

                var endPoint = new IPEndPoint(ipAddress, DiscoveryListenPort);


                socket.Bind(endPoint);
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(GroupAddress, ipAddress));

                Log.Debug("HubDiscoveryWorker:Waiting for broadcast");
                while (!hubDiscoveryWorker.CancellationPending)
                {

                    if (socket.Available > 0)
                    {

                        int receivedLength = socket.Receive(buffer);

                        if (receivedLength == 0)
                        {
                            break;
                        }

                        if (receivedLength < 0)
                        {
                            continue;
                        }
                        string message = Encoding.ASCII.GetString(buffer, 0, receivedLength);
                        if (message.StartsWith(PetStoreHubUrlPrefix))
                        {
                            var url = message.Substring(PetStoreHubUrlPrefix.Length, message.Length - PetStoreHubUrlPrefix.Length - 1);
                            Log.Info($"HubDiscoveryWorker:huburl: {url}");
                            e.Result = new HubDiscoreryResult(url);
                            break;
                        }
                        else
                        {
                            Log.Debug($"HubDiscoveryWorker:Received: {message}");
                        }
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }

            }
            catch (SocketException ex)
            {
                Log.Error("Subscription communication error", ex);
                e.Result = new HubDiscoreryResult(ex);
            }
            finally
            {
                if (socket != null)
                {
                    socket.Dispose();
                    socket = null;
                }


            }
            if (hubDiscoveryWorker.CancellationPending)
            {
                e.Cancel = true;
                Log.Trace("Discovery canceled");
            }

            Running = true;
            Log.Info("Stopped");
        }

        private void HubDiscoveryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            HubDiscoreryResult result = e.Result as HubDiscoreryResult;
            OnDiscoveryCompleted(result);

        }
        void OnDiscoveryCompleted(HubDiscoreryResult hubDiscoreryResult)
        {
            DiscoveryCompleted?.Invoke(this, new HubDisoveryCompletedEventArgs(hubDiscoreryResult));
        }

        internal event EventHandler<HubDisoveryCompletedEventArgs> DiscoveryCompleted;

    }

    class HubDisoveryCompletedEventArgs : EventArgs
    {
        public HubDiscoreryResult Result { get; private set; }
        public HubDisoveryCompletedEventArgs(HubDiscoreryResult hubDiscoreryResult)
        {
            Result = hubDiscoreryResult;
        }

    }

}
