using AMS.Domain.Entities;
using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for CashBook entities.
/// </summary>
public class CashBookRepository : ICashBookRepository
{
    private readonly AMSDbContext _context;

    public CashBookRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<CashBook?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CashBooks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<CashBook?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.CashBooks.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<CashBook>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashBooks.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<CashBook> CashBooks, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.CashBooks.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(c => c.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<CashBook?> GetMainAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashBooks.FirstOrDefaultAsync(c => c.IsMain && c.IsActive, cancellationToken);
    }

    public async Task AddAsync(CashBook cashBook, CancellationToken cancellationToken = default)
    {
        await _context.CashBooks.AddAsync(cashBook, cancellationToken);
    }

    public async Task UpdateAsync(CashBook cashBook, CancellationToken cancellationToken = default)
    {
        _context.CashBooks.Update(cashBook);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cashBook = await _context.CashBooks.FindAsync(new object[] { id }, cancellationToken);
        if (cashBook != null)
        {
            cashBook.IsActive = false;
            cashBook.ModifiedAt = DateTime.UtcNow;
        }
    }
}

/// <summary>
/// Repository implementation for CashBookEntry entities.
/// </summary>
public class CashBookEntryRepository : ICashBookEntryRepository
{
    private readonly AMSDbContext _context;

    public CashBookEntryRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<CashBookEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CashBookEntries
            .Include(e => e.CashBook)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<CashBookEntry>> GetByCashBookIdAsync(Guid cashBookId, CancellationToken cancellationToken = default)
    {
        return await _context.CashBookEntries
            .Where(e => e.CashBookId == cashBookId)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashBookEntry>> GetByFiscalPeriodAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        return await _context.CashBookEntries
            .Where(e => e.FiscalPeriodId == fiscalPeriodId)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashBookEntry>> GetByDateRangeAsync(Guid cashBookId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.CashBookEntries
            .Where(e => e.CashBookId == cashBookId && e.EntryDate >= fromDate && e.EntryDate <= toDate)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CashBookEntry>> GetByVoucherIdAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        return await _context.CashBookEntries.Where(e => e.VoucherId == voucherId).ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<CashBookEntry> Entries, int TotalCount)> GetAllPagedAsync(Guid? cashBookId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.CashBookEntries.Include(e => e.CashBook).AsQueryable();
        if (cashBookId.HasValue)
            query = query.Where(e => e.CashBookId == cashBookId.Value);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(e => e.EntryDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(CashBookEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.CashBookEntries.AddAsync(entry, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<CashBookEntry> entries, CancellationToken cancellationToken = default)
    {
        await _context.CashBookEntries.AddRangeAsync(entries, cancellationToken);
    }

    public async Task UpdateAsync(CashBookEntry entry, CancellationToken cancellationToken = default)
    {
        _context.CashBookEntries.Update(entry);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Repository implementation for BankReconciliation entities.
/// </summary>
public class BankReconciliationRepository : IBankReconciliationRepository
{
    private readonly AMSDbContext _context;

    public BankReconciliationRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<BankReconciliation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BankReconciliations
            .Include(r => r.BankAccount)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<BankReconciliation?> GetByAccountAndPeriodAsync(Guid bankAccountId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.BankAccountId == bankAccountId && r.Year == year && r.Month == month, cancellationToken);
    }

    public async Task<IEnumerable<BankReconciliation>> GetByBankAccountIdAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        return await _context.BankReconciliations
            .Where(r => r.BankAccountId == bankAccountId)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<BankReconciliation> Reconciliations, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.BankReconciliations.Include(r => r.BankAccount).AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(BankReconciliation reconciliation, CancellationToken cancellationToken = default)
    {
        await _context.BankReconciliations.AddAsync(reconciliation, cancellationToken);
    }

    public async Task UpdateAsync(BankReconciliation reconciliation, CancellationToken cancellationToken = default)
    {
        _context.BankReconciliations.Update(reconciliation);
        await Task.CompletedTask;
    }
}
