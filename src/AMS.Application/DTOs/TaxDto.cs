namespace AMS.Application.DTOs;

public class TaxRateDto
{
    public Guid Id { get; set; }
    public string TaxRateKey { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? LegalBasis { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTaxRateDto
{
    public string TaxRateKey { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? LegalBasis { get; set; }
    public string? Description { get; set; }
}

public class PITBracketDto
{
    public Guid Id { get; set; }
    public int BracketNo { get; set; }
    public long FromAmount { get; set; }
    public long? ToAmount { get; set; }
    public decimal TaxRate { get; set; }
    public long QuickDeduction { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? LegalBasis { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePITBracketDto
{
    public int BracketNo { get; set; }
    public long FromAmount { get; set; }
    public long? ToAmount { get; set; }
    public decimal TaxRate { get; set; }
    public long QuickDeduction { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? LegalBasis { get; set; }
}

public class PITCalculationDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal GrossIncome { get; set; }
    public DateTime CalculationDate { get; set; }
    public List<PITAllowanceDto> Allowances { get; set; } = new();
}

public class PITAllowanceDto
{
    public string AllowanceType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PITResultDto
{
    public decimal GrossIncome { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal PITAmount { get; set; }
    public decimal NetIncome { get; set; }
    public int BracketNo { get; set; }
    public decimal BracketRate { get; set; }
}
