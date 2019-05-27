using InfluxDB.Client;
using InfluxDB.Client.Writes;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RestSharp;
using Newtonsoft.Json;
using Sensors.Dht;
using Windows.Devices.Gpio;
using BuildAzure.IoT.Adafruit.BME280;
using System.ComponentModel;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

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
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;
            Loaded += MainPage_Loaded;

            // Initialize the Sensors
            ViewModel = DetailData.GetDetailData();
        }

        public DetailData ViewModel;

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            WorkersManager man = WorkersManager.GetWorkersManager();
            man.SensorsWorker.StatusChanged += SensorsWorker_StatusChanged;

        }

        private void SensorsWorker_StatusChanged(object sender, StatusUpdatedEventArgs e)
        {
            ViewModel.Status = e.Status;
        }

        private void MainPage_Unloaded(object sender, object args)
        {

            WorkersManager man = WorkersManager.GetWorkersManager();
            man.SensorsWorker.StatusChanged -= SensorsWorker_StatusChanged;

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(BasicPage));
            
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

