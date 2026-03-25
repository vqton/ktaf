using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Cfg;

/// <summary>
/// Represents a number sequence for generating unique sequential numbers.
/// Used for voucher numbers, invoice numbers, and other auto-incrementing fields.
/// </summary>
public class NumberSequence : BaseEntity
{
    /// <summary>
    /// Type of sequence (e.g., "VOUCHER_PT", "VOUCHER_PC", "INVOICE").
    /// </summary>
    public string SequenceType { get; set; } = string.Empty;

    /// <summary>
    /// Optional fiscal period ID for period-specific sequences.
    /// </summary>
    public Guid? FiscalPeriodId { get; set; }

    /// <summary>
    /// Current sequence value.
    /// </summary>
    public int CurrentValue { get; set; }

    /// <summary>
    /// Minimum value for the sequence.
    /// </summary>
    public int MinValue { get; set; } = 1;

    /// <summary>
    /// Maximum value for the sequence.
    /// </summary>
    public int MaxValue { get; set; } = 999999;

    /// <summary>
    /// Increment value for each call.
    /// </summary>
    public int Increment { get; set; } = 1;

    /// <summary>
    /// Prefix added to the sequence number (e.g., "PT-").
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Suffix added to the sequence number.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Number of leading zeros for padding (e.g., 5 = "00001").
    /// </summary>
    public int Padding { get; set; } = 5;

    /// <summary>
    /// Indicates if the sequence is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}