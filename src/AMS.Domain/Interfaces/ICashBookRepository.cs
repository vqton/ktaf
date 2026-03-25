using AMS.Domain.Entities;
using AMS.Domain.Entities.DM;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for CashBook entities.
/// </summary>
public interface ICashBookRepository
{
    /// <summary>
    /// Gets a cash book by ID.
    /// </summary>
    Task<CashBook?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cash book by code.
    /// </summary>
    Task<CashBook?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active cash books.
    /// </summary>
    Task<IEnumerable<CashBook>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all cash books with pagination.
    /// </summary>
    Task<(IEnumerable<CashBook> CashBooks, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the main cash book.
    /// </summary>
    Task<CashBook?> GetMainAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new cash book.
    /// </summary>
    Task AddAsync(CashBook cashBook, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a cash book.
    /// </summary>
    Task UpdateAsync(CashBook cashBook, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a cash book.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for CashBookEntry entities.
/// </summary>
public interface ICashBookEntryRepository
{
    /// <summary>
    /// Gets a cash book entry by ID.
    /// </summary>
    Task<CashBookEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entries by cash book ID.
    /// </summary>
    Task<IEnumerable<CashBookEntry>> GetByCashBookIdAsync(Guid cashBookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entries by fiscal period.
    /// </summary>
    Task<IEnumerable<CashBookEntry>> GetByFiscalPeriodAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entries by date range.
    /// </summary>
    Task<IEnumerable<CashBookEntry>> GetByDateRangeAsync(Guid cashBookId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entries by voucher ID.
    /// </summary>
    Task<IEnumerable<CashBookEntry>> GetByVoucherIdAsync(Guid voucherId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entries with pagination.
    /// </summary>
    Task<(IEnumerable<CashBookEntry> Entries, int TotalCount)> GetAllPagedAsync(Guid? cashBookId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new cash book entry.
    /// </summary>
    Task AddAsync(CashBookEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple cash book entries.
    /// </summary>
    Task AddRangeAsync(IEnumerable<CashBookEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a cash book entry.
    /// </summary>
    Task UpdateAsync(CashBookEntry entry, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for BankReconciliation entities.
/// </summary>
public interface IBankReconciliationRepository
{
    /// <summary>
    /// Gets a bank reconciliation by ID.
    /// </summary>
    Task<BankReconciliation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a bank reconciliation by bank account and period.
    /// </summary>
    Task<BankReconciliation?> GetByAccountAndPeriodAsync(Guid bankAccountId, int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets reconciliations by bank account.
    /// </summary>
    Task<IEnumerable<BankReconciliation>> GetByBankAccountIdAsync(Guid bankAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reconciliations with pagination.
    /// </summary>
    Task<(IEnumerable<BankReconciliation> Reconciliations, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new bank reconciliation.
    /// </summary>
    Task AddAsync(BankReconciliation reconciliation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a bank reconciliation.
    /// </summary>
    Task UpdateAsync(BankReconciliation reconciliation, CancellationToken cancellationToken = default);
}
