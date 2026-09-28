using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Models;

/// <summary>
/// Extension methods for pagination
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Paginates an IEnumerable collection in memory.
    /// </summary>
    public static PaginationResult<T> Paginate<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var itemsList = source as IList<T> ?? source.ToList();
        var totalCount = itemsList.Count;
        var items = itemsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginationResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Asynchronously paginates an IQueryable collection at the database level using EF Core async methods.
    /// </summary>
    public static async Task<PaginationResult<T>> PaginateAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

