using PublicIpUploader.Models;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public interface IHttpService
    {
        Task<bool> PostAsync(IpModel model);

        Task<string> GetPublicIpAsync();
    }
}
