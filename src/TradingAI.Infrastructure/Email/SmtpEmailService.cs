using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Infrastructure.Email
{
    public class SmtpEmailService(
        IOptions<EmailSettings> options,
        ILogger<SmtpEmailService> logger) : IEmailService
    {
        private readonly EmailSettings _settings = options.Value;

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                var socketOptions = _settings.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTlsWhenAvailable;

                client.CheckCertificateRevocation = false;

                await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, ct);

                if (!string.IsNullOrWhiteSpace(_settings.UserName))
                    await client.AuthenticateAsync(_settings.UserName, _settings.Password, ct);

                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);

                logger.LogInformation("Email sent to {ToEmail} with subject '{Subject}'.", toEmail, subject);
            }
            catch (Exception ex)
            {
                // Swallow — never block a user flow because email failed.
                logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'.", toEmail, subject);
            }
        }
    }
}
