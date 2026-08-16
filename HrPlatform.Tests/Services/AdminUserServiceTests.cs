using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using HrPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HrPlatform.Tests.Services
{
    public class AdminUserServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public AdminUserServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task GetAllUsersWithRolesAsync_ReturnsUsers()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var user = new ApplicationUser { Id = "user1", UserName = "testuser", Email = "test@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "Admin" });

            var service = new AdminUserService(context, mockUserManager.Object);

            // Act
            var result = await service.GetAllUsersWithRolesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("user1", result[0].UserId);
            Assert.Equal("Admin", result[0].Roles);
        }
        
        [Fact]
        public async Task GetUserByIdAsync_ThrowsKeyNotFound_IfUserDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var mockUserManager = GetMockUserManager();
            var service = new AdminUserService(context, mockUserManager.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetUserByIdAsync("invalid_id"));
        }
        
        [Fact]
        public async Task DeleteAsync_DeletesUserAndRelatedData()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var user = new ApplicationUser { Id = "user1", Email = "test@example.com" };
            context.Users.Add(user);
            
            context.Invitations.Add(new Data.Entities.Invitation { Email = "test@example.com" });
            context.DriverProfiles.Add(new Data.Models.DriverProfile { UserId = "user1" });
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var service = new AdminUserService(context, mockUserManager.Object);

            // Act
            await service.DeleteAsync("user1");

            // Assert
            Assert.Empty(context.Users);
            Assert.Empty(context.Invitations);
            Assert.Empty(context.DriverProfiles);
        }
        
        [Fact]
        public async Task HandleRoleTransitionCleanupAsync_CleansUpDriverData_WhenOldRoleIsDriver()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var user = new ApplicationUser { Id = "user1" };
            context.Users.Add(user);
            context.DriverProfiles.Add(new Data.Models.DriverProfile { UserId = "user1" });
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var service = new AdminUserService(context, mockUserManager.Object);

            // Act
            await service.HandleRoleTransitionCleanupAsync(user, RoleConstants.Driver, RoleConstants.Admin);

            // Assert
            Assert.Empty(context.DriverProfiles);
            Assert.Null(user.CompanyId);
        }
    }
}
