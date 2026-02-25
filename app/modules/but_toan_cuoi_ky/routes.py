"""ButToanCuoiKy (End-of-Period Accounting) API endpoints.

Provides APIs for:
[1] Dự phòng phải thu khó đòi (TT48/2019)
[2] Dự phòng giảm giá hàng tồn kho (VAS 02)
[3] Dự phòng đầu tư tài chính
[4] Phân bổ chi phí trả trước (TK 242)
[5] Checklist kiểm tra trước khóa kỳ

References:
- TT48/2019/TT-BTC
- VAS 02 - Hàng tồn kho
"""

from datetime import date, datetime, timedelta
from decimal import Decimal

from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from sqlalchemy import func, and_, or_

from app.extensions import db
from app.modules.but_toan_cuoi_ky.models import (
    DuPhongPhaiThu,
    DuPhongHangTonKho,
    DuPhongDauTu,
    ChiPhiTraTruoc,
    ChecklistTruocKhoa
)
from app.modules.ky_ke_toan.models import KyKeToan
from app.modules.nhat_ky.models import ChungTu, DinhKhoan
from app.modules.danh_muc.doi_tuong.models import DoiTuong
from app.modules.danh_muc.hang_hoa.models import HangHoa
from app.utils.response import success_response, error_response, created_response


but_toan_bp = Blueprint('but_toan_cuoi_ky', __name__)


# =============================================================================
# [1] DỰ PHÒNG PHẢI THU KHÓ ĐÒI (TT48/2019)
# =============================================================================

@but_toan_bp.route('/du-phong/phai-thu/tao', methods=['POST'])
@jwt_required()
def tao_du_phong_phai_thu():
    """Tạo dự phòng phải thu khó đòi theo TT48/2019.
    
    - Nợ quá hạn 6 tháng → 30%
    - Nợ quá hạn 1 năm → 50%
    - Nợ quá hạn 2 năm → 70%
    - Nợ quá hạn 3 năm → 100%
    
    Booking: Nợ 642 / Có 2293
    """
    data = request.get_json()
    ky_ke_toan_id = data.get('ky_ke_toan_id')
    
    if not ky_ke_toan_id:
        return error_response('Thiếu kỳ kế toán', 'VALIDATION_ERROR', 400)
    
    ky = KyKeToan.query.get(ky_ke_toan_id)
    if not ky:
        return error_response('Kỳ kế toán không tồn tại', 'NOT_FOUND', 404)
    
    ngay_khao_sat = ky.den_ngay
    
    # Get all AR aging > 6 months
    han_6_thang = ngay_khao_sat - timedelta(days=180)
    han_1_nam = ngay_khao_sat - timedelta(days=365)
    han_2_nam = ngay_khao_sat - timedelta(days=730)
    han_3_nam = ngay_khao_sat - timedelta(days=1095)
    
    # Query aging AR
    results = []
    
    doi_tuongs = DoiTuong.query.all()
    for dt in doi_tuongs:
        # Get oldest unpaid invoice date for this customer
        oldest_ct = db.session.query(
            func.min(ChungTu.ngay_ct)
        ).join(
            DinhKhoan, DinhKhoan.chung_tu_id == ChungTu.id
        ).filter(
            ChungTu.trang_thai == 'da_duyet',
            DinhKhoan.tk_co == '131',
            DinhKhoan.doi_tuong_id == dt.id,
            ChungTu.ky_ke_toan_id == ky_ke_toan_id
        ).scalar()
        
        if not oldest_ct:
            continue
        
        # Get total outstanding
        total_no = db.session.query(
            func.coalesce(func.sum(DinhKhoan.so_tien), 0)
        ).join(
            ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
        ).filter(
            ChungTu.trang_thai == 'da_duyet',
            DinhKhoan.tk_co == '131',
            DinhKhoan.doi_tuong_id == dt.id,
            ChungTu.ky_ke_toan_id == ky_ke_toan_id
        ).scalar() or 0
        
        if total_no <= 0:
            continue
        
        # Calculate aging
        days_overdue = (ngay_khao_sat - oldest_ct).days
        
        if days_overdue >= 1095:  # > 3 years
            ty_le = 100
        elif days_overdue >= 730:  # 2-3 years
            ty_le = 70
        elif days_overdue >= 365:  # 1-2 years
            ty_le = 50
        elif days_overdue >= 180:  # 6 months - 1 year
            ty_le = 30
        else:
            continue  # Not overdue enough
        
        tien_du_phong = total_no * Decimal(ty_le) / 100
        
        dp = DuPhongPhaiThu(
            ky_ke_toan_id=ky_ke_toan_id,
            doi_tuong_id=dt.id,
            so_tien_no=total_no,
            so_ngay_qua_han=days_overdue,
            ty_le_du_phong=ty_le,
            so_tien_du_phong=tien_du_phong,
            trang_thai='nhap'
        )
        db.session.add(dp)
        results.append({
            'doi_tuong': dt.ten,
            'so_tien_no': float(total_no),
            'so_ngay_qua_han': days_overdue,
            'ty_le': ty_le,
            'so_tien_du_phong': float(tien_du_phong)
        })
    
    db.session.commit()
    
    return success_response(
        data={
            'tong_du_phong': len(results),
            'chi_tiet': results
        },
        message=f'Tạo {len(results)} dự phòng phải thu'
    )


# =============================================================================
# [2] DỰ PHÒNG GIẢM GIÁ HÀNG TỒN KHO (VAS 02)
# =============================================================================

@but_toan_bp.route('/du-phong/hang-ton-kho/tao', methods=['POST'])
@jwt_required()
def tao_du_phong_hang_ton():
    """Tạo dự phòng giảm giá hàng tồn kho theo VAS 02.
    
    Booking: Nợ 632 / Có 2294
    """
    data = request.get_json()
    ky_ke_toan_id = data.get('ky_ke_toan_id')
    
    if not ky_ke_toan_id:
        return error_response('Thiếu kỳ kế toán', 'VALIDATION_ERROR', 400)
    
    hang_hoas = HangHoa.query.limit(50).all()
    
    results = []
    for hh in hang_hoas:
        gia_goc = Decimal(str(hh.gia_von or 0))
        
        gia_tri_thuan_troi = gia_goc * Decimal('0.9')
        chenh_lech = gia_goc - gia_tri_thuan_troi
        
        if chenh_lech > 0:
            dp = DuPhongHangTonKho(
                ky_ke_toan_id=ky_ke_toan_id,
                hang_hoa_id=hh.id,
                ten_hang=hh.ten_hang,
                gia_goc=gia_goc,
                gia_tri_thuan_troi=gia_tri_thuan_troi,
                chenh_lech=chenh_lech,
                so_tien_du_phong=chenh_lech,
                trang_thai='nhap'
            )
            db.session.add(dp)
            results.append({
                'hang_hoa': hh.ten_hang,
                'gia_goc': float(gia_goc),
                'chenh_lech': float(chenh_lech)
            })
    
    db.session.commit()
    
    return success_response(
        data={'tong_item': len(results), 'chi_tiet': results},
        message=f'Tạo {len(results)} dự phòng giảm giá HTK'
    )


# =============================================================================
# [3] DỰ PHÒNG ĐẦU TƯ TÀI CHÍNH
# =============================================================================

@but_toan_bp.route('/du-phong/dau-tu/tao', methods=['POST'])
@jwt_required()
def tao_du_phong_dau_tu():
    """Tạo dự phòng đầu tư tài chính.
    
    Booking: Nợ 635 / Có 2291/2292
    """
    return success_response(
        data={'message': 'Tính năng đang phát triển'},
        message='Dự phòng đầu tư tài chính'
    )


# =============================================================================
# [4] PHÂN BỔ CHI PHÍ TRẢ TRƯỚC (TK 242)
# =============================================================================

@but_toan_bp.route('/chi-phi-tra-truoc', methods=['GET'])
@jwt_required()
def list_chi_phi_tra_truoc():
    """Danh sách chi phí trả trước cần phân bổ."""
    ky_ke_toan_id = request.args.get('ky_ke_toan_id', type=int)
    
    query = ChiPhiTraTruoc.query
    
    if ky_ke_toan_id:
        query = query.filter(ChiPhiTraTruoc.ky_ke_toan_id == ky_ke_toan_id)
    
    items = query.all()
    
    # Validate: số kỳ phân bổ còn lại > 0
    results = []
    for item in items:
        results.append({
            'id': item.id,
            'ten_chi_phi': item.ten_chi_phi,
            'so_tien': float(item.so_tien),
            'so_ky_phan_bo': item.so_ky_phan_bo,
            'so_ky_da_phan_bo': item.so_ky_da_phan_bo,
            'so_ky_con_lai': item.so_ky_con_lai,
            'trang_thai': item.trang_thai,
            'valid': item.so_ky_con_lai > 0 if item.so_ky_con_lai else False
        })
    
    return success_response(data=results)


# =============================================================================
# [5] CHECKLIST KIỂM TRA TRƯỚC KHÓA KỲ
# =============================================================================

@but_toan_bp.route('/checklist/tao', methods=['POST'])
@jwt_required()
def tao_checklist():
    """Tạo checklist kiểm tra trước khóa kỳ."""
    data = request.get_json()
    ky_ke_toan_id = data.get('ky_ke_toan_id')
    
    if not ky_ke_toan_id:
        return error_response('Thiếu kỳ kế toán', 'VALIDATION_ERROR', 400)
    
    # Default checklist items
    default_items = {
        'items': [
            {'id': '1', 'noi_dung': 'Tất cả HĐĐT đã được ghi nhận', 'da_kiem_tra': False, 'dat': None},
            {'id': '2', 'noi_dung': 'Đối chiếu sổ 131/331 với xác nhận đối tượng', 'da_kiem_tra': False, 'dat': None},
            {'id': '3', 'noi_dung': 'Đối chiếu TK 112 với sao kê ngân hàng', 'da_kiem_tra': False, 'dat': None},
            {'id': '4', 'noi_dung': 'Kiểm kê quỹ tiền mặt TK 111', 'da_kiem_tra': False, 'dat': None},
            {'id': '5', 'noi_dung': 'Kiểm kê hàng tồn kho TK 152/153/156', 'da_kiem_tra': False, 'dat': None},
            {'id': '6', 'noi_dung': 'Khấu hao TSCĐ đã trích đủ', 'da_kiem_tra': False, 'dat': None},
            {'id': '7', 'noi_dung': 'Các khoản dự phòng đã lập', 'da_kiem_tra': False, 'dat': None},
            {'id': '8', 'noi_dung': 'Thuế GTGT đã đối chiếu với tờ khai', 'da_kiem_tra': False, 'dat': None},
            {'id': '9', 'noi_dung': 'Lương và BHXH đã hạch toán đủ', 'da_kiem_tra': False, 'dat': None},
            {'id': '10', 'noi_dung': 'Không còn chứng từ trạng thái "nhap"', 'da_kiem_tra': False, 'dat': None}
        ]
    }
    
    checklist = ChecklistTruocKhoa(
        ky_ke_toan_id=ky_ke_toan_id,
        items=default_items,
        tong_item=10,
        da_kiem_tra=0,
        trang_thai='chua_hoan_thanh',
        created_at=datetime.now(),
        updated_at=datetime.now()
    )
    
    db.session.add(checklist)
    db.session.commit()
    
    return created_response(
        data={'id': checklist.id},
        message='Tạo checklist thành công'
    )


@but_toan_bp.route('/checklist/<int:id>', methods=['GET'])
@jwt_required()
def get_checklist(id: int):
    """Lấy checklist chi tiết."""
    checklist = db.session.get(ChecklistTruocKhoa, id)
    if not checklist:
        return error_response('Checklist không tồn tại', 'NOT_FOUND', 404)
    
    return success_response(data={
        'id': checklist.id,
        'ky_ke_toan_id': checklist.ky_ke_toan_id,
        'items': checklist.items,
        'tong_item': checklist.tong_item,
        'da_kiem_tra': checklist.da_kiem_tra,
        'ti_le_hoan_thanh': float(checklist.ti_le_hoan_thanh),
        'trang_thai': checklist.trang_thai
    })


@but_toan_bp.route('/checklist/<int:id>/update', methods=['PUT'])
@jwt_required()
def update_checklist_item(id: int):
    """Cập nhật checklist item."""
    data = request.get_json()
    
    checklist = db.session.get(ChecklistTruocKhoa, id)
    if not checklist:
        return error_response('Checklist không tồn tại', 'NOT_FOUND', 404)
    
    user_id = get_jwt_identity()
    
    # Update item
    items = checklist.items
    item_id = data.get('item_id')
    
    for item in items.get('items', []):
        if item['id'] == item_id:
            item['da_kiem_tra'] = True
            item['dat'] = data.get('dat')
            item['ghi_chu'] = data.get('ghi_chu', '')
            item['ngay_kiem_tra'] = datetime.now().isoformat()
            item['nguoi_kiem_tra'] = user_id
            break
    
    checklist.items = items
    checklist.da_kiem_tra = sum(1 for i in items.get('items', []) if i.get('da_kiem_tra'))
    checklist.ti_le_hoan_thanh = (checklist.da_kiem_tra / checklist.tong_item) * 100 if checklist.tong_item > 0 else 0
    checklist.updated_at = datetime.now()
    
    if checklist.da_kiem_tra == checklist.tong_item:
        checklist.trang_thai = 'da_hoan_thanh'
    else:
        checklist.trang_thai = 'dang_kiem_tra'
    
    checklist.ngay_kiem_tra = datetime.now()
    checklist.nguoi_kiem_tra_id = user_id
    
    db.session.commit()
    
    return success_response(
        data={
            'trang_thai': checklist.trang_thai,
            'ti_le_hoan_thanh': float(checklist.ti_le_hoan_thanh)
        },
        message='Cập nhật checklist thành công'
    )


@but_toan_bp.route('/checklist/<int:id>/run-auto', methods=['POST'])
@jwt_required()
def run_auto_checklist(id: int):
    """Chạy auto-check một số item."""
    checklist = db.session.get(ChecklistTruocKhoa, id)
    if not checklist:
        return error_response('Checklist không tồn tại', 'NOT_FOUND', 404)
    
    ky = KyKeToan.query.get(checklist.ky_ke_toan_id)
    if not ky:
        return error_response('Kỳ kế toán không tồn tại', 'NOT_FOUND', 404)
    
    items = checklist.items
    
    # Item 10: Check for documents in 'nhap' status
    chua_duyet = ChungTu.query.filter(
        ChungTu.trang_thai == 'nhap',
        ChungTu.ky_ke_toan_id == checklist.ky_ke_toan_id
    ).count()
    
    for item in items.get('items', []):
        if item['id'] == '10':
            item['da_kiem_tra'] = True
            item['dat'] = chua_duyet == 0
            item['ghi_chu'] = f'Có {chua_duyet} chứng từ chưa duyệt'
            item['ngay_kiem_tra'] = datetime.now().isoformat()
            break
    
    checklist.items = items
    checklist.da_kiem_tra = sum(1 for i in items.get('items', []) if i.get('da_kiem_tra'))
    checklist.ti_le_hoan_thanh = (checklist.da_kiem_tra / checklist.tong_item) * 100 if checklist.tong_item > 0 else 0
    checklist.updated_at = datetime.now()
    
    db.session.commit()
    
    return success_response(
        data={
            'items': items,
            'ti_le_hoan_thanh': float(checklist.ti_le_hoan_thanh)
        },
        message='Auto-check hoàn tất'
    )
