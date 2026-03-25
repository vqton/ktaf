using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}