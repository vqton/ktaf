using AMS.Domain.Entities.DM;

namespace AMS.Domain.Interfaces;

public interface IRevenueRepository
{
    Task<Revenue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Revenue>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Revenue>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Revenue>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IEnumerable<Revenue>> GetUnrecognizedAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Revenue> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Revenue revenue, CancellationToken cancellationToken = default);
    Task UpdateAsync(Revenue revenue, CancellationToken cancellationToken = default);
}

public interface IRevenueRecognitionRepository
{
    Task<RevenueRecognition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RevenueRecognition>> GetByRevenueIdAsync(Guid revenueId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RevenueRecognition>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default);
    Task AddAsync(RevenueRecognition recognition, CancellationToken cancellationToken = default);
    Task UpdateAsync(RevenueRecognition recognition, CancellationToken cancellationToken = default);
}

public interface IRevenueReportRepository
{
    Task<RevenueReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RevenueReport>> GetByPeriodAsync(int year, int month, CancellationToken cancellationToken = default);
    Task AddAsync(RevenueReport report, CancellationToken cancellationToken = default);
    Task UpdateAsync(RevenueReport report, CancellationToken cancellationToken = default);
}