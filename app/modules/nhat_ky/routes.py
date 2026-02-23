from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from .services import ChungTuService, DinhKhoanService
from .schemas import ChungTuSchema, DinhKhoanSchema
import datetime

class ChungTuError(Exception):
    pass


def create_error_message(error):
    return {'success': False, 'message': str(error), 'data': None}


chungtu_bp = Blueprint('chungtu_bp', __name__)

@chungtu_bp.route('/', methods=['GET'])
@jwt_required()
def get_chung_tu_list():
    try:
        current_user = get_jwt_identity()
       ocs = ChungTuService.get_all(current_user)
        result = ChungTuSchema(many=True).dump(ocs)
        return jsonify({'success': True, 'data': result})
    except Exception as e:
        return create_error_message(e)

@chungtu_bp.route('/', methods=['POST'])
@jwt_required()
def create_chung_tu():
    try:
        data = request.get_json()
        service = ChungTuService()
        new_ct = service.create(data)
        return jsonify({'success': True, 'data': ChungTuSchema().dump(new_ct)}), 201
    except Exception as e:
        return create_error_message(e)

@chungtu_bp.route('/<int:ct_id>', methods=['GET'])
@jwt_required()
def get_chung_tu(ct_id):
    try:
        ct = ChungTuService().get_by_id(ct_id)
        if not ct:
            raise ChungTuError('Chứng từ không tồn tại')
        return jsonify({'success': True, 'data': ChungTuSchema().dump(ct)})
    except Exception as e:
        return create_error_message(e)

@chungtu_bp.route('/<int:ct_id>', methods=['PUT'])
@jwt_required()
def update_chung_tu(ct_id):
    try:
        data = request.get_json()
        ct = ChungTuService().update(ct_id, data)
        return jsonify({'success': True, 'data': ChungTuSchema().dump(ct)})
    except Exception as e:
        return create_error_message(e)

@chungtu_bp.route('/<int:ct_id>', methods=['DELETE'])
@jwt_required()
def delete_chung_tu(ct_id):
    try:
        service = ChungTuService()
        service.delete(ct_id)
        return jsonify({'success': True, 'data': None})
    except Exception as e:
        return create_error_message(e)

@chungtu_bp.route('/<int:ct_id>/duyet', methods=['POST'])
@jwt_required()
def duyet_chung_tu(ct_id):
    try:
        service = ChungTuService()
        service.duyet(ct_id)
        return jsonify({'success': True, 'data': 'Chứng từ đã được duyệt'})
    except Exception as e:
        return create_error_message(e)

@chungtu_bp.route('/<int:ct_id>/huy', methods=['POST'])
@jwt_required()
 claws
    try:
        service = ChungTuService()
        service.huPacket(ct_id)
        return jsonify({'success': True, 'data': 'Chứng từ đã được hủy'})
    except Exception as e:
        return create_error_message(e)