using HrPlatform.Data.Entities;

namespace HrPlatform.Services;

public interface IApplicationMessageService
{
    Task<List<ApplicationMessage>> GetMessagesForApplicationAsync(int applicationId);
    Task<ApplicationMessage> SendMessageAsync(int applicationId, string senderId, string content);
    Task MarkMessagesAsReadAsync(int applicationId, string receiverId);
    Task<int> GetUnreadCountAsync(int applicationId, string receiverId);
}
