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

public class JobServiceTests
{
    private ApplicationDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllJobs_OrderedByPostedAtDesc()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "C1" });
            context.Jobs.AddRange(
                new Job { Id = 1, PostedAt = new DateTime(2022, 1, 1), CompanyId = 1, Title = "T" },
                new Job { Id = 2, PostedAt = new DateTime(2022, 1, 3), CompanyId = 1, Title = "T" },
                new Job { Id = 3, PostedAt = new DateTime(2022, 1, 2), CompanyId = 1, Title = "T" }
            );
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new JobService(context);
            var result = await service.GetAllAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(3, result[1].Id);
            Assert.Equal(1, result[2].Id);
        }
    }

    [Fact]
    public async Task GetPagedAsync_FiltersAndSortsCorrectly()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "C1" });
            context.Companies.Add(new Company { Id = 2, Name = "C2" });
            context.Jobs.AddRange(
                new Job { Id = 1, IsActive = true, PayRate = 50, RequiredCdlClass = CdlClass.A, PostedAt = new DateTime(2022, 1, 1), MinYearsExperience = 2, CompanyId = 1, Title = "T" },
                new Job { Id = 2, IsActive = false, PayRate = 60, RequiredCdlClass = CdlClass.B, PostedAt = new DateTime(2022, 1, 2), MinYearsExperience = 1, CompanyId = 1, Title = "T" },
                new Job { Id = 3, IsActive = true, PayRate = 70, RequiredCdlClass = null, PostedAt = new DateTime(2022, 1, 3), MinYearsExperience = 5, CompanyId = 2, Title = "T" }
            );
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new JobService(context);

            var pagedAll = await service.GetPagedAsync(1, 10, "all");
            Assert.Equal(3, pagedAll.TotalCount);

            var pagedActive = await service.GetPagedAsync(1, 10, "active");
            Assert.Equal(2, pagedActive.TotalCount);
            
            var pagedInactive = await service.GetPagedAsync(1, 10, "inactive");
            Assert.Equal(1, pagedInactive.TotalCount);
            
            var pagedCompany2 = await service.GetPagedAsync(1, 10, companyId: 2);
            Assert.Equal(1, pagedCompany2.TotalCount);
            Assert.Equal(3, pagedCompany2.Items[0].Id);
            
            var pagedCdlA = await service.GetPagedAsync(1, 10, cdlClass: CdlClass.A);
            Assert.Equal(2, pagedCdlA.TotalCount); // Job 1 (A) and Job 3 (null)
            
            var pagedMinPay = await service.GetPagedAsync(1, 10, minPay: 65);
            Assert.Equal(1, pagedMinPay.TotalCount); // Job 3
            
            var pagedSortByPay = await service.GetPagedAsync(1, 10, sortBy: "pay");
            Assert.Equal(3, pagedSortByPay.Items[0].Id);
            
            var driverProfile = new DriverProfile { UserId="1", FirstName="F", LastName="L", PhoneNumber="1", Email="e@e.com", License = new DriverLicense { Class = CdlClass.A, IssuingState="TX", LicenseNumber="1" }, YearsOfExperience = 3 };
            var pagedMatch = await service.GetPagedAsync(1, 10, matchProfileOnly: true, driverProfile: driverProfile);
            Assert.Equal(1, pagedMatch.TotalCount); // Job 1 matches Class A and MinYears=2. Job 2 requires B, Job 3 requires 5 years.
            Assert.Equal(1, pagedMatch.Items[0].Id);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsJob_WhenFound()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "C1" });
            context.Jobs.Add(new Job { Id = 1, CompanyId = 1, Title = "T" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new JobService(context);
            var result = await service.GetByIdAsync(1);
            Assert.NotNull(result);
            
            var notFound = await service.GetByIdAsync(99);
            Assert.Null(notFound);
        }
    }

    [Fact]
    public async Task CreateAsync_AddsJob()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            var service = new JobService(context);
            context.Companies.Add(new Company { Id = 1, Name = "C1" });
            var result = await service.CreateAsync(new Job { CompanyId = 1, IsActive = true, Title = "T" });
            Assert.NotEqual(0, result.Id);
        }
        
        using (var context = GetDbContext(dbName))
        {
            var count = await context.Jobs.CountAsync();
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task UpdateAsync_UpdatesJob()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "C1" });
            context.Jobs.Add(new Job { Id = 1, CompanyId = 1, PayRate = 10, Title = "T" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new JobService(context);
            var job = await context.Jobs.FindAsync(1);
            job!.PayRate = 20;
            await service.UpdateAsync(job);
        }

        using (var context = GetDbContext(dbName))
        {
            var updated = await context.Jobs.FindAsync(1);
            Assert.Equal(20, updated!.PayRate);
            
        }
    }

    [Fact]
    public async Task SetActiveAsync_UpdatesIsActive()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "C1" });
            context.Jobs.Add(new Job { Id = 1, CompanyId = 1, IsActive = false, Title = "T" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new JobService(context);
            await service.SetActiveAsync(1, true);
            
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.SetActiveAsync(99, true));
        }

        using (var context = GetDbContext(dbName))
        {
            var updated = await context.Jobs.FindAsync(1);
            Assert.True(updated!.IsActive);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesJob()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "C1" });
            context.Jobs.Add(new Job { Id = 1, CompanyId = 1, Title = "T" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new JobService(context);
            await service.DeleteAsync(1);
            await service.DeleteAsync(99);
        }

        using (var context = GetDbContext(dbName))
        {
            Assert.Equal(0, await context.Jobs.CountAsync());
        }
    }

    [Fact]
    public async Task GetCompaniesAsync_ReturnsCompanies()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "Z" });
            context.Companies.Add(new Company { Id = 2, Name = "A" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new JobService(context);
            var companies = await service.GetCompaniesAsync();
            Assert.Equal(2, companies.Count);
            Assert.Equal("A", companies[0].Name);
        }
    }
}
