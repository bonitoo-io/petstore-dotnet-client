using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PetStoreUWPClient
{
    class DoubleConverter
    {
        public static object Convert(object value, string format, string suffix)
        {
            double dval = (double)value;
            string sval = "";
            if(double.IsNaN(dval))
            {
                sval = "-";
            }
            else
            {
                sval = dval.ToString(format) + suffix;
            }
            return sval;
        }

    }

    public class TemperatureConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return DoubleConverter.Convert(value, "F1", "°C");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class HumidityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return DoubleConverter.Convert(value, "F0", "%");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PressureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return DoubleConverter.Convert(value, "F1", "hPa");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
