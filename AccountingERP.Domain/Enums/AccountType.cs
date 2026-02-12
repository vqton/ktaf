namespace AccountingERP.Domain.Enums;

/// <summary>
/// Loại tài khoản kế toán theo TT99/2025/TT-BTC
/// </summary>
/// <remarks>
/// Phân loại tài khoản theo bản chất kinh tế và đặc điểm biến động trong kỳ kế toán.
/// Mỗi loại có quy tắc bù trừ riêng và ảnh hưởng đến việc lập Bảng cân đối kế toán.
/// </remarks>
public enum AccountType
{
    /// <summary>
    /// Tài sản (Nợ) - Các nguồn lực do doanh nghiệp kiểm soát
    /// </summary>
    /// <remarks>
    /// Đặc điểm: Bên Nợ phản ánh tăng, bên Có phản ánh giảm
    /// Số dư thường nằm bên Nợ (trừ TK 129, 229, 319)
    /// Ví dụ: TK 111 (Tiền mặt), TK 112 (Tiền gửi ngân hàng), TK 131 (Phải thu khách hàng)
    /// </remarks>
    TaiSan = 1,

    /// <summary>
    /// Nợ phải trả (Có) - Các khoản nợ và nghĩa vụ phải thanh toán
    /// </summary>
    /// <remarks>
    /// Đặc điểm: Bên Có phản ánh tăng, bên Nợ phản ánh giảm  
    /// Số dư thường nằm bên Có
    /// Ví dụ: TK 331 (Phải trả ngưởi bán), TK 341 (Vay và nợ thuê tài chính)
    /// </remarks>
    NoPhaiTra = 2,

    /// <summary>
    /// Vốn chủ sở hữu (Có) - Nguồn vốn của chủ doanh nghiệp
    /// </summary>
    /// <remarks>
    /// Đặc điểm: Bên Có phản ánh tăng, bên Nợ phản ánh giảm
    /// Số dư thường nằm bên Có (trừ TK 419, TK 811)
    /// Ví dụ: TK 411 (Vốn góp của chủ sở hữu), TK 421 (Lợi nhuận sau thuế chưa phân phối)
    /// </remarks>
    VonChuSoHuu = 3,

    /// <summary>
    /// Doanh thu (Có) - Các khoản thu từ hoạt động sản xuất kinh doanh
    /// </summary>
    /// <remarks>
    /// Đặc điểm: Bên Có phản ánh tăng, bên Nợ phản ánh giảm hoặc kết chuyển
    /// Không có số dư cuối kỳ (kết chuyển hết sang TK 911)
    /// Ví dụ: TK 511 (Doanh thu bán hàng), TK 515 (Doanh thu hoạt động tài chính)
    /// </remarks>
    DoanhThu = 4,

    /// <summary>
    /// Chi phí (Nợ) - Các khoản chi phí phục vụ hoạt động sản xuất kinh doanh
    /// </summary>
    /// <remarks>
    /// Đặc điểm: Bên Nợ phản ánh tăng, bên Có phản ánh giảm hoặc kết chuyển
    /// Không có số dư cuối kỳ (kết chuyển hết sang TK 911)
    /// Ví dụ: TK 632 (Giá vốn hàng bán), TK 642 (Chi phí quản lý doanh nghiệp)
    /// </remarks>
    ChiPhi = 5,

    /// <summary>
    /// Thu nhập khác (Có) - Các khoản thu ngoài hoạt động sản xuất kinh doanh chính
    /// </summary>
    /// <remarks>
    /// Đặc điểm: Bên Có phản ánh tăng, bên Nợ phản ánh kết chuyển
    /// Không có số dư cuối kỳ (kết chuyển hết sang TK 911)
    /// Ví dụ: TK 711 (Thu nhập khác)
    /// </remarks>
    ThuNhapKhac = 6,

    /// <summary>
    /// Chi phí khác (Nợ) - Các khoản chi ngoài hoạt động sản xuất kinh doanh chính
    /// </summary>
    /// <remarks>
    /// Đặc điểm: Bên Nợ phản ánh tăng, bên Có phản ánh kết chuyển
    /// Không có số dư cuối kỳ (kết chuyển hết sang TK 911)
    /// Ví dụ: TK 811 (Chi phí khác) - TK đặc biệt, không được sử dụng trong bút toán thông thường
    /// </remarks>
    ChiPhiKhac = 7
}
