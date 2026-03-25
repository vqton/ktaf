using AMS.Application.Common.Results;
using AMS.Application.DTOs;
using AMS.Domain.Enums;

namespace AMS.Application.Interfaces;

public interface IFiscalPeriodService
{
    Task<FiscalPeriodDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FiscalPeriodDto?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<PagedResult<FiscalPeriodDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<FiscalPeriodDto>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default);
    Task<FiscalPeriodDto?> GetCurrentPeriodAsync(CancellationToken cancellationToken = default);
    Task<FiscalPeriodDto?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<ServiceResult<FiscalPeriodDto>> CreateAsync(CreateFiscalPeriodDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> OpenPeriodAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult> ClosePeriodAsync(Guid id, string closedBy, CancellationToken cancellationToken = default);
    Task<ServiceResult> LockPeriodAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult> GenerateYearPeriodsAsync(int year, CancellationToken cancellationToken = default);
}
