using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a withholding tax entry for foreign contractor withholding.
/// Tracks taxes withheld on payments to foreign contractors.
/// </summary>
public class WithholdingTax : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the vendor.
    /// </summary>
    public Guid VendorId { get; set; }

    /// <summary>
    /// Fiscal period ID.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Voucher ID if applicable.
    /// </summary>
    public Guid? VoucherId { get; set; }

    /// <summary>
    /// Invoice/note number.
    /// </summary>
    public string InvoiceNo { get; set; } = string.Empty;

    /// <summary>
    /// Invoice date.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Payment amount before tax.
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>
    /// Tax rate applied.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Tax amount withheld.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Type of service (consulting, technical, other).
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;

    /// <summary>
    /// Contract number if applicable.
    /// </summary>
    public string? ContractNo { get; set; }

    /// <summary>
    /// Indicates if tax has been paid to authority.
    /// </summary>
    public bool IsTaxPaid { get; set; }

    /// <summary>
    /// Date tax was paid to authority.
    /// </summary>
    public DateTime? TaxPaidDate { get; set; }
}
