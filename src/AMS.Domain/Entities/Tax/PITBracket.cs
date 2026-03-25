using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a Personal Income Tax (TNCN) bracket in the progressive tax table.
/// Used for monthly PIT withholding and annual finalization.
/// </summary>
public class PITBracket : BaseEntity
{
    /// <summary>
    /// Bracket number (1-5 in the Vietnamese tax table).
    /// </summary>
    public int BracketNo { get; set; }

    /// <summary>
    /// Minimum taxable income for this bracket (VND/month).
    /// </summary>
    public long FromAmount { get; set; }

    /// <summary>
    /// Maximum taxable income for this bracket (null = unlimited).
    /// </summary>
    public long? ToAmount { get; set; }

    /// <summary>
    /// Tax rate for this bracket (e.g., 0.05 = 5%).
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Quick deduction amount (Trừ cứu trợ) for this bracket.
    /// </summary>
    public long QuickDeduction { get; set; }

    /// <summary>
    /// Date from which this bracket is effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Date until which this bracket is effective (null = currently active).
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Legal basis for this bracket.
    /// </summary>
    public string? LegalBasis { get; set; }

    /// <summary>
    /// Indicates if the bracket is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
