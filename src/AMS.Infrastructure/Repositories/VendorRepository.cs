using AMS.Domain.Entities;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly AMSDbContext _context;

    public VendorRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Vendor?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors.FirstOrDefaultAsync(v => v.Code == code, cancellationToken);
    }

    public async Task<Vendor?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors.FirstOrDefaultAsync(v => v.TaxCode == taxCode, cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .Where(v => v.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Vendor> Vendors, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Vendors.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);

        var vendors = await query
            .OrderBy(v => v.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (vendors, totalCount);
    }

    public async Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        await _context.Vendors.AddAsync(vendor, cancellationToken);
    }

    public async Task UpdateAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        _context.Vendors.Update(vendor);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vendor = await _context.Vendors.FindAsync(new object[] { id }, cancellationToken);
        if (vendor != null)
        {
            vendor.IsDeleted = true;
            vendor.ModifiedAt = DateTime.UtcNow;
        }
    }
}
