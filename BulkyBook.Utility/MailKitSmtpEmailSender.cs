using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace BulkyBook.Utility
{
    public class MailKitSmtpEmailSender : IEmailSender
    {
        readonly GoogleKeys _keys;

        public MailKitSmtpEmailSender(GoogleKeys keys)
        {
            _keys = keys;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_keys.Email));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate(_keys.Email, _keys.Password);
                emailClient.Send(message);
                emailClient.Disconnect(true);
            }

            return Task.CompletedTask;
        }
    }
}
