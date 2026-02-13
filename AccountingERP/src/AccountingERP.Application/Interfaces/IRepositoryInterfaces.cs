namespace AccountingERP.Application.Interfaces;

/// <summary>
/// Repository interface cho JournalEntry
/// </summary>
public interface IJournalEntryRepository
{
    Task<Domain.Entities.JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.JournalEntry?> GetByEntryNumberAsync(string entryNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.JournalEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.JournalEntry>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.JournalEntry journalEntry, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.JournalEntry journalEntry, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string entryNumber, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface cho Account
/// </summary>
public interface IAccountRepository
{
    Task<Domain.Entities.Account?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Account>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Account>> GetByTypeAsync(Domain.Enums.AccountType type, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.Account account, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.Account account, CancellationToken cancellationToken = default);
}
