namespace PetStoreUWPClient
{
    public class DetailData : BindableBase
    {
        private static DetailData instance = new DetailData();

        public static DetailData GetDetailData()
        {
            return instance;
        }

        private DetailData()
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
