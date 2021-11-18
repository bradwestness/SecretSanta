using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace SecretSanta.Services.Implementation;

public class EmailSender : IEmailSender
{
    private readonly IAppSettings _appSettings;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EmailSender(
        IAppSettings appSettings,
        IWebHostEnvironment webHostEnvironment)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
    }

    public async Task SendAsync(
        IEnumerable<(string? DisplayName, string? Email)>? to,
        string subject,
        string body,
        CancellationToken token)
    {
        if (_webHostEnvironment.IsDevelopment()
            && string.IsNullOrEmpty(_appSettings.SmtpHost))
        {
            return;
        }

        var toAddresses = to
            ?.Where(x => !string.IsNullOrEmpty(x.DisplayName) && !string.IsNullOrEmpty(x.Email))
            ?.Select(x => new MailboxAddress(x.DisplayName, x.Email))
            ?? Array.Empty<MailboxAddress>();

        if (!toAddresses.Any())
        {
            return;
        }

        using var smtp = new SmtpClient();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Santa Claus", _appSettings.SmtpFrom));
        message.To.AddRange(toAddresses);
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html)
        {
            Text = body.Replace(Environment.NewLine, "<br />")
        };

        await smtp.ConnectAsync(_appSettings.SmtpHost, _appSettings.SmtpPort, false, token);
        await smtp.AuthenticateAsync(_appSettings.SmtpUser, _appSettings.SmtpPass, token);
        await smtp.SendAsync(message, token);
        await smtp.DisconnectAsync(true, token);
    }
}
