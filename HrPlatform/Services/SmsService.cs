using brevo_csharp.Api;
using brevo_csharp.Model;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace HrPlatform.Services;

public class SmsService : ISmsService
{
    private readonly string _apiKey;
    private readonly ILogger<SmsService>? _logger;

    public SmsService(IConfiguration configuration, ILogger<SmsService>? logger = null)
    {
        _logger = logger;
        _apiKey = configuration["Brevo:ApiKey"] ?? string.Empty;
        brevo_csharp.Client.Configuration.Default.ApiKey["api-key"] = _apiKey;
    }

    public async Task SendDriverInviteAsync(string phoneNumber, string firstName, string lastName, string content)
    {
        var smsApi = new TransactionalSMSApi();
        var sms = new SendTransacSms(
            sender: "CDL Pool",
            recipient: phoneNumber,
            content: content,
            type: SendTransacSms.TypeEnum.Transactional);
        try
        {
            var result = await smsApi.SendTransacSmsAsync(sms);
            _logger?.LogInformation("SMS sent successfully to {PhoneNumber}. Message ID: {MessageId}", phoneNumber, result.MessageId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
        }
    }
}