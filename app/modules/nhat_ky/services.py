"""Business logic services for NhatKy (Journal) module.

Provides service functions for:
- ChungTu: Journal document operations
- DinhKhoan: Booking entry operations
"""

from datetime import date
from decimal import Decimal
from typing import Optional, List, Dict, Any

from flask import current_app
from flask_jwt_extended import get_jwt_identity

from app.extensions import db
from app.modules.nhat_ky.models import ChungTu, DinhKhoan
from app.utils.validators import (
    validate_but_toan_can_bang,
    validate_tai_khoan_exists,
    validate_so_tien,
    validate_loai_ct
)
from app.utils.ky_ke_toan import check_ky_khoa
from app.utils.so_hieu import generate_so_chung_tu
from app import AppException


def to_dict_dinh_khoan(dk: DinhKhoan) -> dict:
    return {
        'id': dk.id,
        'stt': dk.stt,
        'tk_no': dk.tk_no,
        'tk_co': dk.tk_co,
        'so_tien': float(dk.so_tien) if dk.so_tien else 0,
        'so_tien_nt': float(dk.so_tien_nt) if dk.so_tien_nt else None,
        'ma_nt': dk.ma_nt,
        'ty_gia': float(dk.ty_gia) if dk.ty_gia else 1,
        'doi_tuong_id': dk.doi_tuong_id,
        'hang_hoa_id': dk.hang_hoa_id,
        'dvt': dk.dvt,
        'so_luong': float(dk.so_luong) if dk.so_luong else None,
        'don_gia': float(dk.don_gia) if dk.don_gia else None,
        'dien_giai': dk.dien_giai
    }


def to_dict_chung_tu(ct: ChungTu) -> dict:
    return {
        'id': ct.id,
        'so_ct': ct.so_ct,
        'loai_ct': ct.loai_ct,
        'ngay_ct': ct.ngay_ct.isoformat() if ct.ngay_ct else None,
        'ngay_hach_toan': ct.ngay_hach_toan.isoformat() if ct.ngay_hach_toan else None,
        'dien_giai': ct.dien_giai,
        'doi_tuong_id': ct.doi_tuong_id,
        'trang_thai': ct.trang_thai,
        'created_at': ct.created_at.isoformat() if ct.created_at else None,
        'updated_at': ct.updated_at.isoformat() if ct.updated_at else None,
        'dinh_khoan': [to_dict_dinh_khoan(dk) for dk in ct.dinh_khoan]
    }


def to_dict_list(ct: ChungTu) -> dict:
    return {
        'id': ct.id,
        'so_ct': ct.so_ct,
        'loai_ct': ct.loai_ct,
        'ngay_ct': ct.ngay_ct.isoformat() if ct.ngay_ct else None,
        'ngay_hach_toan': ct.ngay_hach_toan.isoformat() if ct.ngay_hach_toan else None,
        'dien_giai': ct.dien_giai,
        'doi_tuong_id': ct.doi_tuong_id,
        'trang_thai': ct.trang_thai,
        'tong_tien': sum(float(dk.so_tien) for dk in ct.dinh_khoan) if ct.dinh_khoan else 0
    }


def get_chung_tu_by_id(ct_id: int) -> ChungTu:
    ct = db.session.get(ChungTu, ct_id)
    if not ct:
        raise AppException(f'Chứng từ {ct_id} không tồn tại', 404)
    return ct


def list_chung_tu(
    page: int = 1,
    per_page: int = 20,
    loai_ct: Optional[str] = None,
    trang_thai: Optional[str] = None,
    tu_ngay: Optional[date] = None,
    den_ngay: Optional[date] = None,
    doi_tuong_id: Optional[int] = None,
    search: Optional[str] = None
) -> tuple:
    query = ChungTu.query

    if loai_ct:
        query = query.filter(ChungTu.loai_ct == loai_ct)
    if trang_thai:
        query = query.filter(ChungTu.trang_thai == trang_thai)
    if doi_tuong_id:
        query = query.filter(ChungTu.doi_tuong_id == doi_tuong_id)
    if tu_ngay:
        query = query.filter(ChungTu.ngay_hach_toan >= tu_ngay)
    if den_ngay:
        query = query.filter(ChungTu.ngay_hach_toan <= den_ngay)
    if search:
        query = query.filter(
            (ChungTu.so_ct.ilike(f'%{search}%')) |
            (ChungTu.dien_giai.ilike(f'%{search}%'))
        )

    query = query.order_by(ChungTu.ngay_hach_toan.desc(), ChungTu.so_ct.desc())

    pagination = query.paginate(page=page, per_page=per_page, error_out=False)

    return (
        [to_dict_list(ct) for ct in pagination.items],
        {
            'page': page,
            'per_page': per_page,
            'total': pagination.total,
            'pages': pagination.pages
        }
    )


def create_chung_tu(data: dict) -> ChungTu:
    user_id = get_jwt_identity()

    check_ky_khoa(data['ngay_hach_toan'])
    validate_loai_ct(data['loai_ct'])

    dinh_khoan_data = data['dinh_khoan']

    for dk in dinh_khoan_data:
        validate_so_tien(float(dk['so_tien']))

        if dk.get('tk_no'):
            validate_tai_khoan_exists(tk_no=dk['tk_no'])
        if dk.get('tk_co'):
            validate_tai_khoan_exists(tk_co=dk['tk_co'])

    if not validate_but_toan_can_bang(dinh_khoan_data):
        raise AppException('Bút toán không cân bằng: Tổng Nợ phải bằng Tổng Có', 400)

    so_ct = generate_so_chung_tu(data['loai_ct'])

    ct = ChungTu(
        so_ct=so_ct,
        loai_ct=data['loai_ct'],
        ngay_ct=data['ngay_ct'],
        ngay_hach_toan=data['ngay_hach_toan'],
        dien_giai=data.get('dien_giai'),
        doi_tuong_id=data.get('doi_tuong_id'),
        trang_thai='nhap',
        created_by=user_id
    )

    db.session.add(ct)
    db.session.flush()

    for dk_data in dinh_khoan_data:
        dk = DinhKhoan(
            chung_tu_id=ct.id,
            stt=dk_data['stt'],
            tk_no=dk_data.get('tk_no'),
            tk_co=dk_data.get('tk_co'),
            so_tien=dk_data['so_tien'],
            so_tien_nt=dk_data.get('so_tien_nt'),
            ma_nt=dk_data.get('ma_nt', 'VND'),
            ty_gia=dk_data.get('ty_gia', 1),
            doi_tuong_id=dk_data.get('doi_tuong_id'),
            hang_hoa_id=dk_data.get('hang_hoa_id'),
            dvt=dk_data.get('dvt'),
            so_luong=dk_data.get('so_luong'),
            don_gia=dk_data.get('don_gia'),
            dien_giai=dk_data.get('dien_giai')
        )
        db.session.add(dk)

    db.session.commit()

    return ct


def update_chung_tu(ct_id: int, data: dict) -> ChungTu:
    user_id = get_jwt_identity()

    ct = get_chung_tu_by_id(ct_id)

    if ct.trang_thai != 'nhap':
        raise AppException('Chỉ có thể sửa chứng từ ở trạng thái nháp', 400)

    if 'ngay_hach_toan' in data and data['ngay_hach_toan']:
        check_ky_khoa(data['ngay_hach_toan'])

    for key, value in data.items():
        if key != 'trang_thai' and value is not None:
            setattr(ct, key, value)

    ct.updated_by = user_id
    db.session.commit()

    return ct


def delete_chung_tu(ct_id: int) -> None:
    ct = get_chung_tu_by_id(ct_id)

    if ct.trang_thai != 'nhap':
        raise AppException('Chỉ có thể xóa chứng từ ở trạng thái nháp', 400)

    db.session.delete(ct)
    db.session.commit()


def duyet_chung_tu(ct_id: int) -> ChungTu:
    user_id = get_jwt_identity()

    ct = get_chung_tu_by_id(ct_id)

    if ct.trang_thai != 'nhap':
        raise AppException('Chứng từ đã được duyệt hoặc đã hủy', 400)

    if not ct.dinh_khoan or len(ct.dinh_khoan) == 0:
        raise AppException('Chứng từ không có định khoản', 400)

    dinh_khoan_data = []
    for dk in ct.dinh_khoan:
        dinh_khoan_data.append({
            'tk_no': dk.tk_no,
            'tk_co': dk.tk_co,
            'so_tien': float(dk.so_tien)
        })

    if not validate_but_toan_can_bang(dinh_khoan_data):
        raise AppException('Bút toán không cân bằng', 400)

    ct.trang_thai = 'da_duyet'
    ct.updated_by = user_id
    db.session.commit()

    return ct


def huy_chung_tu(ct_id: int) -> ChungTu:
    user_id = get_jwt_identity()

    ct = get_chung_tu_by_id(ct_id)

    if ct.trang_thai != 'da_duyet':
        raise AppException('Chỉ có thể hủy chứng từ đã duyệt', 400)

    ct.trang_thai = 'da_huy'
    ct.updated_by = user_id
    db.session.commit()

    return ct
