namespace HrPlatform.Models;

/// <summary>
/// Generic pagination result model for paginated data
/// </summary>
/// <typeparam name="T">Type of items in the paginated result</typeparam>
public class PaginationResult<T>
{
    /// <summary>
    /// The paginated items for the current page
    /// </summary>
    public required List<T> Items { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public required int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public required int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public required int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}

