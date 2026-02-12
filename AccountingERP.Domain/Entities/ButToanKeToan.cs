namespace AccountingERP.Domain.Entities;

using AccountingERP.Domain.Enums;
using AccountingERP.Domain.ValueObjects;

/// <summary>
/// Bút toán kế toán (Journal Entry) - Entity cốt lõi của hệ thống
/// </summary>
/// <remarks>
/// TT99-Đ10 khoản 2: Chứng từ kế toán phải có đầy đủ các nội dung sau:
/// - Số chứng từ gốc (bắt buộc)
/// - Ngày chứng từ gốc (bắt buộc)
/// - Nội dung nghiệp vụ kinh tế, tài chính phát sinh
/// 
/// Entity này đảm bảo tuân thủ nguyên tắc bút toán kép:
/// - Tổng Nợ = Tổng Có
/// - Tài khoản Nợ ≠ Tài khoản Có
/// - Cấm sử dụng TK 911 trong bút toán thông thường
/// </remarks>
public sealed class ButToanKeToan
{
    private readonly List<DongButToan> _dongButToan = new();

    /// <summary>
    /// Mã định danh duy nhất của bút toán
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Số hiệu bút toán (do hệ thống tự động sinh)
    /// </summary>
    /// <remarks>
    /// Format: BT-YYYYMM-XXXXX (ví dụ: BT-202602-00001)
    /// </remarks>
    public string SoHieuButToan { get; private set; } = string.Empty;

    /// <summary>
    /// Số chứng từ gốc theo TT99-Đ10 khoản 2: Bắt buộc có khi lập chứng từ kế toán
    /// </summary>
    /// <remarks>
    /// Không được để trống. Định dạng: [LoạiCT]-[Năm]-[Số thứ tự]
    /// Ví dụ: "PT-2026-001", "PC-2026-005"
    /// </remarks>
    public string SoChungTuGoc { get; private set; } = string.Empty;

    /// <summary>
    /// Ngày chứng từ gốc theo TT99-Đ10 khoản 2: Bắt buộc có
    /// </summary>
    /// <remarks>
    /// Là ngày thực tế phát sinh nghiệp vụ kinh tế, không được để trống.
    /// Không được là ngày trong tương lai.
    /// </remarks>
    public DateTime NgayChungTuGoc { get; private set; }

    /// <summary>
    /// Ngày ghi sổ kế toán
    /// </summary>
    /// <remarks>
    /// Theo TT99-Đ10 khoản 3: Ngày ghi sổ là ngày ghi nhận nghiệp vụ kinh tế 
    /// vào hệ thống sổ kế toán.
    /// </remarks>
    public DateTime NgayGhiSo { get; private set; }

    /// <summary>
    /// Nội dung nghiệp vụ kinh tế, tài chính
    /// </summary>
    /// <remarks>
    /// TT99-Đ10: Phải ghi rõ ràng, chính xác nội dung nghiệp vụ.
    /// </remarks>
    public string NoiDung { get; private set; } = string.Empty;

    /// <summary>
    /// Trạng thái của bút toán
    /// </summary>
    public TrangThaiButToan TrangThai { get; private set; }

    /// <summary>
    /// Danh sách chi tiết bút toán (Nợ/Có)
    /// </summary>
    public IReadOnlyCollection<DongButToan> DanhSachDongButToan => _dongButToan.AsReadOnly();

    /// <summary>
    /// Tổng số tiền bên Nợ
    /// </summary>
    public decimal TongNo => _dongButToan.Sum(d => d.TienNo.Amount);

    /// <summary>
    /// Tổng số tiền bên Có
    /// </summary>
    public decimal TongCo => _dongButToan.Sum(d => d.TienCo.Amount);

    /// <summary>
    /// Kiểm tra bút toán có cân bằng không (Tổng Nợ = Tổng Có)
    /// </summary>
    public bool IsCanBang => TongNo == TongCo;

    /// <summary>
    /// Ngày tạo bút toán
    /// </summary>
    public DateTime NgayTao { get; private set; }

    /// <summary>
    /// Ngưởi tạo bút toán
    /// </summary>
    public string NguoiTao { get; private set; } = string.Empty;

    /// <summary>
    /// Ngày cập nhật cuối cùng
    /// </summary>
    public DateTime? NgayCapNhat { get; private set; }

    /// <summary>
    /// Ngưởi cập nhật cuối cùng
    /// </summary>
    public string? NguoiCapNhat { get; private set; }

    /// <summary>
    /// Constructor private - chỉ tạo qua factory method
    /// </summary>
    private ButToanKeToan() { }

    /// <summary>
    /// Tạo bút toán mới với validation đầy đủ theo TT99-Đ10
    /// </summary>
    /// <param name="soChungTuGoc">Số chứng từ gốc (bắt buộc)</param>
    /// <param name="ngayChungTuGoc">Ngày chứng từ gốc (bắt buộc)</param>
    /// <param name="ngayGhiSo">Ngày ghi sổ</param>
    /// <param name="noiDung">Nội dung nghiệp vụ</param>
    /// <param name="nguoiTao">Ngưởi tạo</param>
    /// <returns>ButToanKeToan entity</returns>
    /// <exception cref="ArgumentException">Khi thiếu thông tin bắt buộc</exception>
    /// <remarks>
    /// TT99-Đ10 khoản 2: Số chứng từ gốc và ngày chứng từ gốc là bẮT BUỘC
    /// </remarks>
    public static ButToanKeToan TaoMoi(
        string soChungTuGoc,
        DateTime ngayChungTuGoc,
        DateTime ngayGhiSo,
        string noiDung,
        string nguoiTao)
    {
        // Validation TT99-Đ10: Số chứng từ gốc bắt buộc
        if (string.IsNullOrWhiteSpace(soChungTuGoc))
            throw new ArgumentException(
                "TT99-Đ10: Số chứng từ gốc không được để trống. Đây là thông tin bẮT BUỘC.",
                nameof(soChungTuGoc));

        // Validation TT99-Đ10: Ngày chứng từ gốc bắt buộc
        if (ngayChungTuGoc == default)
            throw new ArgumentException(
                "TT99-Đ10: Ngày chứng từ gốc không được để trống. Đây là thông tin bẮT BUỘC.",
                nameof(ngayChungTuGoc));

        // Validation: Ngày chứng từ không được trong tương lai
        if (ngayChungTuGoc > DateTime.Now)
            throw new ArgumentException(
                "Ngày chứng từ gốc không được là ngày trong tương lai.",
                nameof(ngayChungTuGoc));

        // Validation: Ngày ghi sổ không được trong tương lai
        if (ngayGhiSo > DateTime.Now)
            throw new ArgumentException(
                "Ngày ghi sổ không được là ngày trong tương lai.",
                nameof(ngayGhiSo));

        // Validation: Nội dung không được trống
        if (string.IsNullOrWhiteSpace(noiDung))
            throw new ArgumentException(
                "Nội dung nghiệp vụ không được để trống.",
                nameof(noiDung));

        // Validation: Ngưởi tạo không được trống
        if (string.IsNullOrWhiteSpace(nguoiTao))
            throw new ArgumentException(
                "Ngưởi tạo không được để trống.",
                nameof(nguoiTao));

        return new ButToanKeToan
        {
            Id = Guid.NewGuid(),
            SoHieuButToan = SinhSoHieu(ngayGhiSo),
            SoChungTuGoc = soChungTuGoc.Trim(),
            NgayChungTuGoc = ngayChungTuGoc,
            NgayGhiSo = ngayGhiSo,
            NoiDung = noiDung.Trim(),
            TrangThai = TrangThaiButToan.Nhap,
            NgayTao = DateTime.Now,
            NguoiTao = nguoiTao.Trim()
        };
    }

    /// <summary>
    /// Thêm dòng bút toán (Nợ/Có)
    /// </summary>
    /// <param name="taiKhoanNo">Tài khoản Nợ</param>
    /// <param name="taiKhoanCo">Tài khoản Có</param>
    /// <param name="soTien">Số tiền</param>
    /// <param name="noiDungChiTiet">Nội dung chi tiết</param>
    /// <exception cref="InvalidOperationException">Khi vi phạm quy tắc kế toán</exception>
    /// <remarks>
    /// Quy tắc bút toán kép:
    /// - Tài khoản Nợ ≠ Tài khoản Có
    /// - Cấm sử dụng TK 911
    /// - Số tiền > 0
    /// </remarks>
    public void ThemDong(
        AccountCode taiKhoanNo,
        AccountCode taiKhoanCo,
        Money soTien,
        string? noiDungChiTiet = null)
    {
        if (TrangThai != TrangThaiButToan.Nhap)
            throw new InvalidOperationException("Chỉ có thể thêm dòng khi bút toán ở trạng thái NHẬP.");

        // Validation: Tài khoản Nợ ≠ Tài khoản Có
        if (taiKhoanNo == taiKhoanCo)
            throw new InvalidOperationException(
                "Tài khoản Nợ và Tài khoản Có không được giống nhau. " +
                "Vi phạm nguyên tắc bút toán kép.");

        // Validation: Cấm TK 911
        if (taiKhoanNo.Value == "911" || taiKhoanCo.Value == "911")
            throw new InvalidOperationException(
                "TK 911 (Xác định kết quả kinh doanh) không được sử dụng trong bút toán thông thường. " +
                "TK này chỉ dùng để kết chuyển cuối kỳ.");

        var dong = new DongButToan(
            Id,
            taiKhoanNo,
            taiKhoanCo,
            soTien,
            noiDungChiTiet);

        _dongButToan.Add(dong);
    }

    /// <summary>
    /// Ghi sổ bút toán
    /// </summary>
    /// <param name="nguoiGhiSo">Ngưởi ghi sổ</param>
    /// <exception cref="InvalidOperationException">Khi bút toán không hợp lệ</exception>
    public void GhiSo(string nguoiGhiSo)
    {
        if (TrangThai != TrangThaiButToan.Nhap)
            throw new InvalidOperationException("Chỉ có thể ghi sổ khi bút toán ở trạng thái NHẬP.");

        if (_dongButToan.Count == 0)
            throw new InvalidOperationException("Bút toán phải có ít nhất một dòng chi tiết.");

        if (!IsCanBang)
            throw new InvalidOperationException(
                $"Bút toán không cân bằng. Tổng Nợ: {TongNo:N0}, Tổng Có: {TongCo:N0}. " +
                "Vi phạm nguyên tắc bút toán kép: Tổng Nợ phải bằng Tổng Có.");

        TrangThai = TrangThaiButToan.DaGhiSo;
        NgayCapNhat = DateTime.Now;
        NguoiCapNhat = nguoiGhiSo;
    }

    /// <summary>
    /// Hủy bút toán
    /// </summary>
    /// <param name="lyDo">Lý do hủy</param>
    public void Huy(string lyDo)
    {
        if (TrangThai == TrangThaiButToan.DaGhiSo)
            throw new InvalidOperationException("Không thể hủy bút toán đã ghi sổ.");

        TrangThai = TrangThaiButToan.DaHuy;
        NgayCapNhat = DateTime.Now;
    }

    /// <summary>
    /// Sinh số hiệu bút toán tự động
    /// </summary>
    private static string SinhSoHieu(DateTime ngayGhiSo) =>
        $"BT-{ngayGhiSo:yyyyMM}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
}

/// <summary>
/// Chi tiết dòng bút toán (Nợ/Có)
/// </summary>
public sealed class DongButToan
{
    /// <summary>
    /// Mã định danh
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Mã bút toán cha
    /// </summary>
    public Guid ButToanId { get; private set; }

    /// <summary>
    /// Tài khoản Nợ
    /// </summary>
    public AccountCode TaiKhoanNo { get; private set; } = null!;

    /// <summary>
    /// Tài khoản Có
    /// </summary>
    public AccountCode TaiKhoanCo { get; private set; } = null!;

    /// <summary>
    /// Số tiền bên Nợ
    /// </summary>
    public Money TienNo { get; private set; } = null!;

    /// <summary>
    /// Số tiền bên Có
    /// </summary>
    public Money TienCo { get; private set; } = null!;

    /// <summary>
    /// Nội dung chi tiết
    /// </summary>
    public string? NoiDungChiTiet { get; private set; }

    private DongButToan() { }

    public DongButToan(
        Guid butToanId,
        AccountCode taiKhoanNo,
        AccountCode taiKhoanCo,
        Money soTien,
        string? noiDungChiTiet)
    {
        Id = Guid.NewGuid();
        ButToanId = butToanId;
        TaiKhoanNo = taiKhoanNo;
        TaiKhoanCo = taiKhoanCo;
        TienNo = soTien;
        TienCo = soTien;
        NoiDungChiTiet = noiDungChiTiet;
    }
}
