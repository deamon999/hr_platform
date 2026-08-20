using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HrPlatform.Tests.Services
{
    public class LeadServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly LeadService _service;

        public LeadServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _service = new LeadService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetLeadsPagedAsync_FiltersByCompanyId()
        {
            // Arrange
            _db.Leads.AddRange(
                new Lead { Id = 1, FirstName = "A", LastName = "B", CompanyId = 1, Status = LeadStatus.New, CreatedAt = DateTime.UtcNow },
                new Lead { Id = 2, FirstName = "C", LastName = "D", CompanyId = 2, Status = LeadStatus.New, CreatedAt = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetLeadsPagedAsync(1, 10, companyId: 1);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.Items.First().Id);
        }

        [Fact]
        public async Task GetLeadsPagedAsync_FiltersBySearchTerm()
        {
            // Arrange
            _db.Leads.AddRange(
                new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", CompanyId = 1, Status = LeadStatus.New, CreatedAt = DateTime.UtcNow },
                new Lead { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", CompanyId = 1, Status = LeadStatus.New, CreatedAt = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetLeadsPagedAsync(1, 10, searchTerm: "JOHN");

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.Items.First().Id);
        }

        [Fact]
        public async Task GetLeadsPagedAsync_FiltersByStatusAndActionableOnly()
        {
            // Arrange
            _db.Leads.AddRange(
                new Lead { Id = 1, FirstName = "A", LastName = "B", CompanyId = 1, Status = LeadStatus.AttemptContact, CreatedAt = DateTime.UtcNow },
                new Lead { Id = 2, FirstName = "C", LastName = "D", CompanyId = 1, Status = LeadStatus.Hired, CreatedAt = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            // Act
            var actionableResult = await _service.GetLeadsPagedAsync(1, 10, actionableOnly: true);
            var statusResult = await _service.GetLeadsPagedAsync(1, 10, status: LeadStatus.Hired);

            // Assert
            Assert.Single(actionableResult.Items);
            Assert.Equal(1, actionableResult.Items.First().Id);

            Assert.Single(statusResult.Items);
            Assert.Equal(2, statusResult.Items.First().Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsLeadWithCompany()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Test Company" };
            _db.Companies.Add(company);
            var lead = new Lead { Id = 1, FirstName = "Test", LastName = "User", CompanyId = 1, Status = LeadStatus.New };
            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.Company);
            Assert.Equal("Test Company", result.Company.Name);
        }

        [Fact]
        public async Task CreateAsync_AddsLead()
        {
            // Arrange
            var lead = new Lead { FirstName = "New", LastName = "Lead", Status = LeadStatus.New };

            // Act
            var result = await _service.CreateAsync(lead);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equal(1, await _db.Leads.CountAsync());
        }

        [Fact]
        public async Task UpdateAsync_UpdatesLead()
        {
            // Arrange
            var lead = new Lead { Id = 1, FirstName = "Original", LastName = "Lead", Status = LeadStatus.New };
            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();

            // Act
            lead.FirstName = "Updated";
            await _service.UpdateAsync(lead);

            // Assert
            var updatedLead = await _db.Leads.FindAsync(1);
            Assert.Equal("Updated", updatedLead!.FirstName);
        }

        [Fact]
        public async Task DeleteAsync_DeletesLead()
        {
            // Arrange
            var lead = new Lead { Id = 1, FirstName = "Original", LastName = "Lead", Status = LeadStatus.New };
            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(1);

            // Assert
            Assert.Equal(0, await _db.Leads.CountAsync());
        }

        [Fact]
        public async Task GetActionableRemindersAsync_ReturnsValidReminders()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _db.Leads.AddRange(
                new Lead { Id = 1, FirstName = "A", LastName = "B", ReminderDate = now.AddDays(-1), Status = LeadStatus.Contacted, CompanyId = 1 },
                new Lead { Id = 2, FirstName = "C", LastName = "D", ReminderDate = now.AddDays(1), Status = LeadStatus.Contacted, CompanyId = 1 },
                new Lead { Id = 3, FirstName = "E", LastName = "F", ReminderDate = now.AddDays(-1), Status = LeadStatus.Hired, CompanyId = 1 }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetActionableRemindersAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task GetUsersWithLeadsAsync_ReturnsDistinctUsers()
        {
            // Arrange
            var user1 = new ApplicationUser { Id = "u1", Email = "a@test.com" };
            var user2 = new ApplicationUser { Id = "u2", Email = "b@test.com" };
            _db.Users.AddRange(user1, user2);

            _db.Leads.AddRange(
                new Lead { Id = 1, FirstName = "A", LastName = "B", AddedByUserId = "u1", CompanyId = 1, Status = LeadStatus.New },
                new Lead { Id = 2, FirstName = "C", LastName = "D", AddedByUserId = "u1", CompanyId = 1, Status = LeadStatus.New },
                new Lead { Id = 3, FirstName = "E", LastName = "F", AddedByUserId = "u2", CompanyId = 2, Status = LeadStatus.New }
            );
            await _db.SaveChangesAsync();

            // Act
            var result1 = await _service.GetUsersWithLeadsAsync(1);
            var result2 = await _service.GetUsersWithLeadsAsync(null);

            // Assert
            Assert.Single(result1);
            Assert.Equal("u1", result1[0].Id);
            
            Assert.Equal(2, result2.Count);
        }

        [Fact]
        public async Task GetRegisteredEmailsAsync_ReturnsMatchingEmails()
        {
            // Arrange
            _db.Users.Add(new ApplicationUser { Id = "u1", Email = "user@test.com" });
            _db.DriverProfiles.Add(new DriverProfile { Id = 1, Email = "driver@test.com", FirstName = "Test", LastName = "User", PhoneNumber = "1234567890", UserId = "u1" });
            await _db.SaveChangesAsync();

            var emailsToCheck = new List<string> { "user@test.com", "driver@test.com", "unknown@test.com" };

            // Act
            var result = await _service.GetRegisteredEmailsAsync(emailsToCheck);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("user@test.com", result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("driver@test.com", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetLeadsPagedAsync_FiltersGlobalOnly()
        {
            // Arrange
            _db.Leads.AddRange(
                new Lead { Id = 1, FirstName = "A", LastName = "B", CompanyId = 1, Status = LeadStatus.New },
                new Lead { Id = 2, FirstName = "C", LastName = "D", CompanyId = null, Status = LeadStatus.New }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetLeadsPagedAsync(1, 10, globalOnly: true);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(2, result.Items.First().Id);
        }

        [Fact]
        public async Task IsDuplicateLeadAsync_DetectsDuplicates()
        {
            // Arrange
            _db.Leads.AddRange(
                new Lead { Id = 1, FirstName = "A", LastName = "B", Email = "test@example.com", Phone = "123", CompanyId = 1, Status = LeadStatus.New },
                new Lead { Id = 2, FirstName = "C", LastName = "D", Email = "global@example.com", Phone = "456", CompanyId = null, Status = LeadStatus.New }
            );
            await _db.SaveChangesAsync();

            // Assert
            // Same company, same email (case insensitive)
            Assert.True(await _service.IsDuplicateLeadAsync(1, "TEST@example.com", null));
            // Same company, same phone
            Assert.True(await _service.IsDuplicateLeadAsync(1, null, "123"));
            // Global lead, same email
            Assert.True(await _service.IsDuplicateLeadAsync(null, "global@example.com", null));
            // Different company, shouldn't conflict
            Assert.False(await _service.IsDuplicateLeadAsync(2, "test@example.com", null));
            // Same company but excluded Id (for updates)
            Assert.False(await _service.IsDuplicateLeadAsync(1, "test@example.com", null, 1));
        }
    }
}
