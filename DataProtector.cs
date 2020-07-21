using Microsoft.AspNetCore.DataProtection;

namespace PublicIpUploader
{
    public class DataProtector
    {
        private readonly IDataProtector m_DataProtectionProvider;

        public DataProtector(IDataProtectionProvider dataProtectionProvider)
        {
            m_DataProtectionProvider = dataProtectionProvider.CreateProtector("IpEmail.Protector.v1");
        }

        public string Protect(string input) => m_DataProtectionProvider.Protect(input);

        public string Unprotect(string input) => m_DataProtectionProvider.Unprotect(input);
    }
}
