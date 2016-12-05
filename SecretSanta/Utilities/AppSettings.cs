using System.Configuration;

namespace SecretSanta.Utilities
{
    public struct AppSettings
    {
        public static string AdminEmail => ConfigurationManager.AppSettings["SecretSanta:AdminEmail"];
        public static string DataDirectory => ConfigurationManager.AppSettings["SecretSanta:DataDirectory"];
        public static int MaxImagesToLoad => int.Parse(ConfigurationManager.AppSettings["SecretSanta:MaxImagesToLoad"]);
        public static string DefaultPreviewImage => ConfigurationManager.AppSettings["SecretSanta:DefaultPreviewImage"];
        public static string AccountFilePattern => ConfigurationManager.AppSettings["SecretSanta:AccountFilePattern"];
        public static int GiftDollarLimit => int.Parse(ConfigurationManager.AppSettings["SecretSanta:GiftDollarLimit"]);
        public static string SmtpHost => ConfigurationManager.AppSettings["SecretSanta:SmtpHost"];
        public static int SmtpPort => int.Parse(ConfigurationManager.AppSettings["SecretSanta:SmtpPort"]);
        public static string SmtpUser => ConfigurationManager.AppSettings["SecretSanta:SmtpUser"];
        public static string SmtpPass => ConfigurationManager.AppSettings["SecretSanta:SmtpPass"];
    }
}