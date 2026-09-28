namespace HrPlatform.Services;

public interface ISmsService
{
    Task SendDriverInviteAsync(string phoneNumber, string firstName, string lastName, string content);
}