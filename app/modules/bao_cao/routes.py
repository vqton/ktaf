"""BaoCao (Financial Reports) API endpoints.

Provides APIs for:
- B04-DN: Bản thuyết minh BCTC (TT99/2025)
- BCTC digital signatures
- Electronic BCTC submission (XML, eTax)
- Deadline reminders

References:
- TT99/2025/TT-BTC
- Điều 41 Luật Kế toán 2015
"""

import hashlib
import json
from datetime import date, datetime, timedelta
from decimal import Decimal
from typing import Optional

from flask import Blueprint, request, jsonify, send_file
from flask_jwt_extended import jwt_required, get_jwt_identity
from sqlalchemy import func, and_, or_

from app.extensions import db
from app.modules.bao_cao.models import (
    MauBaoCao,
    ChiTietMauBC,
    B04DNThuyetMinh,
    BCTCSignature,
    BCTCSubmission,
    DeadlineReminder
)
from app.modules.ky_ke_toan.models import KyKeToan
from app.modules.nhat_ky.models import ChungTu, DinhKhoan
from app.modules.he_thong_tk.models import HeThongTaiKhoan
from app.modules.danh_muc.doi_tuong.models import DoiTuong
from app.utils.response import success_response, error_response, created_response


bao_cao_bp = Blueprint('bao_cao', __name__)


# =============================================================================
# B04-DN: BẢN THUYẾT MINH BCTC
# =============================================================================

@bao_cao_bp.route('/b04dn/thuyet-minh', methods=['GET'])
@jwt_required()
def get_b04dn_list():
    """Danh sách B04-DN đã lập."""
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    trang_thai = request.args.get('trang_thai')
    
    query = B04DNThuyetMinh.query
    
    if trang_thai:
        query = query.filter(B04DNThuyetMinh.trang_thai == trang_thai)
    
    pagination = query.order_by(B04DNThuyetMinh.created_at.desc()).paginate(
        page=page, per_page=per_page, error_out=False
    )
    
    return success_response(
        data=[{
            'id': b.id,
            'ky_ke_toan': f"{b.ky_ke_toan.thang}/{b.ky_ke_toan.nam}" if b.ky_ke_toan else None,
            'trang_thai': b.trang_thai,
            'chinh_sach_ke_toan': b.chinh_sach_ke_toan[:50] if b.chinh_sach_ke_toan else None,
            'created_at': b.created_at.isoformat() if b.created_at else None
        } for b in pagination.items],
        pagination={'page': page, 'per_page': per_page, 'total': pagination.total}
    )


@bao_cao_bp.route('/b04dn/thuyet-minh/tao', methods=['POST'])
@jwt_required()
def create_b04dn():
    """Tạo mới B04-DN với dữ liệu tự động từ số liệu hệ thống."""
    data = request.get_json()
    ky_ke_toan_id = data.get('ky_ke_toan_id')
    
    if not ky_ke_toan_id:
        return error_response('Thiếu kỳ kế toán', 'VALIDATION_ERROR', 400)
    
    ky = KyKeToan.query.get(ky_ke_toan_id)
    if not ky:
        return error_response('Kỳ kế toán không tồn tại', 'NOT_FOUND', 404)
    
    # Auto-fill data from system
    
    # I. Chính sách kế toán
    chinhsach = {
        'hvat_chuan': 'Chế độ kế toán Việt Nam ban hành theo Thông tư 99/2025/TT-BTC',
        'dvi_tien_te': 'Đồng Việt Nam (VND)',
        'nguyen_tac_ghi_so': 'Ghi sổ theo nguyên tắc giá gốc',
    }
    
    # II. Chi tiết TSCĐ
    tscd_details = {}
    for ma_tk in ['211', '212', '213']:
        tk = HeThongTaiKhoan.query.filter_by(ma_tk=ma_tk).first()
        if tk:
            tong_tien = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
                ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
            ).filter(
                ChungTu.trang_thai == 'da_duyet',
                or_(DinhKhoan.tk_no == ma_tk, DinhKhoan.tk_co == ma_tk),
                ChungTu.ky_ke_toan_id == ky_ke_toan_id
            ).scalar() or 0
            tscd_details[ma_tk] = {
                'ten_tk': tk.ten_tk,
                'so_du': float(tong_tien)
            }
    
    # III. Công nợ > 1 năm
    han_1_nam = ky.den_ngay - timedelta(days=365)
    cong_no_1_nam = []
    
    for ma_tk, ten_tk in [('131', 'Phải thu KH'), ('331', 'Phải trả NB')]:
        query = db.session.query(
            DoiTuong,
            func.sum(DinhKhoan.so_tien).label('cong_no')
        ).join(
            DinhKhoan, DoiTuong.id == DinhKhoan.doi_tuong_id
        ).join(
            ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
        ).filter(
            ChungTu.trang_thai == 'da_duyet',
            or_(
                and_(DinhKhoan.tk_no == ma_tk, DinhKhoan.tk_co.in_(['111', '112'])),
                and_(DinhKhoan.tk_co == ma_tk, DinhKhoan.tk_no.in_(['111', '112']))
            ),
            ChungTu.ky_ke_toan_id == ky_ke_toan_id,
            ChungTu.ngay_ct < han_1_nam
        ).group_by(DoiTuong.id).having(func.sum(DinhKhoan.so_tien) > 0)
        
        for dt, cong_no in query.all():
            cong_no_1_nam.append({
                'doi_tuong': dt.ten,
                'ma_tk': ma_tk,
                'so_tien': float(cong_no)
            })
    
    # IV. Chi tiết vốn chủ sở hữu
    von_csh = {}
    for ma_tk in ['4111', '412', '419']:
        tk = HeThongTaiKhoan.query.filter_by(ma_tk=ma_tk).first()
        if tk:
            tong_tien = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
                ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
            ).filter(
                ChungTu.trang_thai == 'da_duyet',
                DinhKhoan.tk_co == ma_tk,
                ChungTu.ky_ke_toan_id == ky_ke_toan_id
            ).scalar() or 0
            von_csh[ma_tk] = {
                'ten_tk': tk.ten_tk,
                'so_du': float(tong_tien)
            }
    
    # Create B04-DN
    b04dn = B04DNThuyetMinh(
        ky_ke_toan_id=ky_ke_toan_id,
        chinh_sach_ke_toan=json.dumps(chinhsach, ensure_ascii=False),
        phuong_phap_khau_hao='Khấu hao theo đường thẳng',
        phuong_phap_tinh_gia_tri_hang_ton='Nhập kho trước - Xuất kho trước (FIFO)',
        ty_gia_hach_toan='Tỷ giá bình quân gia quyền cuối kỳ',
        chi_tiet_tscd=tscd_details,
        chi_tiet_cong_no_1_nam=cong_no_1_nam,
        chi_tiet_von_csh=von_csh,
        su_kien_sau_ky='Không có sự kiện bất thường nào xảy ra sau ngày kết thúc kỳ kế toán',
        trang_thai='nhap'
    )
    
    db.session.add(b04dn)
    db.session.commit()
    
    return created_response(
        data={
            'id': b04dn.id,
            'ky_ke_toan': f"{ky.thang}/{ky.nam}"
        },
        message='Tạo B04-DN thành công'
    )


@bao_cao_bp.route('/b04dn/thuyet-minh/<int:id>', methods=['GET'])
@jwt_required()
def get_b04dn_detail(id: int):
    """Chi tiết B04-DN."""
    b04dn = db.session.get(B04DNThuyetMinh, id)
    if not b04dn:
        return error_response('B04-DN không tồn tại', 'NOT_FOUND', 404)
    
    return success_response(data={
        'id': b04dn.id,
        'ky_ke_toan': f"{b04dn.ky_ke_toan.thang}/{b04dn.ky_ke_toan.nam}" if b04dn.ky_ke_toan else None,
        'chinh_sach_ke_toan': b04dn.chinh_sach_ke_toan,
        'phuong_phap_khau_hao': b04dn.phuong_phap_khau_hao,
        'phuong_phap_tinh_gia_tri_hang_ton': b04dn.phuong_phap_tinh_gia_tri_hang_ton,
        'ty_gia_hach_toan': b04dn.ty_gia_hach_toan,
        'chi_tiet_tscd': b04dn.chi_tiet_tscd,
        'chi_tiet_cong_no_1_nam': b04dn.chi_tiet_cong_no_1_nam,
        'chi_tiet_von_csh': b04dn.chi_tiet_von_csh,
        'su_kien_sau_ky': b04dn.su_kien_sau_ky,
        'trang_thai': b04dn.trang_thai
    })


# =============================================================================
# BCTC DIGITAL SIGNATURES
# =============================================================================

@bao_cao_bp.route('/b04dn/<int:id>/ky', methods=['POST'])
@jwt_required()
def sign_b04dn(id: int):
    """Ký BCTC (người lập, KT trưởng, Giám đốc)."""
    data = request.get_json()
    user_id = get_jwt_identity()
    
    b04dn = db.session.get(B04DNThuyetMinh, id)
    if not b04dn:
        return error_response('B04-DN không tồn tại', 'NOT_FOUND', 404)
    
    # Get current user info
    from app.modules.auth.models import User
    user = db.session.get(User, user_id)
    
    # Check existing signature
    existing_sig = BCTCSignature.query.filter_by(b04dn_id=id).first()
    
    if not existing_sig:
        # Create new signature record
        sig = BCTCSignature(
            b04dn_id=id,
            nguoi_lap_id=user_id,
            nguoi_lap_ten=user.username if user else '',
            nguoi_lap_chuc_vu='Người lập',
            nguoi_lap_ky='SIMULATED_SIGNATURE',
            nguoi_lap_ky_ma=hashlib.sha256(f"{user_id}_{datetime.now()}".encode()).hexdigest(),
            nguoi_lap_ky_luc=datetime.now(),
            created_at=datetime.now()
        )
        db.session.add(sig)
    else:
        # Update signature based on role
        vai_tro = data.get('vai_tro')  # 'ke_toan_truong' or 'giam_doc'
        
        if vai_tro == 'ke_toan_truong':
            existing_sig.ke_toan_truong_id = user_id
            existing_sig.ke_toan_truong_ten = user.username if user else ''
            existing_sig.ke_toan_truong_chuc_vu = 'Kế toán trưởng'
            existing_sig.ke_toan_truong_ky = 'SIMULATED_SIG_KT'
            existing_sig.ke_toan_truong_ky_ma = hashlib.sha256(f"KT_{user_id}_{datetime.now()}".encode()).hexdigest()
            existing_sig.ke_toan_truong_ky_luc = datetime.now()
        elif vai_tro == 'giam_doc':
            existing_sig.giam_doc_id = user_id
            existing_sig.giam_doc_ten = user.username if user else ''
            existing_sig.giam_doc_chuc_vu = 'Giám đốc'
            existing_sig.giam_doc_ky = 'SIMULATED_SIG_GD'
            existing_sig.giam_doc_ky_ma = hashlib.sha256(f"GD_{user_id}_{datetime.now()}".encode()).hexdigest()
            existing_sig.giam_doc_ky_luc = datetime.now()
    
    # Calculate file hash
    b04dn.trang_thai = 'da_ky'
    
    # Generate file hash
    content_hash = hashlib.sha256(json.dumps({
        'b04dn_id': id,
        'chi_tiet_tscd': b04dn.chi_tiet_tscd,
        'chi_tiet_von_csh': b04dn.chi_tiet_von_csh
    }, default=str).encode()).hexdigest()
    
    if existing_sig:
        existing_sig.file_bc_tc_hash = content_hash
        existing_sig.file_signed_at = datetime.now()
    
    db.session.commit()
    
    return success_response(
        data={
            'id': b04dn.id,
            'trang_thai': b04dn.trang_thai,
            'file_hash': content_hash
        },
        message='Ký BCTC thành công'
    )


@bao_cao_bp.route('/b04dn/<int:id>/signature', methods=['GET'])
@jwt_required()
def get_b04dn_signatures(id: int):
    """Lấy thông tin chữ ký BCTC."""
    b04dn = db.session.get(B04DNThuyetMinh, id)
    if not b04dn:
        return error_response('B04-DN không tồn tại', 'NOT_FOUND', 404)
    
    sig = BCTCSignature.query.filter_by(b04dn_id=id).first()
    
    if not sig:
        return success_response(data=None, message='Chưa có chữ ký')
    
    return success_response(data={
        'nguoi_lap': {
            'ten': sig.nguoi_lap_ten,
            'chuc_vu': sig.nguoi_lap_chuc_vu,
            'ky_luc': sig.nguoi_lap_ky_luc.isoformat() if sig.nguoi_lap_ky_luc else None,
            'hash': sig.nguoi_lap_ky_ma
        },
        'ke_toan_truong': {
            'ten': sig.ke_toan_truong_ten,
            'chuc_vu': sig.ke_toan_truong_chuc_vu,
            'ky_luc': sig.ke_toan_truong_ky_luc.isoformat() if sig.ke_toan_truong_ky_luc else None,
            'hash': sig.ke_toan_truong_ky_ma
        } if sig.ke_toan_truong_ten else None,
        'giam_doc': {
            'ten': sig.giam_doc_ten,
            'chuc_vu': sig.giam_doc_chuc_vu,
            'ky_luc': sig.giam_doc_ky_luc.isoformat() if sig.giam_doc_ky_luc else None,
            'hash': sig.giam_doc_ky_ma
        } if sig.giam_doc_ten else None,
        'file_hash': sig.file_bc_tc_hash
    })


# =============================================================================
# ELECTRONIC BCTC SUBMISSION
# =============================================================================

@bao_cao_bp.route('/b04dn/<int:id>/nop', methods=['POST'])
@jwt_required()
def submit_b04dn(id: int):
    """Nộp BCTC điện tử (XML lên eTax/HTKK)."""
    data = request.get_json()
    
    b04dn = db.session.get(B04DNThuyetMinh, id)
    if not b04dn:
        return error_response('B04-DN không tồn tại', 'NOT_FOUND', 404)
    
    if b04dn.trang_thai != 'da_ky':
        return error_response('B04-DN chưa được ký, không thể nộp', 'INVALID_STATE', 400)
    
    # Simulate XML generation
    xml_content = f"""<?xml version="1.0" encoding="UTF-8"?>
<BCTC xmlns="http://tax.gdt.gov.vn/2023">
    <B04DN>
        <KyKeToan>{b04dn.ky_ke_toan.nam}</KyKeToan>
        <MaSoThue>0312345678</MaSoThue>
        <ChinhSachKeToan>{b04dn.chinh_sach_ke_toan[:100] if b04dn.chinh_sach_ke_toan else ''}</ChinhSachKeToan>
    </B04DN>
</BCTC>"""
    
    # Calculate hash
    xml_hash = hashlib.sha256(xml_content.encode()).hexdigest()
    
    # Create submission record
    submission = BCTCSubmission(
        b04dn_id=id,
        loai_bao_cao='BCTC',
        ky_lap=b04dn.ky_ke_toan.nam if b04dn.ky_ke_toan else None,
        thang_lap=b04dn.ky_ke_toan.thang if b04dn.ky_ke_toan else None,
        ma_tiep_nhan=f"MT_{datetime.now().strftime('%Y%m%d%H%M%S')}",
        ngay_nop=datetime.now(),
        file_xml='BCTC_' + str(id) + '.xml',
        file_xml_hash=xml_hash,
        trang_thai='da_nop'
    )
    
    b04dn.trang_thai = 'da_nop'
    
    db.session.add(submission)
    db.session.commit()
    
    return success_response(
        data={
            'id': submission.id,
            'ma_tiep_nhan': submission.ma_tiep_nhan,
            'ngay_nop': submission.ngay_nop.isoformat(),
            'file_hash': xml_hash
        },
        message='Nộp BCTC thành công'
    )


# =============================================================================
# DEADLINE REMINDERS
# =============================================================================

@bao_cao_bp.route('/deadline/reminders', methods=['GET'])
@jwt_required()
def get_deadline_reminders():
    """Lấy danh sách deadline nhắc nhở."""
    today = date.today()
    
    # Get all active deadlines
    deadlines = DeadlineReminder.query.filter(
        DeadlineReminder.ngay_deadline >= today
    ).order_by(DeadlineReminder.ngay_deadline).all()
    
    result = []
    for d in deadlines:
        days_remaining = (d.ngay_deadline - today).days
        
        result.append({
            'id': d.id,
            'ten_deadline': d.ten_deadline,
            'loai_deadline': d.loai_deadline,
            'ngay_deadline': d.ngay_deadline.isoformat(),
            'so_ngay_con_lai': days_remaining,
            'canh_bao': days_remaining <= d.so_ngay_nhac_truoc,
            'da_nhac': d.da_nhac
        })
    
    return success_response(data=result)


@bao_cao_bp.route('/deadline/reminders', methods=['POST'])
@jwt_required()
def create_deadline_reminder():
    """Tạo deadline nhắc nhở mới."""
    data = request.get_json()
    
    # Auto-create standard deadlines if not exists
    current_year = date.today().year
    
    default_deadlines = [
        {
            'loai_deadline': 'QT_TNDN',
            'ten_deadline': 'Quyết toán TNDN năm ' + str(current_year - 1),
            'ngay_deadline': date(current_year, 3, 31),
            'so_ngay_nhac_truoc': 7
        },
        {
            'loai_deadline': 'QT_TNCN',
            'ten_deadline': 'Quyết toán TNCN năm ' + str(current_year - 1),
            'ngay_deadline': date(current_year, 3, 31),
            'so_ngay_nhac_truoc': 7
        },
        {
            'loai_deadline': 'BCTC',
            'ten_deadline': 'Báo cáo tài chính năm ' + str(current_year - 1),
            'ngay_deadline': date(current_year, 3, 31),
            'so_ngay_nhac_truoc': 14
        }
    ]
    
    created = []
    for d in default_deadlines:
        existing = DeadlineReminder.query.filter_by(
            loai_deadline=d['loai_deadline'],
            ky_lap=current_year - 1
        ).first()
        
        if not existing:
            deadline = DeadlineReminder(
                loai_deadline=d['loai_deadline'],
                ten_deadline=d['ten_deadline'],
                ngay_deadline=d['ngay_deadline'],
                so_ngay_nhac_truoc=d['so_ngay_nhac_truoc'],
                ky_lap=current_year - 1,
                created_at=datetime.now()
            )
            db.session.add(deadline)
            created.append(d['ten_deadline'])
    
    db.session.commit()
    
    return success_response(
        data={'created': created},
        message=f'Tạo {len(created)} deadline mới'
    )


@bao_cao_bp.route('/deadline/check', methods=['GET'])
@jwt_required()
def check_upcoming_deadlines():
    """Kiểm tra deadline sắp đến (trong vòng 30 ngày)."""
    today = date.today()
    check_date = today + timedelta(days=30)
    
    upcoming = DeadlineReminder.query.filter(
        DeadlineReminder.ngay_deadline <= check_date,
        DeadlineReminder.ngay_deadline >= today,
        DeadlineReminder.da_nhac == False
    ).order_by(DeadlineReminder.ngay_deadline).all()
    
    result = []
    for d in upcoming:
        days_left = (d.ngay_deadline - today).days
        
        result.append({
            'id': d.id,
            'ten_deadline': d.ten_deadline,
            'loai_deadline': d.loai_deadline,
            'ngay_deadline': d.ngay_deadline.isoformat(),
            'so_ngay_con_lai': days_left,
            'muc_do_uu_tien': 'cao' if days_left <= 7 else ('trung_binh' if days_left <= 14 else 'thap')
        })
    
    return success_response(data={
        'hom_nay': today.isoformat(),
        'deadlines': result,
        'tong_so': len(result)
    })
