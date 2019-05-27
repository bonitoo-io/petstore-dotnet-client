using BuildAzure.IoT.Adafruit.BME280;
using Sensors.Dht;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace PetStoreUWPClient
{

    public class SensorsWorker
    {
        private const int DHT22_Pin = 17;

        private BackgroundWorker readingWorker;
        private BasicData basicData;
        private DetailData detailData;
        private int readingDelay;

        private Bmp180Sensor bmp180;
        private BME280Sensor bme280;
        private GpioPin dhtPin = null;
        private IDht dhtSensor = null;
        private bool bme280ForceMode = true;

        public SensorsWorker(int delay)
        {
            readingWorker = new BackgroundWorker();
            readingWorker.WorkerSupportsCancellation = true;
            readingWorker.DoWork += ReadingWorker_DoWork;
            this.readingDelay = delay;
            basicData = BasicData.GetBasicData();
            detailData = DetailData.GetDetailData();
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
            Debug.WriteLine("SensorsWorker:InitSensors start");
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
                bme280 = null;
                if (status.Length > 0)
                {
                    status += "\n";
                }
                status += "BME 280 error: " + ex.Message;
            }

            try
            {

                GpioController controller = GpioController.GetDefault();
                dhtPin = GpioController.GetDefault().OpenPin(DHT22_Pin, GpioSharingMode.Exclusive);
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
            if (status.Length > 0) {
                OnStatusChanged(status);
            }
            Debug.WriteLine("SensorsWorker:InitSensors end");
            return bmp180 != null || bme280 != null || dhtSensor != null;
            
        }

        private void ReadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("SensorWorker:ReadingWorker started");
            while(!readingWorker.CancellationPending)
            {
                ReadData().GetAwaiter().GetResult();
                Thread.Sleep(readingDelay);
            }
            if(readingWorker.CancellationPending)
            {
                e.Cancel = true;
            }
            if (bmp180 != null)
            {
                bmp180.Dispose();
            }
            Debug.WriteLine("SensorWorker:ReadingWorker stopped");

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
                }
            }
            catch (Exception ex)
            {
                status = "Bmp180 Error: " + ex.Message;
                Debug.WriteLine(status);
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
                }
            }
            catch (Exception ex)
            {
                status = "Bme280 Error: " + ex.Message;
                Debug.WriteLine(status);
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
                    }
                }
            }
            catch (Exception ex)
            {
                status = "DHT22 Error: " + ex.Message;
                Debug.WriteLine(status);
                detailData.DhtTemperature = double.NaN;
                detailData.DhtHumidity = double.NaN;
            }
            
            try
            {
                //calculate average
                avgTemp /= tempCount;
                avgHum /= humCount;
                avgPres /= presCount;

                basicData.CurrentTemperature = avgTemp;
                basicData.CurrentHumidity = avgHum;
                basicData.CurrentPressure = avgPres;

            }
            catch (Exception ex)
            {
                status = "DB Error: " + ex.Message;
            }


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

    public class StatusUpdatedEventArgs : EventArgs
    {
        public string Status { get; set; }
        public StatusUpdatedEventArgs(string status)
        {
            Status = status;
        }

    }
}
