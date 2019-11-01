using System;
using System.Collections.Generic;
using System.Text;

namespace PublicIpUploader.Models
{
    public class Configuration
    {
        public string IFTTTKey { get; set; }

        public string IFTTTEventName { get; set; }

        public string GoogleSheetsUrl { get; set; }

        public List<string> IpProviders{ get; set; }
    }
}
