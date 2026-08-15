using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Models;
using HrPlatform.Data.Enums;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HrPlatform.Tests
{
    public class DailyMaintenanceServiceTests
    {
        private readonly ServiceCollection _services;
        private readonly ApplicationDbContext _db;
        private readonly Mock<IAdminUserService> _adminUserServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;

        public DailyMaintenanceServiceTests()
        {
            _services = new ServiceCollection();

            // Setup InMemory DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _db = new ApplicationDbContext(options);
            _services.AddSingleton(_db);

            // Mock other services
            _adminUserServiceMock = new Mock<IAdminUserService>();
            _services.AddSingleton(_adminUserServiceMock.Object);

            _emailServiceMock = new Mock<IEmailService>();
            _services.AddSingleton(_emailServiceMock.Object);

            // Add required dependencies that might be requested
            _services.AddLogging();
        }

        private DailyMaintenanceService CreateService()
        {
            var serviceProvider = _services.BuildServiceProvider();
            return new DailyMaintenanceService(serviceProvider, NullLogger<DailyMaintenanceService>.Instance);
        }

        [Fact]
        public async Task CleanUnconfirmedUsers_DeletesOnlyOldUnconfirmedSpam()
        {
            // Arrange
            var userOldSpam = new ApplicationUser { Id = "1", Email = "spam1@test.com", EmailConfirmed = false, TermsAcceptedDate = DateTime.UtcNow.AddHours(-25) };
            var userNewSpam = new ApplicationUser { Id = "2", Email = "spam2@test.com", EmailConfirmed = false, TermsAcceptedDate = DateTime.UtcNow.AddHours(-2) };
            var userVerified = new ApplicationUser { Id = "3", Email = "good@test.com", EmailConfirmed = true, TermsAcceptedDate = DateTime.UtcNow.AddHours(-48) };

            _db.Users.AddRange(userOldSpam, userNewSpam, userVerified);
            await _db.SaveChangesAsync();

            var service = CreateService();

            // Act
            await service.CleanUnconfirmedUsersAsync();

            // Assert
            // Should only delete the 25-hour-old unconfirmed user
            _adminUserServiceMock.Verify(x => x.DeleteAsync("1"), Times.Once);
            _adminUserServiceMock.Verify(x => x.DeleteAsync("2"), Times.Never);
            _adminUserServiceMock.Verify(x => x.DeleteAsync("3"), Times.Never);
        }

        [Fact]
        public async Task CheckCredentialExpiries_SendsLicenseAlerts_For7And30Days()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            var user1 = new ApplicationUser { Id = "u1", Email = "driver1@test.com" };
            var profile1 = new DriverProfile { UserId = "u1", User = user1, FirstName = "John", LastName = "Doe", Email = "driver1@test.com", PhoneNumber = "123" };
            var lic1 = new DriverLicense { DriverProfileId = 1, DriverProfile = profile1, ExpiryDate = today.AddDays(7), LicenseNumber = "A1", IssuingState = "TX", Class = CdlClass.A };

            var user2 = new ApplicationUser { Id = "u2", Email = "driver2@test.com" };
            var profile2 = new DriverProfile { UserId = "u2", User = user2, FirstName = "Jane", LastName = "Smith", Email = "driver2@test.com", PhoneNumber = "123" };
            var lic2 = new DriverLicense { DriverProfileId = 2, DriverProfile = profile2, ExpiryDate = today.AddDays(15), LicenseNumber = "A2", IssuingState = "CA", Class = CdlClass.A };

            _db.Users.AddRange(user1, user2);
            _db.DriverProfiles.AddRange(profile1, profile2);
            _db.DriverLicenses.AddRange(lic1, lic2);
            await _db.SaveChangesAsync();

            var service = CreateService();

            // Act
            await service.CheckCredentialExpiriesAsync();

            // Assert
            _emailServiceMock.Verify(x => x.SendEmailAsync(
                "driver1@test.com", 
                "John Doe", 
                It.Is<string>(s => s.Contains("7 days")), 
                It.IsAny<string>()), 
                Times.Once);

            _emailServiceMock.Verify(x => x.SendEmailAsync(
                "driver2@test.com", 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), 
                Times.Never);
        }

        [Fact]
        public async Task CheckCredentialExpiries_HandlesMissingNamesGracefully()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            // Simulating a case where FirstName and LastName are completely null, missing, or whitespace
            // The service uses Trim(), which could throw if we didn't handle it, but EF Core mapped them as Required
            // In a real database they might be empty strings if skipped.
            var user1 = new ApplicationUser { Id = "u1", Email = "noname@test.com" };
            var profile1 = new DriverProfile { UserId = "u1", User = user1, FirstName = " ", LastName = "", Email = "noname@test.com", PhoneNumber = "123" };
            var lic1 = new DriverLicense { DriverProfileId = 1, DriverProfile = profile1, ExpiryDate = today.AddDays(30), LicenseNumber = "B1", IssuingState = "TX", Class = CdlClass.B };

            _db.Users.Add(user1);
            _db.DriverProfiles.Add(profile1);
            _db.DriverLicenses.Add(lic1);
            await _db.SaveChangesAsync();

            var service = CreateService();

            // Act
            var exception = await Record.ExceptionAsync(() => service.CheckCredentialExpiriesAsync());

            // Assert
            Assert.Null(exception); // Should not throw NullReferenceException

            // It should still attempt to send the email but with an empty string or trimmed spaces for the name
            _emailServiceMock.Verify(x => x.SendEmailAsync(
                "noname@test.com", 
                It.IsAny<string>(), 
                It.Is<string>(s => s.Contains("30 days")), 
                It.IsAny<string>()), 
                Times.Once);
        }

        [Fact]
        public async Task CheckCredentialExpiries_SkipsIfEmailIsNull()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            var user1 = new ApplicationUser { Id = "u1", Email = null }; // Missing email!
            var profile1 = new DriverProfile { UserId = "u1", User = user1, FirstName = "No", LastName = "Email", Email = "a@a.com", PhoneNumber = "123" };
            var card1 = new DriverMedicalCard { DriverProfileId = 1, DriverProfile = profile1, ExpiryDate = today.AddDays(7), SelfCertification = SelfCertificationCategory.NonExceptedInterstate };

            _db.Users.Add(user1);
            _db.DriverProfiles.Add(profile1);
            _db.DriverMedicalCards.Add(card1);
            await _db.SaveChangesAsync();

            var service = CreateService();

            // Act
            await service.CheckCredentialExpiriesAsync();

            // Assert
            _emailServiceMock.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), 
                Times.Never);
        }
    }
}
