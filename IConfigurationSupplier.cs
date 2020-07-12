using PublicIpUploader.Models;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public interface IConfigurationSupplier
    {
        Task<Configuration> GetConfigurationAsync();
    }
}
