using System.ComponentModel.DataAnnotations;

namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object for creating a BankAccount.
/// </summary>
public class CreateBankAccountDto
{
    /// <summary>
    /// Gets or sets the bank identifier.
    /// </summary>
    [Required(ErrorMessage = "Bank ID is required.")]
    public Guid BankId { get; set; }

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    [Required(ErrorMessage = "Account number is required.")]
    [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters.")]
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    [Required(ErrorMessage = "Account name is required.")]
    [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters.")]
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type (e.g., CHECKING, SAVINGS, TERM).
    /// </summary>
    [Required(ErrorMessage = "Account type is required.")]
    [StringLength(20, ErrorMessage = "Account type cannot exceed 20 characters.")]
    public string AccountType { get; set; } = "CHECKING";

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    [StringLength(3, ErrorMessage = "Currency code cannot exceed 3 characters.")]
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Gets or sets the opening balance.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Opening balance must be a non-negative value.")]
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary account.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets the branch name.
    /// </summary>
    [StringLength(100, ErrorMessage = "Branch name cannot exceed 100 characters.")]
    public string? BranchName { get; set; }

    /// <summary>
    /// Gets or sets the account holder name.
    /// </summary>
    [StringLength(100, ErrorMessage = "Account holder name cannot exceed 100 characters.")]
    public string? AccountHolder { get; set; }
}