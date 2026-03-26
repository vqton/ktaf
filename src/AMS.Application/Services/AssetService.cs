using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.Assets;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

/// <summary>
/// Service xử lý các nghiệp vụ liên quan đến tài sản cố định như tạo, cập nhật, tính khấu hao.
/// </summary>
public class AssetService : IAssetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFixedAssetRepository _repository;

    public AssetService(IUnitOfWork unitOfWork, IFixedAssetRepository repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<FixedAssetDto?> GetAssetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<FixedAssetDto?> GetAssetByCodeAsync(string assetCode, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByCodeAsync(assetCode, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<PagedResult<FixedAssetDto>> GetAllAssetsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _repository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return PagedResult<FixedAssetDto>.Create(items, totalCount, page, pageSize);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FixedAssetDto>> GetActiveAssetsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetActiveAssetsAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<FixedAssetDto>> CreateAssetAsync(CreateFixedAssetDto dto, CancellationToken cancellationToken = default)
    {
        var errors = Validate(dto);
        if (errors.Any())
            return ServiceResult<FixedAssetDto>.Failure(errors);

        var existingCode = await _repository.GetByCodeAsync(dto.AssetCode, cancellationToken);
        if (existingCode != null)
            return ServiceResult<FixedAssetDto>.Failure($"Mã tài sản '{dto.AssetCode}' đã tồn tại.");

        var residualValue = dto.ResidualValue ?? 0;
        var bookValue = dto.OriginalCost;

        var entity = new FixedAsset
        {
            Id = Guid.NewGuid(),
            AssetCode = dto.AssetCode,
            AssetName = dto.AssetName,
            AssetGroupId = dto.AssetGroupId,
            SerialNumber = dto.SerialNumber,
            Model = dto.Model,
            AccountId = dto.AccountId,
            OriginalCost = dto.OriginalCost,
            ResidualValue = residualValue,
            UsefulLifeMonths = dto.UsefulLifeMonths,
            AcquisitionDate = dto.AcquisitionDate,
            DepreciationStartDate = dto.DepreciationStartDate ?? dto.AcquisitionDate,
            DepreciationMethod = dto.DepreciationMethod ?? "STRAIGHT_LINE",
            AccumulatedDepreciation = 0,
            BookValue = bookValue,
            DepartmentCode = dto.DepartmentCode,
            Description = dto.Description,
            Status = AssetStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<FixedAssetDto>.Success(MapToDto(entity));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<FixedAssetDto>> UpdateAssetAsync(Guid id, CreateFixedAssetDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult<FixedAssetDto>.Failure("Không tìm thấy tài sản.");

        var errors = Validate(dto);
        if (errors.Any())
            return ServiceResult<FixedAssetDto>.Failure(errors);

        if (entity.AssetCode != dto.AssetCode)
        {
            var existingCode = await _repository.GetByCodeAsync(dto.AssetCode, cancellationToken);
            if (existingCode != null)
                return ServiceResult<FixedAssetDto>.Failure($"Mã tài sản '{dto.AssetCode}' đã tồn tại.");
        }

        entity.AssetCode = dto.AssetCode;
        entity.AssetName = dto.AssetName;
        entity.AssetGroupId = dto.AssetGroupId;
        entity.SerialNumber = dto.SerialNumber;
        entity.Model = dto.Model;
        entity.AccountId = dto.AccountId;
        entity.OriginalCost = dto.OriginalCost;
        entity.ResidualValue = dto.ResidualValue ?? 0;
        entity.UsefulLifeMonths = dto.UsefulLifeMonths;
        entity.AcquisitionDate = dto.AcquisitionDate;
        entity.DepreciationStartDate = dto.DepreciationStartDate ?? dto.AcquisitionDate;
        entity.DepreciationMethod = dto.DepreciationMethod ?? "STRAIGHT_LINE";
        entity.DepartmentCode = dto.DepartmentCode;
        entity.Description = dto.Description;
        entity.BookValue = dto.OriginalCost - (entity.AccumulatedDepreciation ?? 0);
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "system";

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<FixedAssetDto>.Success(MapToDto(entity));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<FixedAssetDto>> CalculateDepreciationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult<FixedAssetDto>.Failure("Không tìm thấy tài sản.");

        var result = CalculateStraightLineDepreciation(
            entity.OriginalCost,
            entity.ResidualValue ?? 0,
            entity.UsefulLifeMonths,
            entity.AccumulatedDepreciation ?? 0);

        entity.AccumulatedDepreciation = result.AccumulatedDepreciation;
        entity.BookValue = result.CurrentBookValue;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<FixedAssetDto>.Success(MapToDto(entity));
    }

    /// <inheritdoc />
    public async Task<ServiceResult> PostDepreciationAsync(Guid id, int year, int month, string voucherNo, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Không tìm thấy tài sản.");

        var schedule = await _repository.GetDepreciationScheduleAsync(id, year, month, cancellationToken);
        if (schedule == null)
            return ServiceResult.Failure("Không tìm thấy lịch trích khấu hao cho kỳ này.");

        if (schedule.IsPosted)
            return ServiceResult.Failure("Kỳ khấu hao này đã được ghi sổ.");

        schedule.IsPosted = true;
        schedule.VoucherId = Guid.NewGuid();

        entity.AccumulatedDepreciation = (entity.AccumulatedDepreciation ?? 0) + schedule.DepreciationAmount;
        entity.BookValue = entity.OriginalCost - entity.AccumulatedDepreciation;

        await _repository.UpdateDepreciationScheduleAsync(schedule, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<DepreciationResultDto> CalculateMonthlyDepreciationAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(assetId, cancellationToken);
        if (entity == null)
            throw new ArgumentException("Không tìm thấy tài sản.", nameof(assetId));

        var result = CalculateStraightLineDepreciation(
            entity.OriginalCost,
            entity.ResidualValue ?? 0,
            entity.UsefulLifeMonths,
            entity.AccumulatedDepreciation ?? 0);

        result.FixedAssetId = entity.Id;
        result.AssetCode = entity.AssetCode;
        result.AssetName = entity.AssetName;
        result.OriginalCost = entity.OriginalCost;
        result.ResidualValue = entity.ResidualValue ?? 0;
        result.UsefulLifeMonths = entity.UsefulLifeMonths;

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DepreciationResultDto>> GenerateDepreciationScheduleAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(assetId, cancellationToken);
        if (entity == null)
            throw new ArgumentException("Không tìm thấy tài sản.", nameof(assetId));

        var schedules = new List<DepreciationResultDto>();
        var startDate = entity.DepreciationStartDate ?? entity.AcquisitionDate;
        var residual = entity.ResidualValue ?? 0;
        var depreciableAmount = entity.OriginalCost - residual;
        var monthlyDepreciation = depreciableAmount / entity.UsefulLifeMonths;

        var currentAccumulated = entity.AccumulatedDepreciation ?? 0;

        for (int i = 0; i < entity.UsefulLifeMonths; i++)
        {
            var periodDate = startDate.AddMonths(i);
            var newAccumulated = currentAccumulated + monthlyDepreciation;

            schedules.Add(new DepreciationResultDto
            {
                FixedAssetId = entity.Id,
                AssetCode = entity.AssetCode,
                AssetName = entity.AssetName,
                OriginalCost = entity.OriginalCost,
                ResidualValue = residual,
                DepreciableAmount = depreciableAmount,
                UsefulLifeMonths = entity.UsefulLifeMonths,
                MonthlyDepreciation = monthlyDepreciation,
                AccumulatedDepreciation = newAccumulated,
                CurrentBookValue = entity.OriginalCost - newAccumulated,
                RemainingMonths = entity.UsefulLifeMonths - i - 1
            });
        }

        return schedules;
    }

    /// <summary>
    /// Kiểm tra dữ liệu đầu vào của tài sản.
    /// </summary>
    /// <param name="dto">Dữ liệu tài sản cần kiểm tra.</param>
    /// <returns>Danh sách lỗi (nếu có).</returns>
    private static List<string> Validate(CreateFixedAssetDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.AssetCode))
            errors.Add("Mã tài sản không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.AssetName))
            errors.Add("Tên tài sản không được để trống.");
        if (dto.OriginalCost <= 0)
            errors.Add("Nguyên giá phải lớn hơn 0.");
        if (dto.UsefulLifeMonths <= 0)
            errors.Add("Thời gian sử dụng hữu ích phải lớn hơn 0.");
        return errors;
    }

    /// <summary>
    /// Chuyển đổi entity FixedAsset sang DTO.
    /// </summary>
    /// <param name="entity">Entity cần chuyển đổi.</param>
    /// <returns>FixedAssetDto.</returns>
    private static FixedAssetDto MapToDto(FixedAsset entity) => new()
    {
        Id = entity.Id,
        AssetCode = entity.AssetCode,
        AssetName = entity.AssetName,
        AssetGroupId = entity.AssetGroupId,
        AssetGroupName = entity.AssetGroup?.Name,
        SerialNumber = entity.SerialNumber,
        Model = entity.Model,
        AccountId = entity.AccountId,
        OriginalCost = entity.OriginalCost,
        ResidualValue = entity.ResidualValue,
        UsefulLifeMonths = entity.UsefulLifeMonths,
        AcquisitionDate = entity.AcquisitionDate,
        DepreciationStartDate = entity.DepreciationStartDate,
        Status = entity.Status,
        DepreciationMethod = entity.DepreciationMethod,
        AccumulatedDepreciation = entity.AccumulatedDepreciation,
        BookValue = entity.BookValue,
        DepartmentCode = entity.DepartmentCode,
        Description = entity.Description
    };

    /// <summary>
    /// Tính khấu hao theo phương pháp đường thẳng.
    /// </summary>
    /// <param name="originalCost">Nguyên giá tài sản.</param>
    /// <param name="residualValue">Giá trị thanh lý.</param>
    /// <param name="usefulLifeMonths">Thời gian sử dụng hữu ích (tháng).</param>
    /// <param name="accumulatedDepreciation">Khấu hao lũy kế hiện tại.</param>
    /// <returns>Kết quả tính khấu hao.</returns>
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