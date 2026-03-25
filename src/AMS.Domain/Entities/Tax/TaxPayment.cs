using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a tax payment entry for tracking tax payments to authorities.
/// </summary>
public class TaxPayment : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the tax declaration.
    /// </summary>
    public Guid TaxDeclarationId { get; set; }

    /// <summary>
    /// Foreign key to the payment voucher.
    /// </summary>
    public Guid? VoucherId { get; set; }

    /// <summary>
    /// Tax type (GTGT, TNDN, TNCN, TTDB).
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Tax period year.
    /// </summary>
    public int PeriodYear { get; set; }

    /// <summary>
    /// Tax period month.
    /// </summary>
    public int PeriodMonth { get; set; }

    /// <summary>
    /// Tax amount paid.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Payment date.
    /// </summary>
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Payment method (CASH, BANK_TRANSFER).
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Tax authority code.
    /// </summary>
    public string? TaxAuthorityCode { get; set; }

    /// <summary>
    /// Document number from tax authority.
    /// </summary>
    public string? DocumentNo { get; set; }
}
