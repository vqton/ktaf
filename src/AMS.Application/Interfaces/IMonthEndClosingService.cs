using AMS.Application.Common.Results;

namespace AMS.Application.Interfaces;

/// <summary>
/// Service interface for month-end closing operations.
/// </summary>
public interface IMonthEndClosingService
{
    /// <summary>
    /// Executes the complete month-end closing process (8 steps).
    /// </summary>
    /// <param name="fiscalPeriodId">The fiscal period ID to close.</param>
    /// <param name="closedBy">Username of the user closing the period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result indicating success or failure.</returns>
    Task<ServiceResult> ExecuteMonthEndClosingAsync(Guid fiscalPeriodId, string closedBy, CancellationToken cancellationToken = default);
}
