using AMS.Domain.Entities;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for managing ledger entry entities.
/// </summary>
public interface ILedgerRepository
{
    /// <summary>
    /// Retrieves a ledger entry by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ledger entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ledger entry if found; otherwise, null.</returns>
    Task<LedgerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all ledger entries for a specific fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of ledger entries in the specified period.</returns>
    Task<IEnumerable<LedgerEntry>> GetByFiscalPeriodAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all ledger entries for a specific account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of ledger entries for the specified account.</returns>
    Task<IEnumerable<LedgerEntry>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all ledger entries for a specific voucher.
    /// </summary>
    /// <param name="voucherId">The unique identifier of the voucher.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of ledger entries for the specified voucher.</returns>
    Task<IEnumerable<LedgerEntry>> GetByVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of ledger entries for a specific fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the ledger entries for the page and the total count.</returns>
    Task<(IEnumerable<LedgerEntry> Entries, int TotalCount)> GetByFiscalPeriodPagedAsync(Guid fiscalPeriodId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all ledger entries.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the ledger entries for the page and the total count.</returns>
    Task<(IEnumerable<LedgerEntry> Entries, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ledger summary (aggregated balances) for a specific fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of ledger summaries for the period.</returns>
    Task<IEnumerable<LedgerSummary>> GetSummaryAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new ledger entry to the repository.
    /// </summary>
    /// <param name="entry">The ledger entry to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple ledger entries in a batch operation.
    /// </summary>
    /// <param name="entries">The collection of ledger entries to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddRangeAsync(IEnumerable<LedgerEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing ledger entry in the repository (for corrections).
    /// </summary>
    /// <param name="entry">The ledger entry to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(LedgerEntry entry, CancellationToken cancellationToken = default);
}