using InfluxDB.Client;
using InfluxDB.Client.Writes;
using MetroLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetStoreUWPClient
{
    public class InfluxDbWorker
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<InfluxDbWorker>();
        private BackgroundWorker dbWorker;
        private InfluxDBClient dBClient;
        private int delay;

        public bool Running { get; private set; }

        public InfluxDbWorker(int delay = 30000)
        {
            this.delay = delay;
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
                if (dBClient != null)
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
            var dbConfig = Config.GetInstance();
            if (dBClient == null && dbConfig.IsDbConfigAvailable())
            {
                try
                {
                    Log.Trace("InfluxDbWorker:InitializeDb");
                    dBClient = InfluxDBClientFactory.Create(dbConfig.url, dbConfig.authToken.ToCharArray());
                    dBClient.Ready();
                }
                catch (Exception ex)
                {
                    Log.Error("Db init error", ex);
                    OnStatusChanged("DB Init Error: " + ex.Message);
                }
            }
        }

        private void WriteToDb()
        {

            if (dBClient != null)
            {
                try
                {
                    Log.Trace("InfluxDbWorker:WriteToDb");
                    var data = BasicData.GetBasicData();
                    var dbConfig = Config.GetInstance();
                    var point = Point.Measurement("air")
                        .Tag("location", dbConfig.location!=null? dbConfig.location:"prosek")
                        .Tag("device_id", dbConfig.deviceId)
                        .Field("temperature", data.CurrentTemperature)
                        .Field("pressure", data.CurrentPressure)
                        .Field("humidity", data.CurrentHumidity);
                    var writeClient = dBClient.GetWriteApi();
                    writeClient.WritePoint(dbConfig.bucket, dbConfig.orgId, point);
                    writeClient.Flush();
                }
                catch (Exception ex)
                {
                    Log.Error("Db Write error", ex);
                    OnStatusChanged("DB Write Error: " + ex.Message);
                }
            }
        }
        private const string FluxQueryTemplate = "from(bucket: \"{0}\") |> range(start: -24h, stop: now()) |> filter(fn: (r) => r._measurement == \"air\" and r._field == \"{1}\" and r.location == \"{2}\") |> aggregateWindow(every: 24h, fn: {3}, createEmpty: false)";
        private void ReadFromDb()
        {
            if (dBClient != null)
            {
                try
                {
                    var data = BasicData.GetBasicData();
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
            var dbConfig = Config.GetInstance();
            var location = dbConfig.location != null ? dbConfig.location : "prosek";
            var fluxTables = dBClient.GetQueryApi().Query(string.Format(FluxQueryTemplate, dbConfig.bucket, field, location, function), dbConfig.orgId);
            object value = null;
            fluxTables.ForEach(fluxTable =>
            {
                var fluxRecords = fluxTable.Records;
                fluxRecords.ForEach(fluxRecord =>
                {
                   Log.Debug($"GetQueryResul: {fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
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
            dBClient.Dispose();
            dBClient = null;
        }

        protected void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, new StatusUpdatedEventArgs(status));
        }

        public event EventHandler<StatusUpdatedEventArgs> StatusChanged;

    }
}
