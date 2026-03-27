using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

/// <summary>
/// Service implementation for managing warehouses.
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWarehouseRepository _warehouseRepository;

    /// <summary>
    /// Initializes a new instance of the WarehouseService class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance.</param>
    /// <param name="warehouseRepository">The warehouse repository instance.</param>
    public WarehouseService(IUnitOfWork unitOfWork, IWarehouseRepository warehouseRepository)
    {
        _unitOfWork = unitOfWork;
        _warehouseRepository = warehouseRepository;
    }

    /// <inheritdoc />
    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<WarehouseDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _warehouseRepository.GetByCodeAsync(code, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WarehouseDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _warehouseRepository.GetAllActiveAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<WarehouseDto> Warehouses, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _warehouseRepository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WarehouseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreate(dto);
        if (errors.Any())
            return ServiceResult<WarehouseDto>.Failure(errors);

        var existing = await _warehouseRepository.GetByCodeAsync(dto.WarehouseCode, cancellationToken);
        if (existing != null)
            return ServiceResult<WarehouseDto>.Failure($"Kho với mã {dto.WarehouseCode} đã tồn tại.");

        var entity = new Warehouse
        {
            Id = Guid.NewGuid(),
            WarehouseCode = dto.WarehouseCode,
            WarehouseName = dto.WarehouseName,
            Address = dto.Address,
            Manager = dto.Manager,
            PricingMethod = dto.PricingMethod,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _warehouseRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<WarehouseDto>.Success(MapToDto(entity));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WarehouseDto>> UpdateAsync(UpdateWarehouseDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateUpdate(dto);
        if (errors.Any())
            return ServiceResult<WarehouseDto>.Failure(errors);

        var entity = await _warehouseRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (entity == null)
            return ServiceResult<WarehouseDto>.Failure("Kho không tồn tại.");

        var existing = await _warehouseRepository.GetByCodeAsync(dto.WarehouseCode, cancellationToken);
        if (existing != null && existing.Id != dto.Id)
            return ServiceResult<WarehouseDto>.Failure($"Kho với mã {dto.WarehouseCode} đã tồn tại.");

        entity.WarehouseCode = dto.WarehouseCode;
        entity.WarehouseName = dto.WarehouseName;
        entity.Address = dto.Address;
        entity.Manager = dto.Manager;
        entity.PricingMethod = dto.PricingMethod;
        entity.IsActive = dto.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "system";

        await _warehouseRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<WarehouseDto>.Success(MapToDto(entity));
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Kho không tồn tại.");

        // Check if warehouse has inventory
        var hasInventory = await _warehouseRepository.GetByIdAsync(id, cancellationToken) is Warehouse warehouse &&
                           warehouse.InventoryBalances != null && warehouse.InventoryBalances.Any();

        if (hasInventory)
            return ServiceResult.Failure("Không thể xóa kho còn hàng tồn kho.");

        await _warehouseRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private static List<string> ValidateCreate(CreateWarehouseDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.WarehouseCode))
            errors.Add("Mã kho là bắt buộc.");

        if (string.IsNullOrWhiteSpace(dto.WarehouseName))
            errors.Add("Tên kho là bắt buộc.");

        return errors;
    }

    private static List<string> ValidateUpdate(UpdateWarehouseDto dto)
    {
        var errors = new List<string>();

        if (dto.Id == Guid.Empty)
            errors.Add("ID là bắt buộc.");

        if (string.IsNullOrWhiteSpace(dto.WarehouseCode))
            errors.Add("Mã kho là bắt buộc.");

        if (string.IsNullOrWhiteSpace(dto.WarehouseName))
            errors.Add("Tên kho là bắt buộc.");

        return errors;
    }

    private static WarehouseDto MapToDto(Warehouse entity) => new()
    {
        Id = entity.Id,
        WarehouseCode = entity.WarehouseCode,
        WarehouseName = entity.WarehouseName,
        Address = entity.Address,
        Manager = entity.Manager,
        PricingMethod = entity.PricingMethod,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        ModifiedAt = entity.ModifiedAt,
        ModifiedBy = entity.ModifiedBy
    };
}