using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Entities.DM;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class BankReconciliationService : IBankReconciliationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IBankTransactionRepository _bankTransactionRepository;
    private readonly IBankReconciliationRepository _reconciliationRepository;
    private readonly IFiscalPeriodRepository _fiscalPeriodRepository;

    public BankReconciliationService(
        IUnitOfWork unitOfWork,
        IBankAccountRepository bankAccountRepository,
        IBankTransactionRepository bankTransactionRepository,
        IBankReconciliationRepository reconciliationRepository,
        IFiscalPeriodRepository fiscalPeriodRepository)
    {
        _unitOfWork = unitOfWork;
        _bankAccountRepository = bankAccountRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _reconciliationRepository = reconciliationRepository;
        _fiscalPeriodRepository = fiscalPeriodRepository;
    }

    public async Task<ServiceResult<BankReconciliationDto>> CreateReconciliationAsync(CreateBankReconciliationDto dto)
    {
        var account = await _bankAccountRepository.GetByIdAsync(dto.BankAccountId);
        if (account == null)
            return ServiceResult<BankReconciliationDto>.Failure("Tài khoản ngân hàng không tồn tại.");

        var existing = await _reconciliationRepository.GetByAccountAndPeriodAsync(dto.BankAccountId, dto.Year, dto.Month);
        if (existing != null)
            return ServiceResult<BankReconciliationDto>.Failure($"Đã có bản đối chiếu cho kỳ {dto.Month}/{dto.Year}.");

        var fromDate = new DateTime(dto.Year, dto.Month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var transactions = await _bankTransactionRepository.GetByDateRangeAsync(dto.BankAccountId, fromDate, toDate);
        var completedTransactions = transactions.Where(t => t.Status == BankTransactionStatus.Completed).ToList();

        var bookClosingBalance = account.OpeningBalance + completedTransactions
            .Where(t => t.TransactionType == BankTransactionType.Deposit || t.TransactionType == BankTransactionType.TransferIn)
            .Sum(t => t.Amount) - completedTransactions
            .Where(t => t.TransactionType == BankTransactionType.Withdrawal || t.TransactionType == BankTransactionType.TransferOut || t.TransactionType == BankTransactionType.Fee)
            .Sum(t => t.Amount + t.FeeAmount);

        var reconciliation = new BankReconciliation
        {
            Id = Guid.NewGuid(),
            BankAccountId = dto.BankAccountId,
            Year = dto.Year,
            Month = dto.Month,
            StatementClosingBalance = 0,
            BookClosingBalance = bookClosingBalance,
            ReconciliationDate = DateTime.UtcNow,
            Status = BankReconciliationStatus.Draft,
            PreparedBy = dto.PreparedBy,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.PreparedBy,
            IsDeleted = false
        };

        await _reconciliationRepository.AddAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<BankReconciliationDto>.Success(MapToDto(reconciliation, account.AccountName));
    }

    public async Task<ServiceResult<BankReconciliationDto>> GetReconciliationByIdAsync(Guid id)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(id);
        if (reconciliation == null)
            return ServiceResult<BankReconciliationDto>.Failure("Bản đối chiếu không tồn tại.");

        var account = await _bankAccountRepository.GetByIdAsync(reconciliation.BankAccountId);
        return ServiceResult<BankReconciliationDto>.Success(MapToDto(reconciliation, account?.AccountName ?? ""));
    }

    public async Task<ServiceResult<List<BankReconciliationDto>>> GetReconciliationsByAccountAsync(Guid bankAccountId)
    {
        var reconciliations = await _reconciliationRepository.GetByBankAccountIdAsync(bankAccountId);
        var account = await _bankAccountRepository.GetByIdAsync(bankAccountId);
        var dtos = reconciliations.Select(r => MapToDto(r, account?.AccountName ?? "")).ToList();
        return ServiceResult<List<BankReconciliationDto>>.Success(dtos);
    }

    public async Task<ServiceResult<BankReconciliationDto>> ReconcileAsync(Guid reconciliationId, List<ReconciliationItemDto> items)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(reconciliationId);
        if (reconciliation == null)
            return ServiceResult<BankReconciliationDto>.Failure("Bản đối chiếu không tồn tại.");

        if (reconciliation.Status == BankReconciliationStatus.Completed)
            return ServiceResult<BankReconciliationDto>.Failure("Bản đối chiếu đã hoàn thành, không thể chỉnh sửa.");

        foreach (var item in items.Where(i => i.IsReconciled))
        {
            var transaction = await _bankTransactionRepository.GetByIdAsync(item.BankTransactionId);
            if (transaction != null)
            {
                transaction.IsReconciled = true;
                transaction.ReconciledDate = DateTime.UtcNow;
                await _bankTransactionRepository.UpdateAsync(transaction);
            }
        }

        reconciliation.ModifiedAt = DateTime.UtcNow;
        await _reconciliationRepository.UpdateAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync();

        var account = await _bankAccountRepository.GetByIdAsync(reconciliation.BankAccountId);
        return ServiceResult<BankReconciliationDto>.Success(MapToDto(reconciliation, account?.AccountName ?? ""));
    }

    public async Task<ServiceResult<BankReconciliationDto>> ApproveReconciliationAsync(Guid reconciliationId, string approvedBy)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(reconciliationId);
        if (reconciliation == null)
            return ServiceResult<BankReconciliationDto>.Failure("Bản đối chiếu không tồn tại.");

        if (reconciliation.Status == BankReconciliationStatus.Completed)
            return ServiceResult<BankReconciliationDto>.Failure("Bản đối chiếu đã hoàn thành.");

        if (Math.Abs(reconciliation.Difference) > 0)
            return ServiceResult<BankReconciliationDto>.Failure($"Còn chênh lệch {reconciliation.Difference:N0} VNĐ. Vui lòng điều chỉnh trước khi phê duyệt.");

        reconciliation.Status = BankReconciliationStatus.Completed;
        reconciliation.ApprovedBy = approvedBy;
        reconciliation.ModifiedAt = DateTime.UtcNow;

        await _reconciliationRepository.UpdateAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync();

        var account = await _bankAccountRepository.GetByIdAsync(reconciliation.BankAccountId);
        return ServiceResult<BankReconciliationDto>.Success(MapToDto(reconciliation, account?.AccountName ?? ""));
    }

    public async Task<ServiceResult<BankReconciliationDto>> CancelReconciliationAsync(Guid reconciliationId)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(reconciliationId);
        if (reconciliation == null)
            return ServiceResult<BankReconciliationDto>.Failure("Bản đối chiếu không tồn tại.");

        if (reconciliation.Status == BankReconciliationStatus.Completed)
            return ServiceResult<BankReconciliationDto>.Failure("Bản đối chiếu đã hoàn thành, không thể hủy.");

        reconciliation.Status = BankReconciliationStatus.Cancelled;
        reconciliation.ModifiedAt = DateTime.UtcNow;

        await _reconciliationRepository.UpdateAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync();

        var account = await _bankAccountRepository.GetByIdAsync(reconciliation.BankAccountId);
        return ServiceResult<BankReconciliationDto>.Success(MapToDto(reconciliation, account?.AccountName ?? ""));
    }

    public async Task<ServiceResult<ReconciliationReportDto>> GetReconciliationReportAsync(Guid bankAccountId, DateTime fromDate, DateTime toDate)
    {
        var account = await _bankAccountRepository.GetByIdAsync(bankAccountId);
        if (account == null)
            return ServiceResult<ReconciliationReportDto>.Failure("Tài khoản ngân hàng không tồn tại.");

        var transactions = await _bankTransactionRepository.GetByDateRangeAsync(bankAccountId, fromDate, toDate);
        var completedTransactions = transactions.Where(t => t.Status == BankTransactionStatus.Completed).ToList();

        var reconciledItems = completedTransactions.Where(t => t.IsReconciled).Select(t => new ReconciliationItemDto
        {
            Id = t.Id,
            BankTransactionId = t.Id,
            TransactionDate = t.TransactionDate,
            Description = t.Description,
            Amount = t.Amount,
            FeeAmount = t.FeeAmount,
            IsReconciled = t.IsReconciled
        }).ToList();

        var unreconciledItems = completedTransactions.Where(t => !t.IsReconciled).Select(t => new ReconciliationItemDto
        {
            Id = t.Id,
            BankTransactionId = t.Id,
            TransactionDate = t.TransactionDate,
            Description = t.Description,
            Amount = t.Amount,
            FeeAmount = t.FeeAmount,
            IsReconciled = t.IsReconciled
        }).ToList();

        var openingFromDate = fromDate.AddMonths(-1);
        var previousTransactions = await _bankTransactionRepository.GetByDateRangeAsync(bankAccountId, openingFromDate, fromDate.AddDays(-1));
        var previousCompleted = previousTransactions.Where(t => t.Status == BankTransactionStatus.Completed).ToList();

        var openingBookBalance = account.OpeningBalance + previousCompleted
            .Where(t => t.TransactionType == BankTransactionType.Deposit || t.TransactionType == BankTransactionType.TransferIn)
            .Sum(t => t.Amount) - previousCompleted
            .Where(t => t.TransactionType == BankTransactionType.Withdrawal || t.TransactionType == BankTransactionType.TransferOut || t.TransactionType == BankTransactionType.Fee)
            .Sum(t => t.Amount + t.FeeAmount);

        var closingBookBalance = openingBookBalance + completedTransactions
            .Where(t => t.TransactionType == BankTransactionType.Deposit || t.TransactionType == BankTransactionType.TransferIn)
            .Sum(t => t.Amount) - completedTransactions
            .Where(t => t.TransactionType == BankTransactionType.Withdrawal || t.TransactionType == BankTransactionType.TransferOut || t.TransactionType == BankTransactionType.Fee)
            .Sum(t => t.Amount + t.FeeAmount);

        var result = new ReconciliationReportDto
        {
            BankAccountId = bankAccountId,
            BankAccountName = account.AccountName,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBankBalance = openingBookBalance,
            ClosingBankBalance = closingBookBalance,
            OpeningBookBalance = openingBookBalance,
            ClosingBookBalance = closingBookBalance,
            TotalReconciledAmount = reconciledItems.Sum(i => i.Amount),
            TotalUnreconciledAmount = unreconciledItems.Sum(i => i.Amount),
            ReconciledItems = reconciledItems,
            UnreconciledItems = unreconciledItems
        };

        return ServiceResult<ReconciliationReportDto>.Success(result);
    }

    public async Task<ServiceResult> UpdateStatementBalanceAsync(Guid reconciliationId, decimal statementBalance)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(reconciliationId);
        if (reconciliation == null)
            return ServiceResult.Failure("Bản đối chiếu không tồn tại.");

        if (reconciliation.Status == BankReconciliationStatus.Completed)
            return ServiceResult.Failure("Bản đối chiếu đã hoàn thành, không thể chỉnh sửa.");

        reconciliation.StatementClosingBalance = statementBalance;
        reconciliation.ModifiedAt = DateTime.UtcNow;

        await _reconciliationRepository.UpdateAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CompleteReconciliationAsync(Guid reconciliationId, string approvedBy)
    {
        var reconciliation = await _reconciliationRepository.GetByIdAsync(reconciliationId);
        if (reconciliation == null)
            return ServiceResult.Failure("Bản đối chiếu không tồn tại.");

        if (Math.Abs(reconciliation.Difference) > 0)
            return ServiceResult.Failure($"Còn chênh lệch {reconciliation.Difference:N0} VNĐ. Vui lòng điều chỉnh trước khi hoàn thành.");

        reconciliation.Status = BankReconciliationStatus.Completed;
        reconciliation.ApprovedBy = approvedBy;
        reconciliation.ModifiedAt = DateTime.UtcNow;

        var unreconciledTransactions = await _bankTransactionRepository.GetUnreconciledAsync(reconciliation.BankAccountId);
        var period = await _fiscalPeriodRepository.GetByYearMonthAsync(reconciliation.Year, reconciliation.Month);
        if (period != null)
        {
            var periodTransactions = unreconciledTransactions.Where(t => t.TransactionDate.Month == reconciliation.Month && t.TransactionDate.Year == reconciliation.Year);
            foreach (var t in periodTransactions)
            {
                t.IsReconciled = true;
                t.ReconciledDate = DateTime.UtcNow;
                await _bankTransactionRepository.UpdateAsync(t);
            }
        }

        await _reconciliationRepository.UpdateAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Success();
    }

    private static BankReconciliationDto MapToDto(BankReconciliation entity, string accountName)
    {
        return new BankReconciliationDto
        {
            Id = entity.Id,
            BankAccountId = entity.BankAccountId,
            BankAccountName = accountName,
            Year = entity.Year,
            Month = entity.Month,
            ReconciliationDate = entity.ReconciliationDate,
            BankBalance = entity.StatementClosingBalance,
            BookBalance = entity.BookClosingBalance,
            Difference = entity.StatementClosingBalance - entity.BookClosingBalance,
            Status = entity.Status.ToString(),
            PreparedBy = entity.PreparedBy ?? "",
            ApprovedBy = entity.ApprovedBy,
            ApprovedDate = entity.ModifiedAt,
            CreatedDate = entity.CreatedAt
        };
    }
}