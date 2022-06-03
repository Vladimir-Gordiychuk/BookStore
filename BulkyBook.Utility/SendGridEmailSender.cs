using BulkyBook.Config;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BulkyBook.Utility
{
    public class SendGridEmailSender : IEmailSender
    {
        readonly SendGridConfig _config;

        public SendGridEmailSender(IOptions<SendGridConfig> config)
        {
            _config = config.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(_config.ApiKey);
            var from = new EmailAddress(_config.FromEmail, _config.FromName);
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, null, htmlMessage);
            return client.SendEmailAsync(message);
        }
    }
}
