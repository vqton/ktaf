using AMS.Domain.Entities.Report;
using AMS.Domain.Interfaces;
using AMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Repositories;

public class FinancialReportRepository : IFinancialReportRepository
{
    private readonly AMSDbContext _context;

    public FinancialReportRepository(AMSDbContext context)
    {
        _context = context;
    }

    public async Task<BalanceSheet?> GetBalanceSheetAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.BalanceSheets
            .FirstOrDefaultAsync(b => b.Year == year && b.Month == month, cancellationToken);
    }

    public async Task<IncomeStatement?> GetIncomeStatementAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.IncomeStatements
            .FirstOrDefaultAsync(i => i.Year == year && i.Month == month, cancellationToken);
    }

    public async Task<CashFlowStatement?> GetCashFlowStatementAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.CashFlowStatements
            .FirstOrDefaultAsync(c => c.Year == year && c.Month == month, cancellationToken);
    }

    public async Task<IEnumerable<FinancialReport>> GetReportsByPeriodAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialReports
            .Where(r => r.Year == year && r.Month == month)
            .OrderBy(r => r.ReportCode)
            .ToListAsync(cancellationToken);
    }

    public async Task AddBalanceSheetAsync(BalanceSheet report, CancellationToken cancellationToken = default)
    {
        await _context.BalanceSheets.AddAsync(report, cancellationToken);
    }

    public async Task AddIncomeStatementAsync(IncomeStatement report, CancellationToken cancellationToken = default)
    {
        await _context.IncomeStatements.AddAsync(report, cancellationToken);
    }

    public async Task AddCashFlowStatementAsync(CashFlowStatement report, CancellationToken cancellationToken = default)
    {
        await _context.CashFlowStatements.AddAsync(report, cancellationToken);
    }

    public async Task UpdateBalanceSheetAsync(BalanceSheet report, CancellationToken cancellationToken = default)
    {
        _context.BalanceSheets.Update(report);
        await Task.CompletedTask;
    }

    public async Task UpdateIncomeStatementAsync(IncomeStatement report, CancellationToken cancellationToken = default)
    {
        _context.IncomeStatements.Update(report);
        await Task.CompletedTask;
    }

    public async Task UpdateCashFlowStatementAsync(CashFlowStatement report, CancellationToken cancellationToken = default)
    {
        _context.CashFlowStatements.Update(report);
        await Task.CompletedTask;
    }
}