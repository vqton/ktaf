using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.Assets;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class AssetService : IAssetService
{
    private readonly IUnitOfWork _unitOfWork;

    public AssetService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<FixedAssetDto?> GetAssetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<FixedAssetDto?> GetAssetByCodeAsync(string assetCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<FixedAssetDto>> GetAllAssetsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<FixedAssetDto>> GetActiveAssetsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<FixedAssetDto>> CreateAssetAsync(CreateFixedAssetDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<FixedAssetDto>> UpdateAssetAsync(Guid id, CreateFixedAssetDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<FixedAssetDto>> CalculateDepreciationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult> PostDepreciationAsync(Guid id, int year, int month, string voucherNo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DepreciationResultDto> CalculateMonthlyDepreciationAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DepreciationResultDto>> GenerateDepreciationScheduleAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public DepreciationResultDto CalculateStraightLineDepreciation(decimal originalCost, decimal residualValue, int usefulLifeMonths, decimal accumulatedDepreciation)
    {
        var residual = residualValue > 0 ? residualValue : 0;
        var depreciableAmount = originalCost - residual;
        var monthlyDepreciation = usefulLifeMonths > 0 ? depreciableAmount / usefulLifeMonths : 0;
        var currentBookValue = originalCost - accumulatedDepreciation;
        var remainingMonths = Math.Max(0, usefulLifeMonths - (int)(accumulatedDepreciation / (monthlyDepreciation > 0 ? monthlyDepreciation : 1)));

        return new DepreciationResultDto
        {
            DepreciableAmount = depreciableAmount,
            MonthlyDepreciation = monthlyDepreciation,
            AccumulatedDepreciation = accumulatedDepreciation,
            CurrentBookValue = currentBookValue,
            RemainingMonths = remainingMonths
        };
    }
}
