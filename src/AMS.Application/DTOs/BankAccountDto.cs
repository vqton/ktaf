using AMS.Domain.Entities.DM;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for BankAccount entity.
/// </summary>
public class BankAccountDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the bank identifier.
    /// </summary>
    public Guid BankId { get; set; }

    /// <summary>
    /// Gets or sets the bank name (for display purposes).
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type (e.g., CHECKING, SAVINGS, TERM).
    /// </important>
    public string AccountType { get; set; } = "CHECKING";

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Gets or sets the opening balance.
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary account.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the branch name.
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// Gets or sets the account holder name.
    /// </summary>
    public string? AccountHolder { get; set; }

    /// <summary>
    /// Gets or sets the date and time the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time the entity was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the entity.
    /// </summary>
    public string? ModifiedBy { get; set; }
}