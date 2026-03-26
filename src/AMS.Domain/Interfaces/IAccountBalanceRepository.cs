using AMS.Domain.Entities;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for managing account balance entities.
/// </summary>
public interface IAccountBalanceRepository
{
    /// <summary>
    /// Gets the account balance for a specific period and account.
    /// </summary>
    Task<AccountBalance?> GetByPeriodAndAccountAsync(Guid fiscalPeriodId, Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all account balances for a specific fiscal period.
    /// </summary>
    Task<IEnumerable<AccountBalance>> GetByPeriodAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all account balances for a specific account across all periods.
    /// </summary>
    Task<IEnumerable<AccountBalance>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest balance for an account (from the most recent open/closed period).
    /// </summary>
    Task<AccountBalance?> GetLatestByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all account balances.
    /// </summary>
    Task<IEnumerable<AccountBalance>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new account balance.
    /// </summary>
    Task AddAsync(AccountBalance balance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple account balances.
    /// </summary>
    Task AddRangeAsync(IEnumerable<AccountBalance> balances, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing account balance.
    /// </summary>
    Task UpdateAsync(AccountBalance balance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple account balances.
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<AccountBalance> balances, CancellationToken cancellationToken = default);
}
