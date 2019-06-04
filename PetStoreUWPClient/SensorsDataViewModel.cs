using PetStoreClientDataModel;

namespace PetStoreUWPClient
{
    public class SensorsDataViewModel : BindableBase
    {
        private static SensorsDataViewModel instance = new SensorsDataViewModel();

        public static SensorsDataViewModel GetSensorsDataViewModel()
        {
            return instance;
        }

        private SensorsDataViewModel()
        {
            Reset();
        }

        public void Reset()
        {
            Bmp180Temperature = double.NaN;
            Bmp180Pressure = double.NaN;
            Bme280Temperature = double.NaN;
            Bme280Humidity = double.NaN;
            Bme280Pressure = double.NaN;
            DhtTemperature = double.NaN;
            DhtHumidity = double.NaN;
            Status = "";
        }
        
        public void Update(MeasuredData measuredData)
        {
            Bmp180Temperature = measuredData.Bmp180Temperature;
            Bmp180Pressure = measuredData.Bmp180Pressure;
            Bme280Temperature = measuredData.Bme280Temperature;
            Bme280Humidity = measuredData.Bme280Humidity;
            Bme280Pressure = measuredData.Bme280Pressure;
            DhtTemperature = measuredData.DhtTemperature;
            DhtHumidity = measuredData.DhtHumidity;
            Status = measuredData.Status;
        }

        private double bmp180Temperature;
        public double Bmp180Temperature {
            get { return bmp180Temperature; }
            set { SetProperty(ref bmp180Temperature, value); }
        }
        private double bme280Temperature;
        public double Bme280Temperature
        {
            get { return bme280Temperature; }
            set { SetProperty(ref bme280Temperature, value); }
        }
        private double dhtTemperature;
        public double DhtTemperature
        {
            get { return dhtTemperature; }
            set { SetProperty(ref dhtTemperature, value); }
        }
        private double bme280Humidity;
        public double Bme280Humidity
        {
            get { return bme280Humidity; }
            set { SetProperty(ref bme280Humidity, value); }
        }
        private double dhtHumidity;
        public double DhtHumidity
        {
            get { return dhtHumidity; }
            set { SetProperty(ref dhtHumidity, value); }
        }
        private double bmp180Pressure;
        public double Bmp180Pressure
        {
            get { return bmp180Pressure; }
            set { SetProperty(ref bmp180Pressure, value); }
        }
        private double bme280Pressure;
        public double Bme280Pressure
        {
            get { return bme280Pressure; }
            set { SetProperty(ref bme280Pressure, value); }
        }
        private string status;
        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }
    }
}
