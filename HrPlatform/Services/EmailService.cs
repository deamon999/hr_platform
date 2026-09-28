using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace HrPlatform.Services;

public class EmailService : IEmailService
{
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _host;
    private readonly string _password;
    private readonly int _port;
    private readonly string _username;
    private readonly ILogger<EmailService>? _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService>? logger = null)
    {
        _logger = logger;
        _host = configuration["Smtp:Host"] ?? throw new ArgumentNullException(nameof(configuration), "Smtp:Host configuration is missing");
        _port = int.TryParse(configuration["Smtp:Port"], out var port) ? port : 587;
        _username = configuration["Smtp:Username"] ?? string.Empty;
        _password = configuration["Smtp:Password"] ?? string.Empty;

        _fromEmail = configuration["Smtp:FromEmail"] ?? "noreply@example.com";
        _fromName = configuration["Smtp:FromName"] ?? "CDL Pool";
    }

    public async Task SendEmailAsync(string email, string? userName,
        string subject, string htmlContent)
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_username, _password);

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_fromName, _fromEmail));
            if (string.IsNullOrWhiteSpace(userName))
                mimeMessage.To.Add(new MailboxAddress("Guest", email));
            else
                mimeMessage.To.Add(new MailboxAddress(userName, email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("html") { Text = htmlContent };

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
            _logger?.LogInformation("Successfully sent email to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending email to {Email}", email);
        }
    }
}