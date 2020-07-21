using PublicIpUploader.Models;
using Serilog;
using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System.IO;

namespace PublicIpUploader
{
    public class EmailManager
    {
        private string PasswordFilePath => Path.Combine(Directory.GetCurrentDirectory(), "configuration", "protectedpassword.dat");
        private readonly IConfigurationSupplier m_ConfigurationSupplier;
        private readonly DataProtector m_DataProtector;

        public EmailManager(
            IConfigurationSupplier configurationSupplier,
            DataProtector dataProtector)
        {
            m_ConfigurationSupplier = configurationSupplier;
            m_DataProtector = dataProtector;
        }

        public bool IsPasswordSet() => File.Exists(PasswordFilePath);

        public async Task<bool> TrySetPasswordAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Password for email can't be null or empty!");
                return false;
            }

            try
            {
                var protectedPassword = m_DataProtector.Protect(input);
                await File.WriteAllTextAsync(PasswordFilePath, protectedPassword);
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Failed to store protected password due to: {ex.Message}.");
                return false;
            }
        }

        public async Task ExecuteAsync(string newIp, string oldIp)
        {
            var (validated, configuration) = await ValidateConfigurationAsync();

            if (!validated)
                return;

            var (success, senderPassword) = await TryGetPasswordAsync();

            if (!success)
                return;

            var messageToSend = new MimeMessage
            {
                Sender = new MailboxAddress("El Pulgo Ip Notifier", configuration.EmailConfiguration.Sender),
                Subject = "IP address for computer has changed!",
                Body = new TextPart(TextFormat.Plain) { Text = $"IP has changed! \nNew IP address: {newIp}\nOld IP address: {oldIp}" }
            };

            messageToSend.To.Add(new MailboxAddress(configuration.EmailConfiguration.Receiver));

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
                        senderPassword);

                    smtpClient.Send(messageToSend);

                    smtpClient.Disconnect(true);
                }

                Console.WriteLine($"Successfully sent email notification with changed IP to '{configuration.EmailConfiguration.Receiver}'.");
                Log.Information($"Successfully sent email notification with changed IP to '{configuration.EmailConfiguration.Receiver}'.");
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, $"Failed to send email notification to '{configuration.EmailConfiguration.Receiver}' due to: {ex.Message}.");
                Console.WriteLine($"Failed to send email notification to '{configuration.EmailConfiguration.Receiver}' due to: {ex.Message}.");
            }
        }

        private async Task<(bool success, string password)> TryGetPasswordAsync()
        {
            if (!IsPasswordSet())
            {
                Console.WriteLine("No password is set! Can't send email!");
                Log.Error("No password is set for email notification. Can't send notification. Either set a password or set 'UseEmailNotification' to false in configuration.json file.");
                return (false, null);
            }

            try
            {
                var protectedPassword = await File.ReadAllTextAsync(PasswordFilePath);
                var password = m_DataProtector.Unprotect(protectedPassword);
                return (true, password);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Failed to get email password due to: {ex.Message}.");
                Log.Error(ex, $"Failed to get email password due to {ex.Message}");
                return (false, null);
            }
        }

        private async Task<(bool, Configuration)> ValidateConfigurationAsync()
        {
            var configuration = await m_ConfigurationSupplier.GetConfigurationAsync();

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.Sender))
            {
                Console.WriteLine("Sender for email is not set.  Can't send email!");
                Log.Error("Sender for email is not set. Can't send email!");
                return (false, null);
            }

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.Receiver))
            {
                Console.WriteLine("Receiver for email is not set.  Can't send email!");
                Log.Error("Receiver for email is not set. Can't send email!");
                return (false, null);
            }

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.SmtpServer))
            {
                Console.WriteLine("SmtpServer for email is not set.  Can't send email!");
                Log.Error("SmtpServer for email is not set. Can't send email!");
                return (false, null);
            }

            if (string.IsNullOrEmpty(configuration.EmailConfiguration?.SenderUserName))
            {
                Console.WriteLine("SenderUserName for email is not set.  Can't send email!");
                Log.Error("SenderUserName for email is not set. Can't send email!");
                return (false, null);
            }

            if (configuration.EmailConfiguration?.Port == int.MaxValue ||
                configuration.EmailConfiguration?.Port == int.MinValue ||
                configuration.EmailConfiguration?.Port == 0)
            {
                Console.WriteLine("Port for email is not set.  Can't send email!");
                Log.Error("Port for email is not set. Can't send email!");
                return (false, null);
            }

            return (true, configuration);
        }
    }
}
