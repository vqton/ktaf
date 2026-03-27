using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

/// <summary>
/// Service implementation for managing bank accounts.
/// </summary>
public class BankAccountService : IBankAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IBankRepository _bankRepository;

    /// <summary>
    /// Initializes a new instance of the BankAccountService class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance.</param>
    /// <param name="bankAccountRepository">The bank account repository instance.</param>
    /// <param name="bankRepository">The bank repository instance.</param>
    public BankAccountService(
        IUnitOfWork unitOfWork,
        IBankAccountRepository bankAccountRepository,
        IBankRepository bankRepository)
    {
        _unitOfWork = unitOfWork;
        _bankAccountRepository = bankAccountRepository;
        _bankRepository = bankRepository;
    }

    /// <inheritdoc />
    public async Task<BankAccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _bankAccountRepository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<BankAccountDto?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        var entity = await _bankAccountRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BankAccountDto>> GetByBankIdAsync(Guid bankId, CancellationToken cancellationToken = default)
    {
        var entities = await _bankAccountRepository.GetByBankIdAsync(bankId, cancellationToken);
        return entities.Select(e => MapToDto(e));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BankAccountDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _bankAccountRepository.GetAllActiveAsync(cancellationToken);
        return entities.Select(e => MapToDto(e));
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<BankAccountDto> Accounts, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _bankAccountRepository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(e => MapToDto(e)).ToList();
        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<BankAccountDto?> GetPrimaryAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _bankAccountRepository.GetPrimaryAsync(cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BankAccountDto>> CreateAsync(CreateBankAccountDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreate(dto);
        if (errors.Any())
            return ServiceResult<BankAccountDto>.Failure(errors);

        var existing = await _bankAccountRepository.GetByAccountNumberAsync(dto.AccountNumber, cancellationToken);
        if (existing != null)
            return ServiceResult<BankAccountDto>.Failure($"Tài khoản ngân hàng {dto.AccountNumber} đã tồn tại.");

        var bank = await _bankRepository.GetByIdAsync(dto.BankId, cancellationToken);
        if (bank == null)
            return ServiceResult<BankAccountDto>.Failure("Ngân hàng không tồn tại.");

        var entity = new BankAccount
        {
            Id = Guid.NewGuid(),
            BankId = dto.BankId,
            AccountNumber = dto.AccountNumber,
            AccountName = dto.AccountName,
            AccountType = dto.AccountType,
            CurrencyCode = dto.CurrencyCode,
            OpeningBalance = dto.OpeningBalance,
            IsPrimary = dto.IsPrimary,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _bankAccountRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // If this is set as primary, unset any other primary accounts
        if (dto.IsPrimary)
        {
            await UnsetOtherPrimaryAccountsAsync(entity.Id, cancellationToken);
        }

        return ServiceResult<BankAccountDto>.Success(MapToDto(entity));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BankAccountDto>> UpdateAsync(UpdateBankAccountDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateUpdate(dto);
        if (errors.Any())
            return ServiceResult<BankAccountDto>.Failure(errors);

        var entity = await _bankAccountRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (entity == null)
            return ServiceResult<BankAccountDto>.Failure("Tài khoản ngân hàng không tồn tại.");

        var existing = await _bankAccountRepository.GetByAccountNumberAsync(dto.AccountNumber, cancellationToken);
        if (existing != null && existing.Id != dto.Id)
            return ServiceResult<BankAccountDto>.Failure($"Tài khoản ngân hàng {dto.AccountNumber} đã tồn tại.");

        var bank = await _bankRepository.GetByIdAsync(dto.BankId, cancellationToken);
        if (bank == null)
            return ServiceResult<BankAccountDto>.Failure("Ngân hàng không tồn tại.");

        entity.BankId = dto.BankId;
        entity.AccountNumber = dto.AccountNumber;
        entity.AccountName = dto.AccountName;
        entity.AccountType = dto.AccountType;
        entity.CurrencyCode = dto.CurrencyCode;
        entity.OpeningBalance = dto.OpeningBalance;
        entity.IsPrimary = dto.IsPrimary;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "system";

        await _bankAccountRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // If this is set as primary, unset any other primary accounts
        if (dto.IsPrimary)
        {
            await UnsetOtherPrimaryAccountsAsync(entity.Id, cancellationToken);
        }

        return ServiceResult<BankAccountDto>.Success(MapToDto(entity));
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _bankAccountRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Tài khoản ngân hàng không tồn tại.");

        // Check if account has transactions
        var hasTransactions = await _bankAccountRepository.GetByIdAsync(id, cancellationToken) is BankAccount account &&
                              account.Transactions != null && account.Transactions.Any();

        if (hasTransactions)
            return ServiceResult.Failure("Không thể xóa tài khoản ngân hàng có giao dịch.");

        await _bankAccountRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SetPrimaryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _bankAccountRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Tài khoản ngân hàng không tồn légale.");

        await UnsetOtherPrimaryAccountsAsync(id, cancellationToken);

        entity.IsPrimary = true;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "system";

        await _bankAccountRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private async Task UnsetOtherPrimaryAccountsAsync(Guid excludeId, CancellationToken cancellationToken)
    {
        var accounts = await _bankAccountRepository.GetAllActiveAsync(cancellationToken);
        foreach (var account in accounts.Where(a => a.Id != excludeId && a.IsPrimary))
        {
            account.IsPrimary = false;
            account.ModifiedAt = DateTime.UtcNow;
            account.ModifiedBy = "system";
            await _bankAccountRepository.UpdateAsync(account, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static List<string> ValidateCreate(CreateBankAccountDto dto)
    {
        var errors = new List<string>();

        if (dto.BankId == Guid.Empty)
            errors.Add("Bank ID is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountNumber))
            errors.Add("Account number is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountName))
            errors.Add("Account name is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountType))
            errors.Add("Account type is required.");

        return errors;
    }

    private static List<string> ValidateUpdate(UpdateBankAccountDto dto)
    {
        var errors = new List<string>();

        if (dto.Id == Guid.Empty)
            errors.Add("ID is required.");

        if (dto.BankId == Guid.Empty)
            errors.Add("Bank ID is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountNumber))
            errors.Add("Account number is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountName))
            errors.Add("Account name is required.");

        if (string.IsNullOrWhiteSpace(dto.AccountType))
            errors.Add("Account type is required.");

        return errors;
    }

    private static BankAccountDto MapToDto(BankAccount entity) => new()
    {
        Id = entity.Id,
        BankId = entity.BankId,
        BankName = entity.Bank?.Name,
        AccountNumber = entity.AccountNumber,
        AccountName = entity.AccountName,
        AccountType = entity.AccountType,
        CurrencyCode = entity.CurrencyCode,
        OpeningBalance = entity.OpeningBalance,
        IsPrimary = entity.IsPrimary,
        IsActive = entity.IsActive,
        BranchName = entity.BranchName,
        AccountHolder = entity.AccountHolder,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        ModifiedAt = entity.ModifiedAt,
        ModifiedBy = entity.ModifiedBy
    };
}