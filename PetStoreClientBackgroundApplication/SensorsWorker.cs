using BuildAzure.IoT.Adafruit.BME280;
using MetroLog;
using PetStoreClientDataModel;
using Sensors.Dht;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace PetStoreClientBackgroundApplication
{

    class SensorsWorker
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<SensorsWorker>();
        private const int DHT22_Pin = 17;

        private BackgroundWorker readingWorker;
        private OverviewData basicData;
        private MeasuredData detailData;
        private int readingDelay;

        private Bmp180Sensor bmp180;
        private BME280Sensor bme280;
        private GpioPin dhtPin = null;
        private IDht dhtSensor = null;
        private bool bme280ForceMode = true;

        public bool Running { get; private set; }

        public SensorsWorker(int delay)
        {
            readingWorker = new BackgroundWorker();
            readingWorker.WorkerSupportsCancellation = true;
            readingWorker.DoWork += ReadingWorker_DoWork;
            this.readingDelay = delay;
            basicData = OverviewData.GetOverviewData();
            detailData = MeasuredData.GetMeasuredData();
        }

        public async Task Start()
        {
            var res = await InitSensors();
            readingWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            readingWorker.CancelAsync();
        }

        private async Task<bool> InitSensors()
        {
            Log.Trace("SensorsWorker:InitSensors start");
            OnStatusChanged("Initialising sensors");
            // Initialize the BMP180 Sensor
            try
            {
                OnStatusChanged("Initialising sensors..BMP180");
                bmp180 = new Bmp180Sensor();
                await bmp180.InitializeAsync();
            }
            catch (Exception ex)
            {
                Log.Info("BMP180 Init Error", ex);
                bmp180 = null;
            }

            try
            {
                OnStatusChanged("Initialising sensors..BME280");
                bme280 = new BME280Sensor();
                // Initialize BME280 Sensor
                await bme280.Initialize(0x76);

                if (bme280ForceMode)
                {
                    await bme280.SetSampling(SensorMode.MODE_FORCED,
                        SensorSampling.SAMPLING_X1, // temperature
                        SensorSampling.SAMPLING_X1, // pressure
                        SensorSampling.SAMPLING_X1, // humidity
                        SensorFilter.FILTER_OFF,
                        StandbyDuration.STANDBY_MS_1000);
                }
            }
            catch (Exception ex)
            {
                Log.Info("BME280 Init Error", ex);
                bme280 = null;
            }

            try
            {
                OnStatusChanged("Initialising sensors..DHT22");
                GpioController controller = GpioController.GetDefault();
                dhtPin = GpioController.GetDefault().OpenPin(DHT22_Pin, GpioSharingMode.Exclusive);
                dhtSensor = new Dht22(dhtPin, GpioPinDriveMode.InputPullUp);
            }
            catch (Exception ex)
            {
                Log.Info("DHT22 Init Error", ex);
                dhtSensor = null;
            }
            string status = "";
            if (bmp180 != null || bme280 != null || dhtSensor != null)
            {
                if(bmp180 != null)
                {
                    status += "BMP180";
                }
                if (bme280 != null)
                {
                    if(status.Length > 0)
                    {
                        status += ", ";
                    }
                    status += "BME280";
                }
                if (dhtSensor != null)
                {
                    if (status.Length > 0)
                    {
                        status += ", ";
                    }
                    status += "DHT22";
                }
                status = $"Found: {status} sensors";
            } else
            {
                status = "No sensor found";
            }
            OnStatusChanged(status);
            await Task.Delay(500);
            Log.Trace($"SensorsWorker:InitSensors end");
            Log.Info(status);
            return bmp180 != null || bme280 != null || dhtSensor != null;
            
        }

        private void ReadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log.Trace("SensorWorker:ReadingWorker started");
            Running = true;
            while (!readingWorker.CancellationPending)
            {
                ReadData().GetAwaiter().GetResult();
                Thread.Sleep(readingDelay);
            }
            if(readingWorker.CancellationPending)
            {
                e.Cancel = true;
            }
            DeinitSensors();
            Running = false;
            Log.Trace("SensorWorker:ReadingWorker stopped");
        }

        private void DeinitSensors()
        {
            if (bmp180 != null)
            {
                bmp180.Dispose();
                bmp180 = null;
            }
            if (bme280 != null)
            {
                bme280.Dispose();
                bme280 = null;
            }
            if (dhtSensor != null)
            {
                dhtSensor = null;
                dhtPin.Dispose();
                dhtPin = null;
            }
        }

        private async Task ReadData()
        {
            string status = "";

            double avgTemp = 0;
            double avgHum = 0;
            double avgPres = 0;
            int tempCount = 0;
            int humCount = 0;
            int presCount = 0;

            Log.Trace("SensorsWorker:ReadData");
            // Read and format Sensor data
            try
            {
                if (bmp180 != null)
                {
                    var sensorData = await bmp180.GetSensorDataAsync(Bmp180AccuracyMode.UltraHighResolution);
                    detailData.Bmp180Temperature = sensorData.Temperature;
                    detailData.Bmp180Pressure = sensorData.Pressure;

                    avgTemp += sensorData.Temperature;
                    ++tempCount;

                    avgPres += sensorData.Pressure;
                    ++presCount;
                    Log.Debug($"BMP180: {detailData.Bmp180Temperature},N/A,{detailData.Bmp180Pressure}");
                }
            }
            catch (Exception ex)
            {
                status = "Bmp180 Error: " + ex.Message;
                Log.Error("Bmp180 read Error", ex);
                detailData.Bmp180Temperature = double.NaN;
                detailData.Bmp180Pressure = double.NaN;
            }


            try
            {
                if (bme280 != null)
                {
                    if (bme280ForceMode)
                    {
                        await bme280.TakeForcedMeasurement();
                    }

                    // Read Temperature
                    detailData.Bme280Temperature = await bme280.ReadTemperature();
                    avgTemp += detailData.Bme280Temperature;
                    ++tempCount;
                    // Read Humidity
                    detailData.Bme280Humidity = await bme280.ReadHumidity();
                    avgHum += detailData.Bme280Humidity;
                    ++humCount;

                    // Read Barometric Pressure
                    detailData.Bme280Pressure = await bme280.ReadPressure() / 100.0;
                    avgPres += detailData.Bme280Pressure;
                    ++presCount;
                    Log.Debug($"BME280: {detailData.Bme280Temperature},{detailData.Bme280Humidity},{detailData.Bme280Pressure}");
                }
            }
            catch (Exception ex)
            {
                status = "Bme280 Error: " + ex.Message;
                Log.Error("Bme280 read Error", ex);
                detailData.Bme280Temperature = double.NaN;
                detailData.Bme280Humidity = double.NaN;
                detailData.Bme280Pressure = double.NaN;
            }

     
            try
            {
                if (dhtSensor != null)
                {
                    DhtReading reading = new DhtReading();
                    reading = await dhtSensor.GetReadingAsync();

                    if (reading.IsValid)
                    {
                        // ***
                        // *** Get the values from the reading.
                        // ***
                        detailData.DhtTemperature = reading.Temperature;
                        detailData.DhtHumidity = reading.Humidity;

                        avgTemp += reading.Temperature;
                        ++tempCount;
                        avgHum += reading.Humidity;
                        ++humCount;
                        Log.Debug($"DHT22: {detailData.DhtTemperature},{detailData.DhtHumidity},N/A,");
                    }
                    else
                    {
                        Log.Debug($"DHT22: invalid reading");
                    }
                }
            }
            catch (Exception ex)
            {
                status = "DHT22 Error: " + ex.Message;
                Log.Error("DHT22 read Error", ex);
                detailData.DhtTemperature = double.NaN;
                detailData.DhtHumidity = double.NaN;
            }
            
           
            //calculate average
            avgTemp /= tempCount;
            avgHum /= humCount;
            avgPres /= presCount;

            basicData.CurrentTemperature = avgTemp;
            basicData.CurrentHumidity = avgHum;
            basicData.CurrentPressure = avgPres;


            if (status.Length > 0)
            {
                OnStatusChanged(status);
            }
        }



        protected void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, new StatusUpdatedEventArgs(status));
        }

        public event EventHandler<StatusUpdatedEventArgs> StatusChanged;

    }

    class StatusUpdatedEventArgs
    {
        public string Status { get; set; }
        public StatusUpdatedEventArgs(string status)
        {
            Status = status;
        }

    }
}
