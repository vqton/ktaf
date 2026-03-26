using AMS.Domain.Entities.Inventory;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for inventory-related entities (Products, Transactions, Balances).
/// </summary>
public class InventoryRepository : IInventoryRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the InventoryRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public InventoryRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Product?> GetProductByCodeAsync(string productCode, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.ProductCode == productCode, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.Where(p => !p.IsDeleted);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.ProductCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.ProductCode)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<InventoryTransaction?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<InventoryTransaction> Transactions, int TotalCount)> GetByProductPagedAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryTransactions.Where(t => t.ProductId == productId && !t.IsDeleted);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<InventoryTransaction> Transactions, int TotalCount)> GetByWarehousePagedAsync(Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryTransactions.Where(t => t.WarehouseId == warehouseId && !t.IsDeleted);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task AddTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.InventoryTransactions.AddAsync(transaction, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryBalance>> GetBalancesAsync(Guid? warehouseId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryBalances
            .Include(b => b.Product)
            .Where(b => !b.IsDeleted);
        
        if (warehouseId.HasValue)
            query = query.Where(b => b.WarehouseId == warehouseId.Value);

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InventoryBalance?> GetProductBalanceAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryBalances
            .FirstOrDefaultAsync(b => b.ProductId == productId && b.WarehouseId == warehouseId && !b.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateBalanceAsync(InventoryBalance balance, CancellationToken cancellationToken = default)
    {
        _context.InventoryBalances.Update(balance);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task AddBalanceAsync(InventoryBalance balance, CancellationToken cancellationToken = default)
    {
        await _context.InventoryBalances.AddAsync(balance, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByDateRangeAsync(DateTime fromDate, DateTime toDate, Guid? warehouseId = null, Guid? productId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryTransactions
            .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate && !t.IsDeleted);

        if (warehouseId.HasValue)
            query = query.Where(t => t.WarehouseId == warehouseId.Value);
        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId.Value);

        return await query.OrderBy(t => t.TransactionDate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryBalance>> GetAllBalancesAsync(Guid? warehouseId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryBalances.Where(b => !b.IsDeleted && b.Quantity > 0);

        if (warehouseId.HasValue)
            query = query.Where(b => b.WarehouseId == warehouseId.Value);

        return await query.ToListAsync(cancellationToken);
    }
}
