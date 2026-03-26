using AMS.Application.Common.Results;

namespace AMS.Application.Interfaces;

public interface IRevenueService
{
    Task<ServiceResult<RevenueDto>> CreateRevenueAsync(CreateRevenueDto dto);
    Task<ServiceResult<RevenueDto>> UpdateRevenueAsync(Guid id, UpdateRevenueDto dto);
    Task<ServiceResult<RevenueDto>> GetRevenueByIdAsync(Guid id);
    Task<ServiceResult<List<RevenueDto>>> GetRevenuesByCustomerAsync(Guid customerId);
    Task<ServiceResult<List<RevenueDto>>> GetRevenuesByPeriodAsync(int year, int month);
    Task<ServiceResult<List<RevenueDto>>> GetUnrecognizedRevenuesAsync(int page, int pageSize);
    Task<ServiceResult<RevenueDto>> RecognizeRevenueAsync(Guid id, int year, int month);
    
    Task<ServiceResult<RevenueReportDto>> GetRevenueSummaryAsync(int year, int month);
    Task<ServiceResult<RevenueReportDto>> GenerateRevenueReportAsync(int year, int month, Guid? customerId = null);
}

public class RevenueDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string RevenueType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime? RecognitionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "VND";
    public bool IsRecognized { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateRevenueDto
{
    public Guid CustomerId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string RevenueType { get; set; } = "SalesRevenue";
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public DateTime? RecognitionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public decimal ExchangeRate { get; set; } = 1m;
}

public class UpdateRevenueDto
{
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public DateTime? RecognitionDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class RevenueReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public decimal GrowthPercentage { get; set; }
    public List<RevenueReportDetailDto> Details { get; set; } = new();
}

public class RevenueReportDetailDto
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal RevenueAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal Percentage { get; set; }
}