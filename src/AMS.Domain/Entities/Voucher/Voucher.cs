using AMS.Domain.Enums;
using AMS.Domain.Exceptions;

namespace AMS.Domain.Entities;

/// <summary>
/// Represents an accounting voucher/document in the system.
/// </summary>
public class Voucher : BaseAuditEntity
{
    /// <summary>
    /// Unique voucher number generated from number sequence.
    /// </summary>
    public string VoucherNo { get; set; } = string.Empty;

    /// <summary>
    /// Type of accounting voucher (PT, PC, BC, BN, etc.).
    /// </summary>
    public VoucherType Type { get; set; }

    /// <summary>
    /// Date of the voucher.
    /// </summary>
    public DateTime VoucherDate { get; set; }

    /// <summary>
    /// Foreign key to the fiscal period this voucher belongs to.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Description or explanation for the voucher.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the voucher in its lifecycle.
    /// </summary>
    public VoucherStatus Status { get; set; } = VoucherStatus.Draft;

    /// <summary>
    /// Total sum of debit amounts in VND.
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// Total sum of credit amounts in VND.
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// Currency code (ISO 4217). Default is VND.
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Exchange rate for foreign currency conversion to VND.
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1m;

    /// <summary>
    /// Row version for optimistic concurrency control.
    /// </summary>
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Navigation property to the fiscal period.
    /// </summary>
    public FiscalPeriod? FiscalPeriod { get; set; }

    /// <summary>
    /// Collection of voucher lines (journal entry details).
    /// </summary>
    public ICollection<VoucherLine> Lines { get; set; } = new List<VoucherLine>();

    /// <summary>
    /// Collection of attached files/documents.
    /// </summary>
    public ICollection<VoucherAttachment> Attachments { get; set; } = new List<VoucherAttachment>();

    /// <summary>
    /// Submits the voucher for approval.
    /// </summary>
    /// <exception cref="DomainException">Thrown when voucher is not in Draft status or has invalid configuration.</exception>
    public void Submit()
    {
        if (Status != VoucherStatus.Draft)
            throw new DomainException("Only draft vouchers can be submitted.");

        if (Lines.Count < 2)
            throw new DomainException("Voucher must have at least 2 lines.");

        if (TotalDebit != TotalCredit)
            throw new DomainException("Total debit must equal total credit.");

        Status = VoucherStatus.Pending;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Approves the voucher.
    /// </summary>
    /// <param name="approverId">Username of the approver.</param>
    /// <exception cref="DomainException">Thrown when voucher is not in Pending status.</exception>
    public void Approve(string approverId)
    {
        if (Status != VoucherStatus.Pending)
            throw new DomainException("Only pending vouchers can be approved.");

        Status = VoucherStatus.Approved;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = approverId;
    }

    /// <summary>
    /// Rejects the voucher and returns it to Draft status.
    /// </summary>
    /// <param name="reason">Reason for rejection.</param>
    /// <exception cref="DomainException">Thrown when voucher is not in Pending status.</exception>
    public void Reject(string reason)
    {
        if (Status != VoucherStatus.Pending)
            throw new DomainException("Only pending vouchers can be rejected.");

        Status = VoucherStatus.Draft;
        ModifiedAt = DateTime.UtcNow;
        Description = $"[REJECTED] {reason}\n{Description}";
    }

    /// <summary>
    /// Posts the voucher to the general ledger.
    /// </summary>
    /// <exception cref="DomainException">Thrown when voucher is not in Approved status.</exception>
    public void Post()
    {
        if (Status != VoucherStatus.Approved)
            throw new DomainException("Only approved vouchers can be posted.");

        Status = VoucherStatus.Posted;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reverses the posted voucher.
    /// </summary>
    /// <param name="reversedById">Username of the user performing the reversal.</param>
    /// <exception cref="DomainException">Thrown when voucher is not Posted or fiscal period is not open.</exception>
    public void Reverse(string reversedById)
    {
        if (Status != VoucherStatus.Posted)
            throw new DomainException("Only posted vouchers can be reversed.");

        if (FiscalPeriod == null || FiscalPeriod.Status != FiscalPeriodStatus.Open)
            throw new DomainException("Fiscal period must be open to reverse.");

        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = reversedById;
    }

    /// <summary>
    /// Adds a new line to the voucher.
    /// </summary>
    /// <param name="accountId">The account ID for this line.</param>
    /// <param name="debitAmount">The debit amount.</param>
    /// <param name="creditAmount">The credit amount.</param>
    /// <param name="description">Optional description for the line.</param>
    /// <exception cref="DomainException">Thrown when voucher is not in Draft status or amounts are invalid.</exception>
    public void AddLine(Guid accountId, decimal debitAmount, decimal creditAmount, string? description = null)
    {
        if (Status != VoucherStatus.Draft)
            throw new DomainException("Cannot add lines to non-draft vouchers.");

        if (debitAmount < 0 || creditAmount < 0)
            throw new DomainException("Amounts cannot be negative.");

        if (debitAmount > 0 && creditAmount > 0)
            throw new DomainException("Line cannot have both debit and credit.");

        Lines.Add(new VoucherLine
        {
            Id = Guid.NewGuid(),
            VoucherId = Id,
            AccountId = accountId,
            DebitAmount = debitAmount,
            CreditAmount = creditAmount,
            Description = description ?? string.Empty
        });

        RecalculateTotals();
    }

    /// <summary>
    /// Removes a line from the voucher.
    /// </summary>
    /// <param name="lineId">The ID of the line to remove.</param>
    /// <exception cref="DomainException">Thrown when voucher is not in Draft status.</exception>
    public void RemoveLine(Guid lineId)
    {
        if (Status != VoucherStatus.Draft)
            throw new DomainException("Cannot remove lines from non-draft vouchers.");

        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            Lines.Remove(line);
            RecalculateTotals();
        }
    }

    /// <summary>
    /// Recalculates the total debit and credit amounts from all lines.
    /// </summary>
    private void RecalculateTotals()
    {
        TotalDebit = Lines.Sum(l => l.DebitAmount);
        TotalCredit = Lines.Sum(l => l.CreditAmount);
    }
}

/// <summary>
/// Represents a single journal entry line within a voucher.
/// </summary>
public class VoucherLine
{
    /// <summary>
    /// Unique identifier for this line.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the parent voucher.
    /// </summary>
    public Guid VoucherId { get; set; }

    /// <summary>
    /// Foreign key to the chart of accounts.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Debit amount in VND.
    /// </summary>
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Credit amount in VND.
    /// </summary>
    public decimal CreditAmount { get; set; }

    /// <summary>
    /// Description for this specific line.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional customer ID for receivable tracking.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Optional vendor ID for payable tracking.
    /// </summary>
    public Guid? VendorId { get; set; }

    /// <summary>
    /// Indicates if this is an excise tax line.
    /// </summary>
    public bool IsExciseTaxLine { get; set; }

    /// <summary>
    /// Corporate income tax adjustment flag for non-deductible expenses.
    /// </summary>
    public string? CitAdjFlag { get; set; }

    /// <summary>
    /// Navigation property to the parent voucher.
    /// </summary>
    public Voucher? Voucher { get; set; }

    /// <summary>
    /// Navigation property to the account.
    /// </summary>
    public ChartOfAccounts? Account { get; set; }

    /// <summary>
    /// Navigation property to the customer.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Navigation property to the vendor.
    /// </summary>
    public Vendor? Vendor { get; set; }
}

/// <summary>
/// Represents an attachment/document associated with a voucher.
/// </summary>
public class VoucherAttachment : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the parent voucher.
    /// </summary>
    public Guid VoucherId { get; set; }

    /// <summary>
    /// Original file name of the attachment.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Path to the file in storage.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// MIME content type of the file.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Optional description for the attachment.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to the parent voucher.
    /// </summary>
    public Voucher? Voucher { get; set; }
}
