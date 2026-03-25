namespace AMS.Application.Accounting.Vouchers.DTOs;

/// <summary>
/// Data transfer object for voucher information.
/// </summary>
public class VoucherDto
{
    /// <summary>
    /// Unique identifier of the voucher.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Voucher number.
    /// </summary>
    public string VoucherNo { get; set; } = string.Empty;

    /// <summary>
    /// Type of voucher (PT, PC, BC, BN, etc.).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Date of the voucher.
    /// </summary>
    public DateTime VoucherDate { get; set; }

    /// <summary>
    /// Foreign key to the fiscal period.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Description of the voucher.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the voucher.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Total debit amount.
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// Total credit amount.
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Exchange rate for foreign currency.
    /// </summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Username of the creator.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// List of voucher lines.
    /// </summary>
    public List<VoucherLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Data transfer object for voucher line information.
/// </summary>
public class VoucherLineDto
{
    /// <summary>
    /// Unique identifier of the line.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Account ID.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Account code.
    /// </summary>
    public string? AccountCode { get; set; }

    /// <summary>
    /// Account name.
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// Debit amount.
    /// </summary>
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Credit amount.
    /// </summary>
    public decimal CreditAmount { get; set; }

    /// <summary>
    /// Line description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID (optional).
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Vendor ID (optional).
    /// </summary>
    public Guid? VendorId { get; set; }

    /// <summary>
    /// Indicates if this is an excise tax line.
    /// </summary>
    public bool IsExciseTaxLine { get; set; }

    /// <summary>
    /// CIT adjustment flag.
    /// </summary>
    public string? CitAdjFlag { get; set; }
}

/// <summary>
/// Data transfer object for creating a new voucher.
/// </summary>
public class CreateVoucherDto
{
    /// <summary>
    /// Type of voucher.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Date of the voucher.
    /// </summary>
    public DateTime VoucherDate { get; set; }

    /// <summary>
    /// Fiscal period ID.
    /// </summary>
    public Guid FiscalPeriodId { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Exchange rate.
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1m;

    /// <summary>
    /// List of lines to create.
    /// </summary>
    public List<CreateVoucherLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Data transfer object for creating a voucher line.
/// </summary>
public class CreateVoucherLineDto
{
    /// <summary>
    /// Account ID.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Debit amount.
    /// </summary>
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Credit amount.
    /// </summary>
    public decimal CreditAmount { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID (optional).
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Vendor ID (optional).
    /// </summary>
    public Guid? VendorId { get; set; }

    /// <summary>
    /// Is excise tax line.
    /// </summary>
    public bool IsExciseTaxLine { get; set; }

    /// <summary>
    /// CIT adjustment flag.
    /// </summary>
    public string? CitAdjFlag { get; set; }
}

/// <summary>
/// Filter criteria for querying vouchers.
/// </summary>
public class VoucherFilterDto
{
    /// <summary>
    /// Filter by voucher number.
    /// </summary>
    public string? VoucherNo { get; set; }

    /// <summary>
    /// Filter by voucher type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Filter by status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter from date.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter to date.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Filter by fiscal period.
    /// </summary>
    public Guid? FiscalPeriodId { get; set; }
}