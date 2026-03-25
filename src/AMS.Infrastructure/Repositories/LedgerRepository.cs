using AMS.Domain.Entities;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for LedgerEntry entities.
/// </summary>
public class LedgerRepository : ILedgerRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the LedgerRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public LedgerRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<LedgerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerEntries.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LedgerEntry>> GetByFiscalPeriodAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerEntries
            .Where(l => l.FiscalPeriodId == fiscalPeriodId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LedgerEntry>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerEntries
            .Where(l => l.AccountId == accountId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LedgerEntry>> GetByVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerEntries
            .Where(l => l.VoucherId == voucherId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<LedgerEntry> Entries, int TotalCount)> GetByFiscalPeriodPagedAsync(Guid fiscalPeriodId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.LedgerEntries.Where(l => l.FiscalPeriodId == fiscalPeriodId);
        var totalCount = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderByDescending(l => l.VoucherDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (entries, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<LedgerEntry> Entries, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.LedgerEntries.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderByDescending(l => l.VoucherDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (entries, totalCount);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LedgerSummary>> GetSummaryAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        var entries = await _context.LedgerEntries
            .Where(l => l.FiscalPeriodId == fiscalPeriodId)
            .ToListAsync(cancellationToken);

        var summary = entries
            .GroupBy(e => new { e.AccountId, e.AccountCode })
            .Select(g => new LedgerSummary
            {
                AccountId = g.Key.AccountId,
                AccountCode = g.Key.AccountCode,
                AccountName = "",
                OpeningDebit = 0,
                OpeningCredit = 0,
                PeriodDebit = g.Sum(e => e.DebitAmount),
                PeriodCredit = g.Sum(e => e.CreditAmount),
                ClosingDebit = g.Sum(e => e.DebitAmount) - g.Sum(e => e.CreditAmount),
                ClosingCredit = g.Sum(e => e.CreditAmount) - g.Sum(e => e.DebitAmount)
            })
            .ToList();

        return summary;
    }

    /// <inheritdoc />
    public async Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.LedgerEntries.AddAsync(entry, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<LedgerEntry> entries, CancellationToken cancellationToken = default)
    {
        await _context.LedgerEntries.AddRangeAsync(entries, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
    {
        _context.LedgerEntries.Update(entry);
        await Task.CompletedTask;
    }
}
