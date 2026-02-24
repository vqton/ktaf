"""Journal entry (Nhật ký) API endpoints.

Provides CRUD operations for:
- ChungTu: Journal documents
- DinhKhoan: Booking entries within documents

Features:
- Automatic balance validation (Nợ = Có)
- Automatic document number generation
- Period locking check
- Approval workflow (duyet/huy)
"""

from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from app.extensions import db
from app.modules.nhat_ky.models import ChungTu, DinhKhoan
from app.modules.he_thong_tk.models import HeThongTaiKhoan
from app.utils.validators import (
    validate_but_toan_can_bang,
    validate_tai_khoan_exists,
    validate_so_tien,
    validate_loai_ct
)
from app.utils.ky_ke_toan import check_ky_khoa
from app.utils.so_hieu import generate_so_chung_tu
from app import AppException
from marshmallow import Schema, fields, validate, ValidationError
from datetime import date
from decimal import Decimal
from typing import List, Dict

nhat_ky_bp = Blueprint('nhat_ky', __name__)


class DinhKhoanSchema(Schema):
    stt = fields.Int(required=True)
    tk_no = fields.Str(required=False, allow_none=True)
    tk_co = fields.Str(required=False, allow_none=True)
    so_tien = fields.Decimal(required=True, places=2, validate=validate.Range(min=0.01))
    so_tien_nt = fields.Decimal(required=False, allow_none=True, places=2)
    ma_nt = fields.Str(required=False, load_default='VND')
    ty_gia = fields.Decimal(required=False, load_default=1.0, places=4)
    doi_tuong_id = fields.Int(required=False, allow_none=True)
    hang_hoa_id = fields.Int(required=False, allow_none=True)
    dvt = fields.Str(required=False, allow_none=True)
    so_luong = fields.Decimal(required=False, allow_none=True, places=4)
    don_gia = fields.Decimal(required=False, allow_none=True, places=2)
    dien_giai = fields.Str(required=False, allow_none=True)


class ChungTuSchema(Schema):
    loai_ct = fields.Str(required=True, validate=validate.OneOf(['PC', 'PT', 'BN', 'BC', 'PNK', 'PXK', 'HDMH', 'HDBL']))
    ngay_ct = fields.Date(required=True)
    ngay_hach_toan = fields.Date(required=True)
    dien_giai = fields.Str(required=False, allow_none=True)
    doi_tuong_id = fields.Int(required=False, allow_none=True)
    dinh_khoan = fields.List(fields.Nested(DinhKhoanSchema), required=True, validate=validate.Length(min=1))


class ChungTuUpdateSchema(Schema):
    ngay_ct = fields.Date(required=False)
    ngay_hach_toan = fields.Date(required=False)
    dien_giai = fields.Str(required=False, allow_none=True)
    doi_tuong_id = fields.Int(required=False, allow_none=True)
    trang_thai = fields.Str(required=False, validate=validate.OneOf(['nhap', 'da_duyet', 'da_huy']))


_create_schema = ChungTuSchema()
_update_schema = ChungTuUpdateSchema()


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


@nhat_ky_bp.route('', methods=['GET'])
@jwt_required()
def list_chung_tu():
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    loai_ct = request.args.get('loai_ct')
    trang_thai = request.args.get('trang_thai')
    tu_ngay = request.args.get('tu_ngay')
    den_ngay = request.args.get('den_ngay')
    doi_tuong_id = request.args.get('doi_tuong_id', type=int)
    search = request.args.get('search')

    query = ChungTu.query

    if loai_ct:
        query = query.filter(ChungTu.loai_ct == loai_ct)
    if trang_thai:
        query = query.filter(ChungTu.trang_thai == trang_thai)
    if doi_tuong_id:
        query = query.filter(ChungTu.doi_tuong_id == doi_tuong_id)
    if tu_ngay:
        query = query.filter(ChungTu.ngay_hach_toan >= date.fromisoformat(tu_ngay))
    if den_ngay:
        query = query.filter(ChungTu.ngay_hach_toan <= date.fromisoformat(den_ngay))
    if search:
        query = query.filter(
            (ChungTu.so_ct.ilike(f'%{search}%')) |
            (ChungTu.dien_giai.ilike(f'%{search}%'))
        )

    query = query.order_by(ChungTu.ngay_hach_toan.desc(), ChungTu.so_ct.desc())

    pagination = query.paginate(page=page, per_page=per_page, error_out=False)

    return jsonify({
        'success': True,
        'data': [to_dict_list(ct) for ct in pagination.items],
        'pagination': {
            'page': page,
            'per_page': per_page,
            'total': pagination.total,
            'pages': pagination.pages
        }
    }), 200


@nhat_ky_bp.route('/<int:id>', methods=['GET'])
@jwt_required()
def get_chung_tu(id: int):
    ct = ChungTu.query.get(id)
    if not ct:
        raise AppException(f'Chứng từ {id} không tồn tại', 404)

    return jsonify({
        'success': True,
        'data': to_dict_chung_tu(ct)
    }), 200


@nhat_ky_bp.route('', methods=['POST'])
@jwt_required()
def create_chung_tu():
    user_id = get_jwt_identity()

    try:
        data = _create_schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    check_ky_khoa(data['ngay_hach_toan'])

    validate_loai_ct(data['loai_ct'])

    dinh_khoan_data = []
    for dk in data['dinh_khoan']:
        validate_so_tien(float(dk['so_tien']))

        if dk.get('tk_no'):
            validate_tai_khoan_exists(tk_no=dk['tk_no'])
        if dk.get('tk_co'):
            validate_tai_khoan_exists(tk_co=dk['tk_co'])

        dinh_khoan_data.append(dk)

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

    return jsonify({
        'success': True,
        'data': to_dict_chung_tu(ct),
        'message': 'Tạo chứng từ thành công'
    }), 201


@nhat_ky_bp.route('/<int:id>', methods=['PUT'])
@jwt_required()
def update_chung_tu(id: int):
    user_id = get_jwt_identity()

    ct = ChungTu.query.get(id)
    if not ct:
        raise AppException(f'Chứng từ {id} không tồn tại', 404)

    if ct.trang_thai != 'nhap':
        raise AppException('Chỉ có thể sửa chứng từ ở trạng thái nháp', 400)

    try:
        data = _update_schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    if 'ngay_hach_toan' in data and data['ngay_hach_toan']:
        check_ky_khoa(data['ngay_hach_toan'])

    for key, value in data.items():
        if key != 'trang_thai' and value is not None:
            setattr(ct, key, value)

    ct.updated_by = user_id
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict_chung_tu(ct),
        'message': 'Cập nhật chứng từ thành công'
    }), 200


@nhat_ky_bp.route('/<int:id>', methods=['DELETE'])
@jwt_required()
def delete_chung_tu(id: int):
    ct = ChungTu.query.get(id)
    if not ct:
        raise AppException(f'Chứng từ {id} không tồn tại', 404)

    if ct.trang_thai != 'nhap':
        raise AppException('Chỉ có thể xóa chứng từ ở trạng thái nháp', 400)

    db.session.delete(ct)
    db.session.commit()

    return jsonify({
        'success': True,
        'message': 'Xóa chứng từ thành công'
    }), 200


@nhat_ky_bp.route('/<int:id>/duyet', methods=['POST'])
@jwt_required()
def duyet_chung_tu(id: int):
    user_id = get_jwt_identity()

    ct = ChungTu.query.get(id)
    if not ct:
        raise AppException(f'Chứng từ {id} không tồn tại', 404)

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

    return jsonify({
        'success': True,
        'data': to_dict_chung_tu(ct),
        'message': 'Duyệt chứng từ thành công'
    }), 200


@nhat_ky_bp.route('/<int:id>/huy', methods=['POST'])
@jwt_required()
def huy_chung_tu(id: int):
    user_id = get_jwt_identity()

    ct = ChungTu.query.get(id)
    if not ct:
        raise AppException(f'Chứng từ {id} không tồn tại', 404)

    if ct.trang_thai != 'da_duyet':
        raise AppException('Chỉ có thể hủy chứng từ đã duyệt', 400)

    ct.trang_thai = 'da_huy'
    ct.updated_by = user_id
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict_chung_tu(ct),
        'message': 'Hủy chứng từ thành công'
    }), 200
