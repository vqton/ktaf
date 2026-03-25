using AMS.Domain.Entities;

namespace AMS.Domain.Interfaces;

/// <summary>
/// Repository interface for managing customer entities.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Retrieves a customer by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a customer by its unique code.
    /// </summary>
    /// <param name="code">The unique code of the customer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<Customer?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a customer by its tax identification number.
    /// </summary>
    /// <param name="taxCode">The tax identification number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<Customer?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active customers.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of active customers.</returns>
    Task<IEnumerable<Customer>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all customers.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the customers for the page and the total count.</returns>
    Task<(IEnumerable<Customer> Customers, int TotalCount)> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new customer to the repository.
    /// </summary>
    /// <param name="customer">The customer to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer in the repository.
    /// </summary>
    /// <param name="customer">The customer to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a customer by setting IsDeleted flag.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}