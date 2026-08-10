using FarmStay.Application.Common.Settings;
using FarmStay.Application.Interfaces.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FarmStay.Infrastructure.Services.Communication
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            bool isHtml = true)
        {
            _logger.LogInformation(
                "Preparing email for {Email}.",
                toEmail);

            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    _emailSettings.SenderName,
                    _emailSettings.SenderEmail));

            email.To.Add(
                MailboxAddress.Parse(toEmail));

            email.Subject = subject;

            email.Body = new TextPart(isHtml ? "html" : "plain")
            {
                Text = htmlBody
            };

            using var smtp = new SmtpClient();

            _logger.LogInformation(
                "Connecting to SMTP server {Host}:{Port}.",
                _emailSettings.SmtpServer,
                _emailSettings.Port);

            await smtp.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.Port,
                SecureSocketOptions.StartTls);

            _logger.LogInformation(
                "SMTP connection established.");

            await smtp.AuthenticateAsync(
                _emailSettings.Username,
                _emailSettings.Password);

            _logger.LogInformation(
                "SMTP authentication successful.");

            await smtp.SendAsync(email);

            _logger.LogInformation(
                "Email sent successfully to {Email}.",
                toEmail);

            await smtp.DisconnectAsync(true);

            _logger.LogInformation(
                "SMTP connection closed.");
        }
    }
}