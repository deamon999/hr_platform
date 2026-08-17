using System;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HrPlatform.Tests.Services
{
    public class DashboardServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public DashboardServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAdminStatsAsync_ReturnsCorrectCounts()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            
            context.DriverProfiles.Add(new DriverProfile { Id = 1, UserId = "1", FirstName = "F", LastName = "L", PhoneNumber = "1", Email = "e@e.com" });
            context.Jobs.Add(new Job { Id = 1, IsActive = true, CompanyId = 1, Title = "T" });
            context.JobApplications.Add(new JobApplication { Id = 1, Status = ApplicationStatus.Pending, AppliedAt = DateTime.UtcNow, UserId = "1", JobId = 1 });
            
            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var stats = await service.GetAdminStatsAsync();

            // Assert
            Assert.Equal(1, stats.TotalDrivers);
            Assert.Equal(1, stats.TotalJobs);
            Assert.Equal(1, stats.OpenJobs);
            Assert.Equal(1, stats.PendingApplications);
            Assert.True(stats.ApplicationsByStatus.ContainsKey(ApplicationStatus.Pending.ToString()));
        }

        [Fact]
        public async Task GetManagerStatsAsync_ReturnsCountsForSpecificCompany()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            
            context.Jobs.Add(new Job { Id = 1, IsActive = true, CompanyId = 1, Title = "T" }); // Belongs to company 1
            context.Jobs.Add(new Job { Id = 2, IsActive = true, CompanyId = 2, Title = "T" }); // Belongs to company 2
            
            context.JobApplications.Add(new JobApplication { Id = 1, JobId = 1, Status = ApplicationStatus.Pending, AppliedAt = DateTime.UtcNow, UserId = "user1" });
            
            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var stats = await service.GetManagerStatsAsync(1);

            // Assert
            Assert.Equal(1, stats.TotalJobs); // Only counts for company 1
            Assert.Equal(1, stats.PendingApplications);
        }
    }
}
