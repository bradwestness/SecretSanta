using System;
using System.Net;
using System.Net.Mail;

namespace SecretSanta.Utilities
{
    public static class EmailMessage
    {
        public static void Send(MailAddress from, MailAddressCollection to, string subject, string body)
        {
            using (var smtp = new SmtpClient(AppSettings.SmtpHost, AppSettings.SmtpPort))
            {
                var message = new MailMessage();
                message.From = from;
                foreach (MailAddress recipient in to)
                {
                    message.To.Add(recipient);
                }
                message.Subject = subject;
                message.Body = body.Replace(Environment.NewLine, "<br />");
                message.From = from;
                message.ReplyToList.Clear();
                message.ReplyToList.Add(from);
                message.IsBodyHtml = true;

                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(AppSettings.SmtpUser, AppSettings.SmtpPass);
                smtp.Send(message);
            }
        }
    }
}