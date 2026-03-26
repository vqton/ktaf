using AMS.Domain.Entities.DM;
using AMS.Domain.Entities.Inventory;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AMSDbContext _context;

    public ProductRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.ProductCode == code, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products.Where(p => !p.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products.Where(p => !p.IsDeleted && p.IsActive).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product != null)
        {
            product.IsDeleted = true;
            product.ModifiedAt = DateTime.UtcNow;
        }
    }
}

public class WarehouseRepository : IWarehouseRepository
{
    private readonly AMSDbContext _context;

    public WarehouseRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses.Where(w => !w.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses.Where(w => !w.IsDeleted && w.IsActive).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        await _context.Warehouses.AddAsync(warehouse, cancellationToken);
    }

    public async Task UpdateAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        _context.Warehouses.Update(warehouse);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses.FindAsync(new object[] { id }, cancellationToken);
        if (warehouse != null)
        {
            warehouse.IsDeleted = true;
            warehouse.ModifiedAt = DateTime.UtcNow;
        }
    }
}