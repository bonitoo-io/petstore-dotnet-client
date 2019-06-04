using PetStoreClientDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetStoreUWPClient
{
    public class SettingsViewModel : BindableBase
    {

        private string hubUrl;
        public string HubUrl
        {
            get { return hubUrl; }
            set { SetProperty(ref hubUrl, value); }
        }

        private string location;
        public string Location
        {
            get { return location; }
            set { SetProperty(ref location, value); }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        public void Load()
        {
            var config = BackgroundJobClient.GetConfig();
            HubUrl = config.HubUrl;
            Location = config.Location;
        }

        public void Save(Config config)
        {
            config.HubUrl = HubUrl;
            config.Location = Location;
        }
    }
}
