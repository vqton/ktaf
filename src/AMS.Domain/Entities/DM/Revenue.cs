using AMS.Domain.Entities;

namespace AMS.Domain.Entities.DM;

public enum RevenueType
{
    SalesRevenue,
    ServiceRevenue,
    OtherRevenue,
    DiscountReceived,
    InterestIncome,
    ExchangeRateGain
}

public class Revenue : BaseAuditEntity
{
    public Guid CustomerId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public RevenueType RevenueType { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount => Amount - TaxAmount;
    public DateTime? RecognitionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? VoucherId { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public decimal ExchangeRate { get; set; } = 1m;
    public bool IsRecognized { get; set; }
    public Customer? Customer { get; set; }
    public Voucher? Voucher { get; set; }
}

public class RevenueRecognition : BaseAuditEntity
{
    public Guid RevenueId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public decimal RecognizedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime RecognitionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Revenue? Revenue { get; set; }
}

public class RevenueReport : BaseAuditEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public decimal GrowthPercentage { get; set; }
    public List<RevenueReportDetail> Details { get; set; } = new();
}

public class RevenueReportDetail
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal RevenueAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal Percentage { get; set; }
}