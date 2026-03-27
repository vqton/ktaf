using AMS.Application.Accounting.Vouchers.DTOs;
using AMS.Application.Accounting.Vouchers.Interfaces;
using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.Interfaces;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Accounting.Vouchers.Services;

/// <summary>
/// Service implementation for managing accounting vouchers.
/// </summary>
public class VoucherService : IVoucherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoucherRepository _voucherRepository;
    private readonly ILedgerService _ledgerService;

    /// <summary>
    /// Initializes a new instance of the VoucherService class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance.</param>
    /// <param name="voucherRepository">The voucher repository instance.</param>
    /// <param name="ledgerService">The ledger service instance.</param>
    public VoucherService(
        IUnitOfWork unitOfWork,
        IVoucherRepository voucherRepository,
        ILedgerService ledgerService)
    {
        _unitOfWork = unitOfWork;
        _voucherRepository = voucherRepository;
        _ledgerService = ledgerService;
    }

    /// <inheritdoc />
    public async Task<VoucherDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdWithLinesAsync(id, cancellationToken);
        return voucher == null ? null : MapToDto(voucher);
    }

    /// <inheritdoc />
    public async Task<PagedResult<VoucherDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (vouchers, totalCount) = await _voucherRepository.GetAllPagedAsync(page, pageSize, cancellationToken);

        var items = vouchers.Select(MapToDto).ToList();
        return PagedResult<VoucherDto>.Create(items, totalCount, page, pageSize);
    }

    /// <inheritdoc />
    public async Task<PagedResult<VoucherDto>> GetByPeriodAsync(Guid fiscalPeriodId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (vouchers, totalCount) = await _voucherRepository.GetByPeriodPagedAsync(fiscalPeriodId, page, pageSize, cancellationToken);

        var items = vouchers.Select(MapToDto).ToList();
        return PagedResult<VoucherDto>.Create(items, totalCount, page, pageSize);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<VoucherDto>> CreateAsync(CreateVoucherDto dto, CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateCreateVoucher(dto);
        if (validationErrors.Any())
            return ServiceResult<VoucherDto>.Failure(validationErrors);

        if (dto.Lines.Count < 2)
            return ServiceResult<VoucherDto>.Failure("Chứng từ phải có ít nhất 2 dòng định khoản.");

        var totalDebit = dto.Lines.Sum(l => l.DebitAmount);
        var totalCredit = dto.Lines.Sum(l => l.CreditAmount);

        if (totalDebit != totalCredit)
            return ServiceResult<VoucherDto>.Failure($"Tổng nợ ({totalDebit:N0}) phải bằng tổng có ({totalCredit:N0}).");

        var voucher = new Domain.Entities.Voucher
        {
            Id = Guid.NewGuid(),
            VoucherNo = await GenerateVoucherNo(dto.Type, dto.FiscalPeriodId, cancellationToken),
            Type = Enum.Parse<VoucherType>(dto.Type),
            VoucherDate = dto.VoucherDate,
            FiscalPeriodId = dto.FiscalPeriodId,
            Description = dto.Description,
            Status = VoucherStatus.Draft,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        };

        foreach (var line in dto.Lines)
        {
            voucher.Lines.Add(new Domain.Entities.VoucherLine
            {
                Id = Guid.NewGuid(),
                VoucherId = voucher.Id,
                AccountId = line.AccountId,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                Description = line.Description,
                CustomerId = line.CustomerId,
                VendorId = line.VendorId,
                IsExciseTaxLine = line.IsExciseTaxLine,
                CitAdjFlag = line.CitAdjFlag
            });
        }

        await _voucherRepository.AddAsync(voucher, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<VoucherDto>.Success(MapToDto(voucher));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<VoucherDto>> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (voucher == null)
            return ServiceResult<VoucherDto>.Failure("Không tìm thấy chứng từ.");

        if (voucher.FiscalPeriod == null)
            return ServiceResult<VoucherDto>.Failure("Kỳ kế toán không tồn tại.");

        if (voucher.FiscalPeriod.Status != FiscalPeriodStatus.Open)
            return ServiceResult<VoucherDto>.Failure($"Kỳ kế toán {voucher.FiscalPeriod.Year}/{voucher.FiscalPeriod.Month} đang đóng, không thể trình duyệt.");

        try
        {
            voucher.Submit();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<VoucherDto>.Success(MapToDto(voucher));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return ServiceResult<VoucherDto>.Failure(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<VoucherDto>> ApproveAsync(Guid id, string approverId, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdAsync(id, cancellationToken);
        if (voucher == null)
            return ServiceResult<VoucherDto>.Failure("Không tìm thấy chứng từ.");

        if (voucher.FiscalPeriod == null)
            return ServiceResult<VoucherDto>.Failure("Kỳ kế toán không tồn tại.");

        if (voucher.FiscalPeriod.Status != FiscalPeriodStatus.Open)
            return ServiceResult<VoucherDto>.Failure($"Kỳ kế toán {voucher.FiscalPeriod.Year}/{voucher.FiscalPeriod.Month} đang đóng, không thể duyệt.");

        try
        {
            voucher.Approve(approverId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<VoucherDto>.Success(MapToDto(voucher));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return ServiceResult<VoucherDto>.Failure(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<VoucherDto>> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdAsync(id, cancellationToken);
        if (voucher == null)
            return ServiceResult<VoucherDto>.Failure("Không tìm thấy chứng từ.");

        if (voucher.FiscalPeriod == null)
            return ServiceResult<VoucherDto>.Failure("Kỳ kế toán không tồn tại.");

        if (voucher.FiscalPeriod.Status != FiscalPeriodStatus.Open)
            return ServiceResult<VoucherDto>.Failure($"Kỳ kế toán {voucher.FiscalPeriod.Year}/{voucher.FiscalPeriod.Month} đang đóng, không thể từ chối.");

        try
        {
            voucher.Reject(reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<VoucherDto>.Success(MapToDto(voucher));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return ServiceResult<VoucherDto>.Failure(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<VoucherDto>> PostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (voucher == null)
            return ServiceResult<VoucherDto>.Failure("Không tìm thấy chứng từ.");

        if (voucher.FiscalPeriod == null)
            return ServiceResult<VoucherDto>.Failure("Kỳ kế toán không tồn tại.");

        if (voucher.FiscalPeriod.Status != FiscalPeriodStatus.Open)
            return ServiceResult<VoucherDto>.Failure($"Kỳ kế toán {voucher.FiscalPeriod.Year}/{voucher.FiscalPeriod.Month} đang đóng, không thể hạch toán.");

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            voucher.Post();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var ledgerResult = await _ledgerService.CreateFromVoucherAsync(id, cancellationToken);
            if (!ledgerResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ServiceResult<VoucherDto>.Failure($"Hạch toán thất bại: {ledgerResult.ErrorMessage}");
            }

            await _unitOfWork.CommitTransactionAsync();
            return ServiceResult<VoucherDto>.Success(MapToDto(voucher));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ServiceResult<VoucherDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ServiceResult<VoucherDto>.Failure($"Lỗi hệ thống: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<VoucherDto>> ReverseAsync(Guid id, string reversedById, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (voucher == null)
            return ServiceResult<VoucherDto>.Failure("Không tìm thấy chứng từ.");

        if (voucher.FiscalPeriod == null)
            return ServiceResult<VoucherDto>.Failure("Kỳ kế toán không tồn tại.");

        if (voucher.FiscalPeriod.Status != FiscalPeriodStatus.Open)
            return ServiceResult<VoucherDto>.Failure($"Kỳ kế toán {voucher.FiscalPeriod.Year}/{voucher.FiscalPeriod.Month} đang đóng, không thể đảo.");

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            voucher.Reverse(reversedById);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reversalVoucherNo = $"{voucher.VoucherNo}-R";
            var ledgerResult = await _ledgerService.CreateReversalEntriesAsync(
                id,
                reversalVoucherNo,
                voucher.VoucherDate,
                cancellationToken);

            if (!ledgerResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ServiceResult<VoucherDto>.Failure($"Đảo bút toán thất bại: {ledgerResult.ErrorMessage}");
            }

            await _unitOfWork.CommitTransactionAsync();
            return ServiceResult<VoucherDto>.Success(MapToDto(voucher));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ServiceResult<VoucherDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ServiceResult<VoucherDto>.Failure($"Lỗi hệ thống: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates the create voucher DTO.
    /// </summary>
    /// <param name="dto">The DTO to validate.</param>
    /// <returns>List of validation error messages.</returns>
    private List<string> ValidateCreateVoucher(CreateVoucherDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Type))
            errors.Add("Loại chứng từ không được để trống.");

        if (dto.VoucherDate == default)
            errors.Add("Ngày chứng từ không hợp lệ.");

        if (dto.FiscalPeriodId == Guid.Empty)
            errors.Add("Kỳ kế toán không hợp lệ.");

        if (dto.Lines == null || !dto.Lines.Any())
            errors.Add("Chứng từ phải có ít nhất 1 dòng định khoản.");

        return errors;
    }

    /// <summary>
    /// Generates a unique voucher number.
    /// </summary>
    /// <param name="type">The voucher type.</param>
    /// <param name="fiscalPeriodId">The fiscal period ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated voucher number.</returns>
    private async Task<string> GenerateVoucherNo(string type, Guid fiscalPeriodId, CancellationToken cancellationToken)
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var sequence = await _voucherRepository.GetNextSequenceAsync(fiscalPeriodId, type, cancellationToken);
        return $"{type}-{year:D4}-{month:D2}-{sequence:D5}";
    }

    /// <summary>
    /// Maps a Voucher entity to a VoucherDto.
    /// </summary>
    /// <param name="voucher">The entity to map.</param>
    /// <returns>The mapped DTO.</returns>
    private static VoucherDto MapToDto(Domain.Entities.Voucher voucher) => new()
    {
        Id = voucher.Id,
        VoucherNo = voucher.VoucherNo,
        Type = voucher.Type.ToString(),
        VoucherDate = voucher.VoucherDate,
        FiscalPeriodId = voucher.FiscalPeriodId,
        Description = voucher.Description,
        Status = voucher.Status.ToString(),
        TotalDebit = voucher.TotalDebit,
        TotalCredit = voucher.TotalCredit,
        CurrencyCode = voucher.CurrencyCode,
        ExchangeRate = voucher.ExchangeRate,
        CreatedAt = voucher.CreatedAt,
        CreatedBy = voucher.CreatedBy,
        Lines = voucher.Lines.Select(l => new VoucherLineDto
        {
            Id = l.Id,
            AccountId = l.AccountId,
            DebitAmount = l.DebitAmount,
            CreditAmount = l.CreditAmount,
            Description = l.Description,
            CustomerId = l.CustomerId,
            VendorId = l.VendorId,
            IsExciseTaxLine = l.IsExciseTaxLine,
            CitAdjFlag = l.CitAdjFlag
        }).ToList()
    };

    /// <inheritdoc />
    public async Task<ServiceResult<VoucherDto>> UpdateAsync(Guid id, VoucherDto voucherDto, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (voucher == null)
            return ServiceResult<VoucherDto>.Failure("Không tìm thấy chứng từ.");

        if (voucher.Status != VoucherStatus.Draft)
            return ServiceResult<VoucherDto>.Failure("Chỉ có thể sửa chứng từ ở trạng thái nháp.");

        var totalDebit = voucherDto.Lines.Sum(l => l.DebitAmount);
        var totalCredit = voucherDto.Lines.Sum(l => l.CreditAmount);

        if (totalDebit != totalCredit)
            return ServiceResult<VoucherDto>.Failure($"Tổng nợ ({totalDebit:N0}) phải bằng tổng có ({totalCredit:N0}).");

        try
        {
            voucher.Description = voucherDto.Description;
            voucher.VoucherDate = voucherDto.VoucherDate;
            voucher.TotalDebit = totalDebit;
            voucher.TotalCredit = totalCredit;

            voucher.Lines.Clear();
            foreach (var line in voucherDto.Lines)
            {
                voucher.Lines.Add(new Domain.Entities.VoucherLine
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucher.Id,
                    AccountId = line.AccountId,
                    DebitAmount = line.DebitAmount,
                    CreditAmount = line.CreditAmount,
                    Description = line.Description,
                    CustomerId = line.CustomerId,
                    VendorId = line.VendorId,
                    IsExciseTaxLine = line.IsExciseTaxLine,
                    CitAdjFlag = line.CitAdjFlag
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<VoucherDto>.Success(MapToDto(voucher));
        }
        catch (Exception ex)
        {
            return ServiceResult<VoucherDto>.Failure($"Lỗi khi cập nhật: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdAsync(id, cancellationToken);
        if (voucher == null)
            return ServiceResult.Failure("Không tìm thấy chứng từ.");

        if (voucher.Status != VoucherStatus.Draft)
            return ServiceResult.Failure("Chỉ có thể xóa chứng từ ở trạng thái nháp.");

        try
        {
            voucher.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"Lỗi khi xóa: {ex.Message}");
        }
    }
}