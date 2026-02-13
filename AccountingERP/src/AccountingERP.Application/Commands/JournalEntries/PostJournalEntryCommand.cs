using AccountingERP.Application.Interfaces;
using MediatR;

namespace AccountingERP.Application.Commands.JournalEntries;

/// <summary>
/// Command: Ghi sổ bút toán
/// </summary>
public record PostJournalEntryCommand(
    Guid JournalEntryId,
    string PostedBy
) : ICommand;

public class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand>
{
    private readonly IJournalEntryRepository _journalEntryRepository;

    public PostJournalEntryCommandHandler(IJournalEntryRepository journalEntryRepository)
    {
        _journalEntryRepository = journalEntryRepository;
    }

    public async Task Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var journalEntry = await _journalEntryRepository.GetByIdAsync(request.JournalEntryId, cancellationToken);
        
        if (journalEntry == null)
            throw new Domain.Exceptions.EntityNotFoundException(nameof(Domain.Entities.JournalEntry), request.JournalEntryId);

        journalEntry.Post(request.PostedBy);
        
        await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);
    }
}
