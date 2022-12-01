namespace SecretSanta.Services.Implementation;

public class AppSettings : IAppSettings
{
    private readonly IConfiguration _configuration;

    public AppSettings(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string AdminEmail => _configuration.GetValue<string>("SecretSanta:AdminEmail") ?? string.Empty;

    public string DataDirectory => _configuration.GetValue<string>("SecretSanta:DataDirectory") ?? string.Empty;

    public string DefaultPreviewImage => _configuration.GetValue<string>("SecretSanta:DefaultPreviewImage") ?? string.Empty;

    public string AccountFilePattern => _configuration.GetValue<string>("SecretSanta:AccountFilePattern") ?? string.Empty;

    public int GiftDollarLimit => _configuration.GetValue<int>("SecretSanta:GiftDollarLimit");

    public string SmtpFrom => _configuration.GetValue<string>("SecretSanta:SmtpFrom") ?? string.Empty;

    public string SmtpHost => _configuration.GetValue<string>("SecretSanta:SmtpHost") ?? string.Empty;

    public int SmtpPort => _configuration.GetValue<int>("SecretSanta:SmtpPort");

    public string SmtpUser => _configuration.GetValue<string>("SecretSanta:SmtpUser") ?? string.Empty;

    public string SmtpPass => _configuration.GetValue<string>("SecretSanta:SmtpPass") ?? string.Empty;
}
