from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from app.extensions import db
from app.modules.danh_muc.ngan_hang.models import NganHang
from app import AppException
from marshmallow import Schema, fields, validate, ValidationError

ngan_hang_bp = Blueprint('ngan_hang', __name__)


class NganHangSchema(Schema):
    ma_nh = fields.Str(required=True, validate=validate.Length(min=1, max=20))
    ten_nh = fields.Str(required=True, validate=validate.Length(min=1, max=255))
    chi_nhanh = fields.Str(required=False, allow_none=True)
    dia_chi = fields.Str(required=False, allow_none=True)
    so_tai_khoan = fields.Str(required=False, allow_none=True)
    chu_tai_khoan = fields.Str(required=False, allow_none=True)
    ma_tk_ngan_hang = fields.Str(required=False, allow_none=True)
    is_active = fields.Bool(required=False, load_default=True)
    metadata = fields.Dict(required=False, load_default={})


class NganHangUpdateSchema(Schema):
    ten_nh = fields.Str(required=False, validate=validate.Length(min=1, max=255))
    chi_nhanh = fields.Str(required=False, allow_none=True)
    dia_chi = fields.Str(required=False, allow_none=True)
    so_tai_khoan = fields.Str(required=False, allow_none=True)
    chu_tai_khoan = fields.Str(required=False, allow_none=True)
    ma_tk_ngan_hang = fields.Str(required=False, allow_none=True)
    is_active = fields.Bool(required=False)
    metadata = fields.Dict(required=False)


_schema = NganHangSchema()
_update_schema = NganHangUpdateSchema()


def to_dict(nh: NganHang) -> dict:
    return {
        'id': nh.id,
        'ma_nh': nh.ma_nh,
        'ten_nh': nh.ten_nh,
        'chi_nhanh': nh.chi_nhanh,
        'dia_chi': nh.dia_chi,
        'so_tai_khoan': nh.so_tai_khoan,
        'chu_tai_khoan': nh.chu_tai_khoan,
        'ma_tk_ngan_hang': nh.ma_tk_ngan_hang,
        'is_active': nh.is_active,
        'metadata': nh.metadata,
        'created_at': nh.created_at.isoformat() if nh.created_at else None,
        'updated_at': nh.updated_at.isoformat() if nh.updated_at else None
    }


@ngan_hang_bp.route('', methods=['GET'])
@jwt_required()
def list_ngan_hang():
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    search = request.args.get('search')
    is_active = request.args.get('is_active', default=True, type=bool)

    query = NganHang.query

    if search:
        query = query.filter(
            (NganHang.ma_nh.ilike(f'%{search}%')) |
            (NganHang.ten_nh.ilike(f'%{search}%'))
        )
    query = query.filter(NganHang.is_active == is_active)

    query = query.order_by(NganHang.ma_nh)

    pagination = query.paginate(page=page, per_page=per_page, error_out=False)

    return jsonify({
        'success': True,
        'data': [to_dict(nh) for nh in pagination.items],
        'pagination': {
            'page': page,
            'per_page': per_page,
            'total': pagination.total,
            'pages': pagination.pages
        }
    }), 200


@ngan_hang_bp.route('/<int:id>', methods=['GET'])
@jwt_required()
def get_ngan_hang(id: int):
    nh = NganHang.query.get(id)
    if not nh:
        raise AppException(f'Ngân hàng {id} không tồn tại', 404)

    return jsonify({
        'success': True,
        'data': to_dict(nh)
    }), 200


@ngan_hang_bp.route('', methods=['POST'])
@jwt_required()
def create_ngan_hang():
    user_id = get_jwt_identity()

    try:
        data = _schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    if NganHang.query.filter_by(ma_nh=data['ma_nh']).first():
        raise AppException(f'Mã ngân hàng {data["ma_nh"]} đã tồn tại', 400)

    nh = NganHang(**data)
    nh.created_by = user_id

    db.session.add(nh)
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(nh),
        'message': 'Tạo ngân hàng thành công'
    }), 201


@ngan_hang_bp.route('/<int:id>', methods=['PUT'])
@jwt_required()
def update_ngan_hang(id: int):
    user_id = get_jwt_identity()

    nh = NganHang.query.get(id)
    if not nh:
        raise AppException(f'Ngân hàng {id} không tồn tại', 404)

    try:
        data = _update_schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    for key, value in data.items():
        if value is not None:
            setattr(nh, key, value)

    nh.updated_by = user_id
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(nh),
        'message': 'Cập nhật ngân hàng thành công'
    }), 200


@ngan_hang_bp.route('/<int:id>', methods=['DELETE'])
@jwt_required()
def delete_ngan_hang(id: int):
    nh = NganHang.query.get(id)
    if not nh:
        raise AppException(f'Ngân hàng {id} không tồn tại', 404)

    nh.is_active = False
    db.session.commit()

    return jsonify({
        'success': True,
        'message': 'Xóa ngân hàng thành công'
    }), 200
