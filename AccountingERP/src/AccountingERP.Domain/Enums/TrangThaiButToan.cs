namespace AccountingERP.Domain.Enums;

/// <summary>
/// Trạng thái của bút toán kế toán
/// </summary>
public enum TrangThaiButToan
{
    /// <summary>
    /// Nhập - Đang nhập liệu, chưa ghi sổ
    /// </summary>
    Nhap = 0,

    /// <summary>
    /// Đã ghi sổ - Bút toán đã được ghi nhận vào sổ kế toán
    /// </summary>
    DaGhiSo = 1,

    /// <summary>
    /// Đã hủy - Bút toán bị hủy bỏ
    /// </summary>
    DaHuy = 2,

    /// <summary>
    /// Đã điều chỉnh - Bút toán đã được điều chỉnh bởi bút toán khác
    /// </summary>
    DaDieuChinh = 3
}
