using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class VendorService : IVendorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVendorRepository _repository;

    public VendorService(IUnitOfWork unitOfWork, IVendorRepository repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<VendorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<VendorDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByCodeAsync(code, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<VendorDto?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByTaxCodeAsync(taxCode, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PagedResult<VendorDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _repository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return PagedResult<VendorDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<VendorDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllActiveAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<ServiceResult<VendorDto>> CreateAsync(CreateVendorDto dto, CancellationToken cancellationToken = default)
    {
        var errors = Validate(dto);
        if (errors.Any())
            return ServiceResult<VendorDto>.Failure(errors);

        var existingCode = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
        if (existingCode != null)
            return ServiceResult<VendorDto>.Failure($"Mã nhà cung cấp '{dto.Code}' đã tồn tại.");

        var existingTaxCode = await _repository.GetByTaxCodeAsync(dto.TaxCode, cancellationToken);
        if (existingTaxCode != null)
            return ServiceResult<VendorDto>.Failure($"Mã số thuế '{dto.TaxCode}' đã tồn tại.");

        var entity = new Vendor
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            TaxCode = dto.TaxCode,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            ContactPerson = dto.ContactPerson,
            BankAccount = dto.BankAccount,
            BankName = dto.BankName,
            CreditLimit = dto.CreditLimit,
            PaymentTermDays = dto.PaymentTermDays,
            IsVatPayer = dto.IsVatPayer,
            InvoiceType = dto.InvoiceType,
            IsActive = dto.IsActive,
            Province = dto.Province,
            District = dto.District,
            Ward = dto.Ward,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<VendorDto>.Success(MapToDto(entity));
    }

    public async Task<ServiceResult<VendorDto>> UpdateAsync(Guid id, UpdateVendorDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult<VendorDto>.Failure("Không tìm thấy nhà cung cấp.");

        var errors = Validate(dto);
        if (errors.Any())
            return ServiceResult<VendorDto>.Failure(errors);

        if (entity.Code != dto.Code)
        {
            var existingCode = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
            if (existingCode != null)
                return ServiceResult<VendorDto>.Failure($"Mã nhà cung cấp '{dto.Code}' đã tồn tại.");
        }

        if (entity.TaxCode != dto.TaxCode)
        {
            var existingTaxCode = await _repository.GetByTaxCodeAsync(dto.TaxCode, cancellationToken);
            if (existingTaxCode != null)
                return ServiceResult<VendorDto>.Failure($"Mã số thuế '{dto.TaxCode}' đã tồn tại.");
        }

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.TaxCode = dto.TaxCode;
        entity.Address = dto.Address;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.ContactPerson = dto.ContactPerson;
        entity.BankAccount = dto.BankAccount;
        entity.BankName = dto.BankName;
        entity.CreditLimit = dto.CreditLimit;
        entity.PaymentTermDays = dto.PaymentTermDays;
        entity.IsVatPayer = dto.IsVatPayer;
        entity.InvoiceType = dto.InvoiceType;
        entity.IsActive = dto.IsActive;
        entity.Province = dto.Province;
        entity.District = dto.District;
        entity.Ward = dto.Ward;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "system";

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<VendorDto>.Success(MapToDto(entity));
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Không tìm thấy nhà cung cấp.");

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private static List<string> Validate(CreateVendorDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Mã nhà cung cấp không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Tên nhà cung cấp không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.TaxCode))
            errors.Add("Mã số thuế không được để trống.");
        return errors;
    }

    private static List<string> Validate(UpdateVendorDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Mã nhà cung cấp không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Tên nhà cung cấp không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.TaxCode))
            errors.Add("Mã số thuế không được để trống.");
        return errors;
    }

    private static VendorDto MapToDto(Vendor entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        TaxCode = entity.TaxCode,
        Address = entity.Address,
        Phone = entity.Phone,
        Email = entity.Email,
        ContactPerson = entity.ContactPerson,
        BankAccount = entity.BankAccount,
        BankName = entity.BankName,
        CreditLimit = entity.CreditLimit,
        PaymentTermDays = entity.PaymentTermDays,
        IsVatPayer = entity.IsVatPayer,
        InvoiceType = entity.InvoiceType,
        IsActive = entity.IsActive,
        Province = entity.Province,
        District = entity.District,
        Ward = entity.Ward,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        ModifiedAt = entity.ModifiedAt,
        ModifiedBy = entity.ModifiedBy
    };
}