using AMS.Domain.Entities.Report;

namespace AMS.Domain.Interfaces;

public interface IFinancialReportRepository
{
    Task<BalanceSheet?> GetBalanceSheetAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IncomeStatement?> GetIncomeStatementAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<CashFlowStatement?> GetCashFlowStatementAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IEnumerable<FinancialReport>> GetReportsByPeriodAsync(int year, int month, CancellationToken cancellationToken = default);
    Task AddBalanceSheetAsync(BalanceSheet report, CancellationToken cancellationToken = default);
    Task AddIncomeStatementAsync(IncomeStatement report, CancellationToken cancellationToken = default);
    Task AddCashFlowStatementAsync(CashFlowStatement report, CancellationToken cancellationToken = default);
    Task UpdateBalanceSheetAsync(BalanceSheet report, CancellationToken cancellationToken = default);
    Task UpdateIncomeStatementAsync(IncomeStatement report, CancellationToken cancellationToken = default);
    Task UpdateCashFlowStatementAsync(CashFlowStatement report, CancellationToken cancellationToken = default);
}