"""AuditLog API endpoints.

Provides APIs for:
- Audit trail logging with IP, user agent
- Log every VIEW of financial reports
- Log every EXPORT of files
- Immutable audit log (cannot delete/modify)
- Digital signature for integrity
- Document retention per Điều 41 Luật Kế toán 2015

References:
- Điều 41 Luật Kế toán 88/2015/QH13
- Điều 41 Luật Kế toán 2015
"""

import hashlib
import json
from datetime import date, datetime
from functools import wraps

from flask import Blueprint, request, g
from flask_jwt_extended import jwt_required, get_jwt_identity
from sqlalchemy import func, and_, or_

from app.extensions import db
from app.modules.audit_log.models import (
    AuditLog,
    AuditLogSignature,
    CauHinhBaoQuan,
    LichSuBaoQuan
)
from app.utils.response import success_response, error_response, created_response


audit_log_bp = Blueprint('audit_log', __name__)


# =============================================================================
# AUDIT LOGGING MIDDLEWARE
# =============================================================================

def create_audit_log(
    action: str,
    resource_type: str = None,
    resource_id: str = None,
    description: str = None,
    old_value: dict = None,
    new_value: dict = None,
    export_file_name: str = None,
    export_file_type: str = None,
    status_code: int = 200,
    error_message: str = None
):
    """Create audit log entry with digital signature."""
    user_id = g.get('current_user_id')
    username = g.get('current_username', 'anonymous')
    
    # Get IP and User Agent
    ip_address = request.remote_addr or '0.0.0.0'
    user_agent = request.headers.get('User-Agent', '')[:500]
    
    # Get request details
    request_method = request.method
    request_path = request.path
    request_body = None
    if request.is_json and request.method in ['POST', 'PUT', 'PATCH']:
        try:
            request_body = json.dumps(request.get_json(), default=str)[:2000]
        except:
            pass
    
    # Create log entry
    log = AuditLog(
        user_id=user_id,
        username=username,
        ip_address=ip_address,
        user_agent=user_agent,
        request_method=request_method,
        request_path=request_path,
        request_body=request_body,
        action=action,
        resource_type=resource_type,
        resource_id=str(resource_id) if resource_id else None,
        description=description,
        old_value=json.dumps(old_value, default=str) if old_value else None,
        new_value=json.dumps(new_value, default=str) if new_value else None,
        export_file_name=export_file_name,
        export_file_type=export_file_type,
        status_code=status_code,
        error_message=error_message,
        created_at=datetime.now(),
        is_immutable=True
    )
    
    # Calculate checksum
    checksum_data = f"{log.user_id}|{log.ip_address}|{log.action}|{log.resource_type}|{log.resource_id}|{log.created_at}"
    log.checksum = hashlib.sha256(checksum_data.encode()).hexdigest()
    
    # Digital signature (simulated - in production would use HSM)
    log.digital_signature = f"SIMULATED_SIG_{log.checksum[:16]}"
    log.signed_at = datetime.now()
    
    db.session.add(log)
    db.session.commit()
    
    return log


def audit_log_middleware(action: str, resource_type: str = None):
    """Decorator to automatically create audit log."""
    def decorator(f):
        @wraps(f)
        @jwt_required()
        def decorated_function(*args, **kwargs):
            user_id = get_jwt_identity()
            g.current_user_id = user_id
            
            # Get current user
            from app.modules.auth.models import User
            user = db.session.get(User, user_id)
            g.current_username = user.username if user else 'unknown'
            
            # Execute the function
            result = f(*args, **kwargs)
            
            # Log success
            create_audit_log(
                action=action,
                resource_type=resource_type,
                description=f"Action {action} completed successfully"
            )
            
            return result
        return decorated_function
    return decorator


# =============================================================================
# AUDIT LOG QUERY APIs
# =============================================================================

@audit_log_bp.route('/logs', methods=['GET'])
@jwt_required()
def list_audit_logs():
    """Danh sách audit log (chỉ đọc, không thể xóa/sửa)."""
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 50, type=int)
    
    user_id = request.args.get('user_id', type=int)
    action = request.args.get('action')
    tu_ngay = request.args.get('tu_ngay')
    den_ngay = request.args.get('den_ngay')
    ip_address = request.args.get('ip_address')
    resource_type = request.args.get('resource_type')
    
    query = AuditLog.query
    
    if user_id:
        query = query.filter(AuditLog.user_id == user_id)
    if action:
        query = query.filter(AuditLog.action == action)
    if ip_address:
        query = query.filter(AuditLog.ip_address.like(f'%{ip_address}%'))
    if resource_type:
        query = query.filter(AuditLog.resource_type == resource_type)
    if tu_ngay:
        query = query.filter(AuditLog.created_at >= tu_ngay)
    if den_ngay:
        query = query.filter(AuditLog.created_at <= den_ngay)
    
    pagination = query.order_by(AuditLog.created_at.desc()).paginate(
        page=page, per_page=per_page, error_out=False
    )
    
    return success_response(
        data=[{
            'id': log.id,
            'username': log.username,
            'ip_address': log.ip_address,
            'action': log.action,
            'resource_type': log.resource_type,
            'resource_id': log.resource_id,
            'description': log.description,
            'created_at': log.created_at.isoformat(),
            'checksum': log.checksum,
            'is_immutable': log.is_immutable
        } for log in pagination.items],
        pagination={'page': page, 'per_page': per_page, 'total': pagination.total}
    )


@audit_log_bp.route('/logs/<int:id>', methods=['GET'])
@jwt_required()
def get_audit_log_detail(id: int):
    """Chi tiết một audit log entry."""
    log = db.session.get(AuditLog, id)
    if not log:
        return error_response('Audit log not found', 'NOT_FOUND', 404)
    
    return success_response(data={
        'id': log.id,
        'user_id': log.user_id,
        'username': log.username,
        'ip_address': log.ip_address,
        'user_agent': log.user_agent,
        'request_method': log.request_method,
        'request_path': log.request_path,
        'action': log.action,
        'resource_type': log.resource_type,
        'resource_id': log.resource_id,
        'description': log.description,
        'old_value': log.old_value,
        'new_value': log.new_value,
        'export_file_name': log.export_file_name,
        'export_file_type': log.export_file_type,
        'status_code': log.status_code,
        'error_message': log.error_message,
        'checksum': log.checksum,
        'digital_signature': log.digital_signature,
        'signed_at': log.signed_at.isoformat() if log.signed_at else None,
        'created_at': log.created_at.isoformat(),
        'is_immutable': log.is_immutable
    })


@audit_log_bp.route('/logs/export', methods=['GET'])
@jwt_required()
def export_audit_logs():
    """Export audit logs (ghi log ai xuat file gi, luc may gio)."""
    # This would export - log the action
    create_audit_log(
        action='export_file',
        resource_type='audit_log',
        description='Export audit log file',
        export_file_name='audit_logs.xlsx',
        export_file_type='xlsx'
    )
    
    return success_response(
        message='Export audit log'
    )


# =============================================================================
# REPORT VIEWING LOG
# =============================================================================

@audit_log_bp.route('/bao-cao-view', methods=['POST'])
@jwt_required()
def log_bao_cao_view():
    """Ghi log mỗi lần XEM báo cáo tài chính."""
    data = request.get_json()
    
    user_id = get_jwt_identity()
    from app.modules.auth.models import User
    user = db.session.get(User, user_id)
    username = user.username if user else 'unknown'
    
    log = AuditLog(
        user_id=user_id,
        username=username,
        ip_address=request.remote_addr or '0.0.0.0',
        user_agent=request.headers.get('User-Agent', '')[:500],
        request_method='VIEW_REPORT',
        request_path=data.get('report_path', ''),
        action='view_report',
        resource_type='bao_cao',
        resource_id=data.get('report_type'),
        description=f"Xem báo cáo: {data.get('report_name')}",
        status_code=200,
        created_at=datetime.now(),
        is_immutable=True
    )
    
    # Checksum
    checksum_data = f"{log.user_id}|{log.action}|{log.resource_type}|{log.created_at}"
    log.checksum = hashlib.sha256(checksum_data.encode()).hexdigest()
    
    db.session.add(log)
    db.session.commit()
    
    return success_response(
        data={'id': log.id},
        message='Ghi log xem báo cáo thành công'
    )


# =============================================================================
# DIGITAL SIGNATURE MANAGEMENT
# =============================================================================

@audit_log_bp.route('/signature', methods=['GET'])
@jwt_required()
def get_signature_keys():
    """Danh sách khóa ký số."""
    keys = AuditLogSignature.query.filter_by(trang_thai='hoat_dong').all()
    
    return success_response(data=[{
        'id': k.id,
        'ten_khoa': k.ten_khoa,
        'loai_khoa': k.loai_khoa,
        'so_serie': k.so_serie,
        'ngay_bat_dau': k.ngay_bat_dau.isoformat() if k.ngay_bat_dau else None,
        'ngay_ket_thuc': k.ngay_ket_thuc.isoformat() if k.ngay_ket_thuc else None
    } for k in keys])


# =============================================================================
# DOCUMENT RETENTION (BAO QUAN TAI LIEU)
# =============================================================================

@audit_log_bp.route('/bao-quan/cau-hinh', methods=['GET'])
@jwt_required()
def get_cau_hinh_bao_quan():
    """Lấy cấu hình lưu trữ tài liệu."""
    configs = CauHinhBaoQuan.query.all()
    
    return success_response(data=[{
        'id': c.id,
        'loai_tai_lieu': c.loai_tai_lieu,
        'ten_tai_lieu': c.ten_tai_lieu,
        'so_nam_luu_tru': c.so_nam_luu_tru,
        'so_ngay_canh_bao_truoc_het_han': c.so_ngay_canh_bao_truoc_het_han,
        'cho_phep_xoa': c.cho_phep_xoa,
        'ghi_chu': c.ghi_chu
    } for c in configs])


@audit_log_bp.route('/bao-quan/cau-hinh', methods=['POST'])
@jwt_required()
def create_cau_hinh_bao_quan():
    """Tạo cấu hình lưu trữ tài liệu."""
    data = request.get_json()
    
    config = CauHinhBaoQuan(
        loai_tai_lieu=data['loai_tai_lieu'],
        ten_tai_lieu=data['ten_tai_lieu'],
        so_nam_luu_tru=data['so_nam_luu_tru'],
        so_ngay_canh_bao_truoc_het_han=data.get('so_ngay_canh_bao_truoc_het_han', 90),
        cho_phep_xoa=data.get('cho_phep_xoa', False),
        ghi_chu=data.get('ghi_chu')
    )
    
    db.session.add(config)
    db.session.commit()
    
    return created_response(
        data={'id': config.id},
        message='Tạo cấu hình bảo quản thành công'
    )


@audit_log_bp.route('/bao-quan/canh-bao', methods=['GET'])
@jwt_required()
def get_canh_bao_het_han():
    """Lấy danh sách tài liệu sắp hết hạn lưu trữ."""
    ngay_canh_bao = date.today()
    
    lich_su = LichSuBaoQuan.query.filter(
        LichSuBaoQuan.trang_thai == 'luu_tru',
        LichSuBaoQuan.ngay_het_han_du_kien <= ngay_canh_bao
    ).all()
    
    # Also get documents with warning
    sap_het_han = LichSuBaoQuan.query.filter(
        LichSuBaoQuan.trang_thai == 'canh_bao_sap_het_han'
    ).all()
    
    return success_response(data={
        'da_het_han': [{
            'id': ls.id,
            'loai_tai_lieu': ls.loai_tai_lieu,
            'ten_tai_lieu': ls.ten_tai_lieu,
            'ngay_tao': ls.ngay_tao.isoformat(),
            'ngay_het_han': ls.ngay_het_han_du_kien.isoformat() if ls.ngay_het_han_du_kien else None
        } for ls in lich_su],
        'sap_het_han': [{
            'id': ls.id,
            'loai_tai_lieu': ls.loai_tai_lieu,
            'ten_tai_lieu': ls.ten_tai_lieu,
            'ngay_tao': ls.ngay_tao.isoformat(),
            'ngay_het_han': ls.ngay_het_han_du_kien.isoformat() if ls.ngay_het_han_du_kien else None
        } for ls in sap_het_han]
    })


@audit_log_bp.route('/bao-quan/lich-su', methods=['GET'])
@jwt_required()
def get_lich_su_bao_quan():
    """Lấy lịch sử bảo quản tài liệu."""
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    trang_thai = request.args.get('trang_thai')
    
    query = LichSuBaoQuan.query
    
    if trang_thai:
        query = query.filter(LichSuBaoQuan.trang_thai == trang_thai)
    
    pagination = query.order_by(LichSuBaoQuan.created_at.desc()).paginate(
        page=page, per_page=per_page, error_out=False
    )
    
    return success_response(
        data=[{
            'id': ls.id,
            'loai_tai_lieu': ls.loai_tai_lieu,
            'ten_tai_lieu': ls.ten_tai_lieu,
            'ngay_tao': ls.ngay_tao.isoformat(),
            'ngay_het_han_du_kien': ls.ngay_het_han_du_kien.isoformat() if ls.ngay_het_han_du_kien else None,
            'trang_thai': ls.trang_thai,
            'hanh_dong': ls.hanh_dong,
            'ghi_chu': ls.ghi_chu
        } for ls in pagination.items],
        pagination={'page': page, 'per_page': per_page, 'total': pagination.total}
    )


@audit_log_bp.route('/bao-quan/khong-cho-phep-xoa', methods=['POST'])
@jwt_required()
def check_delete_allowed():
    """Kiểm tra có được phép xóa chứng từ không.
    
    KHÔNG CHO PHÉP xóa vật lý bất kỳ chứng từ nào đã duyệt.
    """
    data = request.get_json()
    chung_tu_id = data.get('chung_tu_id')
    
    if not chung_tu_id:
        return error_response('Thiếu ID chứng từ', 'VALIDATION_ERROR', 400)
    
    # Check if document is approved
    from app.modules.nhat_ky.models import ChungTu
    ct = db.session.get(ChungTu, chung_tu_id)
    
    if not ct:
        return error_response('Chứng từ không tồn tại', 'NOT_FOUND', 404)
    
    if ct.trang_thai == 'da_duyet':
        return error_response(
            'KHÔNG CHO PHÉP xóa chứng từ đã duyệt. Chỉ hủy theo quy trình có biên bản.',
            'CANNOT_DELETE_APPROVED',
            403
        )
    
    return success_response(
        data={
            'cho_phep_xoa': True,
            'trang_thai': ct.trang_thai
        },
        message='Chứng từ chưa duyệt, có thể xóa'
    )
