using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

#pragma warning disable SYSLIB0014

namespace HrPlatform.Services;

public class EmailService : IEmailService
{
    private readonly bool _enableSsl;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _host;
    private readonly string _password;
    private readonly int _port;
    private readonly string _username;

    public EmailService(IConfiguration configuration)
    {
        _host = configuration["Smtp:Host"] ?? throw new ArgumentNullException("Smtp:Host configuration is missing");
        _port = int.TryParse(configuration["Smtp:Port"], out var port) ? port : 587;
        _username = configuration["Smtp:Username"] ?? "";
        _password = configuration["Smtp:Password"] ?? "";

        _fromEmail = configuration["Smtp:FromEmail"] ?? "noreply@example.com";
        _fromName = configuration["Smtp:FromName"] ?? "CDL Pool";
    }

    public async Task SendEmailAsync(string email, string? userName,
        string subject, string htmlContent)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_username, _password);

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_fromName, _fromEmail));
        if (string.IsNullOrEmpty(userName))
            mimeMessage.To.Add(new MailboxAddress("Guest", email));
        else
            mimeMessage.To.Add(new MailboxAddress(userName, email));
        mimeMessage.Subject = subject;
        mimeMessage.Body = new TextPart("html") { Text = htmlContent };


        try
        {
            await client.SendAsync(mimeMessage);
            Console.WriteLine($"Successfully sent email to {email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email to {email}: {ex.Message}");
        }
        finally
        {
            client.Disconnect(true);
        }
    }
}