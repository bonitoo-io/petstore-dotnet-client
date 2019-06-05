using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using PetStoreClientBackgroundApplication;
using MetroLog;
using MetroLog.Targets;
using Windows.ApplicationModel;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace PetStoreClientBackgroundApplication
{
    public sealed class PestoreClientBackgroundTask : IBackgroundTask
    {
        private ILogger Log;
        private BackgroundTaskDeferral deferral;
        private WorkersManager man;
        private RestServer restRerver;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
#if DEBUG
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget());
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new EtwTarget());
#else
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Info, LogLevel.Fatal, new FileStreamingTarget());
#endif
            try
            {
                // setup the global crash handler...
                //GlobalCrashHandler.Configure();
                Log = LogManagerFactory.DefaultLogManager.GetLogger<PestoreClientBackgroundTask>();
                Log.Info($"Started PestoreClientBackgroundTask {GetAppVersion()}");
                man = WorkersManager.GetWorkersManager();
                restRerver = new RestServer();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                man.Start();
                restRerver.Run();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                taskInstance.Canceled += TaskInstance_Canceled;
                deferral = taskInstance.GetDeferral();
            } catch(Exception ex)
            {
                Log.Fatal("Unexped exception", ex);
            }
            Log.Trace("Stopped");
        }

        private static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            man.Stop().GetAwaiter().GetResult();
            Log.Trace("Canceled");
        }
    }
}
