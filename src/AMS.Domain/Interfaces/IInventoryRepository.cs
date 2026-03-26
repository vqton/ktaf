using AMS.Domain.Entities.Inventory;

namespace AMS.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetProductByCodeAsync(string productCode, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task AddProductAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default);
    
    Task<InventoryTransaction?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<InventoryTransaction> Transactions, int TotalCount)> GetByProductPagedAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IEnumerable<InventoryTransaction> Transactions, int TotalCount)> GetByWarehousePagedAsync(Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<InventoryBalance>> GetBalancesAsync(Guid? warehouseId = null, CancellationToken cancellationToken = default);
    Task<InventoryBalance?> GetProductBalanceAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task UpdateBalanceAsync(InventoryBalance balance, CancellationToken cancellationToken = default);
    Task AddBalanceAsync(InventoryBalance balance, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryTransaction>> GetTransactionsByDateRangeAsync(DateTime fromDate, DateTime toDate, Guid? warehouseId = null, Guid? productId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryBalance>> GetAllBalancesAsync(Guid? warehouseId = null, CancellationToken cancellationToken = default);
}
