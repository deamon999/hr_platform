using System;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HrPlatform.Tests.Services;

public class FakeNavigationManager : NavigationManager
{
    public FakeNavigationManager()
    {
        Initialize("http://localhost/", "http://localhost/");
    }
}

public class JobInvitationServiceTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private Mock<UserManager<ApplicationUser>> GetUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task CreateFromRegistrationAsync_AddsInvitationAndSendsEmail()
    {
        var db = GetDbContext();
        db.Jobs.Add(new Job { Id = 1, Title = "Test Job", Company = new Company { Name = "C1" } });
        await db.SaveChangesAsync();

        var userManager = GetUserManagerMock();
        userManager.Setup(u => u.FindByIdAsync("user1"))
            .ReturnsAsync(new ApplicationUser { Id = "user1", Email = "u@test.com", FirstName = "John", LastName = "Doe" });

        var emailMock = new Mock<IEmailService>();
        var navManager = new FakeNavigationManager();
        var smsMock = new Mock<ISmsService>();

        var service = new JobInvitationService(db, userManager.Object, emailMock.Object, navManager, smsMock.Object);

        await service.CreateFromRegistrationAsync("user1", 1);

        var saved = await db.JobInvitations.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("user1", saved.UserId);
        Assert.Equal(1, saved.JobId);

        emailMock.Verify(e => e.SendEmailAsync("u@test.com", "John Doe", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatusAndReturnsTrue()
    {
        var db = GetDbContext();
        db.JobInvitations.Add(new JobInvitation { Id = 10, UserId = "user1", Status = JobInvitationStatus.Pending });
        await db.SaveChangesAsync();

        var service = new JobInvitationService(db, GetUserManagerMock().Object, Mock.Of<IEmailService>(), new FakeNavigationManager(), Mock.Of<ISmsService>());

        var result = await service.UpdateStatusAsync(10, "user1", JobInvitationStatus.Accepted);

        Assert.True(result);
        var updated = await db.JobInvitations.FindAsync(10);
        Assert.Equal(JobInvitationStatus.Accepted, updated!.Status);
        Assert.NotNull(updated.ReviewedAt);
    }

    [Fact]
    public async Task UpdateStatusAsync_NotFound_ReturnsFalse()
    {
        var db = GetDbContext();
        var service = new JobInvitationService(db, GetUserManagerMock().Object, Mock.Of<IEmailService>(), new FakeNavigationManager(), Mock.Of<ISmsService>());

        var result = await service.UpdateStatusAsync(10, "user1", JobInvitationStatus.Accepted);

        Assert.False(result);
    }
}
