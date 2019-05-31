using MetroLog;
using System.Threading.Tasks;
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
            ViewModel = new SettingsModel();

            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Load();
        }

        public SettingsModel ViewModel { get; set; }


        private void Save_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var config = Config.GetInstance();
            var urlChanged = !ViewModel.HubUrl.Equals(config.hubUrl);
            ViewModel.Save();
            config.Save();
            if(urlChanged)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                RestartManager();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            Close();
        }

        private void Cancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Close();
        }

        private void Reset_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var config = Config.GetInstance();
            config.Reset();
            config.Save();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RestartManager();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Close();

        }

        private void Close()
        {
            Frame.Navigate(typeof(BasicPage));
        }

        private async Task RestartManager()
        {
            await WorkersManager.GetWorkersManager().Stop();
            await WorkersManager.GetWorkersManager().Start();
        }
    }
}
