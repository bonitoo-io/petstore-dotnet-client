using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PetStoreUWPClient
{
    public class DbConfig
    {
        public string deviceId = null;
        public string url = null;
        public string orgId = null;
        public string authToken = null;
        public string bucket = null;
        public string location = null;
        private bool initialised;

        public static DbConfig instance;

        public static DbConfig GetInstance()
        {
            if(instance == null)
            {
                instance = new DbConfig();
                instance.Load();
            }
            return instance;

        }

        public bool IsDbConfigAvailable()
        {
            return initialised;
        }


        public void InitFromJson(string  json)
        {
            var other = JsonConvert.DeserializeObject<DbConfig>(json);
            url = other.url;
            orgId = other.orgId;
            bucket = other.bucket;
            authToken = other.authToken;
            initialised = true;
        }

        public bool Load()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["dburl"];

            if (value != null)
            {
                Debug.WriteLine("DbConfig:Loading");
                url = value as string;
                orgId = localSettings.Values["orgId"] as string;
                bucket = localSettings.Values["bucket"] as string;
                authToken = localSettings.Values["authToken"] as string;
                if(orgId == null || bucket == null || authToken == null)
                {
                    throw new Exception("Invalid configuration");
                }
                initialised = true;
            }
            return initialised;
        }

        public void Save()
        {
            Debug.WriteLine("DbConfig:Save");
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["dburl"] = url;
            localSettings.Values["orgId"] = orgId;
            localSettings.Values["bucket"] = bucket;
            localSettings.Values["authToken"] = authToken;
        }
    }
}
