using System;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using HrPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HrPlatform.Tests.Services
{
    public class InvitationServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public InvitationServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task CreateAsync_AddsInvitationToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var mockUserManager = GetMockUserManager();
            var mockJobInvitationService = new Mock<IJobInvitationService>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSmsService = new Mock<ISmsService>();
            
            var service = new InvitationService(context, mockUserManager.Object, mockJobInvitationService.Object, mockEmailService.Object, mockSmsService.Object);
            
            var invitation = new Invitation { Email = "test@test.com", ExpiresAt = DateTime.UtcNow.AddDays(7) };

            // Act
            var result = await service.CreateAsync(invitation);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, await context.Invitations.CountAsync());
        }

        [Fact]
        public async Task InviteAsync_WithNewUser_SendsEmail()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.FindByEmailAsync("test@test.com")).ReturnsAsync((ApplicationUser?)null);
            
            var mockJobInvitationService = new Mock<IJobInvitationService>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSmsService = new Mock<ISmsService>();
            
            var service = new InvitationService(context, mockUserManager.Object, mockJobInvitationService.Object, mockEmailService.Object, mockSmsService.Object);
            
            // Assume ContactMethod.Email exists
            var invitation = new Invitation { Email = "test@test.com", ContactMethod = HrPlatform.Models.ContactMethod.Email, ExpiresAt = DateTime.UtcNow.AddDays(7) };
            var uri = new Uri("http://localhost");

            // Act
            var result = await service.InviteAsync(invitation, uri);

            // Assert
            Assert.True(result!.Success);
            mockEmailService.Verify(x => x.SendEmailAsync("test@test.com", null, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public async Task ResendAsync_MarksOldAsUsedAndCreatesNew()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var oldInvitation = new Invitation { Id = 1, Email = "test@test.com", IsUsed = false };
            context.Invitations.Add(oldInvitation);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var mockJobInvitationService = new Mock<IJobInvitationService>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSmsService = new Mock<ISmsService>();
            
            var service = new InvitationService(context, mockUserManager.Object, mockJobInvitationService.Object, mockEmailService.Object, mockSmsService.Object);

            // Act
            var result = await service.ResendAsync(1, new Uri("http://localhost"));

            // Assert
            Assert.True(result!.Success);
            var oldInDb = await context.Invitations.AsNoTracking().FirstOrDefaultAsync(i => i.Id == 1);
            Assert.True(oldInDb!.IsUsed); // Old one marked as used
            Assert.Equal(2, await context.Invitations.CountAsync()); // A new one should be created
        }
    }
}
