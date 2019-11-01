using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PublicIpUploader.Models;
using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public class HttpService : IHttpService, IDisposable
    {
        private const string MakerKeyPlaceholder = "_MAKERKEY_";
        private const string MakerEventPlaceholder = "_MAKEREVENT_";

        private const string BaseUrl = "https://maker.ifttt.com/trigger/";
        private readonly string IFTTTMakerUrl = $"{MakerEventPlaceholder}/with/key/{MakerKeyPlaceholder}";

        private HttpClient m_HttpClient;
        private readonly TimeSpan m_TimeOut = TimeSpan.FromSeconds(30);
        private readonly IConfigurationSupplier m_ConfigurationSupplier;

        public HttpService(IConfigurationSupplier configurationSupplier)
        {
            m_ConfigurationSupplier = configurationSupplier;

            m_HttpClient = new HttpClient()
            {
                Timeout = m_TimeOut,
                BaseAddress = new Uri(BaseUrl)
            };

            m_HttpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> PostAsync(IpModel model)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(model).ToLowerInvariant();
                var url = await GetIFTTTMakerUrlAsync();

                if (string.IsNullOrEmpty(url))
                    return false;

                using (var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json"))
                using (var response = await m_HttpClient.PostAsync(url, jsonContent))
                {
                    response.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch (HttpRequestException httpRequestException)
            {
                Log.Error($"Failed to post {model.Value1} to IFTTT '{httpRequestException.Message}'", httpRequestException);
            }
            catch (Exception exception)
            {
                Log.Error($"Failed to post {model.Value1} to IFTTT '{exception.Message}'", exception);
            }

            return false;
        }

        public async Task<string> GetPublicIpAsync()
        {
            var configuration = await m_ConfigurationSupplier.GetConfigurationAsync();

            if (configuration == null || configuration.IpProviders == null || !configuration.IpProviders.Any())
            {
                Log.Error("Failed to load ip providers from configuration.");
                return null;
            }

            foreach (var provider in configuration.IpProviders)
            {
                using (var response = await m_HttpClient.GetAsync(provider))
                {
                    if (!response.IsSuccessStatusCode)
                        continue;


                    var publicIp = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(publicIp))
                        continue;

                    return publicIp;
                }
            }

            Log.Warning("Failed to get ip from the providers.");
            return null;
        }

        public void Dispose()
        {
            m_HttpClient.Dispose();
        }

        private async Task<string> GetIFTTTMakerUrlAsync()
        {
            var configuration = await m_ConfigurationSupplier.GetConfigurationAsync();

            if (configuration == null)
            {
                return string.Empty;
            }
            else if (string.IsNullOrEmpty(configuration.IFTTTEventName))
            {
                Log.Error("Couldn't load configuration IFTTT event name");
                return string.Empty;
            }
            else if (string.IsNullOrEmpty(configuration.IFTTTKey))
            {
                Log.Error("Couldn't load configuration IFTTT key");
                return string.Empty;
            }

            var url = IFTTTMakerUrl
                .Replace(MakerEventPlaceholder, configuration.IFTTTEventName)
                .Replace(MakerKeyPlaceholder, configuration.IFTTTKey);

            return url;
        }
    }
}
