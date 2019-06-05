using PetStoreClientDataModel;
using System;
using System.Linq;

namespace PetStoreConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-h")
                    {
                        PrintHelp();
                    }
                    else if (args[i] == "-host")
                    {
                        if (i++ < args.Length)
                        {
                            Console.WriteLine($"Using host: {args[i]}");
                            BackgroundJobClient.JobUrl = $"http://{args[i]}:8888/api";
                        }
                        else
                        {
                            Console.WriteLine("Missing argument for host");
                            break;
                        }
                    }
                    else if (args[i] == "-view-config")
                    {
                        var config = BackgroundJobClient.GetConfig();
                        Console.WriteLine(config);
                    }
                    else if (args[i] == "-view-data")
                    {
                        PrintOverviewData(BackgroundJobClient.GetOverviewData());
                        Console.WriteLine();
                        PrintSensorData(BackgroundJobClient.GetMeasuredData());
                    }
                    else if (args[i] == "-set-location")
                    {
                        if (i++ < args.Length)
                        {
                            Console.WriteLine($"Using location: {args[i]}");
                            var config = BackgroundJobClient.GetConfig();
                            config.Location = args[i];
                            BackgroundJobClient.UpdateConfig(config);
                        }
                        else
                        {
                            Console.WriteLine("Missing argument for location");
                            break;
                        }
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            if(args.Length == 0)
            {
                PrintHelp();
            }

        }
        static void PrintHelp()
        {
            Console.WriteLine("Args: -h, -host <ip|hostname>,-view-config,-view-data,-set-location <location>");
        }
        static void PrintOverviewData(OverviewData data)
        {
            Console.WriteLine($"Overview data:");
            Console.WriteLine($"\t\tTemperature\tHumidity\tPressure");
            Console.WriteLine($"Actual\t\t{FormatTemperature(data.CurrentTemperature)}\t\t{FormatHumidity(data.CurrentHumidity)}\t\t{FormatPressure(data.CurrentPressure)}");
            Console.WriteLine($"Mean (24h)\t{FormatTemperature(data.MeanTemperature)}\t\t{FormatHumidity(data.MeanHumidity)}\t\t{FormatPressure(data.MeanPressure)}");
            Console.WriteLine($"Min (24h)\t{FormatTemperature(data.MinTemperature)}\t\t{FormatHumidity(data.MinHumidity)}\t\t{FormatPressure(data.MinPressure)}");
            Console.WriteLine($"Max (24h)\t{FormatTemperature(data.MaxTemperature)}\t\t{FormatHumidity(data.MaxHumidity)}\t\t{FormatPressure(data.MaxPressure)}");
        }

        static void PrintSensorData(MeasuredData data)
        {
            Console.WriteLine($"Sensor data:");
            Console.WriteLine($"\t\tBMP180\t\tBME280\t\tDHT22");
            Console.WriteLine($"Temperature\t{FormatTemperature(data.Bmp180Temperature)}\t\t{FormatTemperature(data.Bme280Temperature)}\t\t{FormatTemperature(data.DhtTemperature)}");
            Console.WriteLine($"Humidity\t{FormatHumidity(data.Bme280Humidity)}\t\t\t\t{FormatHumidity(data.DhtHumidity)}");
            Console.WriteLine($"Pressure\t{FormatPressure(data.Bmp180Pressure)}\t{FormatPressure(data.Bme280Pressure)}");
        }


        static string FormatTemperature(double temp)
        {
            return Convert(temp,"F1","°C");
        }
        static string FormatHumidity(double temp)
        {
            return Convert(temp, "F0", "%");
        }
        static string FormatPressure(double temp)
        {
            return Convert(temp, "F1", "hPa");
        }

        static string Convert(double value, string format, string suffix)
        {
            string sval = "";
            if (double.IsNaN(value))
            {
                sval = "-";
            }
            else
            {
                sval = value.ToString(format) + suffix;
            }
            return sval;
        }
    }
}
