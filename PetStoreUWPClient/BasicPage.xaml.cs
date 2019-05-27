using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PetStoreUWPClient
{
    /// <summary>
    /// Fist page showing actual and statics for measurement
    /// </summary>
    public sealed partial class BasicPage : Page
    {
        public BasicData ViewModel;
        private string hubUrl;

        public BasicPage()
        {
            this.InitializeComponent();
            ViewModel = BasicData.GetBasicData();
            
            this.Loaded += BasicPage_Loaded;
            this.Unloaded += BasicPage_Unloaded;
        }

        private void SensorsWorker_StatusChanged(object sender, StatusUpdatedEventArgs e)
        {
            ViewModel.Status = e.Status;
        }

        private void BasicPage_Unloaded(object sender, RoutedEventArgs e)
        {
            WorkersManager man = WorkersManager.GetWorkersManager();
            if (man.HubDiscoveryWorker != null)
            {
                man.HubDiscoveryWorker.DiscoveryCompleted -= HubDiscoveryWorker_DiscoveryCompleted;
            }
            man.SensorsWorker.StatusChanged -= SensorsWorker_StatusChanged;

        }

        private async void BasicPage_Loaded(object sender, RoutedEventArgs e)
        {
            WorkersManager man = WorkersManager.GetWorkersManager();
            await man.Start();
            if (man.HubDiscoveryWorker != null)
            {
                man.HubDiscoveryWorker.DiscoveryCompleted += HubDiscoveryWorker_DiscoveryCompleted;
                ViewModel.Status = "Discovering hub";
            }
            man.SensorsWorker.StatusChanged += SensorsWorker_StatusChanged;
        }

        private void HubDiscoveryWorker_DiscoveryCompleted(object sender, HubDisoveryCompletedEventArgs e)
        {
            var status = "";
            if (e.Result != null)
            {
                var discoveryResult = e.Result as HubDiscoreryResult;
                if (discoveryResult.HubUrl != null)
                {
                    hubUrl = discoveryResult.HubUrl;
                    status = "Discoverered hub url: " + hubUrl;
                }
                else
                {
                    status = "Discoverery error: " + discoveryResult.Error.Message;
                }

            }
             else
            {
                Debug.WriteLine("Discoveryt canceled");
            }
            ViewModel.Status = status;
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(MainPage));
        }

       
    }
}
