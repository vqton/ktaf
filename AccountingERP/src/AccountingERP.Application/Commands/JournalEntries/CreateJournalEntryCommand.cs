using AccountingERP.Application.Interfaces;
using AccountingERP.Domain.Entities;
using MediatR;

namespace AccountingERP.Application.Commands.JournalEntries;

/// <summary>
/// Command: Tạo bút toán mới
/// </summary>
public record CreateJournalEntryCommand(
    string EntryNumber,
    string OriginalDocumentNumber,
    DateTime EntryDate,
    DateTime OriginalDocumentDate,
    string Description,
    string? Reference,
    List<JournalEntryLineDto> Lines
) : ICommand<Guid>;

public record JournalEntryLineDto(
    string AccountCode,
    decimal DebitAmount,
    decimal CreditAmount,
    string Description
);

/// <summary>
/// Handler cho CreateJournalEntryCommand
/// </summary>
public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, Guid>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;

    public CreateJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<Guid> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra trùng số hiệu
        if (await _journalEntryRepository.ExistsAsync(request.EntryNumber, cancellationToken))
            throw new InvalidOperationException($"Bút toán với số hiệu '{request.EntryNumber}' đã tồn tại");

        // Tạo entity
        var journalEntry = JournalEntry.Create(
            request.EntryNumber,
            request.OriginalDocumentNumber,
            request.EntryDate,
            request.OriginalDocumentDate,
            request.Description,
            request.Reference
        );

        // Thêm chi tiết
        foreach (var line in request.Lines)
        {
            // Validate tài khoản tồn tại
            var account = await _accountRepository.GetByCodeAsync(line.AccountCode, cancellationToken);
            if (account == null)
                throw new InvalidOperationException($"Tài khoản '{line.AccountCode}' không tồn tại");

            journalEntry.AddLine(line.AccountCode, line.DebitAmount, line.CreditAmount, line.Description);
        }

        await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);

        return journalEntry.Id;
    }
}
