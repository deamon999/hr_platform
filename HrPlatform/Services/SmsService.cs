using brevo_csharp.Api;
using brevo_csharp.Model;
using Task = System.Threading.Tasks.Task;

namespace HrPlatform.Services;

public class SmsService : ISmsService
{
    private readonly string _apiKey;

    public SmsService(IConfiguration configuration)
    {
        _apiKey = configuration["Brevo:ApiKey"];
        brevo_csharp.Client.Configuration.Default.ApiKey["api-key"] = _apiKey;
    }

    public async Task SendDriverInviteAsync(string phoneNumber, string FirstName, string LastName, string content)
    {
        var smsApi = new TransactionalSMSApi();
        var sms = new SendTransacSms(sender: "CDL Pool",
            recipient: phoneNumber,
            content: content,
            type: SendTransacSms.TypeEnum.Transactional);
        try
        {
            var result = await smsApi.SendTransacSmsAsync(sms);
            Console.WriteLine($"SMS sent! Message ID: {result.MessageId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}