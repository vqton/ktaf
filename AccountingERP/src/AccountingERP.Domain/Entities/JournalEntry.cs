using AccountingERP.Domain.Enums;

namespace AccountingERP.Domain.Entities;

/// <summary>
/// Bút toán (Journal Entry) - Entity chính trong hệ thống kế toán
/// Tuân thủ TT99/2025/TT-BTC: Bắt buộc số chứng từ gốc và ngày chứng từ gốc
/// </summary>
public class JournalEntry : BaseEntity
{
    // Thông tin chứng từ (Bắt buộc theo TT99)
    public string EntryNumber { get; private set; } = string.Empty;           // Số hiệu bút toán
    public string OriginalDocumentNumber { get; private set; } = string.Empty; // Số chứng từ gốc (BẮT BUỘC)
    public DateTime EntryDate { get; private set; }                            // Ngày ghi sổ
    public DateTime OriginalDocumentDate { get; private set; }                // Ngày chứng từ gốc (BẮT BUỘC)
    
    // Nội dung
    public string Description { get; private set; } = string.Empty;          // Diễn giải
    public string? Reference { get; private set; }                           // Số tham chiếu
    
    // Thông tin bổ sung
    public Guid? CustomerId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public string? AttachmentPath { get; private set; }                      // Đường dẫn file đính kèm
    
    // Trạng thái
    public JournalEntryStatus Status { get; private set; }
    public bool IsPosted { get; private set; }
    public DateTime? PostedDate { get; private set; }
    
    // Chi tiết bút toán (1-n)
    private readonly List<JournalEntryLine> _lines = new();
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    // EF Core constructor
    private JournalEntry() : base() { }

    public static JournalEntry Create(
        string entryNumber,
        string originalDocumentNumber,
        DateTime entryDate,
        DateTime originalDocumentDate,
        string description,
        string? reference = null)
    {
        if (string.IsNullOrWhiteSpace(entryNumber))
            throw new ArgumentException("Số hiệu bút toán không được để trống", nameof(entryNumber));
        
        if (string.IsNullOrWhiteSpace(originalDocumentNumber))
            throw new ArgumentException("Số chứng từ gốc không được để trống (TT99)", nameof(originalDocumentNumber));

        return new JournalEntry
        {
            EntryNumber = entryNumber,
            OriginalDocumentNumber = originalDocumentNumber,
            EntryDate = entryDate,
            OriginalDocumentDate = originalDocumentDate,
            Description = description,
            Reference = reference,
            Status = JournalEntryStatus.Draft,
            IsPosted = false
        };
    }

    public void AddLine(string accountCode, decimal debitAmount, decimal creditAmount, string description)
    {
        if (IsPosted)
            throw new InvalidOperationException("Không thể chỉnh sửa bút toán đã ghi sổ");

        var line = JournalEntryLine.Create(Id, accountCode, debitAmount, creditAmount, description);
        _lines.Add(line);
    }

    public void Post(string postedBy)
    {
        if (IsPosted)
            throw new InvalidOperationException("Bút toán đã được ghi sổ");

        if (!_lines.Any())
            throw new InvalidOperationException("Bút toán phải có ít nhất một dòng chi tiết");

        // Kiểm tra cân bằng Nợ/Có
        var totalDebit = _lines.Sum(l => l.DebitAmount);
        var totalCredit = _lines.Sum(l => l.CreditAmount);
        
        if (totalDebit != totalCredit)
            throw new InvalidOperationException("Tổng Nợ phải bằng Tổng Có");

        IsPosted = true;
        PostedDate = DateTime.UtcNow;
        Status = JournalEntryStatus.Posted;
        UpdatedBy = postedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
