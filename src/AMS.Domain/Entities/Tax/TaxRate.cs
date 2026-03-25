using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a tax rate parameter in the system.
/// Supports time-based tax rates (e.g., VAT 8% → 10% from 2027).
/// </summary>
public class TaxRate : BaseEntity
{
    /// <summary>
    /// Unique key for the tax rate (e.g., "GTGT_STANDARD", "CIT_SME").
    /// </summary>
    public string TaxRateKey { get; set; } = string.Empty;

    /// <summary>
    /// Type of tax (GTGT, TNDN, TNCN, TTDB).
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Tax rate value (e.g., 0.1 = 10%).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Date from which this rate is effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Date until which this rate is effective (null = currently active).
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Legal basis for this rate (circular/law reference).
    /// </summary>
    public string? LegalBasis { get; set; }

    /// <summary>
    /// Description of the tax rate.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if the tax rate is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Username of the creator.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
}
