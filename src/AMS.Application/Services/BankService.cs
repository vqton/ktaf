using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities.DM;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

/// <summary>
/// Service implementation for managing banks.
/// </summary>
public class BankService : IBankService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankRepository _bankRepository;

    /// <summary>
    /// Initializes a new instance of the BankService class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance.</param>
    /// <param name="bankRepository">The bank repository instance.</param>
    public BankService(IUnitOfWork unitOfWork, IBankRepository bankRepository)
    {
        _unitOfWork = unitOfWork;
        _bankRepository = bankRepository;
    }

    /// <inheritdoc />
    public async Task<BankDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _bankRepository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<BankDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _bankRepository.GetByCodeAsync(code, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BankDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _bankRepository.GetAllActiveAsync(cancellationToken);
        return entities.Select(e => MapToDto(e));
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<BankDto> Banks, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _bankRepository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(e => MapToDto(e)).ToList();
        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BankDto>> CreateAsync(CreateBankDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreate(dto);
        if (errors.Any())
            return ServiceResult<BankDto>.Failure(errors);

        var existing = await _bankRepository.GetByCodeAsync(dto.Code, cancellationToken);
        if (existing != null)
            return ServiceResult<BankDto>.Failure($"Ngân hàng với mã {dto.Code} đã tồn tại.");

        var entity = new Bank
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            SwiftCode = dto.SwiftCode,
            LogoPath = dto.LogoPath,
            BranchName = dto.BranchName,
            Address = dto.Address,
            Phone = dto.Phone,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _bankRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<BankDto>.Success(MapToDto(entity));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BankDto>> UpdateAsync(UpdateBankDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateUpdate(dto);
        if (errors.Any())
            return ServiceResult<BankDto>.Failure(errors);

        var entity = await _bankRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (entity == null)
            return ServiceResult<BankDto>.Failure("Ngân hàng không tồn tại.");

        var existing = await _bankRepository.GetByCodeAsync(dto.Code, cancellationToken);
        if (existing != null && existing.Id != dto.Id)
            return ServiceResult<BankDto>.Failure($"Ngân hàng với mã {dto.Code} đã tồn tại.");

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.SwiftCode = dto.SwiftCode;
        entity.LogoPath = dto.LogoPath;
        entity.BranchName = dto.BranchName;
        entity.Address = dto.Address;
        entity.Phone = dto.Phone;
        entity.IsActive = dto.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "system";

        await _bankRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<BankDto>.Success(MapToDto(entity));
    }

    private static List<string> ValidateCreate(CreateBankDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Bank code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Bank name is required.");

        return errors;
    }

    private static List<string> ValidateUpdate(UpdateBankDto dto)
    {
        var errors = new List<string>();

        if (dto.Id == Guid.Empty)
            errors.Add("ID is required.");

        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Bank code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Bank name is required.");

        return errors;
    }

    private static BankDto MapToDto(Bank entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        SwiftCode = entity.SwiftCode,
        LogoPath = entity.LogoPath,
        BranchName = entity.BranchName,
        Address = entity.Address,
        Phone = entity.Phone,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        ModifiedAt = entity.ModifiedAt,
        ModifiedBy = entity.ModifiedBy
    };
}