"""Chart of accounts (Hệ thống tài khoản) API endpoints.

Provides CRUD operations for HeThongTaiKhoan:
- GET /: List all accounts with optional tree structure
- POST /: Create new account
- GET /<ma_tk>: Get account details
- PUT /<ma_tk>: Update account
- DELETE /<ma_tk>: Delete account
- GET /tree: Get hierarchical tree view
"""

from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from app.extensions import db
from app.modules.he_thong_tk.models import HeThongTaiKhoan
from app import AppException
from marshmallow import Schema, fields, validate, ValidationError, post_load
from typing import Optional

he_thong_tk_bp = Blueprint('he_thong_tk', __name__)


class HeThongTaiKhoanSchema(Schema):
    ma_tk = fields.Str(required=True, validate=validate.Length(min=1, max=10))
    ten_tk = fields.Str(required=True, validate=validate.Length(min=1, max=255))
    loai_tk = fields.Str(required=True, validate=validate.OneOf(
        ['tai_san', 'nguon_von', 'doanh_thu', 'chi_phi', 'ngoai_bang']
    ))
    cap_tk = fields.Int(required=True, validate=validate.Range(min=1, max=4))
    ma_tk_cha = fields.Str(required=False, allow_none=True, validate=validate.Length(max=10))
    tinh_chat = fields.Str(required=True, validate=validate.OneOf(['du', 'co', 'luong_tinh']))
    co_the_dk = fields.Bool(required=False, load_default=False)
    is_active = fields.Bool(required=False, load_default=True)
    metadata = fields.Dict(required=False, load_default={})


class HeThongTaiKhoanUpdateSchema(Schema):
    ten_tk = fields.Str(required=False, validate=validate.Length(min=1, max=255))
    ma_tk_cha = fields.Str(required=False, allow_none=True)
    tinh_chat = fields.Str(required=False, validate=validate.OneOf(['du', 'co', 'luong_tinh']))
    co_the_dk = fields.Bool(required=False)
    is_active = fields.Bool(required=False)
    metadata = fields.Dict(required=False)


_schema = HeThongTaiKhoanSchema()
_update_schema = HeThongTaiKhoanUpdateSchema()


def to_dict(tk: HeThongTaiKhoan) -> dict:
    return {
        'ma_tk': tk.ma_tk,
        'ten_tk': tk.ten_tk,
        'loai_tk': tk.loai_tk,
        'cap_tk': tk.cap_tk,
        'ma_tk_cha': tk.ma_tk_cha,
        'tinh_chat': tk.tinh_chat,
        'co_the_dk': tk.co_the_dk,
        'is_active': tk.is_active,
        'metadata': tk.metadata,
        'created_at': tk.created_at.isoformat() if tk.created_at else None,
        'updated_at': tk.updated_at.isoformat() if tk.updated_at else None
    }


@he_thong_tk_bp.route('', methods=['GET'])
@jwt_required()
def list_accounts():
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    loai_tk = request.args.get('loai_tk')
    cap_tk = request.args.get('cap_tk', type=int)
    co_the_dk = request.args.get('co_the_dk', type=bool)
    search = request.args.get('search')
    is_active = request.args.get('is_active', default=True, type=bool)

    query = HeThongTaiKhoan.query

    if loai_tk:
        query = query.filter(HeThongTaiKhoan.loai_tk == loai_tk)
    if cap_tk:
        query = query.filter(HeThongTaiKhoan.cap_tk == cap_tk)
    if co_the_dk is not None:
        query = query.filter(HeThongTaiKhoan.co_the_dk == co_the_dk)
    if search:
        query = query.filter(
            (HeThongTaiKhoan.ma_tk.ilike(f'%{search}%')) |
            (HeThongTaiKhoan.ten_tk.ilike(f'%{search}%'))
        )
    query = query.filter(HeThongTaiKhoan.is_active == is_active)

    query = query.order_by(HeThongTaiKhoan.ma_tk)

    pagination = query.paginate(page=page, per_page=per_page, error_out=False)

    return jsonify({
        'success': True,
        'data': [to_dict(tk) for tk in pagination.items],
        'pagination': {
            'page': page,
            'per_page': per_page,
            'total': pagination.total,
            'pages': pagination.pages
        }
    }), 200


@he_thong_tk_bp.route('/<string:ma_tk>', methods=['GET'])
@jwt_required()
def get_account(ma_tk: str):
    tk = HeThongTaiKhoan.query.get(ma_tk)
    if not tk:
        raise AppException(f'Tài khoản {ma_tk} không tồn tại', 404)

    return jsonify({
        'success': True,
        'data': to_dict(tk)
    }), 200


@he_thong_tk_bp.route('', methods=['POST'])
@jwt_required()
def create_account():
    user_id = get_jwt_identity()

    try:
        data = _schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    if HeThongTaiKhoan.query.get(data['ma_tk']):
        raise AppException(f'Tài khoản {data["ma_tk"]} đã tồn tại', 400)

    if data.get('ma_tk_cha'):
        parent = HeThongTaiKhoan.query.get(data['ma_tk_cha'])
        if not parent:
            raise AppException(f'Tài khoản cha {data["ma_tk_cha"]} không tồn tại', 400)
        if parent.cap_tk >= 4:
            raise AppException('Không thể tạo tài khoản con cấp 4', 400)

    tk = HeThongTaiKhoan(**data)
    tk.created_by = user_id

    db.session.add(tk)
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(tk),
        'message': 'Tạo tài khoản thành công'
    }), 201


@he_thong_tk_bp.route('/<string:ma_tk>', methods=['PUT'])
@jwt_required()
def update_account(ma_tk: str):
    user_id = get_jwt_identity()

    tk = HeThongTaiKhoan.query.get(ma_tk)
    if not tk:
        raise AppException(f'Tài khoản {ma_tk} không tồn tại', 404)

    try:
        data = _update_schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    if 'ma_tk_cha' in data and data['ma_tk_cha']:
        parent = HeThongTaiKhoan.query.get(data['ma_tk_cha'])
        if not parent:
            raise AppException(f'Tài khoản cha {data["ma_tk_cha"]} không tồn tại', 400)
        if parent.cap_tk >= 4:
            raise AppException('Không thể gán tài khoản cha cấp 4', 400)

    for key, value in data.items():
        if value is not None:
            setattr(tk, key, value)

    tk.updated_by = user_id
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(tk),
        'message': 'Cập nhật tài khoản thành công'
    }), 200


@he_thong_tk_bp.route('/<string:ma_tk>', methods=['DELETE'])
@jwt_required()
def delete_account(ma_tk: str):
    tk = HeThongTaiKhoan.query.get(ma_tk)
    if not tk:
        raise AppException(f'Tài khoản {ma_tk} không tồn tại', 404)

    has_children = HeThongTaiKhoan.query.filter_by(ma_tk_cha=ma_tk).count() > 0
    if has_children:
        raise AppException('Không thể xóa tài khoản có tài khoản con', 400)

    tk.is_active = False
    db.session.commit()

    return jsonify({
        'success': True,
        'message': 'Xóa tài khoản thành công'
    }), 200


@he_thong_tk_bp.route('/tree', methods=['GET'])
@jwt_required()
def get_account_tree():
    all_accounts = HeThongTaiKhoan.query.filter_by(is_active=True).order_by(HeThongTaiKhoan.ma_tk).all()

    def build_tree(parent_ma_tk=None):
        children = []
        for tk in all_accounts:
            if tk.ma_tk_cha == parent_ma_tk:
                children.append({
                    'ma_tk': tk.ma_tk,
                    'ten_tk': tk.ten_tk,
                    'loai_tk': tk.loai_tk,
                    'cap_tk': tk.cap_tk,
                    'tinh_chat': tk.tinh_chat,
                    'co_the_dk': tk.co_the_dk,
                    'children': build_tree(tk.ma_tk)
                })
        return children

    tree = build_tree()
    return jsonify({
        'success': True,
        'data': tree
    }), 200
