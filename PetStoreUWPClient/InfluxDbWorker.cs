using InfluxDB.Client;
using InfluxDB.Client.Writes;
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
        private BackgroundWorker dbWorker;
        private InfluxDBClient dBClient;
        private int delay;

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

        private void DbWorker_DoWork(object sender, DoWorkEventArgs e)
        {
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
        }

        private void InitializeDb()
        {
            var dbConfig = DbConfig.GetInstance();
            if (dBClient == null && dbConfig.IsDbConfigAvailable())
            {
                try
                {
                    Debug.WriteLine("InfluxDbWorker:InitializeDb");
                    dBClient = InfluxDBClientFactory.Create(dbConfig.url, dbConfig.authToken.ToCharArray());
                    dBClient.Ready();
                }
                catch (Exception ex)
                {
                    OnStatusChanged("Error: " + ex.Message);
                }
            }
        }

        private void WriteToDb()
        {

            if (dBClient != null)
            {
                try
                {
                    Debug.WriteLine("InfluxDbWorker:WriteToDb");
                    var data = BasicData.GetBasicData();
                    var dbConfig = DbConfig.GetInstance();
                    var point = Point.Measurement("air")
                        .Tag("room", dbConfig.location!=null? dbConfig.location:"prosek")
                        .Tag("device", dbConfig.deviceId)
                        .Field("temp", data.CurrentTemperature)
                        .Field("press", data.CurrentPressure)
                        .Field("hum", data.CurrentHumidity);
                    var writeClient = dBClient.GetWriteApi();
                    writeClient.WritePoint(dbConfig.bucket, dbConfig.orgId, point);
                    writeClient.Flush();
                }
                catch (Exception ex)
                {
                    OnStatusChanged("Error: " + ex.Message);
                }
            }
        }
        private const string FluxQueryTemplate = "from(bucket: \"{0}\") |> range(start: -24h, stop: now()) |> filter(fn: (r) => r._measurement == \"air\" and r._field == \"{1}\" and r.room == \"{2}\") |> aggregateWindow(every: 24h, fn: {3}, createEmpty: false)";
        private void ReadFromDb()
        {
            if (dBClient != null)
            {
                try
                {
                    var data = BasicData.GetBasicData();
                    double val = GetQueryResult("temp", "mean");
                    if(val != double.NaN)
                    {
                        data.MeanTemperature = val;
                    }
                    val = GetQueryResult("temp", "min");
                    if (val != double.NaN)
                    {
                        data.MinTemperature = val;
                    }
                    val = GetQueryResult("temp", "max");
                    if (val != double.NaN)
                    {
                        data.MaxTemperature = val;
                    }
                    val = GetQueryResult("hum", "mean");
                    if (val != double.NaN)
                    {
                        data.MeanHumidity = val;
                    }
                    val = GetQueryResult("hum", "min");
                    if (val != double.NaN)
                    {
                        data.MinHumidity = val;
                    }
                    val = GetQueryResult("hum", "max");
                    if (val != double.NaN)
                    {
                        data.MaxHumidity = val;
                    }
                    val = GetQueryResult("press", "mean");
                    if (val != double.NaN)
                    {
                        data.MeanPressure = val;
                    }
                    val = GetQueryResult("press", "min");
                    if (val != double.NaN)
                    {
                        data.MinPressure = val;
                    }
                    val = GetQueryResult("press", "max");
                    if (val != double.NaN)
                    {
                        data.MaxPressure = val;
                    }

                }
                catch (Exception ex)
                {
                    OnStatusChanged("Error: " + ex.Message);
                }

            }
        }

        private double GetQueryResult(string field, string function)
        {
            var dbConfig = DbConfig.GetInstance();
            var location = dbConfig.location != null ? dbConfig.location : "prosek";
            var fluxTables = dBClient.GetQueryApi().Query(string.Format(FluxQueryTemplate, dbConfig.bucket, field, location, function), dbConfig.orgId);
            object value = null;
            fluxTables.ForEach(fluxTable =>
            {
                var fluxRecords = fluxTable.Records;
                fluxRecords.ForEach(fluxRecord =>
                {
                    Debug.WriteLine($"GetQueryResul: {fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
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
            Debug.WriteLine("InfluxDbWorker:RunWorkerComplete");
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
