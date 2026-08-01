using System.Net;
using System.Net.Mail;

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
        _enableSsl = bool.TryParse(configuration["Smtp:EnableSsl"], out var ssl) ? ssl : true;
        
        _fromEmail = configuration["Smtp:FromEmail"] ?? "noreply@example.com";
        _fromName = configuration["Smtp:FromName"] ?? "CDL Pool";
    }

    public async Task SendEmailAsync(string email, string? userName,
        string subject, string htmlContent)
    {
        using var client = new SmtpClient(_host, _port);
        client.Credentials = new NetworkCredential(_username, _password);
        client.EnableSsl = _enableSsl;

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = htmlContent,
            IsBodyHtml = true
        };

        if (string.IsNullOrEmpty(userName))
            mailMessage.To.Add(new MailAddress(email));
        else
            mailMessage.To.Add(new MailAddress(email, userName));

        try
        {
            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"Successfully sent email to {email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email to {email}: {ex.Message}");
        }
    }
}