using System;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Models;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HrPlatform.Tests.Services;

public class ApplicationMessageServiceTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SendMessageAsync_AddsMessageAndReturnsIt()
    {
        var db = GetDbContext();
        db.Users.Add(new ApplicationUser { Id = "user1", FirstName = "Test" });
        await db.SaveChangesAsync();

        var service = new ApplicationMessageService(db);

        var result = await service.SendMessageAsync(1, "user1", "Hello");

        Assert.NotNull(result);
        Assert.Equal(1, result.JobApplicationId);
        Assert.Equal("user1", result.SenderId);
        Assert.Equal("Hello", result.Content);
        Assert.False(result.IsRead);
        Assert.NotNull(result.Sender);
        
        var saved = await db.ApplicationMessages.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("Hello", saved.Content);
    }

    [Fact]
    public async Task GetMessagesForApplicationAsync_ReturnsOrderedMessages()
    {
        var db = GetDbContext();
        db.ApplicationMessages.AddRange(
            new ApplicationMessage { JobApplicationId = 1, Content = "Msg2", SentAt = DateTime.UtcNow.AddMinutes(2) },
            new ApplicationMessage { JobApplicationId = 1, Content = "Msg1", SentAt = DateTime.UtcNow.AddMinutes(1) },
            new ApplicationMessage { JobApplicationId = 2, Content = "Msg3", SentAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new ApplicationMessageService(db);
        var messages = await service.GetMessagesForApplicationAsync(1);

        Assert.Equal(2, messages.Count);
        Assert.Equal("Msg1", messages[0].Content);
        Assert.Equal("Msg2", messages[1].Content);
    }

    [Fact]
    public async Task MarkMessagesAsReadAsync_UpdatesOnlyUnreadForReceiver()
    {
        var db = GetDbContext();
        db.ApplicationMessages.AddRange(
            new ApplicationMessage { JobApplicationId = 1, SenderId = "other", IsRead = false },
            new ApplicationMessage { JobApplicationId = 1, SenderId = "other", IsRead = true },
            new ApplicationMessage { JobApplicationId = 1, SenderId = "receiver", IsRead = false },
            new ApplicationMessage { JobApplicationId = 2, SenderId = "other", IsRead = false }
        );
        await db.SaveChangesAsync();

        var service = new ApplicationMessageService(db);
        await service.MarkMessagesAsReadAsync(1, "receiver");

        var allMessages = await db.ApplicationMessages.ToListAsync();
        Assert.True(allMessages[0].IsRead);
        Assert.True(allMessages[1].IsRead);
        Assert.False(allMessages[2].IsRead);
        Assert.False(allMessages[3].IsRead);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        var db = GetDbContext();
        db.ApplicationMessages.AddRange(
            new ApplicationMessage { JobApplicationId = 1, SenderId = "other", IsRead = false },
            new ApplicationMessage { JobApplicationId = 1, SenderId = "other", IsRead = false },
            new ApplicationMessage { JobApplicationId = 1, SenderId = "receiver", IsRead = false },
            new ApplicationMessage { JobApplicationId = 1, SenderId = "other", IsRead = true }
        );
        await db.SaveChangesAsync();

        var service = new ApplicationMessageService(db);
        var count = await service.GetUnreadCountAsync(1, "receiver");

        Assert.Equal(2, count);
    }
}
