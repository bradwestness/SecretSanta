using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SecretSanta.Utilities
{
    public struct AppSettings
    {
        private static IConfigurationRoot _configuration;

        public static void Initialize(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public static string AdminEmail => _configuration.GetValue<string>("SecretSanta:AdminEmail");
        public static string DataDirectory => _configuration.GetValue<string>("SecretSanta:DataDirectory");
        public static int MaxImagesToLoad => _configuration.GetValue<int>("SecretSanta:MaxImagesToLoad");
        public static string DefaultPreviewImage => _configuration.GetValue<string>("SecretSanta:DefaultPreviewImage");
        public static string AccountFilePattern => _configuration.GetValue<string>("SecretSanta:AccountFilePattern");
        public static int GiftDollarLimit => _configuration.GetValue<int>("SecretSanta:GiftDollarLimit");
        public static string SmtpHost => _configuration.GetValue<string>("SecretSanta:SmtpHost");
        public static int SmtpPort => _configuration.GetValue<int>("SecretSanta:SmtpPort");
        public static string SmtpUser => _configuration.GetValue<string>("SecretSanta:SmtpUser");
        public static string SmtpPass => _configuration.GetValue<string>("SecretSanta:SmtpPass");
        public static CookieAuthenticationOptions Authentication => new CookieAuthenticationOptions
        {
            AuthenticationScheme = "SecretSantaAuthentication",
            LoginPath = new PathString("/Account/LogIn"),
            AccessDeniedPath = new PathString("/Home/Index"),
            AutomaticAuthenticate = true,
            AutomaticChallenge = true
        };
    }
}
