namespace AMS.Application.DTOs;

/// <summary>
/// Data transfer object representing a tax rate.
/// </summary>
public class TaxRateDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the tax rate.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique key of the tax rate.
    /// </summary>
    public string TaxRateKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of tax.
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax rate value (e.g., 0.1 for 10%).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Gets or sets the date from which the tax rate is effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Gets or sets the date until which the tax rate is effective.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Gets or sets the legal basis for the tax rate.
    /// </summary>
    public string? LegalBasis { get; set; }

    /// <summary>
    /// Gets or sets the description of the tax rate.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tax rate is active.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Data transfer object for creating a new tax rate.
/// </summary>
public class CreateTaxRateDto
{
    /// <summary>
    /// Gets or sets the unique key of the tax rate.
    /// </summary>
    public string TaxRateKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of tax.
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax rate value.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Gets or sets the date from which the tax rate is effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Gets or sets the date until which the tax rate is effective.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Gets or sets the legal basis for the tax rate.
    /// </summary>
    public string? LegalBasis { get; set; }

    /// <summary>
    /// Gets or sets the description of the tax rate.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Data transfer object representing a personal income tax (PIT) bracket.
/// </summary>
public class PITBracketDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the PIT bracket.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the bracket number (1-7).
    /// </summary>
    public int BracketNo { get; set; }

    /// <summary>
    /// Gets or sets the minimum taxable income for the bracket.
    /// </summary>
    public long FromAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum taxable income for the bracket (null for the highest bracket).
    /// </summary>
    public long? ToAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax rate for this bracket.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the quick deduction amount for this bracket.
    /// </summary>
    public long QuickDeduction { get; set; }

    /// <summary>
    /// Gets or sets the date from which the bracket is effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Gets or sets the date until which the bracket is effective.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Gets or sets the legal basis for the bracket.
    /// </summary>
    public string? LegalBasis { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the bracket is active.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Data transfer object for creating a new PIT bracket.
/// </summary>
public class CreatePITBracketDto
{
    /// <summary>
    /// Gets or sets the bracket number (1-7).
    /// </summary>
    public int BracketNo { get; set; }

    /// <summary>
    /// Gets or sets the minimum taxable income for the bracket.
    /// </summary>
    public long FromAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum taxable income for the bracket.
    /// </summary>
    public long? ToAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax rate for this bracket.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the quick deduction amount for this bracket.
    /// </summary>
    public long QuickDeduction { get; set; }

    /// <summary>
    /// Gets or sets the date from which the bracket is effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Gets or sets the date until which the bracket is effective.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Gets or sets the legal basis for the bracket.
    /// </summary>
    public string? LegalBasis { get; set; }
}

/// <summary>
/// Data transfer object for PIT calculation input.
/// </summary>
public class PITCalculationDto
{
    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the employee name.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gross income amount.
    /// </summary>
    public decimal GrossIncome { get; set; }

    /// <summary>
    /// Gets or sets the date of calculation.
    /// </summary>
    public DateTime CalculationDate { get; set; }

    /// <summary>
    /// Gets or sets the list of allowances.
    /// </summary>
    public List<PITAllowanceDto> Allowances { get; set; } = new();
}

/// <summary>
/// Data transfer object representing a personal income tax allowance.
/// </summary>
public class PITAllowanceDto
{
    /// <summary>
    /// Gets or sets the type of allowance.
    /// </summary>
    public string AllowanceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the allowance amount.
    /// </summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// Data transfer object representing the result of a PIT calculation.
/// </summary>
public class PITResultDto
{
    /// <summary>
    /// Gets or sets the gross income before deductions.
    /// </summary>
    public decimal GrossIncome { get; set; }

    /// <summary>
    /// Gets or sets the total deductions (personal + allowances).
    /// </summary>
    public decimal TotalDeductions { get; set; }

    /// <summary>
    /// Gets or sets the taxable income after deductions.
    /// </summary>
    public decimal TaxableIncome { get; set; }

    /// <summary>
    /// Gets or sets the calculated PIT amount.
    /// </summary>
    public decimal PITAmount { get; set; }

    /// <summary>
    /// Gets or sets the net income after tax.
    /// </summary>
    public decimal NetIncome { get; set; }

    /// <summary>
    /// Gets or sets the applicable tax bracket number.
    /// </summary>
    public int BracketNo { get; set; }

    /// <summary>
    /// Gets or sets the tax rate of the applicable bracket.
    /// </summary>
    public decimal BracketRate { get; set; }
}

/// <summary>
/// Data transfer object representing a tax declaration.
/// </summary>
public class TaxDeclarationDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the tax declaration.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type of tax (GTGT, TNDN, TNCN).
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax period year.
    /// </summary>
    public int PeriodYear { get; set; }

    /// <summary>
    /// Gets or sets the tax period month.
    /// </summary>
    public int PeriodMonth { get; set; }

    /// <summary>
    /// Gets or sets the period type (MONTH, QUARTER, YEAR).
    /// </summary>
    public string PeriodType { get; set; } = "MONTH";

    /// <summary>
    /// Gets or sets the declaration status.
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Gets or sets the total tax amount due.
    /// </summary>
    public decimal? TotalTaxDue { get; set; }

    /// <summary>
    /// Gets or sets the total tax amount already paid.
    /// </summary>
    public decimal? TotalTaxPaid { get; set; }

    /// <summary>
    /// Gets or sets the difference between tax due and tax paid.
    /// </summary>
    public decimal? TaxDifference { get; set; }

    /// <summary>
    /// Gets or sets the date when the declaration was submitted.
    /// </summary>
    public DateTime? SubmittedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the declaration was accepted.
    /// </summary>
    public DateTime? AcceptedDate { get; set; }

    /// <summary>
    /// Gets or sets the note or comment.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the username of the submitter.
    /// </summary>
    public string? SubmittedBy { get; set; }

    /// <summary>
    /// Gets or sets the username of the approver.
    /// </summary>
    public string? ApprovedBy { get; set; }
}

/// <summary>
/// Data transfer object for creating a new tax declaration.
/// </summary>
public class CreateTaxDeclarationDto
{
    /// <summary>
    /// Gets or sets the type of tax.
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax period year.
    /// </summary>
    public int PeriodYear { get; set; }

    /// <summary>
    /// Gets or sets the tax period month.
    /// </summary>
    public int PeriodMonth { get; set; }

    /// <summary>
    /// Gets or sets the period type.
    /// </summary>
    public string PeriodType { get; set; } = "MONTH";
}

/// <summary>
/// Data transfer object representing a VAT register entry.
/// </summary>
public class VATRegisterDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the VAT register entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the associated voucher identifier.
    /// </summary>
    public Guid VoucherId { get; set; }

    /// <summary>
    /// Gets or sets the vendor or customer identifier.
    /// </summary>
    public Guid VendorId { get; set; }

    /// <summary>
    /// Gets or sets the vendor or customer name.
    /// </summary>
    public string? VendorName { get; set; }

    /// <summary>
    /// Gets or sets the tax code of the vendor or customer.
    /// </summary>
    public string? VendorTaxCode { get; set; }

    /// <summary>
    /// Gets or sets the invoice number.
    /// </summary>
    public string InvoiceNo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice date.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Gets or sets the total amount including VAT.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the VAT amount.
    /// </summary>
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Gets or sets the VAT rate applied.
    /// </summary>
    public decimal VatRate { get; set; }

    /// <summary>
    /// Gets or sets the goods/services amount before VAT.
    /// </summary>
    public decimal GoodsAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the VAT has been claimed for deduction.
    /// </summary>
    public bool IsClaimed { get; set; }

    /// <summary>
    /// Gets or sets the date when the VAT was claimed.
    /// </summary>
    public DateTime? ClaimedDate { get; set; }
}
