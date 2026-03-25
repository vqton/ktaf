using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface ILedgerService
{
    Task<LedgerEntryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<LedgerEntryDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<LedgerEntryDto>> GetByPeriodAsync(Guid fiscalPeriodId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<LedgerEntryDto>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LedgerEntryDto>> GetByVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LedgerSummaryDto>> GetSummaryAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default);
    Task<ServiceResult<LedgerEntryDto>> CreateAsync(CreateLedgerEntryDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateFromVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default);
}