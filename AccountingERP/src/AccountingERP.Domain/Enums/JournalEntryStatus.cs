namespace AccountingERP.Domain.Enums;

/// <summary>
/// Trạng thái bút toán
/// </summary>
public enum JournalEntryStatus
{
    Draft = 0,      // Nháp - Chưa ghi sổ
    Posted = 1,     // Đã ghi sổ
    Cancelled = 2,  // Đã hủy
    Adjusted = 3    // Đã điều chỉnh
}
