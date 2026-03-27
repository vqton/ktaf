using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Report;

public class BalanceSheet : BaseAuditEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime ReportDate { get; set; }
    public string ReportType { get; set; } = "BalanceSheet";
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public List<BalanceSheetLine> Lines { get; set; } = new();
}

public class BalanceSheetLine
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int LineOrder { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public string LineType { get; set; } = string.Empty;
}

public class IncomeStatement : BaseAuditEntity
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
    public List<IncomeStatementLine> Lines { get; set; } = new();
}

public class IncomeStatementLine
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int LineOrder { get; set; }
    public decimal Amount { get; set; }
    public string LineType { get; set; } = string.Empty;
}

public class CashFlowStatement : BaseAuditEntity
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
    public List<CashFlowLine> Lines { get; set; } = new();
}

public class CashFlowLine
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int LineOrder { get; set; }
    public decimal Amount { get; set; }
    public string FlowType { get; set; } = string.Empty;
}

public class FinancialReport : BaseAuditEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string ReportCode { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string PreparedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string JsonData { get; set; } = string.Empty;
}