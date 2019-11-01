using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PublicIpUploader.Models;
using Serilog;

namespace PublicIpUploader
{
    public class ConfigurationSupplier : IConfigurationSupplier
    {
        private Configuration m_CachedConfiguration;

        public string ConfigurationPath => Path.Combine(Directory.GetCurrentDirectory(), "configuration", "configuration.json");

        public ConfigurationSupplier()
        {
        }

        public async Task<Configuration> GetConfigurationAsync()
        {
            if (m_CachedConfiguration != null)
                return m_CachedConfiguration;

            if (!File.Exists(ConfigurationPath))
            {
                Log.Error($"Couldn't find configuration path '{ConfigurationPath}' ...");
                return null;
            }

            try
            {
                var content = await File.ReadAllTextAsync(ConfigurationPath);

                if (!string.IsNullOrEmpty(content))
                {
                    var configuration = JsonConvert.DeserializeObject<Configuration>(content);
                    m_CachedConfiguration = configuration;
                    return configuration;
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Failed to read configuration '{exception.Message}'", exception);
            }

            return null;
        }
    }
}
