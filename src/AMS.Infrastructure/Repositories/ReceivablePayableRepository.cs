using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class ReceivableRepository : IReceivableRepository
{
    private readonly AMSDbContext _context;

    public ReceivableRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<Receivable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Receivables
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Receivable>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Receivables
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.VoucherDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Receivable>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Receivables
            .Where(r => r.VoucherDate >= fromDate && r.VoucherDate <= toDate)
            .OrderByDescending(r => r.VoucherDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Receivable>> GetUnpaidByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Receivables
            .Where(r => r.CustomerId == customerId && (r.Amount - r.PaidAmount) > 0)
            .OrderBy(r => r.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Receivable> Receivables, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Receivables.Include(r => r.Customer).AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.VoucherDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(Receivable receivable, CancellationToken cancellationToken = default)
    {
        await _context.Receivables.AddAsync(receivable, cancellationToken);
    }

    public async Task UpdateAsync(Receivable receivable, CancellationToken cancellationToken = default)
    {
        _context.Receivables.Update(receivable);
        await Task.CompletedTask;
    }
}

public class PayableRepository : IPayableRepository
{
    private readonly AMSDbContext _context;

    public PayableRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<Payable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payables
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Payable>> GetByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.Payables
            .Where(p => p.VendorId == vendorId)
            .OrderByDescending(p => p.VoucherDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payable>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Payables
            .Where(p => p.VoucherDate >= fromDate && p.VoucherDate <= toDate)
            .OrderByDescending(p => p.VoucherDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payable>> GetUnpaidByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.Payables
            .Where(p => p.VendorId == vendorId && (p.Amount - p.PaidAmount) > 0)
            .OrderBy(p => p.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Payable> Payables, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Payables.Include(p => p.Vendor).AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(p => p.VoucherDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(Payable payable, CancellationToken cancellationToken = default)
    {
        await _context.Payables.AddAsync(payable, cancellationToken);
    }

    public async Task UpdateAsync(Payable payable, CancellationToken cancellationToken = default)
    {
        _context.Payables.Update(payable);
        await Task.CompletedTask;
    }
}

public class ReceivablePaymentRepository : IReceivablePaymentRepository
{
    private readonly AMSDbContext _context;

    public ReceivablePaymentRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<ReceivablePayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ReceivablePayments
            .Include(p => p.Receivable)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ReceivablePayment>> GetByReceivableIdAsync(Guid receivableId, CancellationToken cancellationToken = default)
    {
        return await _context.ReceivablePayments
            .Where(p => p.ReceivableId == receivableId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReceivablePayment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.ReceivablePayments
            .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ReceivablePayment payment, CancellationToken cancellationToken = default)
    {
        await _context.ReceivablePayments.AddAsync(payment, cancellationToken);
    }

    public async Task UpdateAsync(ReceivablePayment payment, CancellationToken cancellationToken = default)
    {
        _context.ReceivablePayments.Update(payment);
        await Task.CompletedTask;
    }
}

public class PayablePaymentRepository : IPayablePaymentRepository
{
    private readonly AMSDbContext _context;

    public PayablePaymentRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<PayablePayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PayablePayments
            .Include(p => p.Payable)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PayablePayment>> GetByPayableIdAsync(Guid payableId, CancellationToken cancellationToken = default)
    {
        return await _context.PayablePayments
            .Where(p => p.PayableId == payableId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PayablePayment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.PayablePayments
            .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PayablePayment payment, CancellationToken cancellationToken = default)
    {
        await _context.PayablePayments.AddAsync(payment, cancellationToken);
    }

    public async Task UpdateAsync(PayablePayment payment, CancellationToken cancellationToken = default)
    {
        _context.PayablePayments.Update(payment);
        await Task.CompletedTask;
    }
}

public class AgingReportRepository : IAgingReportRepository
{
    private readonly AMSDbContext _context;

    public AgingReportRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<AgingReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AgingReports.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<AgingReport>> GetByCustomerAsync(Guid customerId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.AgingReports
            .Where(r => r.CustomerId == customerId && r.Year == year && r.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AgingReport>> GetByVendorAsync(Guid vendorId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.AgingReports
            .Where(r => r.VendorId == vendorId && r.Year == year && r.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task<AgingReport?> GetByPeriodAsync(int year, int month, Guid? customerId, Guid? vendorId, CancellationToken cancellationToken = default)
    {
        var query = _context.AgingReports.Where(r => r.Year == year && r.Month == month);

        if (customerId.HasValue)
            query = query.Where(r => r.CustomerId == customerId.Value);
        else if (vendorId.HasValue)
            query = query.Where(r => r.VendorId == vendorId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(AgingReport report, CancellationToken cancellationToken = default)
    {
        await _context.AgingReports.AddAsync(report, cancellationToken);
    }

    public async Task UpdateAsync(AgingReport report, CancellationToken cancellationToken = default)
    {
        _context.AgingReports.Update(report);
        await Task.CompletedTask;
    }
}