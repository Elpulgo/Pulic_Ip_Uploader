using PublicIpUploader.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public interface IConfigurationSupplier
    {
        Task<Configuration> GetConfigurationAsync();
    }
}
