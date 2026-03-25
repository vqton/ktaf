using AMS.Domain.Entities;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class FiscalPeriodRepository : IFiscalPeriodRepository
{
    private readonly AMSDbContext _context;

    public FiscalPeriodRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<FiscalPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<FiscalPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods
            .FirstOrDefaultAsync(p => p.Year == year && p.Month == month, cancellationToken);
    }

    public async Task<IEnumerable<FiscalPeriod>> GetByStatusAsync(FiscalPeriodStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FiscalPeriod>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods
            .Where(p => p.Status == FiscalPeriodStatus.Open)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);
    }

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

    public async Task<FiscalPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalPeriods
            .Where(p => p.Year == date.Year && p.Month == date.Month)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(FiscalPeriod period, CancellationToken cancellationToken = default)
    {
        await _context.FiscalPeriods.AddAsync(period, cancellationToken);
    }

    public async Task UpdateAsync(FiscalPeriod period, CancellationToken cancellationToken = default)
    {
        _context.FiscalPeriods.Update(period);
        await Task.CompletedTask;
    }

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
