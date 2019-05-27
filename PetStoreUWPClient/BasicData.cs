using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetStoreUWPClient
{
    public class BasicData : BindableBase
    {
        private static BasicData instance = new BasicData();

        public static BasicData GetBasicData()
        {
            return instance;
        }
        private BasicData()
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

        private double currentTemperature;
        public double CurrentTemperature {
            get { return currentTemperature; }
            set { SetProperty(ref currentTemperature, value); }
        }
        private double meanTemperature;
        public double MeanTemperature
        {
            get { return meanTemperature; }
            set { SetProperty(ref meanTemperature, value); }
        }
        private double minTemperature;
        public double MinTemperature
        {
            get { return minTemperature; }
            set { SetProperty(ref minTemperature, value); }
        }
        private double maxTemperature;
        public double MaxTemperature
        {
            get { return maxTemperature; }
            set { SetProperty(ref maxTemperature, value); }
        }
        private double currentHumidity;
        public double CurrentHumidity
        {
            get { return currentHumidity; }
            set { SetProperty(ref currentHumidity, value); }
        }
        private double meanHumidity;
        public double MeanHumidity
        {
            get { return meanHumidity; }
            set { SetProperty(ref meanHumidity, value); }
        }
        private double maxHumidity;
        public double MaxHumidity
        {
            get { return maxHumidity; }
            set { SetProperty(ref maxHumidity, value); }
        }
        private double minHumidity;
        public double MinHumidity
        {
            get { return minHumidity; }
            set { SetProperty(ref minHumidity, value); }
        }

        private double currentPressure;
        public double CurrentPressure
        {
            get { return currentPressure; }
            set { SetProperty(ref currentPressure, value); }
        }
        private double meanPressure;
        public double MeanPressure
        {
            get { return meanPressure; }
            set { SetProperty(ref meanPressure, value); }
        }
        private double maxPressure;
        public double MaxPressure
        {
            get { return maxPressure; }
            set { SetProperty(ref maxPressure, value); }
        }
        private double minPressure;
        public double MinPressure
        {
            get { return minPressure; }
            set { SetProperty(ref minPressure, value); }
        }
        private string status;
        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }
    }
}
