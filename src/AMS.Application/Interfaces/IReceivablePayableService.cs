using AMS.Application.Common.Results;

namespace AMS.Application.Interfaces;

public interface IReceivablePayableService
{
    Task<ServiceResult<ReceivableDto>> CreateReceivableAsync(CreateReceivableDto dto);
    Task<ServiceResult<ReceivableDto>> UpdateReceivableAsync(Guid id, UpdateReceivableDto dto);
    Task<ServiceResult<ReceivableDto>> GetReceivableByIdAsync(Guid id);
    Task<ServiceResult<List<ReceivableDto>>> GetReceivablesByCustomerAsync(Guid customerId);
    Task<ServiceResult<List<ReceivableDto>>> GetUnpaidReceivablesAsync(int page, int pageSize);
    Task<ServiceResult<ReceivableDto>> RecordReceivablePaymentAsync(Guid receivableId, RecordPaymentDto dto);

    Task<ServiceResult<PayableDto>> CreatePayableAsync(CreatePayableDto dto);
    Task<ServiceResult<PayableDto>> UpdatePayableAsync(Guid id, UpdatePayableDto dto);
    Task<ServiceResult<PayableDto>> GetPayableByIdAsync(Guid id);
    Task<ServiceResult<List<PayableDto>>> GetPayablesByVendorAsync(Guid vendorId);
    Task<ServiceResult<List<PayableDto>>> GetUnpaidPayablesAsync(int page, int pageSize);
    Task<ServiceResult<PayableDto>> RecordPayablePaymentAsync(Guid payableId, RecordPaymentDto dto);

    Task<ServiceResult<AgingReportDto>> GetReceivableAgingReportAsync(int year, int month, Guid? customerId = null);
    Task<ServiceResult<AgingReportDto>> GetPayableAgingReportAsync(int year, int month, Guid? vendorId = null);
    Task<ServiceResult<AgingReportDto>> GenerateAgingReportAsync(int year, int month, string reportType, Guid? partnerId = null);
}

public class ReceivableDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string ReceivableType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "VND";
    public bool IsReconciled { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateReceivableDto
{
    public Guid CustomerId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string ReceivableType { get; set; } = "TradeReceivable";
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public decimal ExchangeRate { get; set; } = 1m;
}

public class UpdateReceivableDto
{
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class PayableDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string PayableType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "VND";
    public bool IsReconciled { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreatePayableDto
{
    public Guid VendorId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string PayableType { get; set; } = "TradePayable";
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public decimal ExchangeRate { get; set; } = 1m;
}

public class UpdatePayableDto
{
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class RecordPaymentDto
{
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
}

public class AgingReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? VendorId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal Due1To30Days { get; set; }
    public decimal Due31To60Days { get; set; }
    public decimal Due61To90Days { get; set; }
    public decimal DueOver90Days { get; set; }
    public decimal TotalAmount { get; set; }
    public List<AgingReportDetailDto> Details { get; set; } = new();
}

public class AgingReportDetailDto
{
    public Guid Id { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int OverdueDays { get; set; }
    public string AgingPeriod { get; set; } = string.Empty;
}