using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetStoreClientDataModel
{
    public class OverviewData
    {
        private static OverviewData instance = new OverviewData();

        public static OverviewData GetOverviewData()
        {
            return instance;
        }
        private OverviewData()
        {
            
        }

        public void Reset()
        {
            CurrentTemperature = double.NaN;
            MeanTemperature = double.NaN;
            MaxTemperature = double.NaN;
            MinTemperature = double.NaN;
            CurrentHumidity = double.NaN;
            MinHumidity = double.NaN;
            MeanHumidity = double.NaN;
            MaxHumidity = double.NaN;
            CurrentPressure = double.NaN;
            MeanPressure = double.NaN;
            MinPressure = double.NaN;
            MaxPressure = double.NaN;
            Status = "";
        }

        public double CurrentTemperature { get; set; }
        public double MeanTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public double CurrentHumidity { get; set; }
        public double MeanHumidity { get; set; }
        public double MaxHumidity { get; set; }
        public double MinHumidity { get; set; }
        public double CurrentPressure { get; set; }
        public double MeanPressure { get; set; }
        public double MaxPressure { get; set; }
        public double MinPressure { get; set; }
        public string Status { get; set; }
    }
}
