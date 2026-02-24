from decimal import Decimal
from typing import List, Dict


class ValidationError(Exception):
    def __init__(self, message: str, field: str = None):
        self.message = message
        self.field = field
        super().__init__(message)


def validate_but_toan_can_bang(dinh_khoan: List[Dict]) -> bool:
    tong_no = Decimal('0')
    tong_co = Decimal('0')

    for dk in dinh_khoan:
        so_tien = Decimal(str(dk.get('so_tien', 0)))

        if dk.get('tk_no'):
            tong_no += so_tien
        if dk.get('tk_co'):
            tong_co += so_tien

    return tong_no == tong_co


def validate_tai_khoan_exists(tk_no: str = None, tk_co: str = None) -> bool:
    from app.extensions import db
    from sqlalchemy import text

    if tk_no:
        result = db.session.execute(
            text("SELECT 1 FROM accounting.he_thong_tai_khoan WHERE ma_tk = :tk AND co_the_dk = TRUE"),
            {"tk": tk_no}
        ).fetchone()
        if not result:
            raise ValidationError(f"Tài khoản nợ {tk_no} không tồn tại hoặc không được định khoản", "tk_no")

    if tk_co:
        result = db.session.execute(
            text("SELECT 1 FROM accounting.he_thong_tai_khoan WHERE ma_tk = :tk AND co_the_dk = TRUE"),
            {"tk": tk_co}
        ).fetchone()
        if not result:
            raise ValidationError(f"Tài khoản có {tk_co} không tồn tại hoặc không được định khoản", "tk_co")

    return True


def validate_so_tien(so_tien: float) -> bool:
    if so_tien <= 0:
        raise ValidationError("Số tiền phải lớn hơn 0", "so_tien")
    return True


def validate_ngay_ct(ngay_ct: date, ngay_hach_toan: date) -> bool:
    if ngay_ct > ngay_hach_toan:
        raise ValidationError("Ngày chứng từ không được lớn hơn ngày hạch toán", "ngay_ct")
    return True


def validate_loai_ct(loai_ct: str) -> bool:
    valid_types = ['PC', 'PT', 'BN', 'BC', 'PNK', 'PXK', 'HDMH', 'HDBL']
    if loai_ct not in valid_types:
        raise ValidationError(f"Loại chứng từ không hợp lệ: {loai_ct}", "loai_ct")
    return True


from datetime import date
