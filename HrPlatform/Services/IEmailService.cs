namespace HrPlatform.Services;

public interface IEmailService {
    Task SendEmailAsync(string email, string? userName, string subject, string htmlContent);
}