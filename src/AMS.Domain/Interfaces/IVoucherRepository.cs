using AMS.Domain.Entities;
using AMS.Domain.Enums;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for managing accounting voucher entities.
/// </summary>
public interface IVoucherRepository
{
    /// <summary>
    /// Retrieves a voucher by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the voucher.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The voucher if found; otherwise, null.</returns>
    Task<Voucher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a voucher by its unique identifier including all lines and attachments.
    /// </summary>
    /// <param name="id">The unique identifier of the voucher.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The voucher with lines and attachments if found; otherwise, null.</returns>
    Task<Voucher?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all vouchers for a specific fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of vouchers in the specified period.</returns>
    Task<IEnumerable<Voucher>> GetByPeriodAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all vouchers with a specific status.
    /// </summary>
    /// <param name="status">The voucher status to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of vouchers with the specified status.</returns>
    Task<IEnumerable<Voucher>> GetByStatusAsync(Enums.VoucherStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all vouchers.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the vouchers for the page and the total count.</returns>
    Task<(IEnumerable<Voucher> Vouchers, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of vouchers for a specific fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the vouchers for the page and the total count.</returns>
    Task<(IEnumerable<Voucher> Vouchers, int TotalCount)> GetByPeriodPagedAsync(Guid fiscalPeriodId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next sequence number for a voucher type in a fiscal period.
    /// </summary>
    /// <param name="fiscalPeriodId">The unique identifier of the fiscal period.</param>
    /// <param name="voucherType">The voucher type code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The next sequence number.</returns>
    Task<int> GetNextSequenceAsync(Guid fiscalPeriodId, string voucherType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new voucher to the repository.
    /// </summary>
    /// <param name="voucher">The voucher to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Voucher voucher, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing voucher in the repository.
    /// </summary>
    /// <param name="voucher">The voucher to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Voucher voucher, CancellationToken cancellationToken = default);
}
