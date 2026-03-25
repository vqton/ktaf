using AMS.Domain.Entities;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for FiscalPeriod entities.
/// </summary>
public class FiscalPeriodRepository : IFiscalPeriodRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the FiscalPeriodRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public FiscalPeriodRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<FiscalPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FiscalPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods
            .FirstOrDefaultAsync(p => p.Year == year && p.Month == month, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FiscalPeriod>> GetByStatusAsync(FiscalPeriodStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FiscalPeriod>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods
            .Where(p => p.Status == FiscalPeriodStatus.Open)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<FiscalPeriod> Periods, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.FiscalPeriods.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);

        var periods = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (periods, totalCount);
    }

    /// <inheritdoc />
    public async Task<FiscalPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods
            .Where(p => p.Year == date.Year && p.Month == date.Month)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FiscalPeriod period, CancellationToken cancellationToken = default)
    {
        await _context.FiscalPeriods.AddAsync(period, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(FiscalPeriod period, CancellationToken cancellationToken = default)
    {
        _context.FiscalPeriods.Update(period);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var period = await _context.FiscalPeriods.FindAsync(new object[] { id }, cancellationToken);
        if (period != null)
        {
            period.IsDeleted = true;
            period.ModifiedAt = DateTime.UtcNow;
            _context.FiscalPeriods.Update(period);
        }
    }
}
