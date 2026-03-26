using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.Report;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class FinancialReportService : IFinancialReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFinancialReportRepository _reportRepository;
    private readonly IAccountBalanceRepository _accountBalanceRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IChartOfAccountsRepository _accountRepository;

    public FinancialReportService(
        IUnitOfWork unitOfWork,
        IFinancialReportRepository reportRepository,
        IAccountBalanceRepository accountBalanceRepository,
        ILedgerRepository ledgerRepository,
        IChartOfAccountsRepository accountRepository)
    {
        _unitOfWork = unitOfWork;
        _reportRepository = reportRepository;
        _accountBalanceRepository = accountBalanceRepository;
        _ledgerRepository = ledgerRepository;
        _accountRepository = accountRepository;
    }

    public async Task<ServiceResult<BalanceSheetDto>> GetBalanceSheetAsync(int year, int month)
    {
        var existing = await _reportRepository.GetBalanceSheetAsync(year, month);
        if (existing != null)
            return ServiceResult<BalanceSheetDto>.Success(MapToBalanceSheetDto(existing));

        return await GenerateBalanceSheetAsync(year, month);
    }

    public async Task<ServiceResult<IncomeStatementDto>> GetIncomeStatementAsync(int year, int month)
    {
        var existing = await _reportRepository.GetIncomeStatementAsync(year, month);
        if (existing != null)
            return ServiceResult<IncomeStatementDto>.Success(MapToIncomeStatementDto(existing));

        return await GenerateIncomeStatementAsync(year, month);
    }

    public async Task<ServiceResult<CashFlowStatementDto>> GetCashFlowStatementAsync(int year, int month)
    {
        var existing = await _reportRepository.GetCashFlowStatementAsync(year, month);
        if (existing != null)
            return ServiceResult<CashFlowStatementDto>.Success(MapToCashFlowStatementDto(existing));

        return await GenerateCashFlowStatementAsync(year, month);
    }

    public async Task<ServiceResult<BalanceSheetDto>> GenerateBalanceSheetAsync(int year, int month)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var accounts = await _accountRepository.GetAllActiveAsync();
        var accountBalances = await _accountBalanceRepository.GetAllAsync();

        var result = new BalanceSheetDto
        {
            Year = year,
            Month = month,
            ReportDate = toDate,
            Lines = new List<BalanceSheetLineDto>()
        };

        var assets = new List<BalanceSheetLineDto>();
        var liabilities = new List<BalanceSheetLineDto>();
        var equity = new List<BalanceSheetLineDto>();

        foreach (var acc in accounts)
        {
            var balance = accountBalances.FirstOrDefault(b => b.AccountId == acc.Id);
            var closingBalance = balance?.ClosingDebit - balance?.ClosingCredit ?? 0;

            if (acc.AccountNumber >= 100 && acc.AccountNumber < 400)
            {
                assets.Add(new BalanceSheetLineDto
                {
                    AccountCode = acc.AccountNumber.ToString(),
                    AccountName = acc.Name,
                    LineOrder = acc.AccountNumber,
                    OpeningBalance = 0,
                    ClosingBalance = closingBalance,
                    LineType = "Asset"
                });
                result.TotalAssets += closingBalance;
            }
            else if (acc.AccountNumber >= 400 && acc.AccountNumber < 500)
            {
                liabilities.Add(new BalanceSheetLineDto
                {
                    AccountCode = acc.AccountNumber.ToString(),
                    AccountName = acc.Name,
                    LineOrder = acc.AccountNumber,
                    OpeningBalance = 0,
                    ClosingBalance = closingBalance,
                    LineType = "Liability"
                });
                result.TotalLiabilities += closingBalance;
            }
            else if (acc.AccountNumber >= 500)
            {
                equity.Add(new BalanceSheetLineDto
                {
                    AccountCode = acc.AccountNumber.ToString(),
                    AccountName = acc.Name,
                    LineOrder = acc.AccountNumber,
                    OpeningBalance = 0,
                    ClosingBalance = closingBalance,
                    LineType = "Equity"
                });
                result.TotalEquity += closingBalance;
            }
        }

        result.Lines.AddRange(assets.OrderBy(l => l.LineOrder));
        result.Lines.AddRange(liabilities.OrderBy(l => l.LineOrder));
        result.Lines.AddRange(equity.OrderBy(l => l.LineOrder));

        var report = new BalanceSheet
        {
            Id = Guid.NewGuid(),
            Year = year,
            Month = month,
            ReportDate = toDate,
            TotalAssets = result.TotalAssets,
            TotalLiabilities = result.TotalLiabilities,
            TotalEquity = result.TotalEquity,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        await _reportRepository.AddBalanceSheetAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<BalanceSheetDto>.Success(result);
    }

    public async Task<ServiceResult<IncomeStatementDto>> GenerateIncomeStatementAsync(int year, int month)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var accounts = await _accountRepository.GetAllActiveAsync();
        var accountBalances = await _accountBalanceRepository.GetAllAsync();

        var result = new IncomeStatementDto
        {
            Year = year,
            Month = month,
            FromDate = fromDate,
            ToDate = toDate,
            Lines = new List<IncomeStatementLineDto>()
        };

        decimal totalRevenue = 0;
        decimal totalExpenses = 0;

        foreach (var acc in accounts)
        {
            var balance = accountBalances.FirstOrDefault(b => b.AccountId == acc.Id);
            var amount = balance?.ClosingDebit - balance?.ClosingCredit ?? 0;

            if (acc.AccountNumber >= 511 && acc.AccountNumber < 520)
            {
                result.Lines.Add(new IncomeStatementLineDto
                {
                    AccountCode = acc.AccountNumber.ToString(),
                    AccountName = acc.Name,
                    LineOrder = acc.AccountNumber,
                    Amount = amount,
                    LineType = "Revenue"
                });
                totalRevenue += amount;
            }
            else if (acc.AccountNumber >= 600 && acc.AccountNumber < 700)
            {
                result.Lines.Add(new IncomeStatementLineDto
                {
                    AccountCode = acc.AccountNumber.ToString(),
                    AccountName = acc.Name,
                    LineOrder = acc.AccountNumber,
                    Amount = amount,
                    LineType = "Expense"
                });
                totalExpenses += amount;
            }
        }

        result.TotalRevenue = totalRevenue;
        result.TotalExpenses = totalExpenses;
        result.GrossProfit = totalRevenue - totalExpenses;
        result.NetProfitBeforeTax = result.GrossProfit;
        result.NetProfitAfterTax = result.GrossProfit;

        var report = new IncomeStatement
        {
            Id = Guid.NewGuid(),
            Year = year,
            Month = month,
            FromDate = fromDate,
            ToDate = toDate,
            TotalRevenue = result.TotalRevenue,
            TotalExpenses = result.TotalExpenses,
            GrossProfit = result.GrossProfit,
            NetProfitBeforeTax = result.NetProfitBeforeTax,
            NetProfitAfterTax = result.NetProfitAfterTax,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        await _reportRepository.AddIncomeStatementAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<IncomeStatementDto>.Success(result);
    }

    public async Task<ServiceResult<CashFlowStatementDto>> GenerateCashFlowStatementAsync(int year, int month)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var accounts = await _accountRepository.GetAllActiveAsync();
        var accountBalances = await _accountBalanceRepository.GetAllAsync();

        var cashAccounts = accounts.Where(a => new[] { 111, 112, 113 }.Contains(a.AccountNumber)).ToList();

        var result = new CashFlowStatementDto
        {
            Year = year,
            Month = month,
            FromDate = fromDate,
            ToDate = toDate,
            Lines = new List<CashFlowLineDto>()
        };

        decimal totalCash = 0;
        foreach (var acc in cashAccounts)
        {
            var balance = accountBalances.FirstOrDefault(b => b.AccountId == acc.Id);
            var amount = balance?.ClosingDebit - balance?.ClosingCredit ?? 0;
            totalCash += amount;
        }

        result.OpeningCashBalance = 0;
        result.ClosingCashBalance = totalCash;
        result.NetCashFlow = totalCash;

        var report = new CashFlowStatement
        {
            Id = Guid.NewGuid(),
            Year = year,
            Month = month,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningCashBalance = result.OpeningCashBalance,
            ClosingCashBalance = result.ClosingCashBalance,
            NetCashFlow = result.NetCashFlow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };

        await _reportRepository.AddCashFlowStatementAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<CashFlowStatementDto>.Success(result);
    }

    public async Task<ServiceResult<FinancialReportListDto>> GetAllReportsAsync(int year, int month)
    {
        var reports = await _reportRepository.GetReportsByPeriodAsync(year, month);

        var result = new FinancialReportListDto
        {
            Year = year,
            Month = month,
            Reports = new List<ReportSummaryDto>
            {
                new ReportSummaryDto { ReportCode = "B01-DN", ReportName = "Bảng cân đối kế toán", ReportType = "BalanceSheet", Status = "Available" },
                new ReportSummaryDto { ReportCode = "B02-DN", ReportName = "Báo cáo kết quả hoạt động kinh doanh", ReportType = "IncomeStatement", Status = "Available" },
                new ReportSummaryDto { ReportCode = "B03-DN", ReportName = "Báo cáo lưu chuyển tiền tệ", ReportType = "CashFlowStatement", Status = "Available" }
            }
        };

        return ServiceResult<FinancialReportListDto>.Success(result);
    }

    private static BalanceSheetDto MapToBalanceSheetDto(BalanceSheet entity)
    {
        return new BalanceSheetDto
        {
            Year = entity.Year,
            Month = entity.Month,
            ReportDate = entity.ReportDate,
            TotalAssets = entity.TotalAssets,
            TotalLiabilities = entity.TotalLiabilities,
            TotalEquity = entity.TotalEquity
        };
    }

    private static IncomeStatementDto MapToIncomeStatementDto(IncomeStatement entity)
    {
        return new IncomeStatementDto
        {
            Year = entity.Year,
            Month = entity.Month,
            FromDate = entity.FromDate,
            ToDate = entity.ToDate,
            TotalRevenue = entity.TotalRevenue,
            TotalExpenses = entity.TotalExpenses,
            GrossProfit = entity.GrossProfit,
            NetProfitBeforeTax = entity.NetProfitBeforeTax,
            NetProfitAfterTax = entity.NetProfitAfterTax
        };
    }

    private static CashFlowStatementDto MapToCashFlowStatementDto(CashFlowStatement entity)
    {
        return new CashFlowStatementDto
        {
            Year = entity.Year,
            Month = entity.Month,
            FromDate = entity.FromDate,
            ToDate = entity.ToDate,
            OpeningCashBalance = entity.OpeningCashBalance,
            ClosingCashBalance = entity.ClosingCashBalance,
            NetCashFlow = entity.NetCashFlow
        };
    }
}