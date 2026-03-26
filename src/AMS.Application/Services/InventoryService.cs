using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.Inventory;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _inventoryRepository.GetProductByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToProductDto(entity);
    }

    public async Task<ProductDto?> GetProductByCodeAsync(string productCode, CancellationToken cancellationToken = default)
    {
        var entity = await _inventoryRepository.GetProductByCodeAsync(productCode, cancellationToken);
        return entity == null ? null : MapToProductDto(entity);
    }

    public async Task<PagedResult<ProductDto>> GetAllProductsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _inventoryRepository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToProductDto).ToList();
        return PagedResult<ProductDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _inventoryRepository.GetActiveProductsAsync(cancellationToken);
        return entities.Select(MapToProductDto);
    }

    public async Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateProduct(dto);
        if (errors.Any())
            return ServiceResult<ProductDto>.Failure(errors);

        var existing = await _inventoryRepository.GetProductByCodeAsync(dto.ProductCode, cancellationToken);
        if (existing != null)
            return ServiceResult<ProductDto>.Failure($"Mã sản phẩm '{dto.ProductCode}' đã tồn tại.");

        var entity = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = dto.ProductCode,
            ProductName = dto.ProductName,
            ProductNameEn = dto.ProductNameEn,
            ProductGroupId = dto.ProductGroupId,
            Type = dto.Type,
            UnitOfMeasure = dto.UnitOfMeasure,
            UnitPrice = dto.UnitPrice,
            CurrencyCode = dto.CurrencyCode ?? "VND",
            VatRate = dto.VatRate,
            IsExciseTaxItem = dto.IsExciseTaxItem,
            IsActive = true,
            TaxCode = dto.TaxCode,
            WarehouseId = dto.WarehouseId,
            Specification = dto.Specification,
            Origin = dto.Origin,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        };

        await _inventoryRepository.AddProductAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductDto>.Success(MapToProductDto(entity));
    }

    public async Task<ServiceResult<ProductDto>> UpdateProductAsync(Guid id, CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _inventoryRepository.GetProductByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult<ProductDto>.Failure("Sản phẩm không tồn tại.");

        entity.ProductName = dto.ProductName;
        entity.ProductNameEn = dto.ProductNameEn;
        entity.ProductGroupId = dto.ProductGroupId;
        entity.Type = dto.Type;
        entity.UnitOfMeasure = dto.UnitOfMeasure;
        entity.UnitPrice = dto.UnitPrice;
        entity.CurrencyCode = dto.CurrencyCode ?? "VND";
        entity.VatRate = dto.VatRate;
        entity.IsExciseTaxItem = dto.IsExciseTaxItem;
        entity.TaxCode = dto.TaxCode;
        entity.WarehouseId = dto.WarehouseId;
        entity.Specification = dto.Specification;
        entity.Origin = dto.Origin;
        entity.ModifiedAt = DateTime.UtcNow;

        await _inventoryRepository.UpdateProductAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductDto>.Success(MapToProductDto(entity));
    }

    public async Task<InventoryTransactionDto?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _inventoryRepository.GetTransactionByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToTransactionDto(entity);
    }

    public async Task<PagedResult<InventoryTransactionDto>> GetTransactionsByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _inventoryRepository.GetByProductPagedAsync(productId, page, pageSize, cancellationToken);
        var items = entities.Select(MapToTransactionDto).ToList();
        return PagedResult<InventoryTransactionDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<InventoryTransactionDto>> GetTransactionsByWarehouseAsync(Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _inventoryRepository.GetByWarehousePagedAsync(warehouseId, page, pageSize, cancellationToken);
        var items = entities.Select(MapToTransactionDto).ToList();
        return PagedResult<InventoryTransactionDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<ServiceResult<InventoryTransactionDto>> CreateTransactionAsync(CreateInventoryTransactionDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateTransaction(dto);
        if (errors.Any())
            return ServiceResult<InventoryTransactionDto>.Failure(errors);

        var entity = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            VoucherId = dto.VoucherId,
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            TransactionType = dto.TransactionType,
            TransactionDate = dto.TransactionDate,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            TotalAmount = dto.Quantity * dto.UnitCost,
            LotNumber = dto.LotNumber,
            ExpiryDate = dto.ExpiryDate,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        };

        await _inventoryRepository.AddTransactionAsync(entity, cancellationToken);
        await UpdateInventoryBalanceAsync(dto.ProductId, dto.WarehouseId, dto.TransactionType, dto.Quantity, dto.UnitCost, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<InventoryTransactionDto>.Success(MapToTransactionDto(entity));
    }

    public async Task<IEnumerable<InventoryBalanceDto>> GetInventoryBalancesAsync(Guid? warehouseId = null, CancellationToken cancellationToken = default)
    {
        var balances = await _inventoryRepository.GetBalancesAsync(warehouseId, cancellationToken);
        return balances.Select(MapToBalanceDto);
    }

    public async Task<InventoryBalanceDto?> GetProductBalanceAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var balance = await _inventoryRepository.GetProductBalanceAsync(productId, warehouseId, cancellationToken);
        return balance == null ? null : MapToBalanceDto(balance);
    }

    public async Task<IEnumerable<InventoryReportDto>> GetInventoryReportAsync(Guid warehouseId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var balances = await _inventoryRepository.GetBalancesAsync(warehouseId, cancellationToken);

        var report = new List<InventoryReportDto>();
        foreach (var balance in balances)
        {
            var product = await _inventoryRepository.GetProductByIdAsync(balance.ProductId, cancellationToken);
            report.Add(new InventoryReportDto
            {
                ProductId = balance.ProductId,
                ProductCode = product?.ProductCode ?? "",
                ProductName = product?.ProductName ?? "",
                OpeningQuantity = balance.Quantity,
                OpeningValue = balance.Quantity * balance.UnitCost,
                ClosingQuantity = balance.Quantity,
                ClosingValue = balance.Quantity * balance.UnitCost
            });
        }

        return report;
    }

    private async Task UpdateInventoryBalanceAsync(Guid productId, Guid warehouseId, InventoryTransactionType transactionType, decimal quantity, decimal unitCost, CancellationToken cancellationToken)
    {
        var balance = await _inventoryRepository.GetProductBalanceAsync(productId, warehouseId, cancellationToken);

        if (balance == null)
        {
            balance = new InventoryBalance
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = transactionType == InventoryTransactionType.PurchaseIn ? quantity : -quantity,
                UnitCost = unitCost,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _inventoryRepository.AddBalanceAsync(balance, cancellationToken);
        }
        else
        {
            if (transactionType == InventoryTransactionType.PurchaseIn || transactionType == InventoryTransactionType.ReturnIn)
            {
                var totalValue = balance.Quantity * balance.UnitCost + quantity * unitCost;
                var totalQty = balance.Quantity + quantity;
                balance.UnitCost = totalQty > 0 ? totalValue / totalQty : unitCost;
                balance.Quantity = totalQty;
            }
            else
            {
                balance.Quantity -= quantity;
            }
            await _inventoryRepository.UpdateBalanceAsync(balance, cancellationToken);
        }
    }

    private static List<string> ValidateProduct(CreateProductDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.ProductCode))
            errors.Add("Mã sản phẩm không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.ProductName))
            errors.Add("Tên sản phẩm không được để trống.");
        return errors;
    }

    private static List<string> ValidateTransaction(CreateInventoryTransactionDto dto)
    {
        var errors = new List<string>();
        if (dto.Quantity <= 0)
            errors.Add("Số lượng phải lớn hơn 0.");
        if (dto.UnitCost < 0)
            errors.Add("Đơn giá không được âm.");
        return errors;
    }

    private static ProductDto MapToProductDto(Product entity) => new()
    {
        Id = entity.Id,
        ProductCode = entity.ProductCode,
        ProductName = entity.ProductName,
        ProductNameEn = entity.ProductNameEn,
        ProductGroupId = entity.ProductGroupId,
        Type = entity.Type,
        UnitOfMeasure = entity.UnitOfMeasure,
        UnitPrice = entity.UnitPrice,
        CurrencyCode = entity.CurrencyCode,
        VatRate = entity.VatRate,
        IsExciseTaxItem = entity.IsExciseTaxItem,
        IsActive = entity.IsActive,
        TaxCode = entity.TaxCode,
        WarehouseId = entity.WarehouseId,
        Specification = entity.Specification,
        Origin = entity.Origin
    };

    private static InventoryTransactionDto MapToTransactionDto(InventoryTransaction entity) => new()
    {
        Id = entity.Id,
        VoucherId = entity.VoucherId,
        ProductId = entity.ProductId,
        WarehouseId = entity.WarehouseId,
        TransactionType = entity.TransactionType,
        TransactionDate = entity.TransactionDate,
        Quantity = entity.Quantity,
        UnitCost = entity.UnitCost,
        TotalAmount = entity.TotalAmount,
        LotNumber = entity.LotNumber,
        ExpiryDate = entity.ExpiryDate,
        Description = entity.Description
    };

    private static InventoryBalanceDto MapToBalanceDto(InventoryBalance entity) => new()
    {
        ProductId = entity.ProductId,
        WarehouseId = entity.WarehouseId,
        Quantity = entity.Quantity,
        UnitCost = entity.UnitCost,
        TotalValue = entity.Quantity * entity.UnitCost
    };
}
