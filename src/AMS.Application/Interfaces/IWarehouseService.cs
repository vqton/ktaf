using AMS.Application.Common.Results;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

/// <summary>
/// Service interface for managing warehouses.
/// </summary>
public interface IWarehouseService
{
    /// <summary>
    /// Gets a warehouse by its ID.
    /// </summary>
    /// <param name="id">The warehouse ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The warehouse DTO if found; otherwise, null.</returns>
    Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a warehouse by its code.
    /// </summary>
    /// <param name="code">The warehouse code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The warehouse DTO if found; otherwise, null.</returns>
    Task<WarehouseDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active warehouses.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of all active warehouse DTOs.</returns>
    Task<IEnumerable<WarehouseDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all warehouses with pagination.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result of warehouse DTOs.</returns>
    Task<(IEnumerable<WarehouseDto> Warehouses, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new warehouse.
    /// </summary>
    /// <param name="dto">The warehouse data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the created warehouse DTO.</returns>
    Task<ServiceResult<WarehouseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing warehouse.
    /// </summary>
    /// <param name="dto">The warehouse data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Service result with the updated warehouse DTO.</returns>
    Task<ServiceResult<WarehouseDto>> UpdateAsync(UpdateWarehouseDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a warehouse.
    /// </summary>
    /// <param name="id">The warehouse ID.</param>
    /// <param>cancellationToken">Cancellation token.</param>
    /// <returns>Service result.</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}