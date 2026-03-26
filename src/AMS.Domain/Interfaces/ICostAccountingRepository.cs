using AMS.Domain.Entities.DM;

namespace AMS.Domain.Interfaces;

public interface ICostCenterRepository
{
    Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CostCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<CostCenter>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CostCenter>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<CostCenter> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(CostCenter costCenter, CancellationToken cancellationToken = default);
    Task UpdateAsync(CostCenter costCenter, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICostAllocationRepository
{
    Task<CostAllocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CostAllocation>> GetByCostCenterAsync(Guid costCenterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CostAllocation>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IEnumerable<CostAllocation>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task AddAsync(CostAllocation allocation, CancellationToken cancellationToken = default);
    Task UpdateAsync(CostAllocation allocation, CancellationToken cancellationToken = default);
}

public interface ICostAllocationDetailRepository
{
    Task<IEnumerable<CostAllocationDetail>> GetByAllocationIdAsync(Guid allocationId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<CostAllocationDetail> details, CancellationToken cancellationToken = default);
}

public interface ICostReportRepository
{
    Task<CostReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CostReport>> GetByCostCenterAsync(Guid costCenterId, int year, int month, CancellationToken cancellationToken = default);
    Task<IEnumerable<CostReport>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default);
    Task AddAsync(CostReport report, CancellationToken cancellationToken = default);
    Task UpdateAsync(CostReport report, CancellationToken cancellationToken = default);
}