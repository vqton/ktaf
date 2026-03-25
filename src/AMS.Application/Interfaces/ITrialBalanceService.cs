using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

/// <summary>
/// Service interface for trial balance and month-end closing operations.
/// </summary>
public interface ITrialBalanceService
{
    /// <summary>
    /// Gets the trial balance for a specific fiscal period with opening balances.
    /// </summary>
    Task<IEnumerable<LedgerSummaryDto>> GetTrialBalanceAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates account balances based on ledger entries for a period.
    /// </summary>
    Task<ServiceResult> UpdateAccountBalancesAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Carries forward opening balances from previous period.
    /// </summary>
    Task<ServiceResult> CarryForwardOpeningBalancesAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);
}
