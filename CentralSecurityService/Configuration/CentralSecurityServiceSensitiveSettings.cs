namespace CentralSecurityService.Configuration
{
    public class CentralSecurityServiceSensitiveSettings
    {
        public const string SectionName = "CentralSecurityServiceSensitive";

        public static CentralSecurityServiceSensitiveSettings Instance { get; private set; }

        public CentralSecurityServiceSensitiveSettings()
        {
            Instance = this;
        }

        public class EMailSettings
        {
            public string HostName { get; set; }

            public int HostPort { get; set; }

            public string SenderEMailAddress { get; set; }

            public string SenderPassword { get; set; }

            public string FromEMailAddress { get; set; }

            public string ToEMailName { get; set; }

            public string ToEMailAddress { get; set; }
        }

        public EMailSettings EMail { get; set; }
    }
}
