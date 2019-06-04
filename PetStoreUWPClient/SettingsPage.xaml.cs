using MetroLog;
using PetStoreClientDataModel;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PetStoreUWPClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<SettingsPage>();
        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel = new SettingsViewModel();

            Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    ViewModel.Load();
                }
                catch (Exception ex)
                {
                    Log.Error("Load error", ex);
                    ViewModel.Status = "Load error: " + ex.Message;
                }

            });
        }

        public SettingsViewModel ViewModel { get; set; }


        private async void  Save_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    var config = BackgroundJobClient.GetConfig();
                    var urlChanged = !ViewModel.HubUrl.Equals(config.HubUrl);
                    ViewModel.Save(config);
                    BackgroundJobClient.UpdateConfig(config);
                    Close();
                } 
                catch(Exception ex)
                {
                    Log.Error("Save error", ex);
                    ViewModel.Status = "Save error: " + ex.Message;
                }
                
            });
        }

        private void Cancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Close();
        }

        private async void Reset_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    var config = BackgroundJobClient.GetConfig();
                    config.Reset();
                    BackgroundJobClient.UpdateConfig(config);
                    Close();
                }
                catch (Exception ex)
                {
                    Log.Error("Reset error", ex);
                    ViewModel.Status = "Reset error: " + ex.Message;
                }
            });
        }


        private void Close()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame.Navigate(typeof(BasicPage));
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

    }
}
