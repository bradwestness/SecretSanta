using System.Configuration;

namespace SecretSanta.Utilities
{
    public struct AppSettings
    {
        public static string AdminEmail
        {
            get { return ConfigurationManager.AppSettings["SecretSanta:AdminEmail"]; }
        }

        public static string DataDirectory
        {
            get { return ConfigurationManager.AppSettings["SecretSanta:DataDirectory"]; }
        }

        public static string AccountFilePattern
        {
            get { return ConfigurationManager.AppSettings["SecretSanta:AccountFilePattern"]; }
        }

        public static int GiftDollarLimit
        {
            get { return int.Parse(ConfigurationManager.AppSettings["SecretSanta:GiftDollarLimit"]); }
        }

        public static string SmtpHost
        {
            get { return ConfigurationManager.AppSettings["SecretSanta:SmtpHost"]; }
        }

        public static int SmtpPort
        {
            get { return int.Parse(ConfigurationManager.AppSettings["SecretSanta:SmtpPort"]); }
        }

        public static string SmtpUser
        {
            get { return ConfigurationManager.AppSettings["SecretSanta:SmtpUser"]; }
        }

        public static string SmtpPass
        {
            get { return ConfigurationManager.AppSettings["SecretSanta:SmtpPass"]; }
        }
    }
}