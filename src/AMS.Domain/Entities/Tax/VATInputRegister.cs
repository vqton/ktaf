using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a VAT input register entry (Sổ thuế GTGT mua vào - BK 02/GTGT).
/// Tracks VAT invoices received from vendors.
/// </summary>
public class VATInputRegister : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the voucher.
    /// </summary>
    public Guid VoucherId { get; set; }

    /// <summary>
    /// Foreign key to the vendor.
    /// </summary>
    public Guid VendorId { get; set; }

    /// <summary>
    /// Fiscal period ID.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Invoice number from vendor.
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
    /// Indicates if the invoice has been claimed for deduction.
    /// </summary>
    public bool IsClaimed { get; set; }

    /// <summary>
    /// Date when the VAT was claimed.
    /// </summary>
    public DateTime? ClaimedDate { get; set; }
}
