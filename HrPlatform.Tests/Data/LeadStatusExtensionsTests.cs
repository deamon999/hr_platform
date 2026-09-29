using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HrPlatform.Tests.Data;

public class LeadStatusExtensionsTests
{
    [Theory]
    [InlineData(LeadStatus.Hired, true)]
    [InlineData(LeadStatus.NotInterested, true)]
    [InlineData(LeadStatus.Rejected, true)]
    [InlineData(LeadStatus.NotQualified, true)]
    [InlineData(LeadStatus.New, false)]
    [InlineData(LeadStatus.ReadyToStart, false)]
    [InlineData(LeadStatus.AttemptContact, false)]
    [InlineData(LeadStatus.NeedsToBeContacted, false)]
    [InlineData(LeadStatus.ReadyIn2Weeks, false)]
    [InlineData(LeadStatus.Converted, false)]
    [InlineData(LeadStatus.Contacted, false)]
    [InlineData(LeadStatus.Invited, false)]
    public void IsTerminal_ReturnsExpectedResult(LeadStatus status, bool expected)
    {
        Assert.Equal(expected, status.IsTerminal());
    }

    [Theory]
    [InlineData(LeadStatus.New, true)]
    [InlineData(LeadStatus.Hired, false)]
    [InlineData(LeadStatus.NotInterested, false)]
    [InlineData(LeadStatus.Rejected, false)]
    [InlineData(LeadStatus.NotQualified, false)]
    [InlineData(LeadStatus.ReadyToStart, true)]
    [InlineData(LeadStatus.AttemptContact, true)]
    [InlineData(LeadStatus.NeedsToBeContacted, true)]
    [InlineData(LeadStatus.ReadyIn2Weeks, true)]
    [InlineData(LeadStatus.Converted, true)]
    [InlineData(LeadStatus.Contacted, true)]
    [InlineData(LeadStatus.Invited, true)]
    public void IsActionable_ReturnsExpectedResult(LeadStatus status, bool expected)
    {
        Assert.Equal(expected, status.IsActionable());
    }

    [Theory]
    [InlineData(LeadStatus.Converted, true)]
    [InlineData(LeadStatus.Contacted, true)]
    [InlineData(LeadStatus.Invited, true)]
    [InlineData(LeadStatus.New, false)]
    [InlineData(LeadStatus.Hired, false)]
    [InlineData(LeadStatus.ReadyToStart, false)]
    [InlineData(LeadStatus.Rejected, false)]
    public void IsLegacy_ReturnsExpectedResult(LeadStatus status, bool expected)
    {
        Assert.Equal(expected, status.IsLegacy());
    }

    [Fact]
    public async Task LeadExpressions_IsActionable_FiltersCorrectlyInDatabaseQuery()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        db.Leads.AddRange(
            new Lead { FirstName = "New", LastName = "User", Status = LeadStatus.New },
            new Lead { FirstName = "Actionable1", LastName = "User", Status = LeadStatus.ReadyToStart },
            new Lead { FirstName = "Actionable2", LastName = "User", Status = LeadStatus.AttemptContact },
            new Lead { FirstName = "Closed1", LastName = "User", Status = LeadStatus.Hired },
            new Lead { FirstName = "Closed2", LastName = "User", Status = LeadStatus.NotQualified }
        );
        await db.SaveChangesAsync();

        var actionableLeads = await db.Leads
            .Where(LeadExpressions.IsActionable)
            .ToListAsync();

        Assert.Equal(3, actionableLeads.Count);
        Assert.All(actionableLeads, l => Assert.True(l.Status.IsActionable()));
    }

    [Fact]
    public async Task LeadExpressions_IsNotTerminal_FiltersCorrectlyInDatabaseQuery()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        db.Leads.AddRange(
            new Lead { FirstName = "New", LastName = "User", Status = LeadStatus.New },
            new Lead { FirstName = "Actionable", LastName = "User", Status = LeadStatus.ReadyToStart },
            new Lead { FirstName = "Closed1", LastName = "User", Status = LeadStatus.Hired },
            new Lead { FirstName = "Closed2", LastName = "User", Status = LeadStatus.Rejected }
        );
        await db.SaveChangesAsync();

        var nonTerminalLeads = await db.Leads
            .Where(LeadExpressions.IsNotTerminal)
            .ToListAsync();

        Assert.Equal(2, nonTerminalLeads.Count);
        Assert.Contains(nonTerminalLeads, l => l.Status == LeadStatus.New);
        Assert.Contains(nonTerminalLeads, l => l.Status == LeadStatus.ReadyToStart);
    }
}
