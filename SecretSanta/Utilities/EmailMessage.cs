using System.Net;
using System.Net.Mail;

namespace SecretSanta.Utilities
{
    public static class EmailMessage
    {
        public static void Send(MailAddress from, MailAddress to, string subject, string body)
        {
            using (var smtp = new SmtpClient(AppSettings.SmtpHost, AppSettings.SmtpPort))
            {
                var message = new MailMessage(from, to);
                message.Subject = subject;
                message.Body = body;
                message.From = from;
                message.ReplyToList.Clear();
                message.ReplyToList.Add(from);

                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(AppSettings.SmtpUser, AppSettings.SmtpPass);
                smtp.Send(message);
            }
        }
    }
}