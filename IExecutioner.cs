using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public interface IExecutioner
    {
        Task ExecuteAsync();
    }
}
