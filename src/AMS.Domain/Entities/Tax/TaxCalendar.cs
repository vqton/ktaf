using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Tax;

/// <summary>
/// Represents a tax calendar entry for tracking tax declaration deadlines.
/// </summary>
public class TaxCalendar : BaseEntity
{
    /// <summary>
    /// Tax type (GTGT, TNDN, TNCN, TTDB).
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Period type (MONTH, QUARTER, YEAR).
    /// </summary>
    public string PeriodType { get; set; } = string.Empty;

    /// <summary>
    /// Calendar year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Deadline date for declaration.
    /// </summary>
    public DateTime DeclarationDeadline { get; set; }

    /// <summary>
    /// Deadline date for payment.
    /// </summary>
    public DateTime PaymentDeadline { get; set; }

    /// <summary>
    /// Notes about the deadline.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Indicates if this is an extended deadline.
    /// </summary>
    public bool IsExtended { get; set; }

    /// <summary>
    /// Original deadline before extension.
    /// </summary>
    public DateTime? OriginalDeadline { get; set; }
}
