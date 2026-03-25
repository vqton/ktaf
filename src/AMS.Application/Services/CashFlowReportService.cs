using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class CashFlowReportService : ICashFlowReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFiscalPeriodRepository _fiscalPeriodRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IChartOfAccountsRepository _accountRepository;
    private readonly IAccountBalanceRepository _accountBalanceRepository;
    private readonly ICashBookRepository _cashBookRepository;
    private readonly IBankAccountRepository _bankAccountRepository;

    public CashFlowReportService(
        IUnitOfWork unitOfWork,
        IFiscalPeriodRepository fiscalPeriodRepository,
        ILedgerRepository ledgerRepository,
        IChartOfAccountsRepository accountRepository,
        IAccountBalanceRepository accountBalanceRepository,
        ICashBookRepository cashBookRepository,
        IBankAccountRepository bankAccountRepository)
    {
        _unitOfWork = unitOfWork;
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _ledgerRepository = ledgerRepository;
        _accountRepository = accountRepository;
        _accountBalanceRepository = accountBalanceRepository;
        _cashBookRepository = cashBookRepository;
        _bankAccountRepository = bankAccountRepository;
    }

    public async Task<ServiceResult<CashFlowReportDto>> GetCashFlowReportAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var period = await _fiscalPeriodRepository.GetByYearMonthAsync(year, month, cancellationToken);
        if (period == null)
            return ServiceResult<CashFlowReportDto>.Failure($"Kỳ kế toán {month}/{year} không tồn tại.");

        var previousPeriod = await _fiscalPeriodRepository.GetByYearMonthAsync(year, month - 1, cancellationToken);

        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        return await BuildCashFlowReportAsync(period, previousPeriod, fromDate, toDate, cancellationToken);
    }

    public async Task<ServiceResult<CashFlowReportDto>> GetCashFlowReportByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var year = fromDate.Year;
        var month = fromDate.Month;

        var period = await _fiscalPeriodRepository.GetByYearMonthAsync(year, month, cancellationToken);
        if (period == null)
            return ServiceResult<CashFlowReportDto>.Failure($"Kỳ kế toán {month}/{year} không tồn tại.");

        var previousPeriod = await _fiscalPeriodRepository.GetByYearMonthAsync(year, month - 1, cancellationToken);

        return await BuildCashFlowReportAsync(period, previousPeriod, fromDate, toDate, cancellationToken);
    }

    public async Task<ServiceResult<CashFlowReportDto>> GetCashFlowReportByCashBookAsync(Guid cashBookId, int year, int month, CancellationToken cancellationToken = default)
    {
        var cashBook = await _cashBookRepository.GetByIdAsync(cashBookId, cancellationToken);
        if (cashBook == null)
            return ServiceResult<CashFlowReportDto>.Failure("Sổ quỹ không tồn tại.");

        var period = await _fiscalPeriodRepository.GetByYearMonthAsync(year, month, cancellationToken);
        if (period == null)
            return ServiceResult<CashFlowReportDto>.Failure($"Kỳ kế toán {month}/{year} không tồn tại.");

        var previousPeriod = await _fiscalPeriodRepository.GetByYearMonthAsync(year, month - 1, cancellationToken);

        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        return await BuildCashFlowReportAsync(period, previousPeriod, fromDate, toDate, cancellationToken);
    }

    public async Task<ServiceResult<CashFlowReportDto>> GetCashFlowReportByBankAccountAsync(Guid bankAccountId, int year, int month, CancellationToken cancellationToken = default)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, cancellationToken);
        if (bankAccount == null)
            return ServiceResult<CashFlowReportDto>.Failure("Tài khoản ngân hàng không tồn tại.");

        var period = await _fiscalPeriodRepository.GetByYearMonthAsync(year, month, cancellationToken);
        if (period == null)
            return ServiceResult<CashFlowReportDto>.Failure($"Kỳ kế toán {month}/{year} không tồn tại.");

        var previousPeriod = await _fiscalPeriodRepository.GetByYearMonthAsync(year, month - 1, cancellationToken);

        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        return await BuildCashFlowReportAsync(period, previousPeriod, fromDate, toDate, cancellationToken);
    }

    private async Task<ServiceResult<CashFlowReportDto>> BuildCashFlowReportAsync(
        FiscalPeriod period,
        FiscalPeriod? previousPeriod,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var cashAccountCodes = new[] { 111, 112, 113 };
        var accounts = (await _accountRepository.GetAllActiveAsync(cancellationToken))
            .Where(a => cashAccountCodes.Contains(a.AccountNumber))
            .ToList();

        var accountBalances = previousPeriod != null
            ? (await _accountBalanceRepository.GetByPeriodAsync(previousPeriod.Id, cancellationToken)).ToList()
            : new List<AccountBalance>();

        var currentEntries = await _ledgerRepository.GetByFiscalPeriodAsync(period.Id, cancellationToken);

        var openingBalance = accountBalances
            .Where(b => accounts.Any(a => a.Id == b.AccountId))
            .Sum(b => b.ClosingDebit - b.ClosingCredit);

        var currentPeriodEntries = currentEntries.Where(e => accounts.Any(a => a.Id == e.AccountId));
        var closingBalance = openingBalance + currentPeriodEntries.Sum(e => e.DebitAmount - e.CreditAmount);

        var accountDict = accounts.ToDictionary(a => a.Id);
        var details = currentPeriodEntries
            .GroupBy(e => new { e.AccountCode, AccountName = e.Account?.Name ?? e.AccountCode })
            .Select(g => new CashFlowDetailDto
            {
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.AccountName,
                ActivityType = GetActivityType(g.Key.AccountCode),
                Amount = g.Sum(e => e.DebitAmount - e.CreditAmount)
            })
            .Where(d => d.Amount != 0)
            .ToList();

        var (operating, investing, financing) = ClassifyCashFlows(currentPeriodEntries);

        var result = new CashFlowReportDto
        {
            Year = period.Year,
            Month = period.Month,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningCashAndBankBalance = openingBalance,
            ClosingCashAndBankBalance = closingBalance,
            Activities = new CashFlowActivitiesDto
            {
                CashFlowsFromOperatingActivities = operating,
                CashFlowsFromInvestingActivities = investing,
                CashFlowsFromFinancingActivities = financing,
                NetCashFlow = operating + investing + financing
            },
            Details = details
        };

        return ServiceResult<CashFlowReportDto>.Success(result);
    }

    private static (decimal operating, decimal investing, decimal financing) ClassifyCashFlows(IEnumerable<LedgerEntry> entries)
    {
        var operating = entries.Where(e => IsOperatingActivity(e.AccountCode)).Sum(e => e.DebitAmount - e.CreditAmount);
        var investing = entries.Where(e => IsInvestingActivity(e.AccountCode)).Sum(e => e.DebitAmount - e.CreditAmount);
        var financing = entries.Where(e => IsFinancingActivity(e.AccountCode)).Sum(e => e.DebitAmount - e.CreditAmount);

        return (operating, investing, financing);
    }

    private static bool IsOperatingActivity(string accountCode)
    {
        return int.TryParse(accountCode, out var num) && (num >= 100 && num < 400);
    }

    private static bool IsInvestingActivity(string accountCode)
    {
        return int.TryParse(accountCode, out var num) && num >= 200 && num < 210;
    }

    private static bool IsFinancingActivity(string accountCode)
    {
        return int.TryParse(accountCode, out var num) && num >= 400 && num < 500;
    }

    private static string GetActivityType(string accountCode)
    {
        if (IsFinancingActivity(accountCode)) return "Financing";
        if (IsInvestingActivity(accountCode)) return "Investing";
        return "Operating";
    }
}