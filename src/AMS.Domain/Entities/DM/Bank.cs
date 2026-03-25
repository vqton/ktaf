using AMS.Domain.Entities;

namespace AMS.Domain.Entities.DM;

/// <summary>
/// Represents a bank account in the system for cash management.
/// </summary>
public class Bank : BaseAuditEntity
{
    /// <summary>
    /// Name of the bank.
    /// </summary>
    public string BankName { get; set; } = string.Empty;

    /// <summary>
    /// Bank code (SWIFT/ABA code for foreign banks).
    /// </summary>
    public string BankCode { get; set; } = string.Empty;

    /// <summary>
    /// Account number at the bank.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Account holder name.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Currency code (ISO 4217). Default is VND.
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Branch name/location.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Indicates if the bank account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if this is the default bank account.
    /// </summary>
    public bool IsDefault { get; set; }
}
