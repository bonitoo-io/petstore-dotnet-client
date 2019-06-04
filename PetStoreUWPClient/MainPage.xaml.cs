using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PetStoreUWPClient
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        
        
        Random random = new Random(123);


        public MainPage()
        {
            SetUpPageAnimation();
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;
            Loaded += MainPage_Loaded;

            // Initialize the Sensors
            ViewModel = SensorsDataViewModel.GetSensorsDataViewModel();
        }

        public SensorsDataViewModel ViewModel;

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MainPage_Unloaded(object sender, object args)
        {

        }

        private void SetUpPageAnimation()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(BasicPage), null, new SuppressNavigationTransitionInfo());
            
        }

        private void test()
        {
            ViewModel.Bmp180Temperature = random.NextDouble()*random.Next(30);
            ViewModel.Bmp180Pressure = 960+random.Next(50);
            ViewModel.Bme280Humidity = random.Next(90);
            ViewModel.Bme280Temperature = random.NextDouble() * random.Next(30);
        }

    }
}

