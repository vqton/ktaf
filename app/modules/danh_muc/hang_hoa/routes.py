from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from app.extensions import db
from app.modules.danh_muc.hang_hoa.models import HangHoa
from app import AppException
from marshmallow import Schema, fields, validate, ValidationError

hang_hoa_bp = Blueprint('hang_hoa', __name__)


class HangHoaSchema(Schema):
    ma_hh = fields.Str(required=True, validate=validate.Length(min=1, max=50))
    ten_hh = fields.Str(required=True, validate=validate.Length(min=1, max=255))
    loai_hh = fields.Str(required=True, validate=validate.OneOf(['hang_hoa', 'dich_vu', 'vat_tu']))
    don_vi_tinh = fields.Str(required=False, allow_none=True)
    gia_von = fields.Decimal(required=False, allow_none=True, places=2)
    gia_ban = fields.Decimal(required=False, allow_none=True, places=2)
    ma_tk_hh = fields.Str(required=False, allow_none=True)
    ma_tk_gv = fields.Str(required=False, allow_none=True)
    thue_gtgt = fields.Decimal(required=False, load_default=10.0, places=2)
    is_active = fields.Bool(required=False, load_default=True)
    metadata = fields.Dict(required=False, load_default={})


class HangHoaUpdateSchema(Schema):
    ten_hh = fields.Str(required=False, validate=validate.Length(min=1, max=255))
    loai_hh = fields.Str(required=False, validate=validate.OneOf(['hang_hoa', 'dich_vu', 'vat_tu']))
    don_vi_tinh = fields.Str(required=False, allow_none=True)
    gia_von = fields.Decimal(required=False, allow_none=True, places=2)
    gia_ban = fields.Decimal(required=False, allow_none=True, places=2)
    ma_tk_hh = fields.Str(required=False, allow_none=True)
    ma_tk_gv = fields.Str(required=False, allow_none=True)
    thue_gtgt = fields.Decimal(required=False, places=2)
    is_active = fields.Bool(required=False)
    metadata = fields.Dict(required=False)


_schema = HangHoaSchema()
_update_schema = HangHoaUpdateSchema()


def to_dict(hh: HangHoa) -> dict:
    return {
        'id': hh.id,
        'ma_hh': hh.ma_hh,
        'ten_hh': hh.ten_hh,
        'loai_hh': hh.loai_hh,
        'don_vi_tinh': hh.don_vi_tinh,
        'gia_von': float(hh.gia_von) if hh.gia_von else None,
        'gia_ban': float(hh.gia_ban) if hh.gia_ban else None,
        'ma_tk_hh': hh.ma_tk_hh,
        'ma_tk_gv': hh.ma_tk_gv,
        'thue_gtgt': float(hh.thue_gtgt) if hh.thue_gtgt else 10.0,
        'is_active': hh.is_active,
        'metadata': hh.metadata,
        'created_at': hh.created_at.isoformat() if hh.created_at else None,
        'updated_at': hh.updated_at.isoformat() if hh.updated_at else None
    }


@hang_hoa_bp.route('', methods=['GET'])
@jwt_required()
def list_hang_hoa():
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    loai_hh = request.args.get('loai_hh')
    search = request.args.get('search')
    is_active = request.args.get('is_active', default=True, type=bool)

    query = HangHoa.query

    if loai_hh:
        query = query.filter(HangHoa.loai_hh == loai_hh)
    if search:
        query = query.filter(
            (HangHoa.ma_hh.ilike(f'%{search}%')) |
            (HangHoa.ten_hh.ilike(f'%{search}%'))
        )
    query = query.filter(HangHoa.is_active == is_active)

    query = query.order_by(HangHoa.ma_hh)

    pagination = query.paginate(page=page, per_page=per_page, error_out=False)

    return jsonify({
        'success': True,
        'data': [to_dict(hh) for hh in pagination.items],
        'pagination': {
            'page': page,
            'per_page': per_page,
            'total': pagination.total,
            'pages': pagination.pages
        }
    }), 200


@hang_hoa_bp.route('/<int:id>', methods=['GET'])
@jwt_required()
def get_hang_hoa(id: int):
    hh = HangHoa.query.get(id)
    if not hh:
        raise AppException(f'Hàng hóa {id} không tồn tại', 404)

    return jsonify({
        'success': True,
        'data': to_dict(hh)
    }), 200


@hang_hoa_bp.route('', methods=['POST'])
@jwt_required()
def create_hang_hoa():
    user_id = get_jwt_identity()

    try:
        data = _schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    if HangHoa.query.filter_by(ma_hh=data['ma_hh']).first():
        raise AppException(f'Mã hàng hóa {data["ma_hh"]} đã tồn tại', 400)

    hh = HangHoa(**data)
    hh.created_by = user_id

    db.session.add(hh)
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(hh),
        'message': 'Tạo hàng hóa thành công'
    }), 201


@hang_hoa_bp.route('/<int:id>', methods=['PUT'])
@jwt_required()
def update_hang_hoa(id: int):
    user_id = get_jwt_identity()

    hh = HangHoa.query.get(id)
    if not hh:
        raise AppException(f'Hàng hóa {id} không tồn tại', 404)

    try:
        data = _update_schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    for key, value in data.items():
        if value is not None:
            setattr(hh, key, value)

    hh.updated_by = user_id
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(hh),
        'message': 'Cập nhật hàng hóa thành công'
    }), 200


@hang_hoa_bp.route('/<int:id>', methods=['DELETE'])
@jwt_required()
def delete_hang_hoa(id: int):
    hh = HangHoa.query.get(id)
    if not hh:
        raise AppException(f'Hàng hóa {id} không tồn tại', 404)

    hh.is_active = False
    db.session.commit()

    return jsonify({
        'success': True,
        'message': 'Xóa hàng hóa thành công'
    }), 200
