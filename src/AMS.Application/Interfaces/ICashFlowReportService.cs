using AMS.Application.Common.Results;

namespace AMS.Application.Interfaces;

public interface ICashFlowReportService
{
    Task<ServiceResult<CashFlowReportDto>> GetCashFlowReportAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<ServiceResult<CashFlowReportDto>> GetCashFlowReportByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<ServiceResult<CashFlowReportDto>> GetCashFlowReportByCashBookAsync(Guid cashBookId, int year, int month, CancellationToken cancellationToken = default);
    Task<ServiceResult<CashFlowReportDto>> GetCashFlowReportByBankAccountAsync(Guid bankAccountId, int year, int month, CancellationToken cancellationToken = default);
}

public class CashFlowReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Currency { get; set; } = "VND";
    public decimal OpeningCashAndBankBalance { get; set; }
    public decimal ClosingCashAndBankBalance { get; set; }
    public CashFlowActivitiesDto Activities { get; set; } = new();
    public List<CashFlowDetailDto> Details { get; set; } = new();
}

public class CashFlowActivitiesDto
{
    public decimal CashFlowsFromOperatingActivities { get; set; }
    public decimal CashFlowsFromInvestingActivities { get; set; }
    public decimal CashFlowsFromFinancingActivities { get; set; }
    public decimal NetCashFlow { get; set; }
}

public class CashFlowDetailDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}