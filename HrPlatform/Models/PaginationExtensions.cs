
namespace HrPlatform.Models;

/// <summary>
/// Extension methods for pagination
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Paginates an IEnumerable collection
    /// </summary>
    public static PaginationResult<T> Paginate<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var totalCount = source.Count();
        var items = source
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
    /// Asynchronously paginates an IQueryable collection
    /// </summary>
    public static async Task<PaginationResult<T>> PaginateAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
        return await Task.FromResult(source.AsEnumerable().Paginate(pageNumber, pageSize));
    }
}

