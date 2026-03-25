using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IChartOfAccountsService
{
    Task<ChartOfAccountsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ChartOfAccountsDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ChartOfAccountsDto?> GetByAccountNumberAsync(int accountNumber, CancellationToken cancellationToken = default);
    Task<PagedResult<ChartOfAccountsDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChartOfAccountsDto>> GetHierarchyAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChartOfAccountsDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChartOfAccountsDto>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ChartOfAccountsDto>> CreateAsync(CreateChartOfAccountsDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<ChartOfAccountsDto>> UpdateAsync(Guid id, UpdateChartOfAccountsDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}