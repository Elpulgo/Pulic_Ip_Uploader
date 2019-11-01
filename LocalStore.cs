using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;

namespace PublicIpUploader
{
    public class LocalStore : ILocalStore
    {
        private string StoredPath => Path.Combine(Directory.GetCurrentDirectory(), "publicip.dat");

        public LocalStore()
        {
        }

        public string GetStoredPublicIp()
        {
            try
            {
                if (!File.Exists(StoredPath))
                    return string.Empty;

                var publicIp = File.ReadAllText(StoredPath);
                return publicIp;

            }
            catch (Exception exception)
            {
                Log.Error($"Failed to read path '{StoredPath}', couldn't get public ip, '{exception.Message}'", exception);
            }

            return string.Empty;
        }

        public bool TryStorePublicIp(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                Log.Warning($"Failed to store public ip, ip is empty");
                return false;
            }
            try
            {
                File.WriteAllText(StoredPath, ip);
                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"Failed to store public ip '{ip}', '{exception.Message}'", exception);
                return false;
            }
        }
    }
}
