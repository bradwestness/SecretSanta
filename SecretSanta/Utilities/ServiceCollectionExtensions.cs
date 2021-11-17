using SecretSanta.Services;
using SecretSanta.Services.Implementation;

namespace SecretSanta.Utilities;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecretSanta(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<IAppSettings, AppSettings>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddSingleton<IPreviewGenerator, PreviewGenerator>();

        return services;
    }
}
