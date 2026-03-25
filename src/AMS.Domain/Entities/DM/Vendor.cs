namespace AMS.Domain.Entities;

/// <summary>
/// Represents a vendor/supplier in the accounting system.
/// Tracks vendor information for accounts payable and purchasing.
/// </summary>
public class Vendor : BaseLookupEntity
{
    /// <summary>
    /// Tax identification number (Mã số thuế).
    /// </summary>
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// Vendor's address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Name of the contact person.
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Bank account number for payments.
    /// </summary>
    public string? BankAccount { get; set; }

    /// <summary>
    /// Name of the bank.
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Maximum credit limit allowed from this vendor.
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Payment term in days.
    /// </summary>
    public int PaymentTermDays { get; set; }

    /// <summary>
    /// Indicates if vendor is a VAT payer.
    /// </summary>
    public bool IsVatPayer { get; set; } = true;

    /// <summary>
    /// Type of invoice received (e.g., VAT, commercial).
    /// </summary>
    public string? InvoiceType { get; set; }

    /// <summary>
    /// Indicates if the vendor is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Province/State location.
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// District location.
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    /// Ward/Commune location.
    /// </summary>
    public string? Ward { get; set; }

    /// <summary>
    /// Navigation property to voucher lines for this vendor.
    /// </summary>
    public ICollection<VoucherLine> VoucherLines { get; set; } = new List<VoucherLine>();
}
