from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from app.extensions import db
from app.modules.ky_ke_toan.models import KyKeToan
from app import AppException
from marshmallow import Schema, fields, validate, ValidationError
from datetime import date
from calendar import monthrange

ky_ke_toan_bp = Blueprint('ky_ke_toan', __name__)


class KyKeToanSchema(Schema):
    nam = fields.Int(required=True, validate=validate.Range(min=2020, max=2100))
    thang = fields.Int(required=True, validate=validate.Range(min=1, max=12))
    trang_thai = fields.Str(required=False, validate=validate.OneOf(['mo', 'khoa']))


_update_schema = KyKeToanSchema()


def to_dict(ky: KyKeToan) -> dict:
    return {
        'id': ky.id,
        'nam': ky.nam,
        'thang': ky.thang,
        'tu_ngay': ky.tu_ngay.isoformat() if ky.tu_ngay else None,
        'den_ngay': ky.den_ngay.isoformat() if ky.den_ngay else None,
        'trang_thai': ky.trang_thai,
        'created_at': ky.created_at.isoformat() if ky.created_at else None,
        'updated_at': ky.updated_at.isoformat() if ky.updated_at else None
    }


def calculate_ky_dates(nam: int, thang: int) -> tuple:
    tu_ngay = date(nam, thang, 1)
    last_day = monthrange(nam, thang)[1]
    den_ngay = date(nam, thang, last_day)
    return tu_ngay, den_ngay


@ky_ke_toan_bp.route('', methods=['GET'])
@jwt_required()
def list_ky_ke_toan():
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    nam = request.args.get('nam', type=int)
    trang_thai = request.args.get('trang_thai')

    query = KyKeToan.query

    if nam:
        query = query.filter(KyKeToan.nam == nam)
    if trang_thai:
        query = query.filter(KyKeToan.trang_thai == trang_thai)

    query = query.order_by(KyKeToan.nam.desc(), KyKeToan.thang.desc())

    pagination = query.paginate(page=page, per_page=per_page, error_out=False)

    return jsonify({
        'success': True,
        'data': [to_dict(ky) for ky in pagination.items],
        'pagination': {
            'page': page,
            'per_page': per_page,
            'total': pagination.total,
            'pages': pagination.pages
        }
    }), 200


@ky_ke_toan_bp.route('/current', methods=['GET'])
@jwt_required()
def get_current_ky():
    today = date.today()
    ky = KyKeToan.query.filter(
        KyKeToan.tu_ngay <= today,
        KyKeToan.den_ngay >= today
    ).first()

    if not ky:
        return jsonify({
            'success': True,
            'data': None,
            'message': 'Không có kỳ kế toán cho ngày hiện tại'
        }), 200

    return jsonify({
        'success': True,
        'data': to_dict(ky)
    }), 200


@ky_ke_toan_bp.route('', methods=['POST'])
@jwt_required()
def create_ky_ke_toan():
    user_id = get_jwt_identity()

    try:
        data = _update_schema.load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    existing = KyKeToan.query.filter_by(nam=data['nam'], thang=data['thang']).first()
    if existing:
        raise AppException(f'Kỳ kế toán {data["thang"]}/{data["nam"]} đã tồn tại', 400)

    tu_ngay, den_ngay = calculate_ky_dates(data['nam'], data['thang'])

    ky = KyKeToan(
        nam=data['nam'],
        thang=data['thang'],
        tu_ngay=tu_ngay,
        den_ngay=den_ngay,
        trang_thai=data.get('trang_thai', 'mo')
    )
    ky.created_by = user_id

    db.session.add(ky)
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(ky),
        'message': 'Tạo kỳ kế toán thành công'
    }), 201


@ky_ke_toan_bp.route('/<int:id>/khoa', methods=['POST'])
@jwt_required()
def lock_ky_ke_toan(id: int):
    ky = KyKeToan.query.get(id)
    if not ky:
        raise AppException(f'Kỳ kế toán {id} không tồn tại', 404)

    if ky.trang_thai == 'khoa':
        raise AppException('Kỳ kế toán đã khóa', 400)

    ky.trang_thai = 'khoa'
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(ky),
        'message': 'Khóa kỳ kế toán thành công'
    }), 200


@ky_ke_toan_bp.route('/<int:id>/mo', methods=['POST'])
@jwt_required()
def unlock_ky_ke_toan(id: int):
    user_id = get_jwt_identity()

    ky = KyKeToan.query.get(id)
    if not ky:
        raise AppException(f'Kỳ kế toán {id} không tồn tại', 404)

    if ky.trang_thai == 'mo':
        raise AppException('Kỳ kế toán đang mở', 400)

    ky.trang_thai = 'mo'
    ky.updated_by = user_id
    db.session.commit()

    return jsonify({
        'success': True,
        'data': to_dict(ky),
        'message': 'Mở khóa kỳ kế toán thành công'
    }), 200


@ky_ke_toan_bp.route('/init-year', methods=['POST'])
@jwt_required()
def init_year():
    user_id = get_jwt_identity()
    data = request.get_json()
    nam = data.get('nam')

    if not nam:
        return jsonify({'success': False, 'message': 'Thiếu năm'}), 400

    created_count = 0
    for thang in range(1, 13):
        existing = KyKeToan.query.filter_by(nam=nam, thang=thang).first()
        if not existing:
            tu_ngay, den_ngay = calculate_ky_dates(nam, thang)
            ky = KyKeToan(
                nam=nam,
                thang=thang,
                tu_ngay=tu_ngay,
                den_ngay=den_ngay,
                trang_thai='mo'
            )
            ky.created_by = user_id
            db.session.add(ky)
            created_count += 1

    db.session.commit()

    return jsonify({
        'success': True,
        'message': f'Tạo {created_count} kỳ kế toán cho năm {nam}'
    }), 201
