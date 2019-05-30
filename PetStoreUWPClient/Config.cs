using MetroLog;
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
    public class Config
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<Config>();

        public string hubUrl = null;
        public string deviceId = null;
        public string url = null;
        public string orgId = null;
        public string authToken = null;
        public string bucket = null;
        public string location = null;
        private bool initialised;

        public static Config instance;

        public static Config GetInstance()
        {
            if(instance == null)
            {
                instance = new Config();
                instance.Load();
            }
            return instance;

        }

        public void Reset()
        {
            hubUrl = null;
            location = null;
            deviceId = null;
            ResetDbSettings();
        }

        public void ResetDbSettings()
        {
            url = null;
            orgId = null;
            authToken = null;
            bucket = null;
        }

        public bool IsDbConfigAvailable()
        {
            return url != null;
        }


        public void InitFromJson(string  json)
        {
            var other = JsonConvert.DeserializeObject<Config>(json);
            url = other.url;
            orgId = other.orgId;
            bucket = other.bucket;
            authToken = other.authToken;
            deviceId = other.deviceId;
            initialised = true;
        }

        public bool Load()
        {
            Log.Trace("DbConfig:Loading");
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            url = localSettings.Values["dburl"] as string;
            orgId = localSettings.Values["orgId"] as string;
            bucket = localSettings.Values["bucket"] as string;
            authToken = localSettings.Values["authToken"] as string;
            deviceId = localSettings.Values["deviceId"] as string;
            location = localSettings.Values["location"] as string;
            hubUrl = localSettings.Values["hubUrl"] as string;
            Log.Info($"DbConfig:Loaded:\n{ this }");
            initialised = true;
            return initialised;

        }

        public void Save()
        {
            Log.Trace("DbConfig:Save");
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["dburl"] = ValueOrNull(url);
            localSettings.Values["orgId"] = ValueOrNull(orgId);
            localSettings.Values["bucket"] = ValueOrNull(bucket);
            localSettings.Values["authToken"] = ValueOrNull(authToken);
            localSettings.Values["location"] = ValueOrNull(location);
            localSettings.Values["hubUrl"] = ValueOrNull(hubUrl);
            localSettings.Values["deviceId"] = ValueOrNull(deviceId);
            Log.Info($"DbConfig:Saved:\n{ this }");
        }

        private static string ValueOrNull(string val)
        {
            return (val != null && val.Trim().Length > 0) ? val.Trim() : null;
        }

        public override string ToString()
        {
            return $"Config:\n\tdbUrl: {url}\n\torgId: {orgId}\n\tbucket: {bucket}\n\tauthToken: {authToken}\n\tlocation: {location}\n\thubUrl: {hubUrl}\n\tdeviceId: {deviceId}\n";
        }
    }
}
