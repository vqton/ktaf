using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerRepository _repository;

    public CustomerService(IUnitOfWork unitOfWork, ICustomerRepository repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<CustomerDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByCodeAsync(code, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<CustomerDto?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByTaxCodeAsync(taxCode, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PagedResult<CustomerDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _repository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return PagedResult<CustomerDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllActiveAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var errors = Validate(dto);
        if (errors.Any())
            return ServiceResult<CustomerDto>.Failure(errors);

        var existingCode = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
        if (existingCode != null)
            return ServiceResult<CustomerDto>.Failure($"Mã khách hàng '{dto.Code}' đã tồn tại.");

        var existingTaxCode = await _repository.GetByTaxCodeAsync(dto.TaxCode, cancellationToken);
        if (existingTaxCode != null)
            return ServiceResult<CustomerDto>.Failure($"Mã số thuế '{dto.TaxCode}' đã tồn tại.");

        var entity = new Customer
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

        return ServiceResult<CustomerDto>.Success(MapToDto(entity));
    }

    public async Task<ServiceResult<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult<CustomerDto>.Failure("Không tìm thấy khách hàng.");

        var errors = Validate(dto);
        if (errors.Any())
            return ServiceResult<CustomerDto>.Failure(errors);

        if (entity.Code != dto.Code)
        {
            var existingCode = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
            if (existingCode != null)
                return ServiceResult<CustomerDto>.Failure($"Mã khách hàng '{dto.Code}' đã tồn tại.");
        }

        if (entity.TaxCode != dto.TaxCode)
        {
            var existingTaxCode = await _repository.GetByTaxCodeAsync(dto.TaxCode, cancellationToken);
            if (existingTaxCode != null)
                return ServiceResult<CustomerDto>.Failure($"Mã số thuế '{dto.TaxCode}' đã tồn tại.");
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

        return ServiceResult<CustomerDto>.Success(MapToDto(entity));
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Không tìm thấy khách hàng.");

        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private static List<string> Validate(CreateCustomerDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Mã khách hàng không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Tên khách hàng không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.TaxCode))
            errors.Add("Mã số thuế không được để trống.");
        return errors;
    }

    private static List<string> Validate(UpdateCustomerDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Code))
            errors.Add("Mã khách hàng không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Tên khách hàng không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.TaxCode))
            errors.Add("Mã số thuế không được để trống.");
        return errors;
    }

    private static CustomerDto MapToDto(Customer entity) => new()
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