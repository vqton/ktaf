using AMS.Domain.Entities;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class ChartOfAccountsRepository : IChartOfAccountsRepository
{
    private readonly AMSDbContext _context;

    public ChartOfAccountsRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<ChartOfAccounts?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ChartOfAccounts.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<ChartOfAccounts?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Code == code, cancellationToken);
    }

    public async Task<ChartOfAccounts?> GetByAccountNumberAsync(int accountNumber, CancellationToken cancellationToken = default)
    {
        return await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
    }

    public async Task<IEnumerable<ChartOfAccounts>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _context.ChartOfAccounts
            .Where(a => a.ParentId == parentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChartOfAccounts>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ChartOfAccounts
            .Include(a => a.Children)
            .Where(a => a.ParentId == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChartOfAccounts>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ChartOfAccounts
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<ChartOfAccounts> Accounts, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ChartOfAccounts.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);

        var accounts = await query
            .OrderBy(a => a.AccountNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (accounts, totalCount);
    }

    public async Task AddAsync(ChartOfAccounts account, CancellationToken cancellationToken = default)
    {
        await _context.ChartOfAccounts.AddAsync(account, cancellationToken);
    }

    public async Task UpdateAsync(ChartOfAccounts account, CancellationToken cancellationToken = default)
    {
        _context.ChartOfAccounts.Update(account);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _context.ChartOfAccounts.FindAsync(new object[] { id }, cancellationToken);
        if (account != null)
        {
            account.IsDeleted = true;
            account.ModifiedAt = DateTime.UtcNow;
        }
    }
}
