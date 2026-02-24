from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from app.extensions import db
from app.modules.danh_muc.doi_tuong.models import DoiTuong
from app import AppException
from marshmallow import Schema, fields, validate, ValidationError

doi_tuong_bp = Blueprint('doi_tuong', __name__)


class DoiTuongSchema(Schema):
    ma_dt = fields.Str(required=True, validate=validate.Length(min=1, max=50))
    ten_dt = fields.Str(required=True, validate=validate.Length(min=1, max=255))
    loai_dt = fields.Str(required=True, validate=validate.OneOf(['khach_hang', 'nha_cung_cap', 'ca_nhan', 'to_chuc']))
    dia_chi = fields.Str(required=False, allow_none=True)
    dien_thoai = fields.Str(required=False, allow_none=True)
    email = fields.Email(required=False, allow_none=True)
    ma_so_thue = fields.Str(required=False, allow_none=True)
    is_active = fields.Bool(required=False, load_default=True)
    metadata = fields.Dict(required=False, load_default={})


class DoiTuongUpdateSchema(Schema):
    ten_dt = fields.Str(required=False, validate=validate.Length(min=1, max=255))
    loai_dt = fields.Str(required=False, validate=validate.OneOf(['khach_hang', 'nha_cung_cap', 'ca_nhan', 'to_chuc']))
    dia_chi = fields.Str(required=False, allow_none=True)
    dien_thoai = fields.Str(required=False, allow_none=True)
    email = fields.Email(required=False, allow_none=True)
    ma_so_thue = fields.Str(required=False, allow_none=True)
    is_active = fields.Bool(required=False)
    metadata = fields.Dict(required=False)


_schema = DoiTuongSchema()
_update_schema = DoiTuongUpdateSchema()


def to_dict(dt: DoiTuong) -> dict:
    return {
        'id': dt.id,
        'ma_dt': dt.ma_dt,
        'ten_dt': dt.ten_dt,
        'loai_dt': dt.loai_dt,
        'dia_chi': dt.dia_chi,
        'dien_thoai': dt.dien_thoai,
        'email': dt.email,
        'ma_so_thue': dt.ma_so_thue,
        'is_active': dt.is_active,
        'metadata': dt.metadata,
        'created_at': dt.created_at.isoformat() if dt.created_at else None,
        'updated_at': dt.updated_at.isoformat() if dt.updated_at else None
    }


@doi_tuong_bp.route('', methods=['GET'])
@jwt_required()
def list_doi_tuong():
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    loai_dt = request.args.get('loai_dt')
    search = request.args.get('search')
    is_active = request.args.get('is_active', default=True, type=bool)

    query = DoiTuong.query

    if loai_dt:
        query = query.filter(DoiTuong.loai_dt == loai_dt)
    if search:
        query = query.filter(
            (DoiTuong.ma_dt.ilike(f'%{search}%')) |
            (DoiTuong.ten_dt.ilike(f'%{search}%'))
        )
    query = query.filter(DoiTuong.is_active == is_active)

    query = query.order_by(DoiTuong.ma_dt)

    pagination = query.paginate(page=page, per_page=per_page, error_out=False)

    return jsonify({
        'success': True,
        'data': [to_dict(dt) for dt in pagination.items],
        'pagination': {
            'page': page,
            'per_page': per_page,
            'total': pagination.total,
            'pages': pagination.pages
        }
    }), 200


@doi_tuong_bp.route('/<int:id>', methods=['GET'])
@jwt_required()
def get_doi_tuong(id: int):
    dt = DoiTuong.query.get(id)
    if not dt:
        raise AppException(f'Đối tượng {id} không tồn tại', 404)

    return jsonify({
        'success': True,
        'data': to_dict(dt)
    }), 200


@doi_tuong_bp.route('', methods=['POST'])
@jwt_required()
def create_doi_tuong():
    user_id = get_jwt_identity()

    try:
        data = _schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    if DoiTuong.query.filter_by(ma_dt=data['ma_dt']).first():
        raise AppException(f'Mã đối tượng {data["ma_dt"]} đã tồn tại', 400)

    dt = DoiTuong(**data)
    dt.created_by = user_id

    db.session.add(dt)
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(dt),
        'message': 'Tạo đối tượng thành công'
    }), 201


@doi_tuong_bp.route('/<int:id>', methods=['PUT'])
@jwt_required()
def update_doi_tuong(id: int):
    user_id = get_jwt_identity()

    dt = DoiTuong.query.get(id)
    if not dt:
        raise AppException(f'Đối tượng {id} không tồn tại', 404)

    try:
        data = _update_schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    for key, value in data.items():
        if value is not None:
            setattr(dt, key, value)

    dt.updated_by = user_id
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(dt),
        'message': 'Cập nhật đối tượng thành công'
    }), 200


@doi_tuong_bp.route('/<int:id>', methods=['DELETE'])
@jwt_required()
def delete_doi_tuong(id: int):
    dt = DoiTuong.query.get(id)
    if not dt:
        raise AppException(f'Đối tượng {id} không tồn tại', 404)

    dt.is_active = False
    db.session.commit()

    return jsonify({
        'success': True,
        'message': 'Xóa đối tượng thành công'
    }), 200
