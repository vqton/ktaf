"""
File: app/utils/exceptions.py
Module: Utilities — Custom Exception Hierarchy
Mô tả: Định nghĩa hierarchy exception cho nghiệp vụ kế toán.
       Tất cả custom exceptions dùng trong app phải kế thừa từ KeToanBaseError.

Tham chiếu pháp lý:
    - TT99/2025/TT-BTC: Quy định chế độ kế toán doanh nghiệp

Tác giả: [Tên]
Ngày tạo: 2025-01-xx
Cập nhật: [Ngày] — [Mô tả thay đổi]
"""

from typing import Optional
from decimal import Decimal


class KeToanBaseError(Exception):
    """Base exception cho tất cả lỗi nghiệp vụ kế toán.

    Mọi custom exception trong app phải kế thừa từ class này.
    Cung cấp cấu trúc统一的 cho error handling ở tất cả layers.
    """

    def __init__(
        self,
        message: str,
        code: str = "KETOAN_ERROR",
        status_code: int = 400
    ):
        self.message = message
        self.code = code
        self.status_code = status_code
        super().__init__(message)


class ButToanKhongCanBangError(KeToanBaseError):
    """Tổng Nợ ≠ Tổng Có trong bút toán. Vi phạm nguyên tắc ghi sổ kép.

    Theo TT99/2025/TT-BTC, mọi chứng từ kế toán phải đảm bảo
    tổng phát sinh bên Nợ = tổng phát sinh bên Có.
    """

    def __init__(self, tong_no: Decimal, tong_co: Decimal):
        self.tong_no = tong_no
        self.tong_co = tong_co
        super().__init__(
            message=f"Bút toán không cân bằng: Nợ={tong_no:,.0f} VND, Có={tong_co:,.0f} VND",
            code="BUT_TOAN_KHONG_CAN_BANG",
            status_code=400
        )


class KyKeToanDaKhoaError(KeToanBaseError):
    """Kỳ kế toán đã khoá — không được thêm/sửa/xoá chứng từ.

    Kỳ kế toán khi đã khoá sẽ không chấp nhận bất kỳ thao tác
    CREATE/UPDATE/DELETE nào trên bảng chứng_từ.
    """

    def __init__(self, ky_id: int, nam: int, thang: int):
        self.ky_id = ky_id
        self.nam = nam
        self.thang = thang
        super().__init__(
            message=f"Kỳ kế toán {thang}/{nam} đã khoá, không thể thao tác",
            code="KY_KE_TOAN_DA_KHOA",
            status_code=400
        )


class SoChungTuTrungError(KeToanBaseError):
    """Số chứng từ đã tồn tại trong hệ thống.

    Khi sinh số chứng từ mới, nếu số được sinh ra đã tồn tại,
    hệ thống sẽ raise exception này.
    """

    def __init__(self, so_ct: str):
        self.so_ct = so_ct
        super().__init__(
            message=f"Số chứng từ {so_ct} đã tồn tại",
            code="SO_CHUNG_TU_TRUNG",
            status_code=409
        )


class TaiKhoanKhongHopLeError(KeToanBaseError):
    """Tài khoản kế toán không tồn tại hoặc không được định khoản.

    Một số tài khoản (như tài khoản nhóm, tài khoản tổng hợp)
    không được phép sử dụng trong định khoản chi tiết.
    """

    def __init__(
        self,
        ma_tk: str,
        ly_do: Optional[str] = None
    ):
        self.ma_tk = ma_tk
        self.ly_do = ly_do or "Tài khoản không tồn tại hoặc không được định khoản"
        super().__init__(
            message=f"Tài khoản {ma_tk} không hợp lệ: {self.ly_do}",
            code="TAI_KHOAN_KHONG_HOP_LE",
            status_code=400
        )


class ChungTuDaDuyetError(KeToanBaseError):
    """Chứng từ đã được duyệt — không thể chỉnh sửa/huỷ.

    Chứng từ sau khi duyệt sẽ chuyển sang trạng thái readonly.
    """

    def __init__(self, so_ct: str, trang_thai: str):
        self.so_ct = so_ct
        self.trang_thai = trang_thai
        super().__init__(
            message=f"Chứng từ {so_ct} đã {trang_thai}, không thể chỉnh sửa",
            code="CHUNG_TU_DA_DUYET",
            status_code=400
        )


class ChungTuDaHuyError(KeToanBaseError):
    """Chứng từ đã bị huỷ — không thể thao tác."""

    def __init__(self, so_ct: str):
        self.so_ct = so_ct
        super().__init__(
            message=f"Chứng từ {so_ct} đã bị huỷ",
            code="CHUNG_TU_DA_HUY",
            status_code=400
        )


class KhongDuQuyenError(KeToanBaseError):
    """Người dùng không có quyền thực hiện thao tác."""

    def __init__(self, thao_tac: str, tai_nguyen: str):
        self.thao_tac = thao_tac
        self.tai_nguyen = tai_nguyen
        super().__init__(
            message=f"Không có quyền {thao_tac} trên {tai_nguyen}",
            code="KHONG_DU_QUYEN",
            status_code=403
        )


class DuLieuKhongHopLeError(KeToanBaseError):
    """Dữ liệu đầu vào không hợp lệ.

    Dùng cho validation errors từ schema.
    """

    def __init__(self, message: str, errors: Optional[dict] = None):
        self.errors = errors or {}
        super().__init__(
            message=message,
            code="DU_LIEU_KHONG_HOP_LE",
            status_code=400
        )


class TaiNguyenKhongTonTaiError(KeToanBaseError):
    """Tài nguyên (bản ghi) không tồn tại."""

    def __init__(self, ten_bang: str, id_value: int):
        self.ten_bang = ten_bang
        self.id_value = id_value
        super().__init__(
            message=f"Bản ghi id={id_value} trong bảng {ten_bang} không tồn tại",
            code="TAI_NGUYEN_KHONG_TON_TAI",
            status_code=404
        )


class GiaoDichConLaiError(KeToanBaseError):
    """Tồn tại giao dịch liên quan — không thể xoá."""

    def __init__(self, ten_bang: str, so_luong: int):
        self.ten_bang = ten_bang
        self.so_luong = so_luong
        super().__init__(
            message=f"Có {so_luong} giao dịch liên quan trong bảng {ten_bang}, không thể xoá",
            code="GIAO_DICH_CON_LAI",
            status_code=400
        )
