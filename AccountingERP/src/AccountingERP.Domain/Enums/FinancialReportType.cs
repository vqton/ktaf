namespace AccountingERP.Domain.Enums;

/// <summary>
/// Loại báo cáo tài chính theo TT99
/// </summary>
public enum FinancialReportType
{
    /// <summary>
    /// Bảng cân đối kế toán (B01)
    /// </summary>
    BalanceSheet = 1,
    
    /// <summary>
    /// Báo cáo kết quả hoạt động kinh doanh (B02)
    /// </summary>
    IncomeStatement = 2,
    
    /// <summary>
    /// Báo cáo lưu chuyển tiền tệ (B03-DN)
    /// </summary>
    CashFlowStatement = 3,
    
    /// <summary>
    /// Thuyết minh báo cáo tài chính (B09)
    /// </summary>
    NotesToFinancialStatements = 4,
    
    /// <summary>
    /// Báo cáo tình hình tài chính (B04)
    /// </summary>
    FinancialPosition = 5
}
