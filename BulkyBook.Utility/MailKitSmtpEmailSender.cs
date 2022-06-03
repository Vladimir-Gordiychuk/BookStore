using BulkyBook.Config;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BulkyBook.Utility
{
    public class MailKitSmtpEmailSender : IEmailSender
    {
        readonly SmtpConfig _config;

        public MailKitSmtpEmailSender(IOptions<SmtpConfig> keys)
        {
            _config = keys.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config.Email));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect(_config.Host, _config.Port, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate(_config.Email, _config.Password);
                emailClient.Send(message);
                emailClient.Disconnect(true);
            }

            return Task.CompletedTask;
        }
    }
}
