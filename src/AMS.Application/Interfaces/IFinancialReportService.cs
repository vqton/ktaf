using AMS.Application.Common.Results;

namespace AMS.Application.Interfaces;

public interface IFinancialReportService
{
    Task<ServiceResult<BalanceSheetDto>> GetBalanceSheetAsync(int year, int month);
    Task<ServiceResult<IncomeStatementDto>> GetIncomeStatementAsync(int year, int month);
    Task<ServiceResult<CashFlowStatementDto>> GetCashFlowStatementAsync(int year, int month);
    Task<ServiceResult<BalanceSheetDto>> GenerateBalanceSheetAsync(int year, int month);
    Task<ServiceResult<IncomeStatementDto>> GenerateIncomeStatementAsync(int year, int month);
    Task<ServiceResult<CashFlowStatementDto>> GenerateCashFlowStatementAsync(int year, int month);
    Task<ServiceResult<FinancialReportListDto>> GetAllReportsAsync(int year, int month);
}

public class BalanceSheetDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime ReportDate { get; set; }
    public string ReportType { get; set; } = "BalanceSheet";
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public List<BalanceSheetLineDto> Lines { get; set; } = new();
}

public class BalanceSheetLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int LineOrder { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public string LineType { get; set; } = string.Empty;
}

public class IncomeStatementDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ReportType { get; set; } = "IncomeStatement";
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfitBeforeTax { get; set; }
    public decimal NetProfitAfterTax { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public List<IncomeStatementLineDto> Lines { get; set; } = new();
}

public class IncomeStatementLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int LineOrder { get; set; }
    public decimal Amount { get; set; }
    public string LineType { get; set; } = string.Empty;
}

public class CashFlowStatementDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ReportType { get; set; } = "CashFlowStatement";
    public decimal OpeningCashBalance { get; set; }
    public decimal ClosingCashBalance { get; set; }
    public decimal NetCashFlow { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public List<CashFlowLineDto> Lines { get; set; } = new();
}

public class CashFlowLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int LineOrder { get; set; }
    public decimal Amount { get; set; }
    public string FlowType { get; set; } = string.Empty;
}

public class FinancialReportListDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<ReportSummaryDto> Reports { get; set; } = new();
}

public class ReportSummaryDto
{
    public string ReportCode { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}