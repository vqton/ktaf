using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a Special Consumption Tax (Thuế tiêu thụ đặc biệt) item.
/// Tracks current and scheduled tax rates for excise goods (alcohol, tobacco, etc.).
/// </summary>
public class ExciseTaxItem : BaseEntity
{
    /// <summary>
    /// Foreign key to the product (goods subject to excise tax).
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Current excise tax rate.
    /// </summary>
    public decimal CurrentRate { get; set; }

    /// <summary>
    /// JSON string containing scheduled rate changes (for tax rate increases).
    /// Format: [{"year":2027,"rate":0.70},{"year":2028,"rate":0.75}]
    /// </summary>
    public string? ScheduledRatesJson { get; set; }

    /// <summary>
    /// Date from which the current rate is effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Date until which the current rate is effective.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Legal basis for this excise tax rate.
    /// </summary>
    public string? LegalBasis { get; set; }

    /// <summary>
    /// Indicates if the excise tax item is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
