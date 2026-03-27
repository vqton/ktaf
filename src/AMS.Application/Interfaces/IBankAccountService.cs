using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

/// <summary>
/// Service interface for managing bank accounts.
/// </summary>
public interface IBankAccountService
{
    /// <summary>
    /// Gets a bank account by its ID.
    /// </summary>
    /// <param name="id">The bank account ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bank account DTO if found; otherwise, null.</returns>
    Task<BankAccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a bank account by account number.
    /// </summary>
    /// <param name="accountNumber">The account number to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bank account DTO if found; otherwise, null.</returns>
    Task<BankAccountDto?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all bank accounts for a specific bank.
    /// </summary>
    /// <param name="bankId">The bank ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of bank account DTOs.</returns>
    Task<IEnumerable<BankAccountDto>> GetByBankIdAsync(Guid bankId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active bank accounts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of all active bank account DTOs.</returns>
    Task<IEnumerable<BankAccountDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all bank accounts with pagination.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result of bank account DTOs.</returns>
    Task<(IEnumerable<BankAccountDto> Accounts, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the primary bank account.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The primary bank account DTO if found; otherwise, null.</returns>
    Task<BankAccountDto?> GetPrimaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new bank account.
    /// </summary>
    /// <param name="dto">The bank account data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the created bank account DTO.</returns>
    Task<ServiceResult<BankAccountDto>> CreateAsync(CreateBankAccountDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing bank account.
    /// </summary>
    /// <param name="dto">The bank account data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated bank account DTO.</returns>
    Task<ServiceResult<BankAccountDto>> UpdateAsync(UpdateBankAccountDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bank account.
    /// </summary>
    /// <param name="id">The bank account ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result.</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a bank account as the primary account.
    /// </summary>
    /// <param name="id">The bank account ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result.</returns>
    Task<ServiceResult> SetPrimaryAsync(Guid id, CancellationToken cancellationToken = default);
}