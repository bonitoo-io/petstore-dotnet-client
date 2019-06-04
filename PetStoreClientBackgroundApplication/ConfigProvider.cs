using MetroLog;
using Newtonsoft.Json;
using PetStoreClientDataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PetStoreClientBackgroundApplication
{
    class LocalSettingsConfigProvider : IConfigProvider
    {

        public static LocalSettingsConfigProvider Instance { get; } = new LocalSettingsConfigProvider();

        private LocalSettingsConfigProvider() { }

        public int LoadIntValue(string name, int defaultValue = 0)
        {
            int? val = ApplicationData.Current.LocalSettings.Values[name] as int?;
            return val != null ? (int)val : defaultValue;
        }

        public string LoadStringValue(string name, string defaultValue = null)
        {
            string val = ApplicationData.Current.LocalSettings.Values[name] as string;
            return val ?? defaultValue;
        }

        public void SaveIntValue(string name, int? value)
        {
            ApplicationData.Current.LocalSettings.Values[name] = value;
        }

        public void SaveStringValue(string name, string value)
        {
            ApplicationData.Current.LocalSettings.Values[name] = value;
        }
    }
}
