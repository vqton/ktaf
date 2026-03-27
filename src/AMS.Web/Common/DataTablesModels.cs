using System.Linq.Expressions;

namespace AMS.Web.Common;

/// <summary>
/// Request model for DataTables server-side processing.
/// </summary>
public class DataTablesRequest
{
    /// <summary>
    /// Gets or sets the draw counter for request/response matching.
    /// </summary>
    public int Draw { get; set; }

    /// <summary>
    /// Gets or sets the starting record number.
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Gets or sets the number of records to display.
    /// </summary>
    public int Length { get; set; } = 10;

    /// <summary>
    /// Gets or sets the search value.
    /// </summary>
    public Search? Search { get; set; }

    /// <summary>
    /// Gets or sets the column ordering.
    /// </summary>
    public List<ColumnOrder> Order { get; set; } = new();

    /// <summary>
    /// Gets or sets the columns.
    /// </summary>
    public List<Column> Columns { get; set; } = new();
}

/// <summary>
/// Search configuration.
/// </summary>
public class Search
{
    /// <summary>
    /// Gets or sets the search value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether regex is used.
    /// </summary>
    public bool Regex { get; set; }
}

/// <summary>
/// Column ordering configuration.
/// </summary>
public class ColumnOrder
{
    /// <summary>
    /// Gets or sets the column index.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public string Dir { get; set; } = "asc";
}

/// <summary>
/// Column definition.
/// </summary>
public class Column
{
    /// <summary>
    /// Gets or sets the column data source.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the column is searchable.
    /// </summary>
    public bool Searchable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the column is orderable.
    /// </summary>
    public bool Orderable { get; set; } = true;

    /// <summary>
    /// Gets or sets the individual column search value.
    /// </summary>
    public Search? Search { get; set; }
}

/// <summary>
/// Response model for DataTables server-side processing.
/// </summary>
public class DataTablesResponse<T>
{
    /// <summary>
    /// Gets or sets the draw counter for request/response matching.
    /// </summary>
    public int Draw { get; set; }

    /// <summary>
    /// Gets or sets the total number of records (before filtering).
    /// </summary>
    public int RecordsTotal { get; set; }

    /// <summary>
    /// Gets or sets the number of records after filtering.
    /// </summary>
    public int RecordsFiltered { get; set; }

    /// <summary>
    /// Gets or sets the data to display.
    /// </summary>
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Gets or sets any additional error message.
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Helper class for building DataTables queries.
/// </summary>
public static class DataTablesHelper
{
    /// <summary>
    /// Processes a DataTables request and returns the response.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="data">The source data as a list.</param>
    /// <param name="request">The DataTables request.</param>
    /// <param name="searchPredicate">Optional predicate for search functionality.</param>
    /// <param name="orderBy">Optional order by function.</param>
    /// <returns>The DataTables response.</returns>
    public static DataTablesResponse<T> Process<T>(
        List<T> data,
        DataTablesRequest request,
        Func<T, string, bool>? searchPredicate = null,
        Func<T, IComparable>? orderBy = null)
    {
        var recordsTotal = data.Count;
        var filteredData = data;

        // Apply search
        var searchValue = request.Search?.Value;
        if (!string.IsNullOrWhiteSpace(searchValue) && searchPredicate != null)
        {
            filteredData = filteredData.Where(x => searchPredicate(x, searchValue)).ToList();
        }

        var recordsFiltered = filteredData.Count;

        // Apply ordering
        if (request.Order != null && request.Order.Any() && orderBy != null)
        {
            var order = request.Order.First();
            filteredData = order.Dir == "desc"
                ? filteredData.OrderByDescending(orderBy).ToList()
                : filteredData.OrderBy(orderBy).ToList();
        }

        // Apply paging
        if (request.Start >= 0 && request.Length > 0)
        {
            filteredData = filteredData.Skip(request.Start).Take(request.Length).ToList();
        }

        return new DataTablesResponse<T>
        {
            Draw = request.Draw,
            RecordsTotal = recordsTotal,
            RecordsFiltered = recordsFiltered,
            Data = filteredData
        };
    }

    /// <summary>
    /// Processes a DataTables request with async support.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="data">The source data as a list.</param>
    /// <param name="request">The DataTables request.</param>
    /// <param name="searchPredicate">Optional predicate for search functionality.</param>
    /// <param name="orderBy">Optional order by function.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The DataTables response.</returns>
    public static Task<DataTablesResponse<T>> ProcessAsync<T>(
        List<T> data,
        DataTablesRequest request,
        Func<T, string, bool>? searchPredicate = null,
        Func<T, IComparable>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var result = Process(data, request, searchPredicate, orderBy);
        return Task.FromResult(result);
    }
}
