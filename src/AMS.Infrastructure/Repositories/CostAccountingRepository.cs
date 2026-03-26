using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class CostCenterRepository : ICostCenterRepository
{
    private readonly AMSDbContext _context;

    public CostCenterRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CostCenters
            .Include(c => c.Department)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CostCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.CostCenters
            .Include(c => c.Department)
            .FirstOrDefaultAsync(c => c.CostCenterCode == code, cancellationToken);
    }

    public async Task<IEnumerable<CostCenter>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CostCenters
            .Include(c => c.Department)
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.CostCenterCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CostCenter>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await _context.CostCenters
            .Where(c => c.DepartmentId == departmentId && !c.IsDeleted)
            .OrderBy(c => c.CostCenterCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<CostCenter> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.CostCenters.Include(c => c.Department).Where(c => !c.IsDeleted);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(c => c.CostCenterCode).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(CostCenter costCenter, CancellationToken cancellationToken = default)
    {
        await _context.CostCenters.AddAsync(costCenter, cancellationToken);
    }

    public async Task UpdateAsync(CostCenter costCenter, CancellationToken cancellationToken = default)
    {
        _context.CostCenters.Update(costCenter);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var costCenter = await _context.CostCenters.FindAsync(new object[] { id }, cancellationToken);
        if (costCenter != null)
        {
            costCenter.IsDeleted = true;
            costCenter.ModifiedAt = DateTime.UtcNow;
        }
    }
}

public class CostAllocationRepository : ICostAllocationRepository
{
    private readonly AMSDbContext _context;

    public CostAllocationRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<CostAllocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CostAllocations
            .Include(c => c.CostCenter)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<CostAllocation>> GetByCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default)
    {
        return await _context.CostAllocations
            .Where(c => c.CostCenterId == costCenterId)
            .OrderByDescending(c => c.Year).ThenByDescending(c => c.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CostAllocation>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.CostAllocations
            .Include(c => c.CostCenter)
            .Where(c => c.Year == year && c.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CostAllocation>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.CostAllocations
            .Include(c => c.CostCenter)
            .Where(c => c.AllocationDate >= fromDate && c.AllocationDate <= toDate)
            .OrderByDescending(c => c.AllocationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CostAllocation allocation, CancellationToken cancellationToken = default)
    {
        await _context.CostAllocations.AddAsync(allocation, cancellationToken);
    }

    public async Task UpdateAsync(CostAllocation allocation, CancellationToken cancellationToken = default)
    {
        _context.CostAllocations.Update(allocation);
        await Task.CompletedTask;
    }
}

public class CostAllocationDetailRepository : ICostAllocationDetailRepository
{
    private readonly AMSDbContext _context;

    public CostAllocationDetailRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CostAllocationDetail>> GetByAllocationIdAsync(Guid allocationId, CancellationToken cancellationToken = default)
    {
        return await _context.CostAllocationDetails
            .Include(d => d.TargetCostCenter)
            .Where(d => d.CostAllocationId == allocationId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<CostAllocationDetail> details, CancellationToken cancellationToken = default)
    {
        await _context.CostAllocationDetails.AddRangeAsync(details, cancellationToken);
    }
}

public class CostReportRepository : ICostReportRepository
{
    private readonly AMSDbContext _context;

    public CostReportRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<CostReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CostReports
            .Include(r => r.CostCenter)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<CostReport>> GetByCostCenterAsync(Guid costCenterId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.CostReports
            .Where(r => r.CostCenterId == costCenterId && r.Year == year && r.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CostReport>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.CostReports
            .Include(r => r.CostCenter)
            .Where(r => r.Year == year && r.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CostReport report, CancellationToken cancellationToken = default)
    {
        await _context.CostReports.AddAsync(report, cancellationToken);
    }

    public async Task UpdateAsync(CostReport report, CancellationToken cancellationToken = default)
    {
        _context.CostReports.Update(report);
        await Task.CompletedTask;
    }
}