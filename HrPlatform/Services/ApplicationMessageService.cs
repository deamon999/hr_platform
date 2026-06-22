using HrPlatform.Data;
using HrPlatform.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class ApplicationMessageService(ApplicationDbContext context) : IApplicationMessageService
{
    public async Task<List<ApplicationMessage>> GetMessagesForApplicationAsync(int applicationId)
    {
        return await context.ApplicationMessages
            .Include(m => m.Sender)
            .Where(m => m.JobApplicationId == applicationId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<ApplicationMessage> SendMessageAsync(int applicationId, string senderId, string content)
    {
        var message = new ApplicationMessage
        {
            JobApplicationId = applicationId,
            SenderId = senderId,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        context.ApplicationMessages.Add(message);
        await context.SaveChangesAsync();

        // Load the sender so it can be displayed immediately
        await context.Entry(message).Reference(m => m.Sender).LoadAsync();

        return message;
    }

    public async Task MarkMessagesAsReadAsync(int applicationId, string receiverId)
    {
        var unreadMessages = await context.ApplicationMessages
            .Where(m => m.JobApplicationId == applicationId && m.SenderId != receiverId && !m.IsRead)
            .ToListAsync();

        if (unreadMessages.Any())
        {
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnreadCountAsync(int applicationId, string receiverId)
    {
        return await context.ApplicationMessages
            .CountAsync(m => m.JobApplicationId == applicationId && m.SenderId != receiverId && !m.IsRead);
    }
}
