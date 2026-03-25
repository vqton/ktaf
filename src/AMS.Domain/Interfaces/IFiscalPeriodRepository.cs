using AMS.Domain.Entities;
using AMS.Domain.Enums;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for managing fiscal period entities.
/// </summary>
public interface IFiscalPeriodRepository
{
    /// <summary>
    /// Retrieves a fiscal period by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the fiscal period.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fiscal period if found; otherwise, null.</returns>
    Task<FiscalPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a fiscal period by its year and month.
    /// </summary>
    /// <param name="year">The calendar year.</param>
    /// <param name="month">The month (1-12).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fiscal period if found; otherwise, null.</returns>
    Task<FiscalPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all fiscal periods with a specific status.
    /// </summary>
    /// <param name="status">The fiscal period status to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of fiscal periods with the specified status.</returns>
    Task<IEnumerable<FiscalPeriod>> GetByStatusAsync(FiscalPeriodStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all open fiscal periods.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of open fiscal periods.</returns>
    Task<IEnumerable<FiscalPeriod>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all fiscal periods.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the fiscal periods for the page and the total count.</returns>
    Task<(IEnumerable<FiscalPeriod> Periods, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current open fiscal period for a given date.
    /// </summary>
    /// <param name="date">The date to find the period for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fiscal period if found; otherwise, null.</returns>
    Task<FiscalPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new fiscal period to the repository.
    /// </summary>
    /// <param name="period">The fiscal period to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(FiscalPeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing fiscal period in the repository.
    /// </summary>
    /// <param name="period">The fiscal period to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(FiscalPeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a fiscal period by setting IsDeleted flag (use with caution).
    /// </summary>
    /// <param name="id">The unique identifier of the fiscal period to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}