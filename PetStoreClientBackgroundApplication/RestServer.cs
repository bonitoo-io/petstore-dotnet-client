using MetroLog;
using PetStoreClientBackgroundApplication;
using PetStoreClientDataModel;
using Restup.Webserver.Attributes;
using Restup.Webserver.Http;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using Restup.Webserver.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetStoreClientBackgroundApplication
{
    class RestServer
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<RestServer>();
        public async Task Run()
        {
            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<ConfigController>();
            restRouteHandler.RegisterController<OverViewDataController>();
            restRouteHandler.RegisterController<MeasuredDataController>();

            var configuration = new HttpServerConfiguration()
              .ListenOnPort(8888)
              .RegisterRoute("api", restRouteHandler)
              .EnableCors();

            var httpServer = new HttpServer(configuration);
            await httpServer.StartServerAsync();
            Log.Info("started");
            // now make sure the app won't stop after this (eg use a BackgroundTaskDeferral)
        }
    }

    [RestController(InstanceCreationType.Singleton)]
    class ConfigController
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<ConfigController>();
        [UriFormat("/config")]
        public IGetResponse GetConfig()
        {
            Log.Info("GetConfig");
            return new GetResponse(
              GetResponse.ResponseStatus.OK,
               WorkersManager.GetWorkersManager().Config);
        }
        [UriFormat("/config")]
        public IPutResponse UpdateConfig([FromContent]Config data)
        {
            Log.Info("UpdateConfig");
            Log.Debug($"Data: {data}");
            var config = WorkersManager.GetWorkersManager().Config;
            var urlChanged = !data.HubUrl.Equals(config.HubUrl) || !data.Url.Equals(config.Url);
            config.Update(data);
            WorkersManager.GetWorkersManager().Config.Save(LocalSettingsConfigProvider.Instance);
            if (urlChanged)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                WorkersManager.GetWorkersManager().Restart();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            return new PutResponse(PutResponse.ResponseStatus.NoContent);
        }
    }

    [RestController(InstanceCreationType.Singleton)]
    class OverViewDataController
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<OverViewDataController>();
        [UriFormat("/data/overview")]
        public IGetResponse GetOverviewData()
        {
            Log.Trace("GetOverviewData");
            return new GetResponse(
              GetResponse.ResponseStatus.OK,
               OverviewData.GetOverviewData());
        }
    }

    [RestController(InstanceCreationType.Singleton)]
    class MeasuredDataController
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<MeasuredDataController>();
        [UriFormat("/data/meassured")]
        public IGetResponse GetMeasuredData()
        {
            Log.Trace("GetMeasuredData");
            return new GetResponse(
              GetResponse.ResponseStatus.OK,
               MeasuredData.GetMeasuredData());
        }
    }
}
