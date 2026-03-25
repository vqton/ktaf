using AMS.Domain.Entities;
using AMS.Domain.Enums;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a Corporate Income Tax adjustment entry for non-deductible expenses.
/// Tracks expenses that cannot be deducted when calculating CIT.
/// </summary>
public class CITAdjustment : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the voucher line.
    /// </summary>
    public Guid VoucherLineId { get; set; }

    /// <summary>
    /// Fiscal year.
    /// </summary>
    public int FiscalYear { get; set; }

    /// <summary>
    /// Adjustment flag type (GIFT, MEAL, PERSONAL, INTEREST_EXCESS, ENTERTAINMENT).
    /// </summary>
    public CITAdjFlag AdjFlag { get; set; }

    /// <summary>
    /// Original expense amount.
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Non-deductible amount.
    /// </summary>
    public decimal NonDeductibleAmount { get; set; }

    /// <summary>
    /// Description of why this is non-deductible.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Legal basis for the adjustment.
    /// </summary>
    public string? LegalBasis { get; set; }
}
