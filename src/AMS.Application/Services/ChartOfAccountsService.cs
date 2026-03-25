using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class ChartOfAccountsService : IChartOfAccountsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChartOfAccountsRepository _repository;

    public ChartOfAccountsService(IUnitOfWork unitOfWork, IChartOfAccountsRepository repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<ChartOfAccountsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<ChartOfAccountsDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByCodeAsync(code, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<ChartOfAccountsDto?> GetByAccountNumberAsync(int accountNumber, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByAccountNumberAsync(accountNumber, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PagedResult<ChartOfAccountsDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _repository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return PagedResult<ChartOfAccountsDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<ChartOfAccountsDto>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetHierarchyAsync(cancellationToken);
        return BuildHierarchy(entities);
    }

    public async Task<IEnumerable<ChartOfAccountsDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllActiveAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ChartOfAccountsDto>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetByParentIdAsync(parentId, cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<ServiceResult<ChartOfAccountsDto>> CreateAsync(CreateChartOfAccountsDto dto, CancellationToken cancellationToken = default)
    {
        var errors = Validate(dto);
        if (errors.Any())
            return ServiceResult<ChartOfAccountsDto>.Failure(errors);

        var existingCode = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
        if (existingCode != null)
            return ServiceResult<ChartOfAccountsDto>.Failure($"Mã tài khoản '{dto.Code}' đã tồn tại.");

        var existingNumber = await _repository.GetByAccountNumberAsync(dto.AccountNumber, cancellationToken);
        if (existingNumber != null)
            return ServiceResult<ChartOfAccountsDto>.Failure($"Số tài khoản '{dto.AccountNumber}' đã tồn tại.");

        if (dto.ParentId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(dto.ParentId.Value, cancellationToken);
            if (parent == null)
                return ServiceResult<ChartOfAccountsDto>.Failure("Tài khoản cha không tồn tại.");
            if (!parent.IsDetail)
                return ServiceResult<ChartOfAccountsDto>.Failure("Không thể thêm tài khoản con vào tài khoản tổng hợp.");
        }

        var entity = new ChartOfAccounts
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            AccountNumber = dto.AccountNumber,
            AccountType = dto.AccountType,
            ParentId = dto.ParentId,
            IsDetail = dto.IsDetail,
            IsActive = dto.IsActive,
            AllowEntry = dto.AllowEntry,
            TaxCategory = dto.TaxCategory,
            BankAccount = dto.BankAccount,
            BankName = dto.BankName,
            OpeningBalance = dto.OpeningBalance,
            CurrencyCode = dto.CurrencyCode,
            IsBankAccount = dto.IsBankAccount,
            IsTaxAccount = dto.IsTaxAccount,
            IsVatAccount = dto.IsVatAccount,
            VatRateCode = dto.VatRateCode,
            IsRevenueSharing = dto.IsRevenueSharing,
            RevenueSharingPercentage = dto.RevenueSharingPercentage,
            ReconciliationAccount = dto.ReconciliationAccount,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<ChartOfAccountsDto>.Success(MapToDto(entity));
    }

    public async Task<ServiceResult<ChartOfAccountsDto>> UpdateAsync(Guid id, UpdateChartOfAccountsDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult<ChartOfAccountsDto>.Failure("Không tìm thấy tài khoản.");

        var errors = Validate(dto, entity.AccountNumber);
        if (errors.Any())
            return ServiceResult<ChartOfAccountsDto>.Failure(errors);

        if (entity.Code != dto.Code)
        {
            var existingCode = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
            if (existingCode != null)
                return ServiceResult<ChartOfAccountsDto>.Failure($"Mã tài khoản '{dto.Code}' đã tồn tại.");
        }

        if (entity.AccountNumber != dto.AccountNumber)
        {
            var existingNumber = await _repository.GetByAccountNumberAsync(dto.AccountNumber, cancellationToken);
            if (existingNumber != null)
                return ServiceResult<ChartOfAccountsDto>.Failure($"Số tài khoản '{dto.AccountNumber}' đã tồn tại.");
        }

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.AccountNumber = dto.AccountNumber;
        entity.AccountType = dto.AccountType;
        entity.ParentId = dto.ParentId;
        entity.IsDetail = dto.IsDetail;
        entity.IsActive = dto.IsActive;
        entity.AllowEntry = dto.AllowEntry;
        entity.TaxCategory = dto.TaxCategory;
        entity.BankAccount = dto.BankAccount;
        entity.BankName = dto.BankName;
        entity.OpeningBalance = dto.OpeningBalance;
        entity.CurrencyCode = dto.CurrencyCode;
        entity.IsBankAccount = dto.IsBankAccount;
        entity.IsTaxAccount = dto.IsTaxAccount;
        entity.IsVatAccount = dto.IsVatAccount;
        entity.VatRateCode = dto.VatRateCode;
        entity.IsRevenueSharing = dto.IsRevenueSharing;
        entity.RevenueSharingPercentage = dto.RevenueSharingPercentage;
        entity.ReconciliationAccount = dto.ReconciliationAccount;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "system";

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<ChartOfAccountsDto>.Success(MapToDto(entity));
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Không tìm thấy tài khoản.");

        var children = await _repository.GetByParentIdAsync(id, cancellationToken);
        if (children.Any())
            return ServiceResult.Failure("Không thể xóa tài khoản có tài khoản con.");

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private static List<string> Validate(CreateChartOfAccountsDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Mã tài khoản không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Tên tài khoản không được để trống.");
        if (dto.AccountNumber <= 0)
            errors.Add("Số tài khoản phải lớn hơn 0.");
        return errors;
    }

    private static List<string> Validate(UpdateChartOfAccountsDto dto, int originalNumber)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Mã tài khoản không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Tên tài khoản không được để trống.");
        if (dto.AccountNumber <= 0)
            errors.Add("Số tài khoản phải lớn hơn 0.");
        return errors;
    }

    private static ChartOfAccountsDto MapToDto(ChartOfAccounts entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        AccountNumber = entity.AccountNumber,
        AccountType = entity.AccountType,
        ParentId = entity.ParentId,
        IsDetail = entity.IsDetail,
        IsActive = entity.IsActive,
        AllowEntry = entity.AllowEntry,
        TaxCategory = entity.TaxCategory,
        BankAccount = entity.BankAccount,
        BankName = entity.BankName,
        OpeningBalance = entity.OpeningBalance,
        CurrencyCode = entity.CurrencyCode,
        IsBankAccount = entity.IsBankAccount,
        IsTaxAccount = entity.IsTaxAccount,
        IsVatAccount = entity.IsVatAccount,
        VatRateCode = entity.VatRateCode,
        IsRevenueSharing = entity.IsRevenueSharing,
        RevenueSharingPercentage = entity.RevenueSharingPercentage,
        ReconciliationAccount = entity.ReconciliationAccount,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        ModifiedAt = entity.ModifiedAt,
        ModifiedBy = entity.ModifiedBy
    };

    private static IEnumerable<ChartOfAccountsDto> BuildHierarchy(IEnumerable<ChartOfAccounts> entities)
    {
        var lookup = entities.ToDictionary(e => e.Id, e => MapToDto(e));
        var roots = new List<ChartOfAccountsDto>();

        foreach (var entity in entities)
        {
            var dto = lookup[entity.Id];
            if (entity.ParentId == null)
            {
                roots.Add(dto);
            }
            else if (lookup.TryGetValue(entity.ParentId.Value, out var parent))
            {
                parent.Children.Add(dto);
            }
        }

        return roots;
    }
}