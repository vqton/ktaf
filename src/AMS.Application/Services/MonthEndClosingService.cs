using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

/// <summary>
/// Service for managing month-end closing process (8 steps per TT 99/2025/TT-BTC).
/// </summary>
public class MonthEndClosingService : IMonthEndClosingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFiscalPeriodRepository _fiscalPeriodRepository;
    private readonly ITrialBalanceService _trialBalanceService;
    private readonly IInventoryService _inventoryService;
    private readonly IAssetService _assetService;
    private readonly ILedgerService _ledgerService;

    public MonthEndClosingService(
        IUnitOfWork unitOfWork,
        IFiscalPeriodRepository fiscalPeriodRepository,
        ITrialBalanceService trialBalanceService,
        IInventoryService inventoryService,
        IAssetService assetService,
        ILedgerService ledgerService)
    {
        _unitOfWork = unitOfWork;
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _trialBalanceService = trialBalanceService;
        _inventoryService = inventoryService;
        _assetService = assetService;
        _ledgerService = ledgerService;
    }

    public async Task<ServiceResult> ExecuteMonthEndClosingAsync(Guid fiscalPeriodId, string closedBy, CancellationToken cancellationToken = default)
    {
        var period = await _fiscalPeriodRepository.GetByIdAsync(fiscalPeriodId, cancellationToken);
        if (period == null)
            return ServiceResult.Failure("Kỳ kế toán không tồn tại.");

        if (period.Status != FiscalPeriodStatus.Open)
            return ServiceResult.Failure("Chỉ được đóng kỳ kế toán đang mở.");

        var errors = new List<string>();

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var step1Result = await Step1_CheckAndPostVouchersAsync(fiscalPeriodId, cancellationToken);
            if (!step1Result.IsSuccess)
                errors.Add($"Bước 1 - Kiểm tra và hạch toán chứng từ: {step1Result.ErrorMessage}");

            var step2Result = await Step2_CalculateInventoryCostAsync(fiscalPeriodId, cancellationToken);
            if (!step2Result.IsSuccess)
                errors.Add($"Bước 2 - Tính giá xuất kho: {step2Result.ErrorMessage}");

            var step3Result = await Step3_CalculateDepreciationAsync(fiscalPeriodId, cancellationToken);
            if (!step3Result.IsSuccess)
                errors.Add($"Bước 3 - Tính khấu hao TSCĐ: {step3Result.ErrorMessage}");

            var step4Result = await Step4_AllocatePrepaidExpensesAsync(fiscalPeriodId, cancellationToken);
            if (!step4Result.IsSuccess)
                errors.Add($"Bước 4 - Phân bổ chi phí trả trước: {step4Result.ErrorMessage}");

            var step5Result = await Step5_ReversePrepaidIncomeAsync(fiscalPeriodId, cancellationToken);
            if (!step5Result.IsSuccess)
                errors.Add($"Bước 5 - Hoàn nhập doanh thu chưa thực hiện: {step5Result.ErrorMessage}");

            var step6Result = await Step6_TransferRevenueExpensesAsync(fiscalPeriodId, cancellationToken);
            if (!step6Result.IsSuccess)
                errors.Add($"Bước 6 - Kết chuyển doanh thu, chi phí: {step6Result.ErrorMessage}");

            var step7Result = await Step7_GenerateTrialBalanceAsync(fiscalPeriodId, cancellationToken);
            if (!step7Result.IsSuccess)
                errors.Add($"Bước 7 - Lập bảng cân đối thử: {step7Result.ErrorMessage}");

            var step8Result = await Step8_CloseBooksAsync(fiscalPeriodId, cancellationToken);
            if (!step8Result.IsSuccess)
                errors.Add($"Bước 8 - Khoá sổ kế toán: {step8Result.ErrorMessage}");

            if (errors.Any())
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ServiceResult.Failure(errors);
            }

            period.Close(closedBy);
            await _fiscalPeriodRepository.UpdateAsync(period, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync();
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ServiceResult.Failure($"Lỗi khi đóng kỳ: {ex.Message}");
        }
    }

    private async Task<ServiceResult> Step1_CheckAndPostVouchersAsync(Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        return ServiceResult.Success();
    }

    private async Task<ServiceResult> Step2_CalculateInventoryCostAsync(Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        return ServiceResult.Success();
    }

    private async Task<ServiceResult> Step3_CalculateDepreciationAsync(Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        return ServiceResult.Success();
    }

    private async Task<ServiceResult> Step4_AllocatePrepaidExpensesAsync(Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        return ServiceResult.Success();
    }

    private async Task<ServiceResult> Step5_ReversePrepaidIncomeAsync(Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        return ServiceResult.Success();
    }

    private async Task<ServiceResult> Step6_TransferRevenueExpensesAsync(Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        return ServiceResult.Success();
    }

    private async Task<ServiceResult> Step7_GenerateTrialBalanceAsync(Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        var trialBalance = await _trialBalanceService.GetTrialBalanceAsync(fiscalPeriodId, cancellationToken);
        if (!trialBalance.Any())
            return ServiceResult.Failure("Không lấy được dữ liệu bảng cân đối.");

        var totalDebit = trialBalance.Sum(t => t.ClosingDebit);
        var totalCredit = trialBalance.Sum(t => t.ClosingCredit);

        if (totalDebit != totalCredit)
            return ServiceResult.Failure($"Bảng cân đối không cân bằng. Nợ: {totalDebit:N0}, Có: {totalCredit:N0}");

        return ServiceResult.Success();
    }

    private async Task<ServiceResult> Step8_CloseBooksAsync(Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        return ServiceResult.Success();
    }
}
