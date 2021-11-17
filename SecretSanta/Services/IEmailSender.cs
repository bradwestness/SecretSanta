namespace SecretSanta.Services;

public interface IEmailSender
{
    Task SendAsync(
        IEnumerable<(string? DisplayName, string? Email)>? to,
        string subject,
        string body,
        CancellationToken token);
}
