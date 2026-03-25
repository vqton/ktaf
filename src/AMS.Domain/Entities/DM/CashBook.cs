namespace AMS.Domain.Entities.DM;

/// <summary>
/// Cash book entry type: RECEIPT, PAYMENT
/// </summary>
public enum CashBookType
{
    Receipt,
    Payment
}

/// <summary>
/// Cash book status: DRAFT, APPROVED, POSTED
/// </summary>
public enum CashBookStatus
{
    Draft,
    Approved,
    Posted
}

/// <summary>
/// Represents a cash book (Sổ quỹ) - multiple cash funds.
/// </summary>
public class CashBook : BaseAuditEntity
{
    /// <summary>
    /// Cash book code (Mã sổ quỹ).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Cash book name (Tên sổ quỹ).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Is this the main cash book (Sổ quỹ chính).
    /// </summary>
    public bool IsMain { get; set; }

    /// <summary>
    /// Currency code (Mã tiền tệ).
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Opening balance (Số dư đầu kỳ).
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Is active for transactions.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Description/Ghi chú.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Collection of cash book entries.
    /// </summary>
    public ICollection<CashBookEntry> Entries { get; set; } = new List<CashBookEntry>();
}

/// <summary>
/// Represents a cash book entry (Sổ quỹ - dòng).
/// </summary>
public class CashBookEntry : BaseAuditEntity
{
    /// <summary>
    /// Cash book ID.
    /// </summary>
    public Guid CashBookId { get; set; }

    /// <summary>
    /// Fiscal period ID (Kỳ kế toán).
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Entry date (Ngày chứng từ).
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Cash book entry type: RECEIPT (Thu) or PAYMENT (Chi).
    /// </summary>
    public CashBookType EntryType { get; set; }

    /// <summary>
    /// Reference document number (Số chứng từ).
    /// </summary>
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// Description (Diễn giải).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Receipt amount (Số tiền thu) - for RECEIPT type.
    /// </summary>
    public decimal ReceiptAmount { get; set; }

    /// <summary>
    /// Payment amount (Số tiền chi) - for PAYMENT type.
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>
    /// Running balance (Số dư tích lũy).
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Entry status (Trạng thái).
    /// </summary>
    public CashBookStatus Status { get; set; } = CashBookStatus.Draft;

    /// <summary>
    /// Is reconciled with bank (Đối chiếu với ngân hàng).
    /// </summary>
    public bool IsReconciled { get; set; }

    /// <summary>
    /// Voucher ID if linked to accounting voucher.
    /// </summary>
    public Guid? VoucherId { get; set; }

    /// <summary>
    /// Partner name (Đối tác - KH/NCC).
    /// </summary>
    public string? PartnerName { get; set; }

    /// <summary>
    /// Navigation property to CashBook.
    /// </summary>
    public CashBook? CashBook { get; set; }

    /// <summary>
    /// Navigation property to FiscalPeriod.
    /// </summary>
    public Domain.Entities.FiscalPeriod? FiscalPeriod { get; set; }

    /// <summary>
    /// Navigation property to Voucher.
    /// </summary>
    public Voucher? Voucher { get; set; }
}
