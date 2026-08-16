using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HrPlatform.Tests.Services;

public class DriverProfileServiceTests
{
    private ApplicationDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetByUserIdAsync_And_GetByIdAsync_ReturnProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile 
            { 
                Id = 1, UserId = "user1", FirstName = "John", LastName = "Doe",
                License = new DriverLicense { Id = 1, Class = CdlClass.A }
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context);
            
            var byUser = await service.GetByUserIdAsync("user1");
            Assert.NotNull(byUser);
            Assert.Equal("John", byUser.FirstName);
            Assert.NotNull(byUser.License);
            
            var byId = await service.GetByIdAsync(1);
            Assert.NotNull(byId);
            Assert.Equal("John", byId.FirstName);
            
            Assert.Null(await service.GetByIdAsync(99));
            Assert.Null(await service.GetByUserIdAsync("notfound"));
        }
    }

    [Fact]
    public async Task GetAllPagedAsync_FiltersAndSortsCorrectly()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.AddRange(
                new DriverProfile { Id = 1, FirstName = "Alice", LastName = "Smith", YearsOfExperience = 5, License = new DriverLicense { Class = CdlClass.A } },
                new DriverProfile { Id = 2, FirstName = "Bob", LastName = "Jones", YearsOfExperience = 2, License = new DriverLicense { Class = CdlClass.B } },
                new DriverProfile { Id = 3, FirstName = "Charlie", LastName = "Brown", YearsOfExperience = 10, License = new DriverLicense { Class = CdlClass.A, Endorsements = new List<DriverLicenseEndorsement> { new DriverLicenseEndorsement { Endorsement = CdlEndorsement.Hazmat } } } }
            );
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context);

            var all = await service.GetAllPagedAsync(new ProfileSearch());
            Assert.Equal(3, all.TotalCount);
            
            var searchName = await service.GetAllPagedAsync(new ProfileSearch { Name = "Alice" });
            Assert.Equal(1, searchName.TotalCount);

            var cdlA = await service.GetAllPagedAsync(new ProfileSearch { CdlClass = CdlClass.A });
            Assert.Equal(2, cdlA.TotalCount);

            var minExp = await service.GetAllPagedAsync(new ProfileSearch { MinYears = 4 });
            Assert.Equal(2, minExp.TotalCount); // Alice, Charlie

            var hazmat = await service.GetAllPagedAsync(new ProfileSearch { RequiredEndorsement = CdlEndorsement.Hazmat });
            Assert.Equal(1, hazmat.TotalCount);
            Assert.Equal(3, hazmat.Items[0].Id);
        }
    }

    [Fact]
    public async Task GetByCompanyPagedAsync_ReturnsOnlyDriversWhoAppliedToCompanyJobs()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            var user1 = new ApplicationUser { Id = "user1" };
            var user2 = new ApplicationUser { Id = "user2" };
            var jobCompany1 = new Job { Id = 1, CompanyId = 1 };
            
            context.Users.AddRange(user1, user2);
            context.Jobs.Add(jobCompany1);

            context.DriverProfiles.AddRange(
                new DriverProfile { Id = 1, UserId = "user1", FirstName = "Applied", LastName = "One" },
                new DriverProfile { Id = 2, UserId = "user2", FirstName = "DidNot", LastName = "Apply" }
            );

            context.JobApplications.Add(new JobApplication { UserId = "user1", JobId = 1 });

            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context);

            var result = await service.GetByCompanyPagedAsync(new ProfileSearch(), companyId: 1);
            Assert.Single(result.Items);
            Assert.Equal("Applied", result.Items[0].FirstName);
        }
    }

    [Fact]
    public async Task CreateAsync_AddsProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context);
            var result = await service.CreateAsync(new DriverProfile { FirstName = "New", LastName = "Driver", UserId = "user" });
            Assert.NotEqual(0, result.Id);
        }
        
        using (var context = GetDbContext(dbName))
        {
            Assert.Equal(1, await context.DriverProfiles.CountAsync());
        }
    }

    [Fact]
    public async Task UpdateAsync_RejectsUnauthorizedUser()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile { Id = 1, UserId = "owner" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context);
            var profile = new DriverProfile { Id = 1, UserId = "hacker" };
            
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(profile, "hacker"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(new DriverProfile { Id = 99 }, "owner"));
        }
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProfileAndPrunesOrphans()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile 
            { 
                Id = 1, UserId = "owner",
                EmploymentHistory = new List<DriverEmployment> 
                {
                    new DriverEmployment { Id = 1, CompanyName = "Old Job" }
                }
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context);
            
            var updateProfile = new DriverProfile 
            { 
                Id = 1, UserId = "owner",
                FirstName = "Updated",
                EmploymentHistory = new List<DriverEmployment>
                {
                    // Adding a new job, old job should be deleted
                    new DriverEmployment { Id = 0, CompanyName = "New Job" },
                    new DriverEmployment { Id = 0, CompanyName = "" } // Should be pruned
                }
            };

            await service.UpdateAsync(updateProfile, "owner");
        }

        using (var context = GetDbContext(dbName))
        {
            var updated = await context.DriverProfiles.Include(p => p.EmploymentHistory).FirstAsync(p => p.Id == 1);
            Assert.Equal("Updated", updated.FirstName);
            Assert.Single(updated.EmploymentHistory);
            Assert.Equal("New Job", updated.EmploymentHistory.First().CompanyName);
        }
    }

    [Fact]
    public async Task DeleteAsync_RejectsUnauthorizedUser()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile { Id = 1, UserId = "owner" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(1, "hacker", false));
        }
    }

    [Fact]
    public async Task DeleteAsync_DeletesProfile_ForOwnerOrAdmin()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile { Id = 1, UserId = "owner" });
            context.DriverProfiles.Add(new DriverProfile { Id = 2, UserId = "owner" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context);
            
            // Delete as owner
            await service.DeleteAsync(1, "owner", false);
            
            // Delete as admin
            await service.DeleteAsync(2, "admin", true);
        }

        using (var context = GetDbContext(dbName))
        {
            Assert.Equal(0, await context.DriverProfiles.CountAsync());
        }
    }
}
