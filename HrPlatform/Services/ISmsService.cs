namespace HrPlatform.Services;

public interface ISmsService {
    Task SendDriverInviteAsync(string phoneNumber, string FirstName, string LastName, string content);
}