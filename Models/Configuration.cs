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

        public List<string> IpProviders { get; set; }

        public bool UseEmailNotification { get; set; }

        public EmailConfiguration EmailConfiguration { get; set; }
    }

    public class EmailConfiguration
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SenderUserName { get; set; }
    }
}
