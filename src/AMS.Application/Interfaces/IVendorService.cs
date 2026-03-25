using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IVendorService
{
    Task<VendorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VendorDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<VendorDto?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default);
    Task<PagedResult<VendorDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<VendorDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<VendorDto>> CreateAsync(CreateVendorDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<VendorDto>> UpdateAsync(Guid id, UpdateVendorDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}