using System;
using System.Collections.Generic;
using System.Text;
using MetroLog;
using Newtonsoft.Json;
using RestSharp;

namespace PetStoreClientDataModel
{
    public class BackgroundJobClient
    {
        private static ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<BackgroundJobClient>();
        public static string JobUrl = "http://localhost:8888/api";

        public static Config GetConfig()
        {
            Config config = null;
            RestClient jobClient = new RestClient(JobUrl);
            Log.Trace("GetConfig");

            var request = new RestRequest("config", Method.GET);

            jobClient.UserAgent = "Win10IoTCore.PetStore";

            var response = jobClient.Execute(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else
            {
                config = JsonConvert.DeserializeObject<Config>(response.Content);
            }
            Log.Debug($"Config: {config}");
            return config;
        }

        public static void UpdateConfig(Config config)
        {
            RestClient jobClient = new RestClient(JobUrl);
            Log.Trace("UpdateConfig");

            var request = new RestRequest("config", Method.PUT);

            jobClient.UserAgent = "Win10IoTCore.PetStore";

            var body = JsonConvert.SerializeObject(config);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            Log.Debug($"Sending:{body}\n");

            var response = jobClient.Execute(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            if(response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception("Unexpected response: " + (int)response.StatusCode);
            }
        }

        public static MeasuredData GetMeasuredData()
        {
            MeasuredData data = null;
            RestClient jobClient = new RestClient(JobUrl);
            Log.Trace("GetMeasuredData");

            var request = new RestRequest("data/meassured", Method.GET);

            jobClient.UserAgent = "Win10IoTCore.PetStore";

            var response = jobClient.Execute(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else
            {
                data = JsonConvert.DeserializeObject<MeasuredData>(response.Content);
            }
            return data;
        }

        public static OverviewData GetOverviewData()
        {
            OverviewData data = null;
            RestClient jobClient = new RestClient(JobUrl);
            Log.Trace("GetOverviewData");

            var request = new RestRequest("data/overview", Method.GET);

            jobClient.UserAgent = "Win10IoTCore.PetStore";

            var response = jobClient.Execute(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else
            {
                data = JsonConvert.DeserializeObject<OverviewData>(response.Content);
            }
            return data;
        }
    }
}
