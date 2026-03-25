using AMS.Domain.Entities;
using AMS.Domain.Enums;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for managing chart of accounts entities.
/// </summary>
public interface IChartOfAccountsRepository
{
    /// <summary>
    /// Retrieves an account by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the account.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    Task<ChartOfAccounts?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an account by its unique code.
    /// </summary>
    /// <param name="code">The unique code of the account.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    Task<ChartOfAccounts?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an account by its account number.
    /// </summary>
    /// <param name="accountNumber">The account number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    Task<ChartOfAccounts?> GetByAccountNumberAsync(int accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves child accounts for a given parent account.
    /// </summary>
    /// <param name="parentId">The parent account identifier (null for root accounts).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of child accounts.</returns>
    Task<IEnumerable<ChartOfAccounts>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the full account hierarchy.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of root accounts with their children loaded.</returns>
    Task<IEnumerable<ChartOfAccounts>> GetHierarchyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active accounts.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of active accounts.</returns>
    Task<IEnumerable<ChartOfAccounts>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all accounts.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the accounts for the page and the total count.</returns>
    Task<(IEnumerable<ChartOfAccounts> Accounts, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new account to the repository.
    /// </summary>
    /// <param name="account">The account to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(ChartOfAccounts account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing account in the repository.
    /// </summary>
    /// <param name="account">The account to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(ChartOfAccounts account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes an account by setting IsDeleted flag.
    /// </summary>
    /// <param name="id">The unique identifier of the account to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple accounts by their identifiers.
    /// </summary>
    /// <param name="ids">The collection of account identifiers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of accounts found.</returns>
    Task<IEnumerable<ChartOfAccounts>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}