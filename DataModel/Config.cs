using MetroLog;
using Newtonsoft.Json;

namespace PetStoreClientDataModel
{
    public interface IConfigProvider
    {
        string LoadStringValue(string name, string defaultValue = null);
        int LoadIntValue(string name, int defaultValue = 0);
        void SaveStringValue(string name, string value);
        void SaveIntValue(string name, int? value);
    }

    public class Config
    {
        private static ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<Config>();

        public const int DefaultSensorsPeriod = 15000;
        public const int DefaultDbReadWritePeriod = 30000;
        public const int DefaultSubscriptionCheckPeriod = 30000;
        public const string DefaultHubUrl = "https://petstore.bonitoo4influxdata.com/api";

        [JsonProperty("hubUrl")]
        public string HubUrl { get; set; }
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("orgId")]
        public string OrgId { get; set; }
        [JsonProperty("authToken")]
        public string AuthToken { get; set; }
        [JsonProperty("bucket")]
        public string Bucket { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("sensorPeriod")]
        public int SensorsPeriod { get; set; }
        [JsonProperty("dbReadWritePeriod")]
        public int DbReadWritePeriod { get; set; }
        [JsonProperty("subscriptionCheckPeriod")]
        public int SubscriptionCheckPeriod { get; set; }


        public void Reset()
        {
            HubUrl = null;
            Location = null;
            DeviceId = null;
            SensorsPeriod = DefaultSensorsPeriod;
            DbReadWritePeriod = DefaultDbReadWritePeriod;
            SubscriptionCheckPeriod = DefaultSubscriptionCheckPeriod;
            ResetDbSettings();
        }

        public void ResetDbSettings()
        {
            Url = null;
            OrgId = null;
            AuthToken = null;
            Bucket = null;
        }

        public bool IsDbConfigAvailable()
        {
            return Url != null;
        }


        public void UpdateFromSubscriptionRensponseJson(string  json)
        {
            var other = JsonConvert.DeserializeObject<Config>(json);
            Url = other.Url;
            OrgId = other.OrgId;
            Bucket = other.Bucket;
            AuthToken = other.AuthToken;
            DeviceId = other.DeviceId;
        }

        public void Update(Config other)
        {
            HubUrl = other.HubUrl;
            Location = other.Location;
            SensorsPeriod = other.SensorsPeriod;
            DbReadWritePeriod = other.DbReadWritePeriod;
            SubscriptionCheckPeriod = other.SubscriptionCheckPeriod;
        }

       

        public void Load(IConfigProvider provider)
        {
            Log.Trace("Loading");
            Url = provider.LoadStringValue("dburl");
            OrgId = provider.LoadStringValue("orgId");
            Bucket = provider.LoadStringValue("bucket");
            AuthToken = provider.LoadStringValue("authToken");
            DeviceId = provider.LoadStringValue("deviceId");
            Location = provider.LoadStringValue("location");
            HubUrl = provider.LoadStringValue("hubUrl", DefaultHubUrl);
            SensorsPeriod = provider.LoadIntValue("sensorsPeriod", DefaultSensorsPeriod);
            DbReadWritePeriod = provider.LoadIntValue("dbReadWritePeriod", DefaultDbReadWritePeriod);
            SubscriptionCheckPeriod = provider.LoadIntValue("subscriptionCheckPeriod", DefaultSubscriptionCheckPeriod);
            Log.Info($"Loaded: { this }");

        }

        public void Save(IConfigProvider provider)
        {
            Log.Trace("Save");
            provider.SaveStringValue("dburl", ValueOrNull(Url));
            provider.SaveStringValue("orgId", ValueOrNull(OrgId));
            provider.SaveStringValue("bucket", ValueOrNull(Bucket));
            provider.SaveStringValue("authToken", ValueOrNull(AuthToken));
            provider.SaveStringValue("location", ValueOrNull(Location));
            provider.SaveStringValue("hubUrl", ValueOrNull(HubUrl));
            provider.SaveStringValue("deviceId", ValueOrNull(DeviceId));
            provider.SaveIntValue("sensorsPeriod", SensorsPeriod);
            provider.SaveIntValue("dbReadWritePeriod", DbReadWritePeriod);
            provider.SaveIntValue("subscriptionCheckPeriod", SubscriptionCheckPeriod);
            Log.Info($"Saved: { this }");
        }

        private static string ValueOrNull(string val)
        {
            return (val != null && val.Trim().Length > 0) ? val.Trim() : null;
        }
        private static int? ValueOrNull(int val)
        {
            return val != 0 ? (int?)val : null;
        }

        public override string ToString()
        {
            return $"Config:\n" +
                $"\tdbUrl: {Url}\n" +
                $"\torgId: {OrgId}\n" +
                $"\tbucket: {Bucket}\n" +
                $"\tauthToken: {AuthToken}\n" +
                $"\tlocation: {Location}\n" +
                $"\thubUrl: {HubUrl}\n" +
                $"\tdeviceId: {DeviceId}\n" +
                $"\tsensorsPeriod: {SensorsPeriod}\n" +
                $"\tdbReadWritePeriod: {DbReadWritePeriod}\n" +
                $"\tsubscriptionCheckPeriod: {SubscriptionCheckPeriod}\n";

        }
    }
}
