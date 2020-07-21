using PublicIpUploader.Models;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public class Executioner : IExecutioner
    {
        private readonly IHttpService m_HttpService;
        private readonly ILocalStore m_LocalStore;
        private readonly IConfigurationSupplier m_ConfigurationSupplier;
        private readonly EmailManager m_EmailManager;

        public Executioner(
            IHttpService httpService,
            ILocalStore localStore,
            IConfigurationSupplier configurationSupplier,
            EmailManager emailManager
            )
        {
            m_HttpService = httpService;
            m_LocalStore = localStore;
            m_ConfigurationSupplier = configurationSupplier;
            m_EmailManager = emailManager;
        }

        public async Task ExecuteAsync()
        {
            var publicIp = await m_HttpService.GetPublicIpAsync();

            if (string.IsNullOrEmpty(publicIp))
                return;

            var oldIp = m_LocalStore.GetStoredPublicIp();

            if (oldIp.Equals(publicIp, StringComparison.InvariantCultureIgnoreCase))
            {
                Log.Information("Public ip hasn't changed since last time.");
                return;
            }

            var success = await m_HttpService.PostAsync(new IpModel() { Value1 = publicIp });

            if (!success)
                return;

            if (!m_LocalStore.TryStorePublicIp(publicIp))
                return;

            Log.Information($"Successfully triggered IFTTT maker event with public ip '{publicIp}'");

            await SendEmail(publicIp, oldIp);
        }

        private async Task SendEmail(string newIp, string oldIp)
        {
            var configuration = await m_ConfigurationSupplier.GetConfigurationAsync();

            if (!configuration.UseEmailNotification)
                return;

            if (!m_EmailManager.IsPasswordSet())
            {
                Console.WriteLine("Set the password for sender email. It will be protected and saved to disk. (10 sec remaining..)");

                var cancelTokenSource = new CancellationTokenSource();
                cancelTokenSource.CancelAfter(10000);
                var input = await ReadConsoleLineAsync(cancelTokenSource.Token);

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Failed to provide email password. Won't send email.");
                    Log.Error("Failed to provide email password. Won't send email.");
                    return;
                }

                if (!(await m_EmailManager.TrySetPasswordAsync(input)))
                {
                    Console.WriteLine("Failed to set password! Won't send email.");
                    return;
                }
            }

            await m_EmailManager.ExecuteAsync(newIp, oldIp);
        }

        public Task<string> ReadConsoleLineAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                while (!Console.KeyAvailable)
                {
                    if (ct.IsCancellationRequested)
                        return null;

                    Thread.Sleep(100);
                }

                return Console.ReadLine();
            });
        }
    }
}
