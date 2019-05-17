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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PetStoreClient
{
    class DbConfig
    {
        public string deviceId = null;
        public string url = null;
        public string orgId = null;
        public string authToken = null;
        public string bucket = null;
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static int DHT22_PIN = 17;
        private Bmp180Sensor bmp180;
        private BME280Sensor bme280;
        private Timer periodicTimer;
        private InfluxDBClient dBClient;
        private DbConfig config;
        private GpioPin dhtPin = null;
        private IDht dhtSensor = null;


        public MainPage()
        {
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;

            // Initialize the Sensors
            InitializeSensors();
        }
        private void MainPage_Unloaded(object sender, object args)
        {
            /* Cleanup */
            bmp180.Dispose();
        }

        private async void InitializeSensors()
        {
            string status = "";

            // Initialize the BMP180 Sensor
            try
            {
                bmp180 = new Bmp180Sensor();
                await bmp180.InitializeAsync();
            }
            catch (Exception ex)
            {
                bmp180 = null;
                status = "BMP 180 error: " + ex.Message;
            }

            try
            { 
                bme280 = new BME280Sensor();
                // Initialize BME280 Sensor
                await bme280.Initialize(0x76);

                await bme280.SetSampling(SensorMode.MODE_FORCED,
                    SensorSampling.SAMPLING_X1, // temperature
                    SensorSampling.SAMPLING_X1, // pressure
                    SensorSampling.SAMPLING_X1, // humidity
                    SensorFilter.FILTER_OFF,
                    StandbyDuration.STANDBY_MS_1000);
            }
            catch (Exception ex)
            {
                bme280 = null;
                if(status.Length > 0)
                {
                    status += "\n";
                }
                status += "BME 280 error: " + ex.Message;
            }

            try
            {

                GpioController controller = GpioController.GetDefault();
                dhtPin = GpioController.GetDefault().OpenPin(DHT22_PIN, GpioSharingMode.Exclusive);
                dhtSensor = new Dht22(dhtPin, GpioPinDriveMode.InputPullUp);
            }
            catch (Exception ex)
            {
                dhtSensor = null;
                if (status.Length > 0)
                {
                    status += "\n";
                }
                status += "DHT 22 error: " + ex.Message;
            }
            // UI updates must be invoked on the UI thread
            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                statusTextBlock.Text = status;
            });

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (bmp180 == null)
                return;


            if (periodicTimer == null)
            {
                periodicTimer = new Timer(this.TimerCallback, null, 0, 5000);
                var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    button.Content = "Stop";
                });
            }
            else
            {
                periodicTimer.Dispose();
                var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    button.Content = "Start";
                    bmp180TempTextBlock.Text = "-";
                    bmp180PresTextBlock.Text = "-";
                    bme280TempTextBlock.Text = "-";
                    bme280HumTextBlock.Text = "-";
                    bme280PresTextBlock.Text = "-";
                    dhtTempTextBlock.Text = "-";
                    dhtHumTextBlock.Text = "-";

                });
                periodicTimer = null;
            }
        }

        private void TimerCallback(object state)
        {
            if (config == null)
            {
                Subscribe();
            }
             ReadData();
           

        }

        private async void ReadData()
        {
            string bmp180TempText = "-",
                bmp180PresText = "-", 
                dhtTempText = "-", 
                dhtHumText = "-", 
                bme280TempText = "-", 
                bme280HumText = "-", 
                bme280PresText = "-";
            string status = "";

            double temp = 0;
            double hum = 0;
            double pres = 0;
            int tempCount = 0;
            int humCount = 0;
            int presCount = 0;

            // Read and format Sensor data
            try
            {
                if (bmp180 != null)
                {
                    var sensorData = await bmp180.GetSensorDataAsync(Bmp180AccuracyMode.UltraHighResolution);
                    bmp180TempText = sensorData.Temperature.ToString("F1") + "°C";
                    bmp180PresText = sensorData.Pressure.ToString("F2") + "hPa";

                    temp += sensorData.Temperature;
                    ++tempCount;

                    pres += sensorData.Pressure;
                    ++presCount;
                }
            }
            catch (Exception ex)
            {
                bmp180TempText = "Sensor Error: " + ex.Message;
                bmp180PresText = "-";
            }

            try { 
                if (bme280 != null)
                {
                    await bme280.TakeForcedMeasurement();

                    // Read Temperature
                    var bme280Temp = await bme280.ReadTemperature();
                    bme280TempText = bme280Temp.ToString("F1") + "°C";

                    temp += bme280Temp;
                    ++tempCount;

                    // Read Humidity
                    var bme280Hum = await bme280.ReadHumidity();
                    bme280HumText = bme280Hum.ToString("F0") + "°C";
                    hum += bme280Hum;
                    ++humCount;

                    // Read Barometric Pressure

                    var bme280Pres = await bme280.ReadPressure();
                    bme280PresText = bme280Pres.ToString("F2") + "hPa";
                    pres += bme280Pres;
                    ++presCount;
                }
            }
            catch (Exception ex)
            {
                bme280TempText = "Sensor Error: " + ex.Message;
                bme280HumText = "-";
                bme280PresText = "-";
            }

            try { 
                if (dhtSensor != null)
                {
                    DhtReading reading = new DhtReading();
                    reading = await dhtSensor.GetReadingAsync().AsTask();

                    if (reading.IsValid)
                    {
                        // ***
                        // *** Get the values from the reading.
                        // ***
                        dhtTempText = "" + reading.Temperature + "°C";
                        dhtHumText = "" + reading.Humidity + "%";

                        temp += reading.Temperature;
                        ++tempCount;
                        hum += reading.Humidity;
                        ++humCount;
                    }
                    else
                    {
                        dhtTempText = "-";
                        dhtHumText = "-";
                    }
                }

               
            }
            catch (Exception ex)
            {
                dhtTempText = "Sensor Error: " + ex.Message;
                dhtHumText = "-";
            }

            try
            {
                if (dBClient != null)
                {
                    //calculate average
                    temp /= tempCount;
                    hum /= humCount;
                    pres /= presCount;

                    WriteToDb(temp, pres, hum);
                }
            } catch(Exception ex)
            {
                status = "DB Error: " + ex.Message;
            }

            // UI updates must be invoked on the UI thread
            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bmp180PresTextBlock.Text = bmp180PresText;
                bmp180TempTextBlock.Text = bmp180TempText;

                bme280HumTextBlock.Text = bme280HumText;
                bme280PresTextBlock.Text = bme280PresText;
                bme280TempTextBlock.Text = bme280TempText;

                dhtHumTextBlock.Text = dhtHumText;
                dhtTempTextBlock.Text = dhtTempText;
                statusTextBlock.Text = status;
            });
        }

        private void UpdateStatus(string status)
        {
            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                statusTextBlock.Text = status;
            });
        }

        private void Subscribe()
        {
            var status = "";
            //todo: url to ui
            RestClient hubClient = new RestClient("http://192.168.1.72:8080/api");
            

            var request = new RestRequest("register/{id}", Method.GET);
            //equest.AddUrlSegment("id", "1234-5678-9012-3456"); // replaces matching token in request.Resource
            request.AddUrlSegment("id", GetSerialNumber());

            hubClient.UserAgent = "Win10IoTCore.PetStore";

            var response = hubClient.Execute(request);
            if (response.ErrorException != null)
            {
                status = "Error: " + response.ErrorException.Message;
            }
            else
            {

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        status = "Device accepted";
                        config = JsonConvert.DeserializeObject<DbConfig>(response.Content);
                        InitializeDb();
                        break;
                    case System.Net.HttpStatusCode.Created:
                        status = "Waiting for device authorization";
                        break;
                    default:
                        status = "Uknown response: " + response.StatusCode.ToString();
                        break;

                }

            }
            UpdateStatus(status);



        }

        private void InitializeDb()
        {
            if (dBClient == null && config != null)
            {            
                try
                {
                    dBClient = InfluxDBClientFactory.Create(config.url, config.authToken.ToCharArray());
                    dBClient.Health();
                }
                catch (Exception ex)
                {
                    UpdateStatus("Error: " + ex.Message);
                }


            }
        }

        private void WriteToDb(double temp, double press, double humidity)
        {
            var status = "Updating";
            UpdateStatus(status);
            try
            {
                if(dBClient != null)
                {
                    
                    //
                    var point = Point.Measurement("air")
                        .Tag("room", "prosek")
                        .Tag("device", config.deviceId)
                        .Field("temp", temp)
                        .Field("press", press)
                        .Field("hum", humidity);
                    var writeClient = dBClient.GetWriteApi();
                    writeClient.WritePoint(config.bucket, config.orgId, point);
                    writeClient.Flush();
                    status = "";
                } else
                {
                    throw new Exception("Not initialized db client");
                }

            } catch(Exception ex)
            {
                status = "Error: " + ex.Message;
            }
            UpdateStatus(status);
        }

        //"B827 EBD4 17DE"
        private string GetSerialNumber()
        {
            var macAddr = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                           where nic.OperationalStatus == OperationalStatus.Up
                           select nic.GetPhysicalAddress().ToString()
                        ).FirstOrDefault();
            var uid = string.Format("{0}-{1}-{2}-0000", macAddr.Substring(0, 4), macAddr.Substring(4, 4), macAddr.Substring(8, 4));
            return uid;
        }

    }
}

