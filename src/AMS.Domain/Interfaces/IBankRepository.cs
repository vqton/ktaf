using AMS.Domain.Entities.DM;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for Bank entities.
/// </summary>
public interface IBankRepository
{
    /// <summary>
    /// Gets a bank by ID.
    /// </summary>
    Task<Bank?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a bank by code.
    /// </summary>
    Task<Bank?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active banks.
    /// </summary>
    Task<IEnumerable<Bank>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all banks with pagination.
    /// </summary>
    Task<(IEnumerable<Bank> Banks, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new bank.
    /// </summary>
    Task AddAsync(Bank bank, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a bank.
    /// </summary>
    Task UpdateAsync(Bank bank, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for BankAccount entities.
/// </summary>
public interface IBankAccountRepository
{
    /// <summary>
    /// Gets a bank account by ID.
    /// </summary>
    Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a bank account by account number.
    /// </summary>
    Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all bank accounts for a bank.
    /// </summary>
    Task<IEnumerable<BankAccount>> GetByBankIdAsync(Guid bankId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active bank accounts.
    /// </summary>
    Task<IEnumerable<BankAccount>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all bank accounts with pagination.
    /// </summary>
    Task<(IEnumerable<BankAccount> Accounts, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the primary bank account.
    /// </summary>
    Task<BankAccount?> GetPrimaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new bank account.
    /// </summary>
    Task AddAsync(BankAccount account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a bank account.
    /// </summary>
    Task UpdateAsync(BankAccount account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bank account.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for BankTransaction entities.
/// </summary>
public interface IBankTransactionRepository
{
    /// <summary>
    /// Gets a bank transaction by ID.
    /// </summary>
    Task<BankTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by bank account ID.
    /// </summary>
    Task<IEnumerable<BankTransaction>> GetByBankAccountIdAsync(Guid bankAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by date range.
    /// </summary>
    Task<IEnumerable<BankTransaction>> GetByDateRangeAsync(Guid bankAccountId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unreconciled transactions.
    /// </summary>
    Task<IEnumerable<BankTransaction>> GetUnreconciledAsync(Guid bankAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by voucher ID.
    /// </summary>
    Task<IEnumerable<BankTransaction>> GetByVoucherIdAsync(Guid voucherId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transactions with pagination.
    /// </summary>
    Task<(IEnumerable<BankTransaction> Transactions, int TotalCount)> GetAllPagedAsync(Guid? bankAccountId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new bank transaction.
    /// </summary>
    Task AddAsync(BankTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple bank transactions.
    /// </summary>
    Task AddRangeAsync(IEnumerable<BankTransaction> transactions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a bank transaction.
    /// </summary>
    Task UpdateAsync(BankTransaction transaction, CancellationToken cancellationToken = default);
}
