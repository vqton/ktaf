using AccountingERP.Application.Interfaces;
using AccountingERP.Domain.Entities;
using AccountingERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AccountingERP.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho JournalEntry
/// </summary>
public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly AccountingDbContext _context;

    public JournalEntryRepository(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber, CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.EntryNumber == entryNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<JournalEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(j => j.Lines)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JournalEntry>> GetByDateRangeAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(j => j.Lines)
            .Where(j => j.EntryDate >= fromDate && j.EntryDate <= toDate)
            .OrderBy(j => j.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
    {
        await _context.JournalEntries.AddAsync(journalEntry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
    {
        _context.JournalEntries.Update(journalEntry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.JournalEntries.FindAsync(new object[] { id }, cancellationToken);
        if (entry != null)
        {
            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string entryNumber, CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .AnyAsync(j => j.EntryNumber == entryNumber, cancellationToken);
    }
}
