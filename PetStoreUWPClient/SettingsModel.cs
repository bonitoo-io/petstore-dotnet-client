using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetStoreUWPClient
{
    public class SettingsModel : BindableBase
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

        public void Load()
        {
            var config = Config.GetInstance();
            HubUrl = config.hubUrl;
            Location = config.location;
        }

        public void Save()
        {
            var config = Config.GetInstance();
            config.hubUrl = HubUrl;
            config.location = Location;
        }
    }
}
