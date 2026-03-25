using AMS.Domain.Enums;
using AMS.Domain.Exceptions;

namespace AMS.Domain.Entities;

/// <summary>
/// Represents a fiscal accounting period (typically monthly).
/// </summary>
public class FiscalPeriod : BaseAuditEntity
{
    /// <summary>
    /// The calendar year of the period.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// The month of the period (1-12).
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Current status of the fiscal period.
    /// </summary>
    public FiscalPeriodStatus Status { get; set; } = FiscalPeriodStatus.Open;

    /// <summary>
    /// Collection of vouchers in this period.
    /// </summary>
    public ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();

    /// <summary>
    /// Collection of ledger entries in this period.
    /// </summary>
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();

    /// <summary>
    /// Opens the fiscal period for posting vouchers.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the period is locked.</exception>
    public void Open()
    {
        if (Status == FiscalPeriodStatus.Locked)
            throw new DomainException("Cannot open a locked fiscal period.");

        Status = FiscalPeriodStatus.Open;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Closes the fiscal period after all vouchers are processed.
    /// </summary>
    /// <param name="closedBy">Username of the user closing the period.</param>
    /// <exception cref="DomainException">Thrown when period is not open or has pending vouchers.</exception>
    public void Close(string closedBy)
    {
        if (Status != FiscalPeriodStatus.Open)
            throw new DomainException("Only open fiscal periods can be closed.");

        var hasPendingVouchers = Vouchers.Any(v => v.Status == VoucherStatus.Pending);
        if (hasPendingVouchers)
            throw new DomainException("Cannot close period with pending vouchers.");

        Status = FiscalPeriodStatus.Closed;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = closedBy;
    }

    /// <summary>
    /// Locks the fiscal period to prevent any modifications.
    /// </summary>
    /// <exception cref="DomainException">Thrown when period is not closed.</exception>
    public void Lock()
    {
        if (Status != FiscalPeriodStatus.Closed)
            throw new DomainException("Only closed periods can be locked.");

        Status = FiscalPeriodStatus.Locked;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether vouchers can be posted to this period.
    /// </summary>
    /// <returns>True if the period is open; otherwise, false.</returns>
    public bool CanPostVouchers() => Status == FiscalPeriodStatus.Open;
}
