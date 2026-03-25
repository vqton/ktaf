using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Bank entities.
/// </summary>
public class BankRepository : IBankRepository
{
    private readonly AMSDbContext _context;

    public BankRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<Bank?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Banks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Bank?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Banks.FirstOrDefaultAsync(b => b.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Bank>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Banks.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Bank> Banks, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Banks.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(b => b.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(Bank bank, CancellationToken cancellationToken = default)
    {
        await _context.Banks.AddAsync(bank, cancellationToken);
    }

    public async Task UpdateAsync(Bank bank, CancellationToken cancellationToken = default)
    {
        _context.Banks.Update(bank);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Repository implementation for BankAccount entities.
/// </summary>
public class BankAccountRepository : IBankAccountRepository
{
    private readonly AMSDbContext _context;

    public BankAccountRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts
            .Include(b => b.Bank)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts
            .Include(b => b.Bank)
            .FirstOrDefaultAsync(b => b.AccountNumber == accountNumber, cancellationToken);
    }

    public async Task<IEnumerable<BankAccount>> GetByBankIdAsync(Guid bankId, CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts.Where(b => b.BankId == bankId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BankAccount>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts
            .Include(b => b.Bank)
            .Where(b => b.IsActive)
            .OrderBy(b => b.AccountName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<BankAccount> Accounts, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.BankAccounts.Include(b => b.Bank).AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(b => b.AccountName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<BankAccount?> GetPrimaryAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts
            .Include(b => b.Bank)
            .FirstOrDefaultAsync(b => b.IsPrimary && b.IsActive, cancellationToken);
    }

    public async Task AddAsync(BankAccount account, CancellationToken cancellationToken = default)
    {
        await _context.BankAccounts.AddAsync(account, cancellationToken);
    }

    public async Task UpdateAsync(BankAccount account, CancellationToken cancellationToken = default)
    {
        _context.BankAccounts.Update(account);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _context.BankAccounts.FindAsync(new object[] { id }, cancellationToken);
        if (account != null)
        {
            account.IsActive = false;
            account.ModifiedAt = DateTime.UtcNow;
        }
    }
}

/// <summary>
/// Repository implementation for BankTransaction entities.
/// </summary>
public class BankTransactionRepository : IBankTransactionRepository
{
    private readonly AMSDbContext _context;

    public BankTransactionRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<BankTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BankTransactions
            .Include(t => t.BankAccount)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<BankTransaction>> GetByBankAccountIdAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        return await _context.BankTransactions
            .Where(t => t.BankAccountId == bankAccountId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BankTransaction>> GetByDateRangeAsync(Guid bankAccountId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.BankTransactions
            .Where(t => t.BankAccountId == bankAccountId && t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BankTransaction>> GetUnreconciledAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        return await _context.BankTransactions
            .Where(t => t.BankAccountId == bankAccountId && !t.IsReconciled && t.Status == BankTransactionStatus.Completed)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BankTransaction>> GetByVoucherIdAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        return await _context.BankTransactions.Where(t => t.VoucherId == voucherId).ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<BankTransaction> Transactions, int TotalCount)> GetAllPagedAsync(Guid? bankAccountId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.BankTransactions.Include(t => t.BankAccount).AsQueryable();
        if (bankAccountId.HasValue)
            query = query.Where(t => t.BankAccountId == bankAccountId.Value);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(t => t.TransactionDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(BankTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.BankTransactions.AddAsync(transaction, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<BankTransaction> transactions, CancellationToken cancellationToken = default)
    {
        await _context.BankTransactions.AddRangeAsync(transactions, cancellationToken);
    }

    public async Task UpdateAsync(BankTransaction transaction, CancellationToken cancellationToken = default)
    {
        _context.BankTransactions.Update(transaction);
        await Task.CompletedTask;
    }
}
