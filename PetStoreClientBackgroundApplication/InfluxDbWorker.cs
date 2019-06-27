using InfluxDB.Client;
using InfluxDB.Client.Writes;
using MetroLog;
using PetStoreClientDataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetStoreClientBackgroundApplication
{
    class InfluxDbWorker
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<InfluxDbWorker>();
        private BackgroundWorker dbWorker;
        private InfluxDBClient dbClient;
        private QueryApi dbQueryApi;
        private int delay;
        private int originalDelay;

        public bool Running { get; private set; }

        public InfluxDbWorker(int delay = 30000)
        {
            this.delay = delay;
            originalDelay = delay;
            dbWorker = new BackgroundWorker();
            dbWorker.WorkerSupportsCancellation = true;
            dbWorker.DoWork += DbWorker_DoWork;
            dbWorker.RunWorkerCompleted += DbWorker_RunWorkerCompleted; 
        }

       

        public void Start()
        {
            InitializeDb();
            dbWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            dbWorker.CancelAsync();
        }

        private void DbWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log.Trace("InfluxDbWorker:Started");
            Running = true;
            while (!dbWorker.CancellationPending)
            {
                if (dbClient != null)
                {
                    WriteToDb();
                    ReadFromDb();
                }
                Thread.Sleep(delay);
            }
            if (dbWorker.CancellationPending)
            {
                e.Cancel = true;
            }
            Running = false;
            Log.Trace("InfluxDbWorker:Stopped");
        }

        private void InitializeDb()
        {
            var dbConfig = WorkersManager.GetWorkersManager().Config;
            if (dbClient == null && dbConfig.IsDbConfigAvailable())
            {
                try
                {
                    Log.Trace("InfluxDbWorker:InitializeDb");
                    dbClient = InfluxDBClientFactory.Create(dbConfig.Url, dbConfig.AuthToken.ToCharArray());
                    dbClient.Health();
                    dbQueryApi = dbClient.GetQueryApi();

                }
                catch (Exception ex)
                {
                    Log.Error("Db init error", ex);
                    OnStatusChanged("DB Init Error: " + ex.Message);
                }
            }
        }

        private void DbWriteApi_EventHandler(object sender, EventArgs e)
        {
            if (e is WriteErrorEvent errorEvent)
            {
                Log.Error("Write Error", errorEvent.Exception);
                OnStatusChanged("DB Write Error: " + errorEvent.Exception.Message);
                if (delay == originalDelay)
                {
                    delay *= 5;
                    Log.Info($"Prolonging delay to {delay}");
                }
            }
        }

        private void WriteToDb()
        {

            if (dbClient != null)
            {
                try
                {
                    Log.Trace("WriteToDb");
                    var data = OverviewData.GetOverviewData();
                    var dbConfig = WorkersManager.GetWorkersManager().Config;
                    var point = Point.Measurement("air")
                        .Tag("location", dbConfig.Location != null ? dbConfig.Location : "prosek")
                        .Tag("device_id", dbConfig.DeviceId);
                    int validFields = 0;
                    if (!double.IsNaN(data.CurrentTemperature)) {
                        point.Field("temperature", data.CurrentTemperature);
                        ++validFields;
                    }
                    if (!double.IsNaN(data.CurrentPressure))
                    {
                        point.Field("pressure", data.CurrentPressure);
                        ++validFields;
                    }
                    if (!double.IsNaN(data.CurrentHumidity)) {
                        
                        point.Field("humidity", data.CurrentHumidity);
                        ++validFields;
                    }
                    if (validFields > 0)
                    {
                        using (var dbWriteApi = dbClient.GetWriteApi())
                        {
                            dbWriteApi.EventHandler += DbWriteApi_EventHandler;
                            dbWriteApi.WritePoint(dbConfig.Bucket, dbConfig.OrgId, point);
                            dbWriteApi.Flush();
                        }
                        
                        if (delay != originalDelay)
                        {
                            delay = originalDelay;
                            Log.Info($"Restoring delay to {delay}");
                        }
                    } else
                    {
                        Log.Debug("InfluxDbWorker:WriteToDb - nothing to write, all values are invalid");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Db Write error", ex);
                    OnStatusChanged("DB Write Error: " + ex.Message);
                    //probably temporarly net down or db error, prolong delay to not overload with exception
                    if(delay == originalDelay)
                    {
                        delay *= 5;
                        Log.Info($"Prolonging delay to {delay}");
                    }

                }
            }
        }
        private const string FluxQueryTemplate = "from(bucket: \"{0}\") |> range(start: -24h, stop: now()) |> filter(fn: (r) => r._measurement == \"air\" and r._field == \"{1}\" and r.location == \"{2}\") |> aggregateWindow(every: 24h, fn: {3}, createEmpty: false)";
        private void ReadFromDb()
        {
            if (dbQueryApi != null)
            {
                try
                {
                    Log.Trace("ReadFromDb");
                    var data = OverviewData.GetOverviewData();
                    double val = GetQueryResult("temperature", "mean");
                    if(val != double.NaN)
                    {
                        data.MeanTemperature = val;
                    }
                    val = GetQueryResult("temperature", "min");
                    if (val != double.NaN)
                    {
                        data.MinTemperature = val;
                    }
                    val = GetQueryResult("temperature", "max");
                    if (val != double.NaN)
                    {
                        data.MaxTemperature = val;
                    }
                    val = GetQueryResult("humidity", "mean");
                    if (val != double.NaN)
                    {
                        data.MeanHumidity = val;
                    }
                    val = GetQueryResult("humidity", "min");
                    if (val != double.NaN)
                    {
                        data.MinHumidity = val;
                    }
                    val = GetQueryResult("humidity", "max");
                    if (val != double.NaN)
                    {
                        data.MaxHumidity = val;
                    }
                    val = GetQueryResult("pressure", "mean");
                    if (val != double.NaN)
                    {
                        data.MeanPressure = val;
                    }
                    val = GetQueryResult("pressure", "min");
                    if (val != double.NaN)
                    {
                        data.MinPressure = val;
                    }
                    val = GetQueryResult("pressure", "max");
                    if (val != double.NaN)
                    {
                        data.MaxPressure = val;
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Db Query error", ex);
                    OnStatusChanged("DB Query Error: " + ex.Message);
                }

            }
        }

        private double GetQueryResult(string field, string function)
        {
            var dbConfig = WorkersManager.GetWorkersManager().Config;
            var location = dbConfig.Location != null ? dbConfig.Location : "unknown";
            var fluxTables = dbQueryApi.Query(string.Format(FluxQueryTemplate, dbConfig.Bucket, field, location, function), dbConfig.OrgId).GetAwaiter().GetResult();
            object value = null;
            fluxTables.ForEach(fluxTable =>
            {
                var fluxRecords = fluxTable.Records;
                fluxRecords.ForEach(fluxRecord =>
                {
                   //Log.Debug($"GetQueryResul: {fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
                   value = (double)fluxRecord.GetValue();
                });
            });
            if (value != null)
            {
                return (double)value;
            }
            else
            {
                return double.NaN;
            }

        }

        private void DbWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log.Trace("InfluxDbWorker:RunWorkerComplete");
            dbQueryApi = null;
            if (dbClient != null)
            {
                dbClient.Dispose();
                dbClient = null;
            }

        }

        protected void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, new StatusUpdatedEventArgs(status));
        }

        public event EventHandler<StatusUpdatedEventArgs> StatusChanged;

    }
}
