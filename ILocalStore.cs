using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public interface ILocalStore
    {
        bool TryStorePublicIp(string ip);

        string GetStoredPublicIp();
    }
}
