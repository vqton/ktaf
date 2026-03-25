using AMS.Application.Common.Constants;
using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Enums;
using AMS.Domain.Interfaces;

namespace AMS.Application.Services;

public class FiscalPeriodService : IFiscalPeriodService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFiscalPeriodRepository _repository;

    public FiscalPeriodService(IUnitOfWork unitOfWork, IFiscalPeriodRepository repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<FiscalPeriodDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<FiscalPeriodDto?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByYearMonthAsync(year, month, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PagedResult<FiscalPeriodDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, AppConstants.MaxPageSize);
        var (entities, totalCount) = await _repository.GetAllPagedAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return PagedResult<FiscalPeriodDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<FiscalPeriodDto>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetOpenPeriodsAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<FiscalPeriodDto?> GetCurrentPeriodAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetPeriodForDateAsync(DateTime.Today, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<FiscalPeriodDto?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetPeriodForDateAsync(date, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<ServiceResult<FiscalPeriodDto>> CreateAsync(CreateFiscalPeriodDto dto, CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreate(dto);
        if (errors.Any())
            return ServiceResult<FiscalPeriodDto>.Failure(errors);

        var existing = await _repository.GetByYearMonthAsync(dto.Year, dto.Month, cancellationToken);
        if (existing != null)
            return ServiceResult<FiscalPeriodDto>.Failure($"Kỳ kế toán {dto.Year}/{dto.Month} đã tồn tại.");

        var entity = new FiscalPeriod
        {
            Id = Guid.NewGuid(),
            Year = dto.Year,
            Month = dto.Month,
            Status = FiscalPeriodStatus.Open,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsDeleted = false
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<FiscalPeriodDto>.Success(MapToDto(entity));
    }

    public async Task<ServiceResult> OpenPeriodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Kỳ kế toán không tồn tại.");

        try
        {
            entity.Open();
            await _repository.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return ServiceResult.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult> ClosePeriodAsync(Guid id, string closedBy, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Kỳ kế toán không tồn tại.");

        try
        {
            entity.Close(closedBy);
            await _repository.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return ServiceResult.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult> LockPeriodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ServiceResult.Failure("Kỳ kế toán không tồn tại.");

        try
        {
            entity.Lock();
            await _repository.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return ServiceResult.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult> GenerateYearPeriodsAsync(int year, CancellationToken cancellationToken = default)
    {
        for (int month = 1; month <= 12; month++)
        {
            var existing = await _repository.GetByYearMonthAsync(year, month, cancellationToken);
            if (existing != null)
                continue;

            var period = new FiscalPeriod
            {
                Id = Guid.NewGuid(),
                Year = year,
                Month = month,
                Status = month < DateTime.Today.Month && year <= DateTime.Today.Year 
                    ? FiscalPeriodStatus.Closed 
                    : FiscalPeriodStatus.Open,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                IsDeleted = false
            };

            await _repository.AddAsync(period, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private static List<string> ValidateCreate(CreateFiscalPeriodDto dto)
    {
        var errors = new List<string>();
        if (dto.Year < 2000 || dto.Year > 2100)
            errors.Add("Năm kế toán không hợp lệ.");
        if (dto.Month < 1 || dto.Month > 12)
            errors.Add("Tháng kế toán không hợp lệ.");
        return errors;
    }

    private static FiscalPeriodDto MapToDto(FiscalPeriod entity) => new()
    {
        Id = entity.Id,
        Year = entity.Year,
        Month = entity.Month,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        ModifiedAt = entity.ModifiedAt,
        ModifiedBy = entity.ModifiedBy
    };
}
