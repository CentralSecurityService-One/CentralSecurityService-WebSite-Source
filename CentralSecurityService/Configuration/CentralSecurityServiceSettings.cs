using static CentralSecurityService.Configuration.CentralSecurityServiceSettings;

namespace CentralSecurityService.Configuration
{
    public class CentralSecurityServiceSettings
    {
        public const string SectionName = "CentralSecurityService";

        public static CentralSecurityServiceSettings Instance { get; private set; }

        public CentralSecurityServiceSettings()
        {
            Instance = this;
        }

        public class SensitiveSettings
        {
            public string Folder { get; set; }
        }

        public SensitiveSettings Sensitive { get; set; }
    }
}
