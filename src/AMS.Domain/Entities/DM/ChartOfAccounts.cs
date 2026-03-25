using AMS.Domain.Enums;

namespace AMS.Domain.Entities;

/// <summary>
/// Represents an account in the chart of accounts (Hệ thống tài khoản kế toán).
/// Follows Vietnamese accounting standards (TT 99/2025) with hierarchical structure.
/// </summary>
public class ChartOfAccounts : BaseLookupEntity
{
    /// <summary>
    /// Account number (typically 3-4 digits following Vietnamese accounting standards).
    /// </summary>
    public int AccountNumber { get; set; }

    /// <summary>
    /// Classification type of the account (Asset, Liability, Equity, Revenue, Expense).
    /// </summary>
    public AccountType AccountType { get; set; }

    /// <summary>
    /// Parent account ID for hierarchical structure (null for root accounts).
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Indicates if this is a detail account that can be used in voucher lines.
    /// </summary>
    public bool IsDetail { get; set; } = true;

    /// <summary>
    /// Indicates if the account is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if journal entries can be posted to this account.
    /// </summary>
    public bool AllowEntry { get; set; } = true;

    /// <summary>
    /// Tax category for this account (if applicable).
    /// </summary>
    public string? TaxCategory { get; set; }

    /// <summary>
    /// Bank account number (for bank accounts).
    /// </summary>
    public string? BankAccount { get; set; }

    /// <summary>
    /// Bank name (for bank accounts).
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Opening balance for the account.
    /// </summary>
    public string? OpeningBalance { get; set; }

    /// <summary>
    /// Default currency code for this account.
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Indicates if this is a bank account (112x, 341x, etc.).
    /// </summary>
    public bool IsBankAccount { get; set; } = false;

    /// <summary>
    /// Indicates if this is a tax account.
    /// </summary>
    public bool IsTaxAccount { get; set; } = false;

    /// <summary>
    /// Indicates if this is a VAT account.
    /// </summary>
    public bool IsVatAccount { get; set; } = false;

    /// <summary>
    /// VAT rate code for VAT accounts.
    /// </summary>
    public string? VatRateCode { get; set; }

    /// <summary>
    /// Indicates if revenue sharing is enabled for this account.
    /// </summary>
    public bool IsRevenueSharing { get; set; } = false;

    /// <summary>
    /// Revenue sharing percentage (if enabled).
    /// </summary>
    public string? RevenueSharingPercentage { get; set; }

    /// <summary>
    /// Reconciliation account code for AR/AP reconciliation.
    /// </summary>
    public string? ReconciliationAccount { get; set; }

    /// <summary>
    /// Navigation property to the parent account.
    /// </summary>
    public ChartOfAccounts? Parent { get; set; }

    /// <summary>
    /// Navigation property to child accounts.
    /// </summary>
    public ICollection<ChartOfAccounts> Children { get; set; } = new List<ChartOfAccounts>();

    /// <summary>
    /// Navigation property to voucher lines using this account.
    /// </summary>
    public ICollection<VoucherLine> VoucherLines { get; set; } = new List<VoucherLine>();

    /// <summary>
    /// Navigation property to ledger entries for this account.
    /// </summary>
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}
