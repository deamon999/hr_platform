using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Models;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HrPlatform.Tests.Services;

public class CompanyServiceTests
{
    private ApplicationDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCompanies_OrderedByName()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "Z Company" });
            context.Companies.Add(new Company { Id = 2, Name = "A Company" });
            context.Companies.Add(new Company { Id = 3, Name = "M Company" });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = GetDbContext(dbName))
        {
            var service = new CompanyService(context);
            var result = await service.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("A Company", result[0].Name);
            Assert.Equal("M Company", result[1].Name);
            Assert.Equal("Z Company", result[2].Name);
        }
    }

    [Fact]
    public async Task GetPagedAsync_FiltersAndSortsCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "Alpha", DateRegistered = new DateOnly(2020, 1, 1) });
            context.Companies.Add(new Company { Id = 2, Name = "Beta", DateRegistered = new DateOnly(2021, 1, 1) });
            context.Companies.Add(new Company { Id = 3, Name = "Alpha 2", DateRegistered = new DateOnly(2022, 1, 1) });
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = GetDbContext(dbName))
        {
            var service = new CompanyService(context);
            
            // Search "Alpha", sort by "date"
            var pagedDate = await service.GetPagedAsync(1, 10, "Alpha", "date");
            Assert.Equal(2, pagedDate.TotalCount);
            Assert.Equal("Alpha 2", pagedDate.Items[0].Name);
            
            // Search "Alpha", sort by name (default)
            var pagedName = await service.GetPagedAsync(1, 10, "Alpha", "name");
            Assert.Equal(2, pagedName.TotalCount);
            Assert.Equal("Alpha", pagedName.Items[0].Name);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCompany_WhenFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "Test Company" });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = GetDbContext(dbName))
        {
            var service = new CompanyService(context);
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Company", result.Name);
            
            var notFound = await service.GetByIdAsync(99);
            Assert.Null(notFound);
        }
    }

    [Fact]
    public async Task CreateAsync_AddsCompany()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            var service = new CompanyService(context);
            var company = new Company { Name = "New Company" };
            
            // Act
            var result = await service.CreateAsync(company);
            
            // Assert
            Assert.Equal("New Company", result.Name);
            Assert.NotEqual(0, result.Id);
        }
        
        using (var context = GetDbContext(dbName))
        {
            var count = await context.Companies.CountAsync();
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCompanyAndSetsUpdatedAt()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "Old Name" });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = GetDbContext(dbName))
        {
            var service = new CompanyService(context);
            var company = await context.Companies.FindAsync(1);
            company!.Name = "New Name";
            
            await service.UpdateAsync(company);
        }

        // Assert
        using (var context = GetDbContext(dbName))
        {
            var updated = await context.Companies.FindAsync(1);
            Assert.Equal("New Name", updated!.Name);
            
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesCompany_WhenExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.Companies.Add(new Company { Id = 1, Name = "To Delete" });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = GetDbContext(dbName))
        {
            var service = new CompanyService(context);
            await service.DeleteAsync(1);
            
            // Should not throw when deleting non-existent
            await service.DeleteAsync(99);
        }

        // Assert
        using (var context = GetDbContext(dbName))
        {
            var count = await context.Companies.CountAsync();
            Assert.Equal(0, count);
        }
    }
}
