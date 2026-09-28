using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HrPlatform.Tests.Models;

public class PaginationExtensionsTests
{
    [Fact]
    public void Paginate_InMemory_PaginatesCorrectly()
    {
        var source = Enumerable.Range(1, 25).ToList();

        var resultPage1 = source.Paginate(1, 10);
        Assert.Equal(10, resultPage1.Items.Count);
        Assert.Equal(1, resultPage1.Items[0]);
        Assert.Equal(10, resultPage1.Items[9]);
        Assert.Equal(25, resultPage1.TotalCount);
        Assert.Equal(3, resultPage1.TotalPages);
        Assert.True(resultPage1.HasNextPage);
        Assert.False(resultPage1.HasPreviousPage);

        var resultPage3 = source.Paginate(3, 10);
        Assert.Equal(5, resultPage3.Items.Count);
        Assert.Equal(21, resultPage3.Items[0]);
        Assert.Equal(25, resultPage3.Items[4]);
        Assert.False(resultPage3.HasNextPage);
        Assert.True(resultPage3.HasPreviousPage);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-5, 10, 1, 10)]
    [InlineData(1, 0, 1, 10)]
    [InlineData(1, -20, 1, 10)]
    [InlineData(-1, -1, 1, 10)]
    public void Paginate_InvalidPageParameters_UsesSafeDefaults(
        int inputPageNumber, int inputPageSize, int expectedPageNumber, int expectedPageSize)
    {
        var source = Enumerable.Range(1, 30).ToList();

        var result = source.Paginate(inputPageNumber, inputPageSize);

        Assert.Equal(expectedPageNumber, result.PageNumber);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(10, result.Items.Count);
    }

    [Fact]
    public async Task PaginateAsync_DatabaseQuery_PaginatesCorrectly()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        for (int i = 1; i <= 25; i++)
        {
            db.Companies.Add(new Company { Name = $"Company {i}", ContactEmail = $"c{i}@example.com" });
        }
        await db.SaveChangesAsync();

        var result = await db.Companies.AsNoTracking().OrderBy(c => c.Id).PaginateAsync(2, 10);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task PaginateAsync_InvalidParameters_UsesSafeDefaults()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        for (int i = 1; i <= 15; i++)
        {
            db.Companies.Add(new Company { Name = $"Company {i}", ContactEmail = $"c{i}@example.com" });
        }
        await db.SaveChangesAsync();

        var result = await db.Companies.AsNoTracking().PaginateAsync(0, -5);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(10, result.Items.Count);
    }

    [Fact]
    public async Task PaginateAsync_WithCancellationToken_HonorsToken()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        db.Companies.Add(new Company { Name = "Test", ContactEmail = "test@example.com" });
        await db.SaveChangesAsync();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await db.Companies.PaginateAsync(1, 10, cts.Token);
        });
    }
}
