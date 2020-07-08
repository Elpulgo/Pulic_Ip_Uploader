using PublicIpUploader.Models;
using Serilog;
using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace PublicIpUploader
{
    public class EmailManager
    {
        private readonly IConfigurationSupplier m_ConfigurationSupplier;

        public EmailManager(IConfigurationSupplier configurationSupplier)
        {
            m_ConfigurationSupplier = configurationSupplier;
        }

        private async Task<bool> ValidateConfigurationAsync()
        {
            var configuration = await m_ConfigurationSupplier.GetConfigurationAsync();

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.Sender))
                return false;

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.Receiver))
                return false;

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.SmtpServer))
                return false;

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.SenderUserName))
                return false;

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.SenderPassword))
                return false;

            if (configuration.EmailConfiguration?.Port == int.MaxValue ||
                configuration.EmailConfiguration?.Port == int.MinValue ||
                configuration.EmailConfiguration?.Port == 0)
                return false;

            return true;
        }

        public string Get()
        {
            EmailMessage message = new EmailMessage();
            message.Sender = new MailboxAddress("Self", _notificationMetadata.Sender);
            message.Reciever = new MailboxAddress("Self", _notificationMetadata.Reciever);
            message.Subject = "Welcome";
            message.Content = "Hello World!";
            var mimeMessage = CreateEmailMessage(message);

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect(
                    _notificationMetadata.SmtpServer,
                    _notificationMetadata.Port,
                    true);

                smtpClient.Authenticate(
                    _notificationMetadata.UserName,
                    _notificationMetadata.Password);

                smtpClient.Send(mimeMessage);

                smtpClient.Disconnect(true);
            }

            return "Email sent successfully";
        }
    }
}
