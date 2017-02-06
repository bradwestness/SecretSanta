using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System.Collections.Generic;

namespace SecretSanta.Utilities
{
    public static class EmailMessage
    {
        public static void Send(MailboxAddress from, IEnumerable<MailboxAddress> to, string subject, string body)
        {
            using (var smtp = new SmtpClient())
            {
                var message = new MimeMessage();
                message.From.Add(from);
                message.To.AddRange(to);
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = body };

                smtp.Connect(AppSettings.SmtpHost, AppSettings.SmtpPort, true);
                smtp.Authenticate(AppSettings.SmtpUser, AppSettings.SmtpPass);
                smtp.Send(message);
                smtp.Disconnect(true);
            }
        }
    }
}
