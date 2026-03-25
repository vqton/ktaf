using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a VAT output register entry (Sổ thuế GTGT bán ra - BK 01/GTGT).
/// Tracks VAT invoices issued to customers.
/// </summary>
public class VATOutputRegister : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the voucher.
    /// </summary>
    public Guid VoucherId { get; set; }

    /// <summary>
    /// Foreign key to the customer.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Fiscal period ID.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Invoice number issued.
    /// </summary>
    public string InvoiceNo { get; set; } = string.Empty;

    /// <summary>
    /// Invoice date.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Total amount including VAT.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// VAT amount.
    /// </summary>
    public decimal VatAmount { get; set; }

    /// <summary>
    /// VAT rate applied.
    /// </summary>
    public decimal VatRate { get; set; }

    /// <summary>
    /// Goods/Services amount before VAT.
    /// </summary>
    public decimal GoodsAmount { get; set; }

    /// <summary>
    /// Indicates if this is a credit note (hóa đơn điều chỉnh).
    /// </summary>
    public bool IsCreditNote { get; set; }

    /// <summary>
    /// Reference to original invoice if this is a credit note.
    /// </summary>
    public Guid? OriginalInvoiceId { get; set; }
}
