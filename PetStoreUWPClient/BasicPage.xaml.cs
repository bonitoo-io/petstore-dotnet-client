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
        public OverviewDataViewModel ViewModel;

        public BasicPage()
        {
            this.InitializeComponent();
            ViewModel = OverviewDataViewModel.GetOverviewDataViewModel();
            
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
