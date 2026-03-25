namespace AMS.Application.DTOs;

public class VendorDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public bool IsVatPayer { get; set; } = true;
    public string? InvoiceType { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}

public class CreateVendorDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public bool IsVatPayer { get; set; } = true;
    public string? InvoiceType { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
}

public class UpdateVendorDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public bool IsVatPayer { get; set; } = true;
    public string? InvoiceType { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
}