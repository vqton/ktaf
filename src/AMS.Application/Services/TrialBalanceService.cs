using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

/// <summary>
/// Service for trial balance and month-end closing operations.
/// </summary>
public class TrialBalanceService : ITrialBalanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IFiscalPeriodRepository _fiscalPeriodRepository;
    private readonly IAccountBalanceRepository _accountBalanceRepository;
    private readonly IChartOfAccountsRepository _accountRepository;

    public TrialBalanceService(
        IUnitOfWork unitOfWork,
        ILedgerRepository ledgerRepository,
        IFiscalPeriodRepository fiscalPeriodRepository,
        IAccountBalanceRepository accountBalanceRepository,
        IChartOfAccountsRepository accountRepository)
    {
        _unitOfWork = unitOfWork;
        _ledgerRepository = ledgerRepository;
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _accountBalanceRepository = accountBalanceRepository;
        _accountRepository = accountRepository;
    }

    public async Task<IEnumerable<LedgerSummaryDto>> GetTrialBalanceAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        var period = await _fiscalPeriodRepository.GetByIdAsync(fiscalPeriodId, cancellationToken);
        if (period == null)
            return Enumerable.Empty<LedgerSummaryDto>();

        var previousPeriod = await _fiscalPeriodRepository.GetByYearMonthAsync(period.Year, period.Month - 1, cancellationToken);

        var previousBalances = previousPeriod != null
            ? (await _accountBalanceRepository.GetByPeriodAsync(previousPeriod.Id, cancellationToken)).ToDictionary(b => b.AccountId)
            : new Dictionary<Guid, AccountBalance>();

        var currentEntries = await _ledgerRepository.GetByFiscalPeriodAsync(fiscalPeriodId, cancellationToken);
        var currentPeriodMovements = currentEntries
            .GroupBy(e => new { e.AccountId, e.AccountCode })
            .ToDictionary(
                g => g.Key.AccountId,
                g => new { Debit = g.Sum(e => e.DebitAmount), Credit = g.Sum(e => e.CreditAmount) });

        var accountIds = previousBalances.Keys.Union(currentPeriodMovements.Keys).ToList();
        var accounts = (await _accountRepository.GetByIdsAsync(accountIds, cancellationToken)).ToDictionary(a => a.Id, a => a.Name);

        var result = new List<LedgerSummaryDto>();
        foreach (var accountId in accountIds)
        {
            var prev = previousBalances.GetValueOrDefault(accountId);
            var curr = currentPeriodMovements.GetValueOrDefault(accountId);

            var openingDebit = prev?.ClosingDebit ?? 0;
            var openingCredit = prev?.ClosingCredit ?? 0;
            var periodDebit = curr?.Debit ?? 0;
            var periodCredit = curr?.Credit ?? 0;

            var netMovement = periodDebit - periodCredit;
            decimal closingDebit, closingCredit;

            if (openingDebit > 0 || openingCredit > 0)
            {
                var totalDebit = openingDebit + netMovement;
                if (totalDebit >= 0)
                {
                    closingDebit = totalDebit;
                    closingCredit = 0;
                }
                else
                {
                    closingDebit = 0;
                    closingCredit = Math.Abs(totalDebit);
                }
            }
            else
            {
                if (netMovement >= 0)
                {
                    closingDebit = 0;
                    closingCredit = netMovement;
                }
                else
                {
                    closingDebit = Math.Abs(netMovement);
                    closingCredit = 0;
                }
            }

            result.Add(new LedgerSummaryDto
            {
                AccountId = accountId,
                AccountCode = prev?.AccountCode ?? currentPeriodMovements.GetValueOrDefault(accountId)?.ToString() ?? "",
                AccountName = accounts.GetValueOrDefault(accountId, ""),
                OpeningDebit = openingDebit,
                OpeningCredit = openingCredit,
                DebitTurnover = periodDebit,
                CreditTurnover = periodCredit,
                ClosingDebit = closingDebit,
                ClosingCredit = closingCredit
            });
        }

        return result.OrderBy(r => r.AccountCode);
    }

    public async Task<ServiceResult> UpdateAccountBalancesAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        var period = await _fiscalPeriodRepository.GetByIdAsync(fiscalPeriodId, cancellationToken);
        if (period == null)
            return ServiceResult.Failure("Kỳ kế toán không tồn tại.");

        if (period.Status != FiscalPeriodStatus.Open && period.Status != FiscalPeriodStatus.Closed)
            return ServiceResult.Failure("Kỳ kế toán đã bị khóa.");

        var previousPeriod = await _fiscalPeriodRepository.GetByYearMonthAsync(period.Year, period.Month - 1, cancellationToken);
        var previousBalances = previousPeriod != null
            ? (await _accountBalanceRepository.GetByPeriodAsync(previousPeriod.Id, cancellationToken)).ToDictionary(b => b.AccountId, b => b.ClosingDebit)
            : new Dictionary<Guid, decimal>();

        var currentEntries = await _ledgerRepository.GetByFiscalPeriodAsync(fiscalPeriodId, cancellationToken);
        var periodMovements = currentEntries
            .GroupBy(e => e.AccountId)
            .ToDictionary(g => g.Key, g => new { Debit = g.Sum(e => e.DebitAmount), Credit = g.Sum(e => e.CreditAmount) });

        var allAccountIds = previousBalances.Keys.Union(periodMovements.Keys).ToList();
        var accounts = (await _accountRepository.GetByIdsAsync(allAccountIds, cancellationToken)).ToDictionary(a => a.Id, a => a.Code);

        var balancesToUpdate = new List<AccountBalance>();

        foreach (var accountId in allAccountIds)
        {
            var existingBalance = await _accountBalanceRepository.GetByPeriodAndAccountAsync(fiscalPeriodId, accountId, cancellationToken);
            var openingBalance = previousBalances.GetValueOrDefault(accountId, 0);
            var movement = periodMovements.GetValueOrDefault(accountId);
            var periodDebit = movement?.Debit ?? 0;
            var periodCredit = movement?.Credit ?? 0;

            var closingBalance = openingBalance + periodDebit - periodCredit;

            if (existingBalance == null)
            {
                existingBalance = new AccountBalance
                {
                    Id = Guid.NewGuid(),
                    FiscalPeriodId = fiscalPeriodId,
                    AccountId = accountId,
                    AccountCode = accounts.GetValueOrDefault(accountId, ""),
                    OpeningDebit = openingBalance > 0 ? openingBalance : 0,
                    OpeningCredit = openingBalance < 0 ? Math.Abs(openingBalance) : 0,
                    PeriodDebit = periodDebit,
                    PeriodCredit = periodCredit,
                    ClosingDebit = closingBalance > 0 ? closingBalance : 0,
                    ClosingCredit = closingBalance < 0 ? Math.Abs(closingBalance) : 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    IsDeleted = false
                };
                await _accountBalanceRepository.AddAsync(existingBalance, cancellationToken);
            }
            else
            {
                existingBalance.OpeningDebit = openingBalance > 0 ? openingBalance : 0;
                existingBalance.OpeningCredit = openingBalance < 0 ? Math.Abs(openingBalance) : 0;
                existingBalance.PeriodDebit = periodDebit;
                existingBalance.PeriodCredit = periodCredit;
                existingBalance.ClosingDebit = closingBalance > 0 ? closingBalance : 0;
                existingBalance.ClosingCredit = closingBalance < 0 ? Math.Abs(closingBalance) : 0;
                existingBalance.ModifiedAt = DateTime.UtcNow;
                balancesToUpdate.Add(existingBalance);
            }
        }

        if (balancesToUpdate.Any())
        {
            await _accountBalanceRepository.UpdateRangeAsync(balancesToUpdate, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CarryForwardOpeningBalancesAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        var period = await _fiscalPeriodRepository.GetByIdAsync(fiscalPeriodId, cancellationToken);
        if (period == null)
            return ServiceResult.Failure("Kỳ kế toán không tồn tại.");

        var previousPeriod = await _fiscalPeriodRepository.GetByYearMonthAsync(period.Year, period.Month - 1, cancellationToken);
        if (previousPeriod == null)
            return ServiceResult.Success();

        var previousBalances = await _accountBalanceRepository.GetByPeriodAsync(previousPeriod.Id, cancellationToken);
        if (!previousBalances.Any())
            return ServiceResult.Success();

        var accounts = (await _accountRepository.GetAllActiveAsync(cancellationToken)).ToDictionary(a => a.Id, a => a.Code);

        foreach (var prevBalance in previousBalances)
        {
            var existingBalance = await _accountBalanceRepository.GetByPeriodAndAccountAsync(fiscalPeriodId, prevBalance.AccountId, cancellationToken);
            if (existingBalance == null)
            {
                var newBalance = new AccountBalance
                {
                    Id = Guid.NewGuid(),
                    FiscalPeriodId = fiscalPeriodId,
                    AccountId = prevBalance.AccountId,
                    AccountCode = accounts.GetValueOrDefault(prevBalance.AccountId, prevBalance.AccountCode),
                    OpeningDebit = prevBalance.ClosingDebit,
                    OpeningCredit = prevBalance.ClosingCredit,
                    PeriodDebit = 0,
                    PeriodCredit = 0,
                    ClosingDebit = prevBalance.ClosingDebit,
                    ClosingCredit = prevBalance.ClosingCredit,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    IsDeleted = false
                };
                await _accountBalanceRepository.AddAsync(newBalance, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
