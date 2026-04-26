namespace HrPlatform.Services;

public class SmsService : ISmsService {
    public Task SendDriverInviteAsync(string phoneNumber, string FirstName, string LastName, string inviteLink) {
        Console.WriteLine($"Phone: {phoneNumber}, First Name: {FirstName}, Last Name: {LastName}, Link: {inviteLink}");
        return Task.CompletedTask;
    }
}