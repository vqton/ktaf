using AMS.Domain.Entities;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AMSDbContext _context;

    public CustomerRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Customer?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<Customer?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.TaxCode == taxCode, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Customer> Customers, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);

        var customers = await query
            .OrderBy(c => c.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (customers, totalCount);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(customer);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync(new object[] { id }, cancellationToken);
        if (customer != null)
        {
            customer.IsDeleted = true;
            customer.ModifiedAt = DateTime.UtcNow;
        }
    }
}
