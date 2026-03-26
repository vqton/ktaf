using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class RevenueRepository : IRevenueRepository
{
    private readonly AMSDbContext _context;

    public RevenueRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<Revenue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Revenues
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Revenue>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Revenues
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.VoucherDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Revenue>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Revenues
            .Include(r => r.Customer)
            .Where(r => r.VoucherDate >= fromDate && r.VoucherDate <= toDate)
            .OrderByDescending(r => r.VoucherDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Revenue>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);
        return await GetByDateRangeAsync(fromDate, toDate, cancellationToken);
    }

    public async Task<IEnumerable<Revenue>> GetUnrecognizedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Revenues
            .Include(r => r.Customer)
            .Where(r => !r.IsRecognized)
            .OrderBy(r => r.RecognitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Revenue> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Revenues.Include(r => r.Customer).Where(r => !r.IsDeleted);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.VoucherDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(Revenue revenue, CancellationToken cancellationToken = default)
    {
        await _context.Revenues.AddAsync(revenue, cancellationToken);
    }

    public async Task UpdateAsync(Revenue revenue, CancellationToken cancellationToken = default)
    {
        _context.Revenues.Update(revenue);
        await Task.CompletedTask;
    }
}

public class RevenueRecognitionRepository : IRevenueRecognitionRepository
{
    private readonly AMSDbContext _context;

    public RevenueRecognitionRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueRecognition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RevenueRecognitions
            .Include(r => r.Revenue)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<RevenueRecognition>> GetByRevenueIdAsync(Guid revenueId, CancellationToken cancellationToken = default)
    {
        return await _context.RevenueRecognitions
            .Where(r => r.RevenueId == revenueId)
            .OrderByDescending(r => r.RecognitionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RevenueRecognition>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.RevenueRecognitions
            .Include(r => r.Revenue)
            .Where(r => r.Year == year && r.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RevenueRecognition recognition, CancellationToken cancellationToken = default)
    {
        await _context.RevenueRecognitions.AddAsync(recognition, cancellationToken);
    }

    public async Task UpdateAsync(RevenueRecognition recognition, CancellationToken cancellationToken = default)
    {
        _context.RevenueRecognitions.Update(recognition);
        await Task.CompletedTask;
    }
}

public class RevenueReportRepository : IRevenueReportRepository
{
    private readonly AMSDbContext _context;

    public RevenueReportRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RevenueReports.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<RevenueReport>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.RevenueReports
            .Where(r => r.Year == year && r.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RevenueReport report, CancellationToken cancellationToken = default)
    {
        await _context.RevenueReports.AddAsync(report, cancellationToken);
    }

    public async Task UpdateAsync(RevenueReport report, CancellationToken cancellationToken = default)
    {
        _context.RevenueReports.Update(report);
        await Task.CompletedTask;
    }
}