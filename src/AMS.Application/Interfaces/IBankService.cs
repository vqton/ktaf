using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

/// <summary>
/// Service interface for managing banks.
/// </summary>
public interface IBankService
{
    /// <summary>
    /// Gets a bank by its ID.
    /// </summary>
    /// <param name="id">The bank ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bank DTO if found; otherwise, null.</returns>
    Task<BankDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a bank by its code.
    /// </summary>
    /// <param name="code">The bank code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bank DTO if found; otherwise, null.</returns>
    Task<BankDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active banks.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of all active bank DTOs.</returns>
    Task<IEnumerable<BankDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all banks with pagination.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result of bank DTOs.</returns>
    Task<(IEnumerable<BankDto> Banks, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new bank.
    /// </summary>
    /// <param name="dto">The bank data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the created bank DTO.</returns>
    Task<ServiceResult<BankDto>> CreateAsync(CreateBankDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing bank.
    /// </summary>
    /// <param name="dto">The bank data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated bank DTO.</returns>
    Task<ServiceResult<BankDto>> UpdateAsync(UpdateBankDto dto, CancellationToken cancellationToken = default);
}