"""ThanhTra (Inspection & Audit) API endpoints.

Provides APIs for:
[1] Hồ sơ thanh tra tự động (Auto-generated inspection dossier)
[2] Kiểm tra nội bộ trước thanh tra (Internal pre-inspection checks)
[3] Đối chiếu tờ khai thuế với sổ sách (Tax declaration reconciliation)
[4] Hồ sơ kiểm toán (Audit package)
[5] Theo dõi kiến nghị thanh tra (Inspection recommendations tracking)

References:
- Luật Thanh tra 11/2022/QH15
- Luật Quản lý Thuế 38/2019/QH14 Chương X
"""

from datetime import date, datetime, timedelta
from decimal import Decimal
from typing import Optional

from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from sqlalchemy import func, and_, or_

from app.extensions import db
from app.modules.thanh_tra.models import (
    QuyetDinhThanhTra,
    BienBanLamViec,
    KienNghiThanhTra,
    PhieuThuThueTruyThu,
    MauKiemToan,
    DuLieuKiemToan
)
from app.modules.nhat_ky.models import ChungTu, DinhKhoan
from app.modules.he_thong_tk.models import HeThongTaiKhoan
from app.modules.ky_ke_toan.models import KyKeToan
from app.modules.danh_muc.doi_tuong.models import DoiTuong
from app.utils.response import success_response, error_response, created_response
from marshmallow import ValidationError


thanh_tra_bp = Blueprint('thanh_tra', __name__)


# =============================================================================
# [1] HỒ SƠ THANH TRA TỰ ĐỘNG (Auto-generated Inspection Dossier)
# =============================================================================

@thanh_tra_bp.route('/ho-so/xuat-chung-tu', methods=['POST'])
@jwt_required()
def xuat_chung_tu_theo_khoang_ngay():
    """Xuất toàn bộ chứng từ theo khoảng thời gian được yêu cầu.
    
    Request body:
    - tu_ngay: date
    - den_ngay: date
    - loai_ct: optional filter
    """
    data = request.get_json()
    tu_ngay = data.get('tu_ngay')
    den_ngay = data.get('den_ngay')
    loai_ct = data.get('loai_ct')
    
    if not tu_ngay or not den_ngay:
        return error_response('Thiếu ngày bắt đầu hoặc kết thúc', 'VALIDATION_ERROR', 400)
    
    query = ChungTu.query.filter(
        ChungTu.ngay_ct >= tu_ngay,
        ChungTu.ngay_ct <= den_ngay,
        ChungTu.trang_thai == 'da_duyet'
    )
    
    if loai_ct:
        query = query.filter(ChungTu.loai_ct == loai_ct)
    
    chung_tu = query.order_by(ChungTu.ngay_ct, ChungTu.so_ct).all()
    
    result = []
    for ct in chung_tu:
        result.append({
            'id': ct.id,
            'so_ct': ct.so_ct,
            'loai_ct': ct.loai_ct,
            'ngay_ct': ct.ngay_ct.isoformat(),
            'dien_giai': ct.dien_giai,
            'dinh_khoan': [
                {
                    'stt': dk.stt,
                    'tk_no': dk.tk_no,
                    'tk_co': dk.tk_co,
                    'so_tien': float(dk.so_tien) if dk.so_tien else 0,
                    'dien_giai': dk.dien_giai
                }
                for dk in ct.dinh_khoan
            ]
        })
    
    return success_response(
        data=result,
        message=f'Xuất {len(result)} chứng từ trong kỳ'
    )


@thanh_tra_bp.route('/ho-so/so-cai', methods=['GET'])
@jwt_required()
def xuat_so_cai():
    """Xuất sổ cái theo khoảng thời gian.
    
    Query params:
    - tk: mã tài khoản (optional)
    - tu_ngay: date
    - den_ngay: date
    """
    tk = request.args.get('tk')
    tu_ngay = request.args.get('tu_ngay')
    den_ngay = request.args.get('den_ngay')
    
    if not tu_ngay or not den_ngay:
        return error_response('Thiếu ngày bắt đầu hoặc kết thúc', 'VALIDATION_ERROR', 400)
    
    query = db.session.query(
        DinhKhoan.tk_no,
        DinhKhoan.tk_co,
        ChungTu.so_ct,
        ChungTu.ngay_ct,
        DinhKhoan.so_tien,
        DinhKhoan.dien_giai,
        ChungTu.loai_ct
    ).join(
        ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
    ).filter(
        ChungTu.ngay_ct >= tu_ngay,
        ChungTu.ngay_ct <= den_ngay,
        ChungTu.trang_thai == 'da_duyet'
    )
    
    if tk:
        query = query.filter(
            or_(DinhKhoan.tk_no == tk, DinhKhoan.tk_co == tk)
        )
    
    rows = query.order_by(DinhKhoan.tk_no, ChungTu.ngay_ct, ChungTu.so_ct).all()
    
    result = []
    for r in rows:
        result.append({
            'tk': r.tk_no or r.tk_co,
            'so_ct': r.so_ct,
            'ngay_ct': r.ngay_ct.isoformat(),
            'loai_ct': r.loai_ct,
            'so_tien': float(r.so_tien) if r.so_tien else 0,
            'dien_giai': r.dien_giai
        })
    
    return success_response(data=result)


@thanh_tra_bp.route('/ho-so/danh-muc-chung-tu', methods=['GET'])
@jwt_required()
def danh_muc_chung_tu():
    """Danh mục chứng từ theo loại.
    
    Query params:
    - loai_ct: PC, PT, BN, BC, PNK, PXK...
    - tu_ngay, den_ngay
    """
    loai_ct = request.args.get('loai_ct')
    tu_ngay = request.args.get('tu_ngay')
    den_ngay = request.args.get('den_ngay')
    
    query = ChungTu.query.filter(
        ChungTu.trang_thai == 'da_duyet'
    )
    
    if loai_ct:
        query = query.filter(ChungTu.loai_ct == loai_ct)
    if tu_ngay and den_ngay:
        query = query.filter(
            ChungTu.ngay_ct >= tu_ngay,
            ChungTu.ngay_ct <= den_ngay
        )
    
    chung_tu = query.order_by(ChungTu.loai_ct, ChungTu.ngay_ct, ChungTu.so_ct).all()
    
    result = {
        'tong_so': len(chung_tu),
        'theo_loai': {},
        'danh_sach': []
    }
    
    for ct in chung_tu:
        loai = ct.lo_ct
        if loai not in result['theo_loai']:
            result['theo_loai'][loai] = 0
        result['theo_loai'][loai] += 1
        
        result['danh_sach'].append({
            'so_ct': ct.so_ct,
            'ngay_ct': ct.ngay_ct.isoformat(),
            'dien_giai': ct.dien_giai,
            'tong_tien': sum(float(dk.so_tien) for dk in ct.dinh_khoan)
        })
    
    return success_response(data=result)


@thanh_tra_bp.route('/ho-so/doi-chieu-cong-no', methods=['GET'])
@jwt_required()
def doi_chieu_cong_no():
    """Bảng đối chiếu công nợ với từng đối tượng.
    
    Query params:
    - doi_tuong_id: optional
    - tk: 131 (phải thu) hoặc 331 (phải trả)
    - den_ngay: date
    """
    doi_tuong_id = request.args.get('doi_tuong_id', type=int)
    tk = request.args.get('tk', '131')
    den_ngay = request.args.get('den_ngay')
    
    if not den_ngay:
        return error_response('Thiếu ngày đối chiếu', 'VALIDATION_ERROR', 400)
    
    if tk not in ['131', '331']:
        return error_response('TK phải là 131 hoặc 331', 'VALIDATION_ERROR', 400)
    
    doi_tuongs = [DoiTuong.query.get(doi_tuong_id)] if doi_tuong_id else DoiTuong.query.all()
    
    result = []
    for dt in doi_tuongs:
        if not dt:
            continue
        
        no_dau_ky = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
            ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
        ).filter(
            ChungTu.trang_thai == 'da_duyet',
            ChungTu.ngay_ct < den_ngay,
            getattr(DinhKhoan, 'tk_no' if tk == '131' else 'tk_co') == tk,
            DinhKhoan.doi_tuong_id == dt.id
        ).scalar() or 0
        
        co_dau_ky = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
            ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
        ).filter(
            ChungTu.trang_thai == 'da_duyet',
            ChungTu.ngay_ct < den_ngay,
            getattr(DinhKhoan, 'tk_co' if tk == '131' else 'tk_no') == tk,
            DinhKhoan.doi_tuong_id == dt.id
        ).scalar() or 0
        
        no_trong_ky = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
            ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
        ).filter(
            ChungTu.trang_thai == 'da_duyet',
            ChungTu.ngay_ct <= den_ngay,
            getattr(DinhKhoan, 'tk_no' if tk == '131' else 'tk_co') == tk,
            DinhKhoan.doi_tuong_id == dt.id
        ).scalar() or 0
        
        co_trong_ky = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
            ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
        ).filter(
            ChungTu.trang_thai == 'da_duyet',
            ChungTu.ngay_ct <= den_ngay,
            getattr(DinhKhoan, 'tk_co' if tk == '131' else 'tk_no') == tk,
            DinhKhoan.doi_tuong_id == dt.id
        ).scalar() or 0
        
        cuoi_ky = (no_dau_ky + no_trong_ky) - (co_dau_ky + co_trong_ky)
        
        result.append({
            'doi_tuong_id': dt.id,
            'ten_doi_tuong': dt.ten,
            'ma_doi_tuong': dt.ma_dt,
            'no_dau_ky': float(no_dau_ky),
            'co_dau_ky': float(co_dau_ky),
            'no_trong_ky': float(no_trong_ky),
            'co_trong_ky': float(co_trong_ky),
            'cuoi_ky': float(cuoi_ky)
        })
    
    return success_response(data=result)


# =============================================================================
# [2] KIỂM TRA NỘI BỘ TRƯỚC THANH TRA (Internal Pre-inspection Checks)
# =============================================================================

@thanh_tra_bp.route('/kiem-tra/hoa-don-tien-mat', methods=['GET'])
@jwt_required()
def kiem_tra_hoa_don_tien_mat():
    """Rà soát HĐ mua vào > 20 triệu thanh toán tiền mặt.
    
    Rủi ro: bị loại chi phí theo Luật Thuế TNDN.
    """
    tu_ngay = request.args.get('tu_ngay')
    den_ngay = request.args.get('den_ngay')
    
    if not tu_ngay or not den_ngay:
        return error_response('Thiếu khoảng thời gian', 'VALIDATION_ERROR', 400)
    
    query = db.session.query(
        ChungTu,
        func.sum(DinhKhoan.so_tien).label('tong_tien')
    ).join(
        DinhKhoan, ChungTu.id == DinhKhoan.chung_tu_id
    ).filter(
        ChungTu.trang_thai == 'da_duyet',
        ChungTu.ngay_ct >= tu_ngay,
        ChungTu.ngay_ct <= den_ngay,
        or_(
            and_(DinhKhoan.tk_no == '1111', DinhKhoan.tk_co.in_(['152', '153', '154', '156', '211', '213'])),
            and_(DinhKhoan.tk_co == '1111', DinhKhoan.tk_no.in_(['152', '153', '154', '156', '211', '213']))
        )
    ).group_by(ChungTu.id).having(func.sum(DinhKhoan.so_tien) > 20000000)
    
    results = []
    for ct, tong in query.all():
        results.append({
            'so_ct': ct.so_ct,
            'ngay_ct': ct.ngay_ct.isoformat(),
            'dien_giai': ct.dien_giai,
            'tong_tien': float(tong),
            'rui_ro': 'Có thể bị loại chi phí tiền mặt > 20 triệu'
        })
    
    return success_response(
        data=results,
        message=f'Có {len(results)} HĐ thanh toán tiền mặt > 20 triệu'
    )


@thanh_tra_bp.route('/kiem-tra/hoa-don-qua-han', methods=['GET'])
@jwt_required()
def kiem_tra_hoa_don_qua_han():
    """Rà soát HĐ đầu vào quá 6 tháng chưa kê khai khấu trừ.
    
    Rủi ro: hết thời hạn khấu trừ.
    """
    ngay_khao_sat = request.args.get('ngay_khao_sat', date.today())
    
    han_khau_tru = ngay_khao_sat - timedelta(days=180)
    
    query = db.session.query(
        DinhKhoan,
        ChungTu
    ).join(
        ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
    ).filter(
        ChungTu.trang_thai == 'da_duyet',
        DinhKhoan.tk_co.in_(['1331', '1332']),
        ChungTu.ngay_ct < han_khau_tru,
        ~DinhKhoan.id.in_(
            db.session.query(DinhKhoan.id).join(
                ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
            ).filter(
                DinhKhoan.tk_no == '1331',
                ChungTu.ngay_ct > han_khau_tru
            )
        )
    ).limit(100)
    
    results = []
    for dk, ct in query.all():
        so_ngay_qua_han = (ngay_khao_sat - ct.ngay_ct).days
        results.append({
            'so_ct': ct.so_ct,
            'ngay_ct': ct.ngay_ct.isoformat(),
            'tk_thue': dk.tk_co,
            'so_tien': float(dk.so_tien) if dk.so_tien else 0,
            'so_ngay_qua_han': so_ngay_qua_han,
            'rui_ro': f'Quá hạn khấu trừ {so_ngay_qua_han} ngày'
        })
    
    return success_response(
        data=results,
        message=f'Có {len(results)} HĐ đầu vào quá 6 tháng chưa khấu trừ'
    )


@thanh_tra_bp.route('/kiem-tra/tscd-khau-hao-ngoai-khung', methods=['GET'])
@jwt_required()
def kiem_tra_tscd_khau_hao():
    """Rà soát TSCĐ khấu hao ngoài khung TT23/2023.
    
    Rủi ro: bị điều chỉnh tăng thu nhập chịu thuế.
    """
    query = db.session.query(
        HeThongTaiKhoan
    ).filter(
        HeThongTaiKhoan.ma_tk.in_(['211', '212', '213'])
    )
    
    results = []
    for tk in query.all():
        results.append({
            'ma_tk': tk.ma_tk,
            'ten_tk': tk.ten_tk,
            'khuyen_cao': 'Kiểm tra mức khấu hao so với TT23/2023'
        })
    
    return success_response(data=results)


@thanh_tra_bp.route('/kiem-tra/luong-khong-hop-dong', methods=['GET'])
@jwt_required()
def kiem_tra_luong_khong_hop_dong():
    """Rà soát chi phí lương không có HĐLĐ / bảng lương.
    
    Rủi ro: bị loại chi phí.
    """
    return success_response(
        data=[],
        message='Cần tích hợp module lương để kiểm tra'
    )


@thanh_tra_bp.route('/kiem-tra/cong-no-qua-han', methods=['GET'])
@jwt_required()
def kiem_tra_cong_no_qua_han():
    """Rà soát công nợ > 3 năm chưa lập dự phòng.
    
    Rủi ro: bị tính thu nhập chịu thuế theo Thông tư 48.
    """
    den_ngay = request.args.get('den_ngay', date.today())
    han_3_nam = den_ngay - timedelta(days=1095)
    
    results = []
    
    for tk_ma, tk_ten in [('131', 'Phải thu khách hàng'), ('331', 'Phải trả người bán')]:
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
                and_(DinhKhoan.tk_no == tk_ma, DinhKhoan.tk_co.in_(['111', '112'])),
                and_(DinhKhoan.tk_co == tk_ma, DinhKhoan.tk_no.in_(['111', '112']))
            ),
            ChungTu.ngay_ct < han_3_nam
        ).group_by(DoiTuong.id).having(func.sum(DinhKhoan.so_tien) > 0)
        
        for dt, cong_no in query.all():
            results.append({
                'doi_tuong': dt.ten,
                'ma_tk': tk_ma,
                'ten_tk': tk_ten,
                'so_tien': float(cong_no),
                'ngay_lien_cuoi': '> 3 năm',
                'rui_ro': 'Cần lập dự phòng hoặc xử lý'
            })
    
    return success_response(
        data=results,
        message=f'Có {len(results)} công nợ > 3 năm cần xử lý'
    )


@thanh_tra_bp.route('/kiem-tra/doanh-thu-bat-thuong', methods=['GET'])
@jwt_required()
def kiem_tra_doanh_thu_bat_thuong():
    """Rà soát doanh thu bất thường (biến động > 30% so kỳ trước).
    """
    ky_hien_tai_id = request.args.get('ky_ke_toan_id', type=int)
    
    if not ky_hien_tai_id:
        return error_response('Thiếu kỳ kế toán', 'VALIDATION_ERROR', 400)
    
    ky_hien_tai = KyKeToan.query.get(ky_hien_tai_id)
    if not ky_hien_tai:
        return error_response('Kỳ kế toán không tồn tại', 'NOT_FOUND', 404)
    
    ky_truoc = KyKeToan.query.filter(
        KyKeToan.nam == ky_hien_tai.nam if ky_hien_tai.thang > 1 else ky_hien_tai.nam - 1,
        KyKeToan.thang == ky_hien_tai.thang - 1 if ky_hien_tai.thang > 1 else 12
    ).first()
    
    results = []
    
    for tk_ma, tk_ten in [('5111', 'Doanh thu bán hàng'), ('5112', 'Doanh thu DV'), ('515', 'Doanh thu TC')]:
        hien_tai = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
            ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
        ).filter(
            ChungTu.trang_thai == 'da_duyet',
            DinhKhoan.tk_co == tk_ma,
            ChungTu.ky_ke_toan_id == ky_hien_tai_id
        ).scalar() or 0
        
        truoc = 0
        if ky_truoc:
            truoc = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
                ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
            ).filter(
                ChungTu.trang_thai == 'da_duyet',
                DinhKhoan.tk_co == tk_ma,
                ChungTu.ky_ke_toan_id == ky_truoc.id
            ).scalar() or 0
        
        if truoc > 0:
            bien_dong = ((hien_tai - truoc) / truoc) * 100
            if abs(bien_dong) > 30:
                results.append({
                    'ma_tk': tk_ma,
                    'ten_tk': tk_ten,
                    'ky_hien_tai': float(hien_tai),
                    'ky_truoc': float(truoc),
                    'bien_dong_%': round(float(bien_dong), 2),
                    'canh_bao': 'Biến động bất thường > 30%'
                })
    
    return success_response(data=results)


# =============================================================================
# [3] ĐỐI CHIẾU TỜ KHAI THUẾ VỚI SỔ SÁCH
# =============================================================================

@thanh_tra_bp.route('/doi-chieu/thue-gtgt', methods=['GET'])
@jwt_required()
def doi_chieu_thue_gtgt():
    """Đối chiếu thuế GTGT: sổ sách vs tờ khai.
    
    Returns differences between:
    - Doanh thu trên sổ vs tờ khai GTGT
    - Thuế GTGT đầu ra trên sổ vs tờ khai
    - Thuế GTGT đầu vào khấu trừ trên sổ vs tờ khai
    """
    ky_ke_toan_id = request.args.get('ky_ke_toan_id', type=int)
    
    if not ky_ke_toan_id:
        return error_response('Thiếu kỳ kế toán', 'VALIDATION_ERROR', 400)
    
    doanh_thu_sd = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
        ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
    ).filter(
        ChungTu.trang_thai == 'da_duyet',
        DinhKhoan.tk_co.in_(['5111', '5112', '5113']),
        ChungTu.ky_ke_toan_id == ky_ke_toan_id
    ).scalar() or 0
    
    thue_gtgt_dau_ra = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
        ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
    ).filter(
        ChungTu.trang_thai == 'da_duyet',
        DinhKhoan.tk_co == '3331',
        ChungTu.ky_ke_toan_id == ky_ke_toan_id
    ).scalar() or 0
    
    thue_gtgt_dau_vao = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
        ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
    ).filter(
        ChungTu.trang_thai == 'da_duyet',
        DinhKhoan.tk_no == '1331',
        ChungTu.ky_ke_toan_id == ky_ke_toan_id
    ).scalar() or 0
    
    return success_response(data={
        'doanh_thu_ban_hang': float(doanh_thu_sd),
        'thue_gtgt_dau_ra': float(thue_gtgt_dau_ra),
        'thue_gtgt_dau_vao': float(thue_gtgt_dau_vao),
        'thue_gtgt_phai_nop': float(thue_gtgt_dau_ra - thue_gtgt_dau_vao),
        'huong_dan': 'So sánh với tờ khai GTGT mẫu 01/GTGT'
    })


# =============================================================================
# [4] HỒ SƠ KIỂM TOÁN (Audit Package)
# =============================================================================

@thanh_tra_bp.route('/kiem-toan/lead-schedule', methods=['GET'])
@jwt_required()
def tao_lead_schedule():
    """Tạo lead schedule cho tài khoản.
    
    Query params:
    - tk: mã tài khoản
    - ky_ke_toan_id: kỳ kế toán
    """
    tk = request.args.get('tk')
    ky_ke_toan_id = request.args.get('ky_ke_toan_id', type=int)
    
    if not tk or not ky_ke_toan_id:
        return error_response('Thiếu thông tin', 'VALIDATION_ERROR', 400)
    
    ky = KyKeToan.query.get(ky_ke_toan_id)
    if not ky:
        return error_response('Kỳ kế toán không tồn tại', 'NOT_FOUND', 404)
    
    so_du_dau_ky = 0
    phat_sinh_trong_ky_no = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
        ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
    ).filter(
        ChungTu.trang_thai == 'da_duyet',
        DinhKhoan.tk_no == tk,
        ChungTu.ky_ke_toan_id == ky_ke_toan_id
    ).scalar() or 0
    
    phat_sinh_trong_ky_co = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
        ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
    ).filter(
        ChungTu.trang_thai == 'da_duyet',
        DinhKhoan.tk_co == tk,
        ChungTu.ky_ke_toan_id == ky_ke_toan_id
    ).scalar() or 0
    
    so_du_cuoi_ky = so_du_dau_ky + phat_sinh_trong_ky_no - phat_sinh_trong_ky_co
    
    return success_response(data={
        'tk': tk,
        'ky_ke_toan': f'{ky.thang}/{ky.nam}',
        'so_du_dau_ky': float(so_du_dau_ky),
        'phat_sinh_no': float(phat_sinh_trong_ky_no),
        'phat_sinh_co': float(phat_sinh_trong_ky_co),
        'so_du_cuoi_ky': float(so_du_cuoi_ky)
    })


@thanh_tra_bp.route('/kiem-toan/bank-reconciliation', methods=['GET'])
@jwt_required()
def bank_reconciliation():
    """Đối chiếu ngân hàng - Bank reconciliation.
    
    Query params:
    - ngan_hang_id: tài khoản ngân hàng
    - den_ngay: ngày đối chiếu
    """
    ngan_hang_id = request.args.get('ngan_hang_id', type=int)
    den_ngay = request.args.get('den_ngay')
    
    if not ngan_hang_id or not den_ngay:
        return error_response('Thiếu thông tin', 'VALIDATION_ERROR', 400)
    
    from app.modules.danh_muc.ngan_hang.models import NganHang
    
    nh = NganHang.query.get(ngan_hang_id)
    if not nh:
        return error_response('Ngân hàng không tồn tại', 'NOT_FOUND', 404)
    
    so_du_ngan_hang = db.session.query(func.coalesce(func.sum(DinhKhoan.so_tien), 0)).join(
        ChungTu, DinhKhoan.chung_tu_id == ChungTu.id
    ).filter(
        ChungTu.trang_thai == 'da_duyet',
        DinhKhoan.tk_no == '1121',
        DinhKhoan.ngan_hang_id == ngan_hang_id,
        ChungTu.ngay_ct <= den_ngay
    ).scalar() or 0
    
    return success_response(data={
        'ngan_hang': nh.ten_tai_khoan,
        'so_du_so_sach': float(so_du_ngan_hang),
        'huong_dan': 'Cần đối chiếu với sao kê ngân hàng'
    })


# =============================================================================
# [5] THEO DÕI KIẾN NGHỊ THANH TRA
# =============================================================================

@thanh_tra_bp.route('/kien-nghi', methods=['GET'])
@jwt_required()
def list_kien_nghi():
    """Danh sách kiến nghị thanh tra."""
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    trang_thai = request.args.get('trang_thai')
    quyet_dinh_id = request.args.get('quyet_dinh_id', type=int)
    
    query = KienNghiThanhTra.query
    
    if trang_thai:
        query = query.filter(KienNghiThanhTra.trang_thai == trang_thai)
    if quyet_dinh_id:
        query = query.filter(KienNghiThanhTra.quyet_dinh_id == quyet_dinh_id)
    
    pagination = query.order_by(KienNghiThanhTra.ngay_kien_nghi.desc()).paginate(
        page=page, per_page=per_page, error_out=False
    )
    
    return success_response(
        data=[{
            'id': kn.id,
            'so_kien_nghi': kn.so_kien_nghi,
            'ngay_kien_nghi': kn.ngay_kien_nghi.isoformat() if kn.ngay_kien_nghi else None,
            'noi_dung': kn.noi_dung,
            'muc_do': kn.muc_do,
            'trang_thai': kn.trang_thai,
            'han_xu_ly': kn.han_xu_ly.isoformat() if kn.han_xu_ly else None,
            'ngay_xu_ly': kn.ngay_xu_ly.isoformat() if kn.ngay_xu_ly else None
        } for kn in pagination.items],
        pagination={'page': page, 'per_page': per_page, 'total': pagination.total}
    )


@thanh_tra_bp.route('/kien-nghi', methods=['POST'])
@jwt_required()
def create_kien_nghi():
    """Tạo mới kiến nghị thanh tra."""
    data = request.get_json()
    
    kn = KienNghiThanhTra(
        quyet_dinh_id=data['quyet_dinh_id'],
        so_kien_nghi=data.get('so_kien_nghi'),
        ngay_kien_nghi=data.get('ngay_kien_nghi'),
        noi_dung=data['noi_dung'],
        muc_do=data.get('muc_do', 'binh_thuong'),
        trang_thai=data.get('trang_thai', 'cho_xu_ly'),
        han_xu_ly=data.get('han_xu_ly'),
        chung_tu_dinh_kem=data.get('chung_tu_dinh_kem'),
        ghi_chu=data.get('ghi_chu')
    )
    
    db.session.add(kn)
    db.session.commit()
    
    return created_response(
        data={'id': kn.id},
        message='Tạo kiến nghị thành công'
    )


@thanh_tra_bp.route('/kien-nghi/<int:id>', methods=['PUT'])
@jwt_required()
def update_kien_nghi(id: int):
    """Cập nhật kiến nghị thanh tra."""
    kn = db.session.get(KienNghiThanhTra, id)
    if not kn:
        return error_response('Kiến nghị không tồn tại', 'NOT_FOUND', 404)
    
    data = request.get_json()
    
    for key in ['trang_thai', 'ngay_xu_ly', 'ghi_chu']:
        if key in data:
            setattr(kn, key, data[key])
    
    db.session.commit()
    
    return success_response(
        data={'id': kn.id, 'trang_thai': kn.trang_thai},
        message='Cập nhật kiến nghị thành công'
    )


@thanh_tra_bp.route('/phieu-thu', methods=['POST'])
@jwt_required()
def create_phieu_thu_thue():
    """Tạo phiếu nộp thuế truy thu.
    
    Request body:
    - kien_nghi_id: int
    - ngay_nop: date
    - thue_gtgt, thue_tndn, tien_phat, tien_cham_nop: Decimal
    """
    data = request.get_json()
    
    tong_tien = (
        (data.get('thue_gtgt') or 0) +
        (data.get('thue_tndn') or 0) +
        (data.get('thue_khac') or 0) +
        (data.get('tien_phat') or 0) +
        (data.get('tien_cham_nop') or 0)
    )
    
    pt = PhieuThuThueTruyThu(
        kien_nghi_id=data['kien_nghi_id'],
        ngay_nop=data['ngay_nop'],
        thue_gtgt=data.get('thue_gtgt', 0),
        thue_tndn=data.get('thue_tndn', 0),
        thue_khac=data.get('thue_khac', 0),
        tien_phat=data.get('tien_phat', 0),
        tien_cham_nop=data.get('tien_cham_nop', 0),
        tong_tien=tong_tien,
        chung_tu_goc=data.get('chung_tu_goc'),
        ghi_chu=data.get('ghi_chu')
    )
    
    db.session.add(pt)
    db.session.commit()
    
    return created_response(
        data={'id': pt.id, 'tong_tien': float(pt.tong_tien)},
        message='Tạo phiếu nộp thuế thành công'
    )


# =============================================================================
# QUẢN LÝ QUYẾT ĐỊNH THANH TRA
# =============================================================================

@thanh_tra_bp.route('/quyet-dinh', methods=['GET'])
@jwt_required()
def list_quyet_dinh():
    """Danh sách quyết định thanh tra."""
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    trang_thai = request.args.get('trang_thai')
    
    query = QuyetDinhThanhTra.query
    
    if trang_thai:
        query = query.filter(QuyetDinhThanhTra.trang_thai == trang_thai)
    
    pagination = query.order_by(QuyetDinhThanhTra.ngay_quyet_dinh.desc()).paginate(
        page=page, per_page=per_page, error_out=False
    )
    
    return success_response(
        data=[{
            'id': qd.id,
            'so_quyet_dinh': qd.so_quyet_dinh,
            'ngay_quyet_dinh': qd.ngay_quyet_dinh.isoformat(),
            'loai_thanh_tra': qd.loai_thanh_tra,
            'co_quan_thanh_tra': qd.co_quan_thanh_tra,
            'trang_thai': qd.trang_thai,
            'tu_ngay': qd.tu_ngay.isoformat(),
            'den_ngay': qd.den_ngay.isoformat()
        } for qd in pagination.items],
        pagination={'page': page, 'per_page': per_page, 'total': pagination.total}
    )


@thanh_tra_bp.route('/quyet-dinh', methods=['POST'])
@jwt_required()
def create_quyet_dinh():
    """Tạo mới quyết định thanh tra."""
    data = request.get_json()
    
    qd = QuyetDinhThanhTra(
        so_quyet_dinh=data['so_quyet_dinh'],
        ngay_quyet_dinh=data['ngay_quyet_dinh'],
        loai_thanh_tra=data['loai_thanh_tra'],
        co_quan_thanh_tra=data['co_quan_thanh_tra'],
        can_bo_thanh_tra=data.get('can_bo_thanh_tra'),
        tu_ngay=data['tu_ngay'],
        den_ngay=data['den_ngay'],
        pham_vi=data.get('pham_vi'),
        trang_thai=data.get('trang_thai', 'dang_dien_ra'),
        ghi_chu=data.get('ghi_chu')
    )
    
    db.session.add(qd)
    db.session.commit()
    
    return created_response(
        data={'id': qd.id},
        message='Tạo quyết định thanh tra thành công'
    )
