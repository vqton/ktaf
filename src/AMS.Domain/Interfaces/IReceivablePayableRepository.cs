using AMS.Domain.Entities.DM;

namespace AMS.Domain.Interfaces;

public interface IReceivableRepository
{
    Task<Receivable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Receivable>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Receivable>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Receivable>> GetUnpaidByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Receivable> Receivables, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Receivable receivable, CancellationToken cancellationToken = default);
    Task UpdateAsync(Receivable receivable, CancellationToken cancellationToken = default);
}

public interface IPayableRepository
{
    Task<Payable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payable>> GetByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payable>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payable>> GetUnpaidByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Payable> Payables, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Payable payable, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payable payable, CancellationToken cancellationToken = default);
}

public interface IReceivablePaymentRepository
{
    Task<ReceivablePayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReceivablePayment>> GetByReceivableIdAsync(Guid receivableId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReceivablePayment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task AddAsync(ReceivablePayment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReceivablePayment payment, CancellationToken cancellationToken = default);
}

public interface IPayablePaymentRepository
{
    Task<PayablePayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PayablePayment>> GetByPayableIdAsync(Guid payableId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PayablePayment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task AddAsync(PayablePayment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(PayablePayment payment, CancellationToken cancellationToken = default);
}

public interface IAgingReportRepository
{
    Task<AgingReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AgingReport>> GetByCustomerAsync(Guid customerId, int year, int month, CancellationToken cancellationToken = default);
    Task<IEnumerable<AgingReport>> GetByVendorAsync(Guid vendorId, int year, int month, CancellationToken cancellationToken = default);
    Task<AgingReport?> GetByPeriodAsync(int year, int month, Guid? customerId, Guid? vendorId, CancellationToken cancellationToken = default);
    Task AddAsync(AgingReport report, CancellationToken cancellationToken = default);
    Task UpdateAsync(AgingReport report, CancellationToken cancellationToken = default);
}