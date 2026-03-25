namespace AMS.Application.DTOs;

public class LedgerEntryDto
{
    public Guid Id { get; set; }
    public Guid FiscalPeriodId { get; set; }
    public Guid? VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "VND";
    public decimal ExchangeRate { get; set; }
    public decimal AmountInBaseCurrency { get; set; }
    public Guid? PartnerId { get; set; }
    public string? PartnerType { get; set; }
    public string? CostCenter { get; set; }
    public string? ProjectCode { get; set; }
    public string? ContractNo { get; set; }
    public bool IsAdjustEntry { get; set; }
    public string? RefVoucherNo { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class LedgerSummaryDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal DebitTurnover { get; set; }
    public decimal CreditTurnover { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public class CreateLedgerEntryDto
{
    public Guid FiscalPeriodId { get; set; }
    public Guid? VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "VND";
    public decimal ExchangeRate { get; set; } = 1m;
    public Guid? PartnerId { get; set; }
    public string? PartnerType { get; set; }
    public string? CostCenter { get; set; }
    public string? ProjectCode { get; set; }
    public string? ContractNo { get; set; }
    public bool IsAdjustEntry { get; set; }
    public string? RefVoucherNo { get; set; }
}