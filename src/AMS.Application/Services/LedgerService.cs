using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class LedgerService : ILedgerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IChartOfAccountsRepository _accountRepository;

    public LedgerService(
        IUnitOfWork unitOfWork,
        ILedgerRepository ledgerRepository,
        IVoucherRepository voucherRepository,
        IChartOfAccountsRepository accountRepository)
    {
        _unitOfWork = unitOfWork;
        _ledgerRepository = ledgerRepository;
        _voucherRepository = voucherRepository;
        _accountRepository = accountRepository;
    }

    public async Task<LedgerEntryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _ledgerRepository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PagedResult<LedgerEntryDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _ledgerRepository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return PagedResult<LedgerEntryDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<LedgerEntryDto>> GetByPeriodAsync(Guid fiscalPeriodId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _ledgerRepository.GetByFiscalPeriodPagedAsync(fiscalPeriodId, page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return PagedResult<LedgerEntryDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<LedgerEntryDto>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var entities = await _ledgerRepository.GetByAccountAsync(accountId, cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LedgerEntryDto>> GetByVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        var entities = await _ledgerRepository.GetByVoucherAsync(voucherId, cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LedgerSummaryDto>> GetSummaryAsync(Guid fiscalPeriodId, CancellationToken cancellationToken = default)
    {
        var summaries = await _ledgerRepository.GetSummaryAsync(fiscalPeriodId, cancellationToken);
        return summaries.Select(MapToSummaryDto);
    }

    public async Task<ServiceResult<LedgerEntryDto>> CreateAsync(CreateLedgerEntryDto dto, CancellationToken cancellationToken = default)
    {
        var errors = Validate(dto);
        if (errors.Any())
            return ServiceResult<LedgerEntryDto>.Failure(errors);

        if (dto.DebitAmount == 0 && dto.CreditAmount == 0)
            return ServiceResult<LedgerEntryDto>.Failure("Số tiền nợ hoặc có phải lớn hơn 0.");

        if (dto.DebitAmount > 0 && dto.CreditAmount > 0)
            return ServiceResult<LedgerEntryDto>.Failure("Chỉ được hạch toán nợ hoặc có, không được hạch toán cả hai.");

        var account = await _accountRepository.GetByIdAsync(dto.AccountId, cancellationToken);
        if (account == null)
            return ServiceResult<LedgerEntryDto>.Failure("Tài khoản không tồn tại.");

        if (!account.AllowEntry)
            return ServiceResult<LedgerEntryDto>.Failure($"Tài khoản '{account.Code}' không cho phép hạch toán.");

        var entity = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            FiscalPeriodId = dto.FiscalPeriodId,
            VoucherId = dto.VoucherId,
            VoucherNo = dto.VoucherNo,
            VoucherDate = dto.VoucherDate,
            AccountId = dto.AccountId,
            AccountCode = dto.AccountCode,
            DebitAmount = dto.DebitAmount,
            CreditAmount = dto.CreditAmount,
            Description = dto.Description,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            PartnerId = dto.PartnerId,
            PartnerType = dto.PartnerType,
            CostCenter = dto.CostCenter,
            ProjectCode = dto.ProjectCode,
            ContractNo = dto.ContractNo,
            IsAdjustEntry = dto.IsAdjustEntry,
            RefVoucherNo = dto.RefVoucherNo,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        };

        await _ledgerRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<LedgerEntryDto>.Success(MapToDto(entity));
    }

    public async Task<ServiceResult> CreateFromVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdWithLinesAsync(voucherId, cancellationToken);
        if (voucher == null)
            return ServiceResult.Failure("Không tìm thấy chứng từ.");

        if (voucher.Status != VoucherStatus.Approved && voucher.Status != VoucherStatus.Posted)
            return ServiceResult.Failure("Chỉ được hạch toán chứng từ đã duyệt.");

        var existingEntries = await _ledgerRepository.GetByVoucherAsync(voucherId, cancellationToken);
        if (existingEntries.Any())
            return ServiceResult.Failure("Chứng từ đã được hạch toán.");

        var entries = voucher.Lines.Select(line => new LedgerEntry
        {
            Id = Guid.NewGuid(),
            FiscalPeriodId = voucher.FiscalPeriodId,
            VoucherId = voucher.Id,
            VoucherNo = voucher.VoucherNo,
            VoucherDate = voucher.VoucherDate,
            AccountId = line.AccountId,
            AccountCode = "",
            DebitAmount = line.DebitAmount,
            CreditAmount = line.CreditAmount,
            Description = line.Description,
            CurrencyCode = voucher.CurrencyCode ?? "VND",
            ExchangeRate = voucher.ExchangeRate,
            PartnerId = line.CustomerId ?? line.VendorId,
            PartnerType = line.CustomerId.HasValue ? "Customer" : (line.VendorId.HasValue ? "Vendor" : null),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        }).ToList();

        await _ledgerRepository.AddRangeAsync(entries, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private static List<string> Validate(CreateLedgerEntryDto dto)
    {
        var errors = new List<string>();
        if (dto.FiscalPeriodId == Guid.Empty)
            errors.Add("Kỳ kế toán không hợp lệ.");
        if (string.IsNullOrWhiteSpace(dto.VoucherNo))
            errors.Add("Số chứng từ không được để trống.");
        if (dto.AccountId == Guid.Empty)
            errors.Add("Tài khoản không hợp lệ.");
        return errors;
    }

    private static LedgerEntryDto MapToDto(LedgerEntry entity) => new()
    {
        Id = entity.Id,
        FiscalPeriodId = entity.FiscalPeriodId,
        VoucherId = entity.VoucherId,
        VoucherNo = entity.VoucherNo,
        VoucherDate = entity.VoucherDate,
        AccountId = entity.AccountId,
        AccountCode = entity.AccountCode,
        DebitAmount = entity.DebitAmount,
        CreditAmount = entity.CreditAmount,
        Description = entity.Description,
        CurrencyCode = entity.CurrencyCode,
        ExchangeRate = entity.ExchangeRate,
        AmountInBaseCurrency = entity.AmountInBaseCurrency,
        PartnerId = entity.PartnerId,
        PartnerType = entity.PartnerType,
        CostCenter = entity.CostCenter,
        ProjectCode = entity.ProjectCode,
        ContractNo = entity.ContractNo,
        IsAdjustEntry = entity.IsAdjustEntry,
        RefVoucherNo = entity.RefVoucherNo,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy
    };

    private static LedgerSummaryDto MapToSummaryDto(LedgerSummary entity) => new()
    {
        AccountId = entity.AccountId,
        AccountCode = entity.AccountCode,
        AccountName = entity.AccountName,
        OpeningDebit = entity.OpeningDebit,
        OpeningCredit = entity.OpeningCredit,
        DebitTurnover = entity.PeriodDebit,
        CreditTurnover = entity.PeriodCredit,
        ClosingDebit = entity.ClosingDebit,
        ClosingCredit = entity.ClosingCredit
    };
}