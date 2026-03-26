using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Domain.Enums;

namespace AMS.Application.Interfaces;

public interface IInventoryService
{
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByCodeAsync(string productCode, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetAllProductsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProductDto>> UpdateProductAsync(Guid id, CreateProductDto dto, CancellationToken cancellationToken = default);

    Task<InventoryTransactionDto?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<InventoryTransactionDto>> GetTransactionsByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<InventoryTransactionDto>> GetTransactionsByWarehouseAsync(Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ServiceResult<InventoryTransactionDto>> CreateTransactionAsync(CreateInventoryTransactionDto dto, CancellationToken cancellationToken = default);

    Task<IEnumerable<InventoryBalanceDto>> GetInventoryBalancesAsync(Guid? warehouseId = null, CancellationToken cancellationToken = default);
    Task<InventoryBalanceDto?> GetProductBalanceAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);

    Task<IEnumerable<InventoryReportDto>> GetInventoryReportAsync(Guid warehouseId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
