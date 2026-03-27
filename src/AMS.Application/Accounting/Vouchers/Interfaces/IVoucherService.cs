using AMS.Application.Common.Results;
using AMS.Application.Accounting.Vouchers.DTOs;

namespace AMS.Application.Accounting.Vouchers.Interfaces;

/// <summary>
/// Service interface for managing accounting vouchers.
/// </summary>
public interface IVoucherService
{
    /// <summary>
    /// Gets a voucher by its ID.
    /// </summary>
    /// <param name="id">The voucher ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The voucher DTO if found; otherwise, null.</returns>
    Task<VoucherDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all vouchers with pagination.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result of voucher DTOs.</returns>
    Task<PagedResult<VoucherDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vouchers for a specific fiscal period with pagination.
    /// </summary>
    /// <param name="fiscalPeriodId">The fiscal period ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result of voucher DTOs.</returns>
    Task<PagedResult<VoucherDto>> GetByPeriodAsync(Guid fiscalPeriodId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new voucher.
    /// </summary>
    /// <param name="voucher">The voucher data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the created voucher DTO.</returns>
    Task<ServiceResult<VoucherDto>> CreateAsync(CreateVoucherDto voucher, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a voucher for approval.
    /// </summary>
    /// <param name="id">The voucher ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated voucher DTO.</returns>
    Task<ServiceResult<VoucherDto>> SubmitAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a pending voucher.
    /// </summary>
    /// <param name="id">The voucher ID.</param>
    /// <param name="approverId">Username of the approver.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated voucher DTO.</returns>
    Task<ServiceResult<VoucherDto>> ApproveAsync(Guid id, string approverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a pending voucher.
    /// </summary>
    /// <param name="id">The voucher ID.</param>
    /// <param name="reason">Reason for rejection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated voucher DTO.</returns>
    Task<ServiceResult<VoucherDto>> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts an approved voucher to the general ledger.
    /// </summary>
    /// <param name="id">The voucher ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated voucher DTO.</returns>
    Task<ServiceResult<VoucherDto>> PostAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses a posted voucher.
    /// </summary>
    /// <param name="id">The voucher ID.</param>
    /// <param name="reversedById">Username of the user reversing the voucher.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated voucher DTO.</returns>
    Task<ServiceResult<VoucherDto>> ReverseAsync(Guid id, string reversedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing voucher.
    /// </summary>
    /// <param name="id">The voucher ID.</param>
    /// <param name="voucher">The voucher data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated voucher DTO.</returns>
    Task<ServiceResult<VoucherDto>> UpdateAsync(Guid id, VoucherDto voucher, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a voucher.
    /// </summary>
    /// <param name="id">The voucher ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result.</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}