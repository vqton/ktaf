using AMS.Domain.Entities;
using AMS.Domain.Entities.Cfg;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Voucher entities.
/// </summary>
public class VoucherRepository : IVoucherRepository
{
    private readonly AMSDbContext _context;

    /// <summary>
    /// Initializes a new instance of the VoucherRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public VoucherRepository(AMSDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Voucher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .Include(v => v.FiscalPeriod)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Voucher?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .Include(v => v.FiscalPeriod)
            .Include(v => v.Lines)
            .Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Voucher>> GetByPeriodAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .Where(v => v.FiscalPeriodId == fiscalPeriodId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Voucher>> GetByStatusAsync(AMS.Domain.Enums.VoucherStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .Where(v => v.Status == status)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Voucher> Vouchers, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Vouchers.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);

        var vouchers = await query
            .OrderByDescending(v => v.VoucherDate)
            .ThenByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (vouchers, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Voucher> Vouchers, int TotalCount)> GetByPeriodPagedAsync(Guid fiscalPeriodId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Vouchers.Where(v => v.FiscalPeriodId == fiscalPeriodId);
        var totalCount = await query.CountAsync(cancellationToken);

        var vouchers = await query
            .OrderByDescending(v => v.VoucherDate)
            .ThenByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (vouchers, totalCount);
    }

    /// <inheritdoc />
    public async Task<int> GetNextSequenceAsync(Guid fiscalPeriodId, string voucherType, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.NumberSequences
            .FirstOrDefaultAsync(s => s.SequenceType == voucherType && s.FiscalPeriodId == fiscalPeriodId, cancellationToken);

        if (sequence == null)
        {
            sequence = new NumberSequence
            {
                Id = Guid.NewGuid(),
                SequenceType = voucherType,
                FiscalPeriodId = fiscalPeriodId,
                CurrentValue = 0,
                IsActive = true
            };
            _context.NumberSequences.Add(sequence);
        }

        sequence.CurrentValue++;
        await _context.SaveChangesAsync(cancellationToken);

        return sequence.CurrentValue;
    }

    /// <inheritdoc />
    public async Task AddAsync(Voucher voucher, CancellationToken cancellationToken = default)
    {
        await _context.Vouchers.AddAsync(voucher, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Voucher voucher, CancellationToken cancellationToken = default)
    {
        _context.Vouchers.Update(voucher);
        await Task.CompletedTask;
    }
}
