namespace AMS.Application.Common.Results;

/// <summary>
/// Represents a paginated result set for list queries.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// List of items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Creates a new paged result.
    /// </summary>
    /// <param name="items">List of items for the current page.</param>
    /// <param name="totalCount">Total number of items.</param>
    /// <param name="page">Current page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A new PagedResult instance.</returns>
    public static PagedResult<T> Create(List<T> items, int totalCount, int page, int pageSize) =>
        new()
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
}