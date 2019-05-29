using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace PetStoreUWPClient
{
    /// <summary>
    /// Fist page showing actual and statics for measurement
    /// </summary>
    public sealed partial class BasicPage : Page
    {
        public BasicData ViewModel;

        public BasicPage()
        {
            this.InitializeComponent();
            ViewModel = BasicData.GetBasicData();
            
            this.Loaded += BasicPage_Loaded;
            this.Unloaded += BasicPage_Unloaded;
        }

        private void StatusChanged(object sender, StatusUpdatedEventArgs e)
        {
            ViewModel.Status = e.Status;
        }

        private void BasicPage_Unloaded(object sender, RoutedEventArgs e)
        {
            WorkersManager man = WorkersManager.GetWorkersManager();
            man.StatusChanged -= StatusChanged;

        }

        private async void BasicPage_Loaded(object sender, RoutedEventArgs e)
        {
            WorkersManager man = WorkersManager.GetWorkersManager();
            man.StatusChanged += StatusChanged;
            await man.Start();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(MainPage));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
