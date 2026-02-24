"""Validation utilities for accounting operations.

Provides validation functions for:
- Balance validation (Nợ = Có)
- Account existence validation
- Amount, date, and document type validation

Raises:
    ValidationError: When validation fails with field name and message.
"""

from datetime import date
from decimal import Decimal
from typing import List, Dict, Optional


class ValidationError(Exception):
    """Custom exception for validation errors with field information."""

    def __init__(self, message: str, field: Optional[str] = None):
        self.message = message
        self.field = field
        super().__init__(message)


def validate_but_toan_can_bang(dinh_khoan: List[Dict]) -> bool:
    """Validate that total debit equals total credit.

    Args:
        dinh_khoan: List of journal entries with 'tk_no', 'tk_co', 'so_tien'

    Returns:
        True if balanced (tong_no == tong_co)

    Raises:
        ValidationError: If debits don't equal credits
    """
    tong_no = Decimal('0')
    tong_co = Decimal('0')

    for dk in dinh_khoan:
        so_tien = Decimal(str(dk.get('so_tien', 0)))

        if dk.get('tk_no'):
            tong_no += so_tien
        if dk.get('tk_co'):
            tong_co += so_tien

    if tong_no != tong_co:
        raise ValidationError(
            f"Bút toán không cân bằng: Nợ {tong_no} ≠ Có {tong_co}",
            "so_tien"
        )
    return True


def validate_tai_khoan_exists(tk_no: Optional[str] = None, tk_co: Optional[str] = None) -> bool:
    """Validate that debit and credit accounts exist and can be used for booking.

    Args:
        tk_no: Debit account code (optional)
        tk_co: Credit account code (optional)

    Returns:
        True if all accounts exist and are bookable

    Raises:
        ValidationError: If account doesn't exist or cannot be booked
    """
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
    """Validate that amount is positive.

    Args:
        so_tien: Amount to validate

    Returns:
        True if amount > 0

    Raises:
        ValidationError: If amount <= 0
    """
    if so_tien <= 0:
        raise ValidationError("Số tiền phải lớn hơn 0", "so_tien")
    return True


def validate_ngay_ct(ngay_ct: date, ngay_hach_toan: date) -> bool:
    """Validate that document date is not after accounting date.

    Args:
        ngay_ct: Document date
        ngay_hach_toan: Accounting date

    Returns:
        True if ngay_ct <= ngay_hach_toan

    Raises:
        ValidationError: If ngay_ct > ngay_hach_toan
    """
    if ngay_ct > ngay_hach_toan:
        raise ValidationError("Ngày chứng từ không được lớn hơn ngày hạch toán", "ngay_ct")
    return True


def validate_loai_ct(loai_ct: str) -> bool:
    """Validate document type is in allowed list.

    Args:
        loai_ct: Document type code

    Returns:
        True if document type is valid

    Raises:
        ValidationError: If document type is invalid
    """
    valid_types = ['PC', 'PT', 'BN', 'BC', 'PNK', 'PXK', 'HDMH', 'HDBL']
    if loai_ct not in valid_types:
        raise ValidationError(f"Loại chứng từ không hợp lệ: {loai_ct}", "loai_ct")
    return True
