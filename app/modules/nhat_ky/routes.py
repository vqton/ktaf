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

from datetime import date
from flask import Blueprint, request
from flask_jwt_extended import jwt_required, get_jwt_identity

from app.extensions import db
from app.modules.nhat_ky.models import ChungTu
from app.modules.nhat_ky import services
from app.modules.nhat_ky.schemas import (
    create_schema,
    update_schema,
    detail_schema,
    list_schema
)
from app.utils.response import success_response, error_response, created_response
from marshmallow import ValidationError

nhat_ky_bp = Blueprint('nhat_ky', __name__)


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

    tu_ngay_date = date.fromisoformat(tu_ngay) if tu_ngay else None
    den_ngay_date = date.fromisoformat(den_ngay) if den_ngay else None

    items, pagination = services.list_chung_tu(
        page=page,
        per_page=per_page,
        loai_ct=loai_ct,
        trang_thai=trang_thai,
        tu_ngay=tu_ngay_date,
        den_ngay=den_ngay_date,
        doi_tuong_id=doi_tuong_id,
        search=search
    )

    return success_response(data=items, pagination=pagination)


@nhat_ky_bp.route('/<int:id>', methods=['GET'])
@jwt_required()
def get_chung_tu(id: int):
    ct = services.get_chung_tu_by_id(id)
    return success_response(data=services.to_dict_chung_tu(ct))


@nhat_ky_bp.route('', methods=['POST'])
@jwt_required()
def create_chung_tu():
    try:
        data = create_schema.load(request.get_json())
    except ValidationError as err:
        return error_response('Validation error', 'VALIDATION_ERROR', 400, err.messages)

    ct = services.create_chung_tu(data)
    return created_response(
        data=services.to_dict_chung_tu(ct),
        message='Tạo chứng từ thành công'
    )


@nhat_ky_bp.route('/<int:id>', methods=['PUT'])
@jwt_required()
def update_chung_tu(id: int):
    try:
        data = update_schema.load(request.get_json())
    except ValidationError as err:
        return error_response('Validation error', 'VALIDATION_ERROR', 400, err.messages)

    ct = services.update_chung_tu(id, data)
    return success_response(
        data=services.to_dict_chung_tu(ct),
        message='Cập nhật chứng từ thành công'
    )


@nhat_ky_bp.route('/<int:id>', methods=['DELETE'])
@jwt_required()
def delete_chung_tu(id: int):
    services.delete_chung_tu(id)
    return success_response(message='Xóa chứng từ thành công')


@nhat_ky_bp.route('/<int:id>/duyet', methods=['POST'])
@jwt_required()
def duyet_chung_tu(id: int):
    ct = services.duyet_chung_tu(id)
    return success_response(
        data=services.to_dict_chung_tu(ct),
        message='Duyệt chứng từ thành công'
    )


@nhat_ky_bp.route('/<int:id>/huy', methods=['POST'])
@jwt_required()
def huy_chung_tu(id: int):
    ct = services.huy_chung_tu(id)
    return success_response(
        data=services.to_dict_chung_tu(ct),
        message='Hủy chứng từ thành công'
    )
