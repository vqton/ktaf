using AMS.Domain.Enums;

namespace AMS.Application.DTOs;

public class ChartOfAccountsDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AccountNumber { get; set; }
    public AccountType AccountType { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentCode { get; set; }
    public bool IsDetail { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool AllowEntry { get; set; } = true;
    public string? TaxCategory { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? OpeningBalance { get; set; }
    public string? CurrencyCode { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsTaxAccount { get; set; }
    public bool IsVatAccount { get; set; }
    public string? VatRateCode { get; set; }
    public bool IsRevenueSharing { get; set; }
    public string? RevenueSharingPercentage { get; set; }
    public string? ReconciliationAccount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public List<ChartOfAccountsDto> Children { get; set; } = new();
}

public class CreateChartOfAccountsDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AccountNumber { get; set; }
    public AccountType AccountType { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsDetail { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool AllowEntry { get; set; } = true;
    public string? TaxCategory { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? OpeningBalance { get; set; }
    public string? CurrencyCode { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsTaxAccount { get; set; }
    public bool IsVatAccount { get; set; }
    public string? VatRateCode { get; set; }
    public bool IsRevenueSharing { get; set; }
    public string? RevenueSharingPercentage { get; set; }
    public string? ReconciliationAccount { get; set; }
}

public class UpdateChartOfAccountsDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AccountNumber { get; set; }
    public AccountType AccountType { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsDetail { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool AllowEntry { get; set; } = true;
    public string? TaxCategory { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? OpeningBalance { get; set; }
    public string? CurrencyCode { get; set; }
    public bool IsBankAccount { get; set; }
    public bool IsTaxAccount { get; set; }
    public bool IsVatAccount { get; set; }
    public string? VatRateCode { get; set; }
    public bool IsRevenueSharing { get; set; }
    public string? RevenueSharingPercentage { get; set; }
    public string? ReconciliationAccount { get; set; }
}