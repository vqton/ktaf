using AMS.Domain.Enums;

namespace AMS.Domain.Entities;

/// <summary>
/// Represents a single entry in the general ledger (Sổ cái).
/// Ledger entries are append-only records created when vouchers are posted.
/// </summary>
public class LedgerEntry : BaseAuditEntity
{
    /// <summary>
    /// Foreign key to the fiscal period.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Foreign key to the source voucher (if any).
    /// </summary>
    public Guid? VoucherId { get; set; }

    /// <summary>
    /// Voucher number from the source document.
    /// </summary>
    public string VoucherNo { get; set; } = string.Empty;

    /// <summary>
    /// Date of the source voucher.
    /// </summary>
    public DateTime VoucherDate { get; set; }

    /// <summary>
    /// Foreign key to the chart of accounts.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Account code for quick reference.
    /// </summary>
    public string AccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Debit amount in base currency.
    /// </summary>
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Credit amount in base currency.
    /// </summary>
    public decimal CreditAmount { get; set; }

    /// <summary>
    /// Description of the entry.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Currency code (ISO 4217). Default is VND.
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Exchange rate for foreign currency conversion.
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1m;

    /// <summary>
    /// Calculated amount in base currency (VND).
    /// </summary>
    public decimal AmountInBaseCurrency => (DebitAmount > 0 ? DebitAmount : CreditAmount) * ExchangeRate;

    /// <summary>
    /// Optional partner ID for AR/AP tracking (Customer or Vendor).
    /// </summary>
    public Guid? PartnerId { get; set; }

    /// <summary>
    /// Type of partner (Customer or Vendor).
    /// </summary>
    public string? PartnerType { get; set; }

    /// <summary>
    /// Optional cost center code.
    /// </summary>
    public string? CostCenter { get; set; }

    /// <summary>
    /// Optional project code.
    /// </summary>
    public string? ProjectCode { get; set; }

    /// <summary>
    /// Optional contract number reference.
    /// </summary>
    public string? ContractNo { get; set; }

    /// <summary>
    /// Indicates if this is an adjusting entry.
    /// </summary>
    public bool IsAdjustEntry { get; set; } = false;

    /// <summary>
    /// Reference to the original voucher number if this is a reversal.
    /// </summary>
    public string? RefVoucherNo { get; set; }

    /// <summary>
    /// Navigation property to the fiscal period.
    /// </summary>
    public FiscalPeriod? FiscalPeriod { get; set; }

    /// <summary>
    /// Navigation property to the source voucher.
    /// </summary>
    public Voucher? Voucher { get; set; }

    /// <summary>
    /// Navigation property to the account.
    /// </summary>
    public ChartOfAccounts? Account { get; set; }
}

/// <summary>
/// Represents a summary of ledger balances for an account in a period.
/// Used for trial balance and financial reports.
/// </summary>
public class LedgerSummary
{
    /// <summary>
    /// Unique identifier of the account.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Account code.
    /// </summary>
    public string AccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Account name.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Opening debit balance at the start of the period.
    /// </summary>
    public decimal OpeningDebit { get; set; }

    /// <summary>
    /// Opening credit balance at the start of the period.
    /// </summary>
    public decimal OpeningCredit { get; set; }

    /// <summary>
    /// Total debit movement during the period.
    /// </summary>
    public decimal PeriodDebit { get; set; }

    /// <summary>
    /// Total credit movement during the period.
    /// </summary>
    public decimal PeriodCredit { get; set; }

    /// <summary>
    /// Closing debit balance at the end of the period.
    /// </summary>
    public decimal ClosingDebit { get; set; }

    /// <summary>
    /// Closing credit balance at the end of the period.
    /// </summary>
    public decimal ClosingCredit { get; set; }
}
