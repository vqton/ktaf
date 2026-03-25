using AMS.Domain.Entities;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for managing vendor entities.
/// </summary>
public interface IVendorRepository
{
    /// <summary>
    /// Retrieves a vendor by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the vendor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The vendor if found; otherwise, null.</returns>
    Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a vendor by its unique code.
    /// </summary>
    /// <param name="code">The unique code of the vendor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The vendor if found; otherwise, null.</returns>
    Task<Vendor?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a vendor by its tax identification number.
    /// </summary>
    /// <param name="taxCode">The tax identification number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The vendor if found; otherwise, null.</returns>
    Task<Vendor?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active vendors.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of active vendors.</returns>
    Task<IEnumerable<Vendor>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all vendors.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the vendors for the page and the total count.</returns>
    Task<(IEnumerable<Vendor> Vendors, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new vendor to the repository.
    /// </summary>
    /// <param name="vendor">The vendor to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing vendor in the repository.
    /// </summary>
    /// <param name="vendor">The vendor to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Vendor vendor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a vendor by setting IsDeleted flag.
    /// </summary>
    /// <param name="id">The unique identifier of the vendor to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}