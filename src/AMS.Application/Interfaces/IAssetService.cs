using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IAssetService
{
    Task<FixedAssetDto?> GetAssetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FixedAssetDto?> GetAssetByCodeAsync(string assetCode, CancellationToken cancellationToken = default);
    Task<PagedResult<FixedAssetDto>> GetAllAssetsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<FixedAssetDto>> GetActiveAssetsAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<FixedAssetDto>> CreateAssetAsync(CreateFixedAssetDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<FixedAssetDto>> UpdateAssetAsync(Guid id, CreateFixedAssetDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<FixedAssetDto>> CalculateDepreciationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult> PostDepreciationAsync(Guid id, int year, int month, string voucherNo, CancellationToken cancellationToken = default);

    Task<DepreciationResultDto> CalculateMonthlyDepreciationAsync(Guid assetId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DepreciationResultDto>> GenerateDepreciationScheduleAsync(Guid assetId, CancellationToken cancellationToken = default);
}
