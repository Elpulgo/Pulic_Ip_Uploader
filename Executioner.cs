using PublicIpUploader.Models;
using Serilog;
using System;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public class Executioner : IExecutioner
    {
        private readonly IHttpService m_HttpService;
        private readonly ILocalStore m_LocalStore;

        public Executioner(
            IHttpService httpService,
            ILocalStore localStore
            )
        {
            m_HttpService = httpService;
            m_LocalStore = localStore;
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
        }
    }
}
