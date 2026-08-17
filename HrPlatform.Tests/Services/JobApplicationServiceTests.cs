using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HrPlatform.Tests.Services;

public class JobApplicationServiceTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ApplyAsync_AddsApplicationAndReturnsIt()
    {
        var db = GetDbContext();
        var emailMock = new Mock<IEmailService>();
        var smsMock = new Mock<ISmsService>();

        var service = new JobApplicationService(db, emailMock.Object, smsMock.Object);
        var result = await service.ApplyAsync(10, "user1");

        Assert.NotNull(result);
        Assert.Equal(10, result.JobId);
        Assert.Equal("user1", result.UserId);

        var saved = await db.JobApplications.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal(10, saved.JobId);
    }

    [Fact]
    public async Task ReviewAsync_UpdatesStatusAndSendsNotifications()
    {
        var db = GetDbContext();
        var app = new JobApplication 
        { 
            Id = 1, 
            JobId = 10, 
            UserId = "user1", 
            Status = ApplicationStatus.UnderReview 
        };
        db.JobApplications.Add(app);
        
        var user = new ApplicationUser { Id = "user1", Email = "test@test.com", FirstName = "John", LastName = "Doe" };
        db.Users.Add(user);
        
        db.Jobs.Add(new Job { Id = 10, Title = "Driver", CompanyId = 1, Company = new Company { Id = 1, Name = "Inc" }});
        await db.SaveChangesAsync();

        var emailMock = new Mock<IEmailService>();
        var smsMock = new Mock<ISmsService>();

        var service = new JobApplicationService(db, emailMock.Object, smsMock.Object);

        await service.ReviewAsync(1, ApplicationStatus.Accepted, "Good job");

        var updated = await db.JobApplications.FindAsync(1);
        Assert.Equal(ApplicationStatus.Accepted, updated!.Status);
        Assert.Equal("Good job", updated.ReviewerNotes);

        emailMock.Verify(e => e.SendEmailAsync("test@test.com", "John Doe", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAllFilteredAsync_DriverFilter_ReturnsDriverApplications()
    {
        var db = GetDbContext();
        db.Companies.Add(new Company { Id = 1, Name = "C1" });
        db.Jobs.Add(new Job { Id = 10, Title = "T", CompanyId = 1 });
        db.JobApplications.AddRange(
            new JobApplication { Id = 1, UserId = "user1", JobId = 10, User = new ApplicationUser { Id = "user1" } },
            new JobApplication { Id = 2, UserId = "user2", JobId = 10, User = new ApplicationUser { Id = "user2" } }
        );
        await db.SaveChangesAsync();

        var service = new JobApplicationService(db, Mock.Of<IEmailService>(), Mock.Of<ISmsService>());

        var result = await service.GetAllFilteredAsync("user1", isManager: false, isDriver: true, companyId: null);

        Assert.Single(result);
        Assert.Equal("user1", result.First().UserId);
    }
}
