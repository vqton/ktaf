namespace AccountingERP.Domain.Entities;

/// <summary>
/// Chi tiết bút toán (Journal Entry Line)
/// </summary>
public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; private set; }
    public string AccountCode { get; private set; } = string.Empty;  // Mã tài khoản (TT99 format: 111, 112, 131...)
    public decimal DebitAmount { get; private set; }                 // Số tiền Nợ
    public decimal CreditAmount { get; private set; }                // Số tiền Có
    public string Description { get; private set; } = string.Empty;  // Diễn giải chi tiết
    public string? CostCenterCode { get; private set; }              // Mã bộ phận/phí tập trung
    public string? ProjectCode { get; private set; }                 // Mã dự án
    
    // Navigation property
    public JournalEntry JournalEntry { get; private set; } = null!;

    private JournalEntryLine() : base() { }

    public static JournalEntryLine Create(
        Guid journalEntryId,
        string accountCode,
        decimal debitAmount,
        decimal creditAmount,
        string description)
    {
        if (string.IsNullOrWhiteSpace(accountCode))
            throw new ArgumentException("Mã tài khoản không được để trống", nameof(accountCode));

        if (debitAmount < 0 || creditAmount < 0)
            throw new ArgumentException("Số tiền không được âm");

        if (debitAmount > 0 && creditAmount > 0)
            throw new ArgumentException("Một dòng chỉ được Nợ HOẶC Có, không cả hai");

        return new JournalEntryLine
        {
            JournalEntryId = journalEntryId,
            AccountCode = accountCode,
            DebitAmount = debitAmount,
            CreditAmount = creditAmount,
            Description = description
        };
    }
}
