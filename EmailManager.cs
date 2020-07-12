using PublicIpUploader.Models;
using Serilog;
using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace PublicIpUploader
{
    public class EmailManager
    {
        private readonly IConfigurationSupplier m_ConfigurationSupplier;

        public EmailManager(IConfigurationSupplier configurationSupplier)
        {
            m_ConfigurationSupplier = configurationSupplier;
        }

        private async Task<(bool, Configuration)> ValidateConfigurationAsync()
        {
            var configuration = await m_ConfigurationSupplier.GetConfigurationAsync();

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.Sender))
                return (false, null);

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.Receiver))
                return (false, null);

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.SmtpServer))
                return (false, null);

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.SenderUserName))
                return (false, null);

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.SenderPassword))
                return (false, null);

            if (configuration.EmailConfiguration?.Port == int.MaxValue ||
                configuration.EmailConfiguration?.Port == int.MinValue ||
                configuration.EmailConfiguration?.Port == 0)
                return (false, null);

            return (true, configuration);
        }

        public async Task Execute()
        {
            var (validated, configuration) = await ValidateConfigurationAsync();


            var messageToSend = new MimeMessage
            {
                Sender = new MailboxAddress("El Pulgo Ip Notifier", configuration.EmailConfiguration.Sender),
                Subject = "IP address for computer has changed!",
                Body = new TextPart(TextFormat.Plain) { Text = "IP has changed..." }
            };

            messageToSend.To.Add(new MailboxAddress(configuration.EmailConfiguration.Receiver));

            Console.WriteLine($"Sender usename is: {configuration.EmailConfiguration.SenderUserName}");
            Console.WriteLine($"Sender pass is: {configuration.EmailConfiguration.SenderPassword}");

            try
            {
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(
                        configuration.EmailConfiguration.SmtpServer,
                        configuration.EmailConfiguration.Port,
                        true);

                    smtpClient.Authenticate(
                        configuration.EmailConfiguration.SenderUserName,
                        configuration.EmailConfiguration.SenderPassword);

                    smtpClient.Send(messageToSend);

                    smtpClient.Disconnect(true);
                }

                Console.WriteLine("Email was sent successfully!");
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }
    }
}
