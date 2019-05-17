using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Linq;
using System.Threading;
using RestSharp;
using Newtonsoft.Json;

namespace PetStoreClientTest
{
    class OnboardingResponse
    {
        public String deviceId;
        public String url;
        public String orgId;
        public String authToken;
        public String bucket;
    }

    class Program
    {
        static Random random = new Random(123);
        static OnboardingResponse config;
        static void Main(string[] args)
        {
            Console.WriteLine("Petstore test client is starting");
            Console.WriteLine("Press any key to end");
            while (true)
            {
                loop();
                for (var i = 0; i < 1000; i++)
                {
                    if (Console.KeyAvailable)
                    {
                        goto aa;
                    }
                    Thread.Sleep(5);
                }
            }

            aa: Console.WriteLine("Exiting");
            return;
        }

        static void loop()
        {
            if(config == null)
            {
                subscribe();
            } else
            {
                writePoints(config);
            }

        }

        static void subscribe()
        {
            RestClient hubClient = new RestClient("http://localhost:8080/api");

            var request = new RestRequest("register/{id}", Method.GET);
            request.AddUrlSegment("id", "1234-5678-9012-3456"); // replaces matching token in request.Resource

            hubClient.UserAgent = "Win10IoTCore.PetStore";

            var response = hubClient.Execute(request);
            if (response.ErrorException != null)
            {
                Console.WriteLine("Error: " + response.ErrorException.Message);
            }
            else
            {

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        Console.WriteLine("Device accepted: " + response.Content);
                        config = JsonConvert.DeserializeObject<OnboardingResponse>(response.Content);
                        break;
                    case System.Net.HttpStatusCode.Created:
                        Console.WriteLine("Waiting for device authorization: " + response.Content);
                        break;
                    default:
                        Console.WriteLine("Uknown response: " + response.StatusCode + ", " + response.Content);
                        break;

                }

            }

        }

        static void writePoints(OnboardingResponse config)
        {
            InfluxDBClient dBClient;
            

            dBClient = InfluxDBClientFactory.Create(config.url, config.authToken.ToCharArray());
            
            var point = Point.Measurement("m1")
                            .Tag("device", config.deviceId)
                            .Field("value", random.Next());
            var writeClient = dBClient.GetWriteApi();
            Console.WriteLine("Writing");
            writeClient.WritePoint(config.bucket, config.orgId, point);
            writeClient.Flush();
        }
    }
}
