using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a Corporate Income Tax loss carry forward entry.
/// Tracks accumulated losses that can be carried forward for up to 5 years.
/// </summary>
public class CITLossCarryForward : BaseEntity
{
    /// <summary>
    /// Fiscal year when the loss was incurred.
    /// </summary>
    public int FiscalYear { get; set; }

    /// <summary>
    /// Amount of loss incurred in the year.
    /// </summary>
    public decimal LossAmount { get; set; }

    /// <summary>
    /// Amount of loss remaining to be carried forward.
    /// </summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>
    /// Year up to which the loss can be carried forward.
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Amount used in previous years.
    /// </summary>
    public decimal UsedAmount { get; set; }

    /// <summary>
    /// Notes about the loss.
    /// </summary>
    public string? Note { get; set; }
}
