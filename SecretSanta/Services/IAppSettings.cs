namespace SecretSanta.Services;

public interface IAppSettings
{
    string AdminEmail { get; }

    string DataDirectory { get; }

    string DefaultPreviewImage { get; }

    string AccountFilePattern { get; }

    int GiftDollarLimit { get; }

    string SmtpFrom { get; }

    string SmtpHost { get; }

    int SmtpPort { get; }

    string SmtpUser { get; }

    string SmtpPass { get; }
}
