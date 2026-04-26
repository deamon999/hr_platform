using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;

namespace HrPlatform.Services;

public class EmailService : IEmailService
{
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _apiKey;

    public EmailService(IConfiguration configuration)
    {
        _apiKey = configuration["Brevo:ApiKey"];
        _fromEmail = configuration["Brevo:FromEmail"];
        _fromName = configuration["Brevo:FromName"];

        brevo_csharp.Client.Configuration.Default.ApiKey["api-key"] = _apiKey;
    }

    public async System.Threading.Tasks.Task SendEmailAsync(string email, string? userName,
        string subject, string htmlContent)
    {
        var api = new TransactionalEmailsApi();
        var emailObject = new SendSmtpEmail(
            to: [new SendSmtpEmailTo(email: email, name: userName)],
            sender: new SendSmtpEmailSender(name: _fromName, email: _fromEmail),
            subject: subject,
            htmlContent: htmlContent
        );
        try
        {
            var res = await api.SendTransacEmailAsync(emailObject);
            Console.WriteLine($"Message id: {res.MessageId}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}