namespace AMS.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing database operations and transactions.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    Task RollbackTransactionAsync();
}
