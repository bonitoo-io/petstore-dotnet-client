namespace PetStoreClientDataModel
{
    public class MeasuredData
    {
        private static MeasuredData instance = new MeasuredData();

        public static MeasuredData GetMeasuredData()
        {
            return instance;
        }

        private MeasuredData()
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

        public double Bmp180Temperature { get; set; }
        public double Bme280Temperature { get; set; }
        public double DhtTemperature { get; set; }
        public double Bme280Humidity { get; set; }
        public double DhtHumidity { get; set; }
        public double Bmp180Pressure { get; set; }
        public double Bme280Pressure { get; set; }
        public string Status { get; set; }
    }
}
