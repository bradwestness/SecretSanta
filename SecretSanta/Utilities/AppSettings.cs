namespace SecretSanta.Utilities;

public struct AppSettings
{
    private static IConfiguration? _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static string AdminEmail => _configuration.GetValue<string>("SecretSanta:AdminEmail");
    public static string DataDirectory => _configuration.GetValue<string>("SecretSanta:DataDirectory");
    public static int MaxImagesToLoad => _configuration.GetValue<int>("SecretSanta:MaxImagesToLoad");
    public static string DefaultPreviewImage => _configuration.GetValue<string>("SecretSanta:DefaultPreviewImage");
    public static string AccountFilePattern => _configuration.GetValue<string>("SecretSanta:AccountFilePattern");
    public static int GiftDollarLimit => _configuration.GetValue<int>("SecretSanta:GiftDollarLimit");
    public static string SmtpFrom => _configuration.GetValue<string>("SecretSanta:SmtpFrom");
    public static string SmtpHost => _configuration.GetValue<string>("SecretSanta:SmtpHost");
    public static int SmtpPort => _configuration.GetValue<int>("SecretSanta:SmtpPort");
    public static string SmtpUser => _configuration.GetValue<string>("SecretSanta:SmtpUser");
    public static string SmtpPass => _configuration.GetValue<string>("SecretSanta:SmtpPass");
    public static string LoginPath => new PathString("/Account/LogIn");
    public static string LogoutPath => new PathString("/Home/Index");
    public static string ErrorPath => new PathString("/Home/Error");
    public static TimeSpan SessionTimeout => TimeSpan.FromMinutes(60);
}
