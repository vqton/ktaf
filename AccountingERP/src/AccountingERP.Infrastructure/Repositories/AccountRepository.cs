using AccountingERP.Application.Interfaces;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.Enums;
using AccountingERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AccountingERP.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho Account
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly AccountingDbContext _context;

    public AccountRepository(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Code == code, cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetByTypeAsync(AccountType type, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.Type == type)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
