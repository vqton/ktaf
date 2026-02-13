using FluentValidation;

namespace AccountingERP.Application.Validators;

/// <summary>
/// Validator cho CreateJournalEntryCommand
/// </summary>
public class CreateJournalEntryCommandValidator : AbstractValidator<Commands.JournalEntries.CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.EntryNumber)
            .NotEmpty().WithMessage("Số hiệu bút toán không được để trống")
            .MaximumLength(20).WithMessage("Số hiệu không được quá 20 ký tự");

        RuleFor(x => x.OriginalDocumentNumber)
            .NotEmpty().WithMessage("Số chứng từ gốc không được để trống (TT99)")
            .MaximumLength(50).WithMessage("Số chứng từ gốc không được quá 50 ký tự");

        RuleFor(x => x.EntryDate)
            .NotEmpty().WithMessage("Ngày ghi sổ không được để trống")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Ngày ghi sổ không được trong tương lai");

        RuleFor(x => x.OriginalDocumentDate)
            .NotEmpty().WithMessage("Ngày chứng từ gốc không được để trống (TT99)")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Ngày chứng từ gốc không được trong tương lai");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Diễn giải không được để trống")
            .MaximumLength(500).WithMessage("Diễn giải không được quá 500 ký tự");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Bút toán phải có ít nhất một dòng chi tiết");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.AccountCode)
                .NotEmpty().WithMessage("Mã tài khoản không được để trống")
                .Matches(@"^\d{3,4}$").WithMessage("Mã tài khoản phải có 3-4 chữ số");

            line.RuleFor(x => x)
                .Must(x => x.DebitAmount > 0 || x.CreditAmount > 0)
                .WithMessage("Dòng bút toán phải có số tiền Nợ hoặc Có");

            line.RuleFor(x => x)
                .Must(x => !(x.DebitAmount > 0 && x.CreditAmount > 0))
                .WithMessage("Một dòng chỉ được Nợ HOẶC Có, không cả hai");
        });
    }
}
