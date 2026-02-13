using AccountingERP.Application.Interfaces;
using AccountingERP.Domain.Enums;
using MediatR;

namespace AccountingERP.Application.Queries.JournalEntries;

/// <summary>
/// Query: Lấy bút toán theo ID
/// </summary>
public record GetJournalEntryByIdQuery(Guid Id) : IQuery<JournalEntryDto?>;

/// <summary>
/// Query: Lấy danh sách bút toán
/// </summary>
public record GetJournalEntriesQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    JournalEntryStatus? Status = null
) : IQuery<IReadOnlyList<JournalEntryListDto>>;

/// <summary>
/// DTO: Chi tiết bút toán
/// </summary>
public record JournalEntryDto(
    Guid Id,
    string EntryNumber,
    string OriginalDocumentNumber,
    DateTime EntryDate,
    DateTime OriginalDocumentDate,
    string Description,
    string? Reference,
    JournalEntryStatus Status,
    bool IsPosted,
    DateTime? PostedDate,
    IReadOnlyList<JournalEntryLineDto> Lines
);

/// <summary>
/// DTO: Danh sách bút toán (summary)
/// </summary>
public record JournalEntryListDto(
    Guid Id,
    string EntryNumber,
    DateTime EntryDate,
    string Description,
    JournalEntryStatus Status,
    decimal TotalAmount
);

/// <summary>
/// DTO: Chi tiết dòng bút toán
/// </summary>
public record JournalEntryLineDto(
    Guid Id,
    string AccountCode,
    string AccountName,
    decimal DebitAmount,
    decimal CreditAmount,
    string Description
);

public class GetJournalEntryByIdQueryHandler : IRequestHandler<GetJournalEntryByIdQuery, JournalEntryDto?>
{
    private readonly IJournalEntryRepository _repository;

    public GetJournalEntryByIdQueryHandler(IJournalEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<JournalEntryDto?> Handle(GetJournalEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (entry == null) return null;

        return new JournalEntryDto(
            entry.Id,
            entry.EntryNumber,
            entry.OriginalDocumentNumber,
            entry.EntryDate,
            entry.OriginalDocumentDate,
            entry.Description,
            entry.Reference,
            entry.Status,
            entry.IsPosted,
            entry.PostedDate,
            entry.Lines.Select(l => new JournalEntryLineDto(
                l.Id,
                l.AccountCode,
                "", // TODO: Get account name
                l.DebitAmount,
                l.CreditAmount,
                l.Description
            )).ToList()
        );
    }
}
