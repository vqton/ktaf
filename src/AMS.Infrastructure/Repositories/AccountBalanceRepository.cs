using AMS.Domain.Entities;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AccountBalance entities.
/// </summary>
public class AccountBalanceRepository : IAccountBalanceRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the AccountBalanceRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AccountBalanceRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<AccountBalance?> GetByPeriodAndAccountAsync(Guid fiscalPeriodId, Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountBalances
            .FirstOrDefaultAsync(b => b.FiscalPeriodId == fiscalPeriodId && b.AccountId == accountId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AccountBalance>> GetByPeriodAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountBalances
            .Where(b => b.FiscalPeriodId == fiscalPeriodId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AccountBalance>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountBalances
            .Where(b => b.AccountId == accountId)
            .OrderByDescending(b => b.FiscalPeriodId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AccountBalance?> GetLatestByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountBalances
            .Where(b => b.AccountId == accountId)
            .OrderByDescending(b => b.FiscalPeriodId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(AccountBalance balance, CancellationToken cancellationToken = default)
    {
        await _context.AccountBalances.AddAsync(balance, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<AccountBalance> balances, CancellationToken cancellationToken = default)
    {
        await _context.AccountBalances.AddRangeAsync(balances, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(AccountBalance balance, CancellationToken cancellationToken = default)
    {
        _context.AccountBalances.Update(balance);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task UpdateRangeAsync(IEnumerable<AccountBalance> balances, CancellationToken cancellationToken = default)
    {
        _context.AccountBalances.UpdateRange(balances);
        await Task.CompletedTask;
    }
}
