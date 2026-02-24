"""HoaDonDienTu (E-Invoice) API endpoints.

Provides APIs for:
[1] Đăng ký & Cấu hình (Registration & Configuration)
[2] Lập HĐĐT bán ra (Create E-invoices for sales)
[3] Tiếp nhận HĐĐT mua vào (Receive E-invoices for purchases)
[4] Xử lý HĐ điều chỉnh/thay thế/hủy
[5] Báo cáo HĐĐT
[6] Tuân thủ (Compliance)

References:
- Nghị định 123/2020/NĐ-CP
- Thông tư 78/2021/TT-BTC
- Quyết định 1830/QĐ-TCT (schema XML HĐĐT)
"""

from datetime import date, datetime
from decimal import Decimal
from typing import Optional

from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from sqlalchemy import func, and_, or_

from app.extensions import db
from app.modules.hoa_don_dien_tu.models import (
    CauHinhHoaDon,
    MauSoKyHieu,
    HoaDonBanRa,
    HoaDonChiTiet,
    HoaDonMuaVao,
    HoaDonDieuChinh,
    LichSuXuLyHD
)
from app.modules.danh_muc.doi_tuong.models import DoiTuong
from app.modules.danh_muc.hang_hoa.models import HangHoa
from app.utils.response import success_response, error_response, created_response
from marshmallow import ValidationError


hoa_don_bp = Blueprint('hoa_don_dien_tu', __name__)


# =============================================================================
# [1] ĐĂNG KÝ & CẤU HÌNH
# =============================================================================

@hoa_don_bp.route('/cau-hinh', methods=['GET'])
@jwt_required()
def get_cau_hinh():
    """Lấy cấu hình hóa đơn điện tử."""
    cau_hinh = CauHinhHoaDon.query.first()
    if not cau_hinh:
        return error_response('Chưa có cấu hình hóa đơn', 'NOT_FOUND', 404)
    
    return success_response(data={
        'id': cau_hinh.id,
        'ma_so_thue': cau_hinh.ma_so_thue,
        'ten_doanh_nghiep': cau_hinh.ten_doanh_nghiep,
        'dia_chi': cau_hinh.dia_chi,
        'trang_thai': cau_hinh.trang_thai,
        'ngay_dang_ky': cau_hinh.ngay_dang_ky.isoformat() if cau_hinh.ngay_dang_ky else None
    })


@hoa_don_bp.route('/cau-hinh', methods=['POST'])
@jwt_required()
def create_cau_hinh():
    """Tạo cấu hình hóa đơn điện tử."""
    data = request.get_json()
    
    existing = CauHinhHoaDon.query.filter_by(ma_so_thue=data['ma_so_thue']).first()
    if existing:
        return error_response('Mã số thuế đã có cấu hình', 'ALREADY_EXISTS', 400)
    
    cau_hinh = CauHinhHoaDon(
        ma_so_thue=data['ma_so_thue'],
        ten_doanh_nghiep=data['ten_doanh_nghiep'],
        dia_chi=data.get('dia_chi'),
        cqt_username=data.get('cqt_username'),
        cqt_password=data.get('cqt_password'),
        cqt_api_url=data.get('cqt_api_url', 'https://hdinvoice.gdt.gov.vn/api'),
        smtp_host=data.get('smtp_host'),
        smtp_port=data.get('smtp_port', 587),
        smtp_username=data.get('smtp_username'),
        smtp_password=data.get('smtp_password'),
        email_from=data.get('email_from'),
        trang_thai='cho_duyet'
    )
    
    db.session.add(cau_hinh)
    db.session.commit()
    
    return created_response(
        data={'id': cau_hinh.id},
        message='Tạo cấu hình hóa đơn thành công'
    )


@hoa_don_bp.route('/mau-so', methods=['GET'])
@jwt_required()
def list_mau_so():
    """Danh sách mẫu số, ký hiệu hóa đơn."""
    cau_hinh_id = request.args.get('cau_hinh_id', type=int)
    trang_thai = request.args.get('trang_thai')
    
    query = MauSoKyHieu.query
    
    if cau_hinh_id:
        query = query.filter(MauSoKyHieu.cau_hinh_id == cau_hinh_id)
    if trang_thai:
        query = query.filter(MauSoKyHieu.trang_thai == trang_thai)
    
    mau_so = query.order_by(MauSoKyHieu.mau_so, MauSoKyHieu.ky_hieu).all()
    
    return success_response(data=[{
        'id': m.id,
        'mau_so': m.mau_so,
        'ky_hieu': m.ky_hieu,
        'so_quyet_dinh': m.so_quyet_dinh,
        'ngay_phat_hanh': m.ngay_phat_hanh.isoformat() if m.ngay_phat_hanh else None,
        'ngay_het_han': m.ngay_het_han.isoformat() if m.ngay_het_han else None,
        'so_hien_tai': m.so_hien_tai,
        'trang_thai': m.trang_thai
    } for m in mau_so])


@hoa_don_bp.route('/mau-so', methods=['POST'])
@jwt_required()
def create_mau_so():
    """Tạo mẫu số, ký hiệu hóa đơn."""
    data = request.get_json()
    
    mau_so = MauSoKyHieu(
        cau_hinh_id=data['cau_hinh_id'],
        mau_so=data['mau_so'],
        ky_hieu=data['ky_hieu'],
        so_quyet_dinh=data.get('so_quyet_dinh'),
        ngay_phat_hanh=data.get('ngay_phat_hanh'),
        ngay_het_han=data.get('ngay_het_han'),
        prefix=data.get('prefix', ''),
        suffix=data.get('suffix', ''),
        trang_thai='hoat_dong'
    )
    
    db.session.add(mau_so)
    db.session.commit()
    
    return created_response(
        data={'id': mau_so.id},
        message='Tạo mẫu số hóa đơn thành công'
    )


# =============================================================================
# [2] LẬP HĐĐT BÁN RA
# =============================================================================

@hoa_don_bp.route('/ban-ra', methods=['GET'])
@jwt_required()
def list_hoa_don_ban():
    """Danh sách hóa đơn bán ra."""
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    trang_thai = request.args.get('trang_thai')
    tu_ngay = request.args.get('tu_ngay')
    den_ngay = request.args.get('den_ngay')
    mau_so = request.args.get('mau_so')
    
    query = HoaDonBanRa.query
    
    if trang_thai:
        query = query.filter(HoaDonBanRa.trang_thai == trang_thai)
    if tu_ngay:
        query = query.filter(HoaDonBanRa.ngay_lap_hoa_don >= tu_ngay)
    if den_ngay:
        query = query.filter(HoaDonBanRa.ngay_lap_hoa_don <= den_ngay)
    if mau_so:
        query = query.filter(HoaDonBanRa.mau_so == mau_so)
    
    pagination = query.order_by(
        HoaDonBanRa.ngay_lap_hoa_don.desc(),
        HoaDonBanRa.so_hoa_don.desc()
    ).paginate(page=page, per_page=per_page, error_out=False)
    
    return success_response(
        data=[{
            'id': hd.id,
            'so_hoa_don': hd.so_hoa_don,
            'mau_so': hd.mau_so,
            'ky_hieu': hd.ky_hieu,
            'ngay_lap': hd.ngay_lap_hoa_don.isoformat(),
            'ten_mua': hd.ten_mua,
            'tong_tien_thanh_toan': float(hd.tong_tien_thanh_toan),
            'trang_thai': hd.trang_thai,
            'da_thanh_toan': float(hd.da_thanh_toan)
        } for hd in pagination.items],
        pagination={'page': page, 'per_page': per_page, 'total': pagination.total}
    )


@hoa_don_bp.route('/ban-ra', methods=['POST'])
@jwt_required()
def create_hoa_don_ban():
    """Tạo hóa đơn bán ra mới."""
    data = request.get_json()
    user_id = get_jwt_identity()
    
    mau_so_ky_hieu_id = data.get('mau_so_ky_hieu_id')
    mau_so_ky_hieu = MauSoKyHieu.query.get(mau_so_ky_hieu_id)
    if not mau_so_ky_hieu:
        return error_response('Mẫu số ký hiệu không tồn tại', 'NOT_FOUND', 400)
    
    # Generate invoice number
    mau_so_ky_hieu.so_hien_tai += 1
    so_hoa_don = f"{mau_so_ky_hieu.prefix}{mau_so_ky_hieu.so_hien_tai:07d}{mau_so_ky_hieu.suffix}"
    
    # Get config for seller info
    cau_hinh = CauHinhHoaDon.query.first()
    if not cau_hinh:
        return error_response('Chưa có cấu hình hóa đơn', 'NOT_FOUND', 400)
    
    # Calculate totals
    tong_tien_hang = Decimal(0)
    tong_tien_thue = Decimal(0)
    chi_tiet_data = data.get('chi_tiet', [])
    
    for ct in chi_tiet_data:
        tien_hang = (Decimal(str(ct.get('so_luong', 1))) * Decimal(str(ct.get('don_gia', 0))))
        thue_suat = Decimal(str(ct.get('thue_suat', 10)))
        tien_thue = tien_hang * thue_suat / 100
        tong_tien_hang += tien_hang
        tong_tien_thue += tien_thue
    
    tong_tien_thanh_toan = tong_tien_hang + tong_tien_thue - Decimal(str(data.get('tong_giam_gia', 0)))
    
    # Create invoice
    hoa_don = HoaDonBanRa(
        mau_so_ky_hieu_id=mau_so_ky_hieu_id,
        mau_so=mau_so_ky_hieu.mau_so,
        ky_hieu=mau_so_ky_hieu.ky_hieu,
        so_hoa_don=so_hoa_don,
        ngay_lap_hoa_don=data.get('ngay_lap_hoa_don', date.today()),
        
        # Seller
        ma_so_thue_ban=cau_hinh.ma_so_thue,
        ten_ban=cau_hinh.ten_doanh_nghiep,
        dia_chi_ban=cau_hinh.dia_chi,
        dien_thoai_ban=cau_hinh.dien_thoai_ban,
        
        # Buyer
        ma_so_thue_mua=data.get('ma_so_thue_mua'),
        ten_mua=data['ten_mua'],
        dia_chi_mua=data.get('dia_chi_mua'),
        dien_thoai_mua=data.get('dien_thoai_mua'),
        email_mua=data.get('email_mua'),
        nguoi_mua=data.get('nguoi_mua'),
        
        # Payment
        hinh_thuc_thanh_toan=data.get('hinh_thuc_thanh_toan', 'TM'),
        ma_khach_hang=data.get('ma_khach_hang'),
        
        # Money
        tong_tien_hang=tong_tien_hang,
        tong_tien_thue=tong_tien_thue,
        tong_giam_gia=data.get('tong_giam_gia', 0),
        tong_tien_thanh_toan=tong_tien_thanh_toan,
        
        # VAT breakdown
        thue_suat=Decimal(str(data.get('thue_suat', 10))),
        
        # Status
        trang_thai='nhap',
        loai_hoa_don=data.get('loai_hoa_don', 'ban'),
        ghi_chu=data.get('ghi_chu')
    )
    
    db.session.add(hoa_don)
    db.session.flush()
    
    # Add invoice details
    stt = 1
    for ct in chi_tiet_data:
        tien_hang = Decimal(str(ct.get('so_luong', 1))) * Decimal(str(ct.get('don_gia', 0)))
        thue_suat = Decimal(str(ct.get('thue_suat', 10)))
        tien_thue = tien_hang * thue_suat / 100
        
        chi_tiet = HoaDonChiTiet(
            hoa_don_id=hoa_don.id,
            stt=stt,
            ten_hang=ct['ten_hang'],
            ma_hang=ct.get('ma_hang'),
            don_vi_tinh=ct.get('don_vi_tinh'),
            so_luong=ct.get('so_luong', 1),
            don_gia=ct.get('don_gia', 0),
            thue_suat=thue_suat,
            tien_hang=tien_hang,
            tien_thue=tien_thue,
            hang_hoa_id=ct.get('hang_hoa_id')
        )
        db.session.add(chi_tiet)
        stt += 1
    
    # Add history
    lich_su = LichSuXuLyHD(
        hoa_don_id=hoa_don.id,
        hanh_dong='tao',
        noi_dung='Tạo hóa đơn mới',
        ket_qua='thanh_cong',
        thoi_diem=datetime.now()
    )
    db.session.add(lich_su)
    
    db.session.commit()
    
    return created_response(
        data={
            'id': hoa_don.id,
            'so_hoa_don': hoa_don.so_hoa_don,
            'tong_tien_thanh_toan': float(hoa_don.tong_tien_thanh_toan)
        },
        message='Tạo hóa đơn thành công'
    )


@hoa_don_bp.route('/ban-ra/<int:id>/ky-so', methods=['POST'])
@jwt_required()
def ky_so_hoa_don(id: int):
    """Ký số hóa đơn (gửi CQT)."""
    hoa_don = HoaDonBanRa.query.get(id)
    if not hoa_don:
        return error_response('Hóa đơn không tồn tại', 'NOT_FOUND', 404)
    
    if hoa_don.trang_thai not in ['nhap', 'cho_ky']:
        return error_response('Hóa đơn không thể ký số', 'INVALID_STATE', 400)
    
    # Simulate signing (in real implementation, would call HSM/USB Token)
    hoa_don.trang_thai = 'da_ky'
    hoa_don.thoi_diem_ky = datetime.now()
    hoa_don.chu_ky_so = 'SIMULATED_SIGNATURE'
    
    # Simulate CQT response (in real implementation, would call CQT API)
    import uuid
    hoa_don.ma_hoa_don_cqt = str(uuid.uuid4()).upper().replace('-', '')[:21]
    hoa_don.ngay_xac_nhan = datetime.now()
    hoa_don.trang_thai = 'da_xac_nhan'
    
    # Add history
    lich_su = LichSuXuLyHD(
        hoa_don_id=hoa_don.id,
        hanh_dong='ky_so',
        noi_dung='Ký số và gửi CQT',
        ket_qua='thanh_cong',
        ma_loi_cqt='000',
        thong_bao_cqt='Chấp nhận',
        thoi_diem=datetime.now()
    )
    db.session.add(lich_su)
    db.session.commit()
    
    return success_response(
        data={
            'id': hoa_don.id,
            'ma_hoa_don_cqt': hoa_don.ma_hoa_don_cqt,
            'trang_thai': hoa_don.trang_thai
        },
        message='Ký số hóa đơn thành công'
    )


# =============================================================================
# [3] TIẾP NHẬN HĐĐT MUA VÀO
# =============================================================================

@hoa_don_bp.route('/mua-vao', methods=['GET'])
@jwt_required()
def list_hoa_don_mua():
    """Danh sách hóa đơn mua vào."""
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 20, type=int)
    trang_thai = request.args.get('trang_thai')
    
    query = HoaDonMuaVao.query
    
    if trang_thai:
        query = query.filter(HoaDonMuaVao.trang_thai == trang_thai)
    
    pagination = query.order_by(HoaDonMuaVao.ngay_lap_hoa_don.desc()).paginate(
        page=page, per_page=per_page, error_out=False
    )
    
    return success_response(
        data=[{
            'id': hd.id,
            'so_hoa_don': hd.so_hoa_don,
            'ten_ban': hd.ten_ban,
            'ma_so_thue_ban': hd.ma_so_thue_ban,
            'ngay_lap': hd.ngay_lap_hoa_don.isoformat(),
            'tong_tien_thanh_toan': float(hd.tong_tien_thanh_toan),
            'tong_tien_thue': float(hd.tong_tien_thue),
            'trang_thai': hd.trang_thai,
            'canh_bao': hd.canh_bao
        } for hd in pagination.items],
        pagination={'page': page, 'per_page': per_page, 'total': pagination.total}
    )


@hoa_don_bp.route('/mua-vao/tra-cuu', methods=['POST'])
@jwt_required()
def tra_cuu_hoa_don_mua():
    """Tra cứu hóa đơn mua vào trên CQT."""
    data = request.get_json()
    ma_hoa_don = data.get('ma_hoa_don')
    
    if not ma_hoa_don:
        return error_response('Thiếu mã hóa đơn', 'VALIDATION_ERROR', 400)
    
    # Simulate CQT API call (in real implementation, would call hdinvoice.gdt.gov.vn)
    existing = HoaDonMuaVao.query.filter_by(ma_hoa_don_cqt=ma_hoa_don).first()
    if existing:
        return success_response(
            data={
                'ton_tai': True,
                'hoa_don': {
                    'id': existing.id,
                    'so_hoa_don': existing.so_hoa_don,
                    'ten_ban': existing.ten_ban,
                    'tong_tien': float(existing.tong_tien_thanh_toan),
                    'trang_thai': existing.trang_thai
                }
            },
            message='Hóa đơn đã tồn tại trong hệ thống'
        )
    
    # Return sample data for simulation
    return success_response(
        data={
            'ton_tai': False,
            'thong_tin': {
                'ma_hoa_don': ma_hoa_don,
                'so_hoa_don': 'HD001/2025',
                'mau_so': '01GTKT0',
                'ky_hieu': 'AA/23P',
                'ten_ban': 'Công ty TNHH ABC',
                'ma_so_thue': '0312345678',
                'tong_tien': 110000000,
                'tong_thue': 10000000,
                'trang_thai': 'Hoạt động'
            }
        },
        message='Tra cứu thành công'
    )


# =============================================================================
# [4] XỬ LÝ HĐ ĐIỀU CHỈNH/THAY THẾ/HỦY
# =============================================================================

@hoa_don_bp.route('/ban-ra/<int:id>/huy', methods=['POST'])
@jwt_required()
def huy_hoa_don(id: int):
    """Hủy hóa đơn (cần biên bản hủy)."""
    data = request.get_json()
    hoa_don = HoaDonBanRa.query.get(id)
    
    if not hoa_don:
        return error_response('Hóa đơn không tồn tại', 'NOT_FOUND', 404)
    
    if hoa_don.trang_thai not in ['da_ky', 'da_xac_nhan']:
        return error_response('Hóa đơn không thể hủy', 'INVALID_STATE', 400)
    
    # Create adjustment record
    dieuchinh = HoaDonDieuChinh(
        hoa_don_goc_id=id,
        loai_dieu_chinh='huy',
        ly_do=data.get('ly_do', 'Hủy theo biên bản'),
        tong_tien_goc=hoa_don.tong_tien_thanh_toan,
        tong_thue_goc=hoa_don.tong_tien_thue,
        trang_thai='da_duyet'
    )
    
    hoa_don.trang_thai = 'huy'
    
    # Add history
    lich_su = LichSuXuLyHD(
        hoa_don_id=hoa_don.id,
        hanh_dong='huy',
        noi_dung=f"Hủy hóa đơn: {data.get('ly_do')}",
        ket_qua='thanh_cong',
        thoi_diem=datetime.now()
    )
    db.session.add(dieuchinh)
    db.session.add(lich_su)
    db.session.commit()
    
    return success_response(
        data={'id': hoa_don.id, 'trang_thai': 'huy'},
        message='Hủy hóa đơn thành công'
    )


# =============================================================================
# [5] BÁO CÁO HĐĐT
# =============================================================================

@hoa_don_bp.route('/bao-cao/ban-ra', methods=['GET'])
@jwt_required()
def bao_cao_ban_ra():
    """Báo cáo hóa đơn bán ra theo kỳ."""
    tu_ngay = request.args.get('tu_ngay')
    den_ngay = request.args.get('den_ngay')
    
    if not tu_ngay or not den_ngay:
        return error_response('Thiếu khoảng thời gian', 'VALIDATION_ERROR', 400)
    
    query = HoaDonBanRa.query.filter(
        HoaDonBanRa.ngay_lap_hoa_don >= tu_ngay,
        HoaDonBanRa.ngay_lap_hoa_don <= den_ngay,
        HoaDonBanRa.trang_thai.in_(['da_ky', 'da_xac_nhan'])
    )
    
    tong_tien = db.session.query(func.sum(HoaDonBanRa.tong_tien_thanh_toan)).filter(
        HoaDonBanRa.ngay_lap_hoa_don >= tu_ngay,
        HoaDonBanRa.ngay_lap_hoa_don <= den_ngay,
        HoaDonBanRa.trang_thai.in_(['da_ky', 'da_xac_nhan'])
    ).scalar() or 0
    
    tong_thue = db.session.query(func.sum(HoaDonBanRa.tong_tien_thue)).filter(
        HoaDonBanRa.ngay_lap_hoa_don >= tu_ngay,
        HoaDonBanRa.ngay_lap_hoa_don <= den_ngay,
        HoaDonBanRa.trang_thai.in_(['da_ky', 'da_xac_nhan'])
    ).scalar() or 0
    
    so_luong = query.count()
    
    return success_response(data={
        'tu_ngay': tu_ngay,
        'den_ngay': den_ngay,
        'so_luong': so_luong,
        'tong_tien_hang': float(tong_tien - tong_thue),
        'tong_tien_thue': float(tong_thue),
        'tong_tien_thanh_toan': float(tong_tien)
    })


@hoa_don_bp.route('/bao-cao/mua-vao', methods=['GET'])
@jwt_required()
def bao_cao_mua_vao():
    """Báo cáo hóa đơn mua vào theo kỳ."""
    tu_ngay = request.args.get('tu_ngay')
    den_ngay = request.args.get('den_ngay')
    
    if not tu_ngay or not den_ngay:
        return error_response('Thiếu khoảng thời gian', 'VALIDATION_ERROR', 400)
    
    tong_tien = db.session.query(func.sum(HoaDonMuaVao.tong_tien_thanh_toan)).filter(
        HoaDonMuaVao.ngay_lap_hoa_don >= tu_ngay,
        HoaDonMuaVao.ngay_lap_hoa_don <= den_ngay
    ).scalar() or 0
    
    tong_thue = db.session.query(func.sum(HoaDonMuaVao.tong_tien_thue)).filter(
        HoaDonMuaVao.ngay_lap_hoa_don >= tu_ngay,
        HoaDonMuaVao.ngay_lap_hoa_don <= den_ngay
    ).scalar() or 0
    
    so_luong = HoaDonMuaVao.query.filter(
        HoaDonMuaVao.ngay_lap_hoa_don >= tu_ngay,
        HoaDonMuaVao.ngay_lap_hoa_don <= den_ngay
    ).count()
    
    return success_response(data={
        'tu_ngay': tu_ngay,
        'den_ngay': den_ngay,
        'so_luong': so_luong,
        'tong_tien_hang': float(tong_tien - tong_thue),
        'tong_tien_thue': float(tong_thue),
        'tong_tien_thanh_toan': float(tong_tien)
    })


@hoa_don_bp.route('/bao-cao/doi-chieu', methods=['GET'])
@jwt_required()
def doi_chieu_hoa_don_so():
    """Đối chiếu HĐĐT với sổ kế toán."""
    ky_ke_toan_id = request.args.get('ky_ke_toan_id', type=int)
    
    if not ky_ke_toan_id:
        return error_response('Thiếu kỳ kế toán', 'VALIDATION_ERROR', 400)
    
    # Get invoice totals
    hd_ban = db.session.query(
        func.count(HoaDonBanRa.id).label('so_luong'),
        func.sum(HoaDonBanRa.tong_tien_thanh_toan).label('tong_tien')
    ).filter(
        HoaDonBanRa.ky_ke_toan_id == ky_ke_toan_id,
        HoaDonBanRa.trang_thai.in_(['da_ky', 'da_xac_nhan'])
    ).first()
    
    hd_mua = db.session.query(
        func.count(HoaDonMuaVao.id).label('so_luong'),
        func.sum(HoaDonMuaVao.tong_tien_thanh_toan).label('tong_tien')
    ).filter(
        HoaDonMuaVao.ky_ke_toan_id == ky_ke_toan_id
    ).first()
    
    return success_response(data={
        'ban_ra': {
            'so_luong': hd_ban.so_luong or 0,
            'tong_tien': float(hd_ban.tong_tien or 0)
        },
        'mua_vao': {
            'so_luong': hd_mua.so_luong or 0,
            'tong_tien': float(hd_mua.tong_tien or 0)
        }
    })


# =============================================================================
# [6] TUÂN THỦ & LƯU TRỮ
# =============================================================================

@hoa_don_bp.route('/lich-su/<int:id>', methods=['GET'])
@jwt_required()
def get_lich_su(id: int):
    """Lấy lịch sử xử lý hóa đơn (log không thể xóa)."""
    hoa_don = HoaDonBanRa.query.get(id)
    if not hoa_don:
        return error_response('Hóa đơn không tồn tại', 'NOT_FOUND', 404)
    
    lich_su = LichSuXuLyHD.query.filter_by(hoa_don_id=id).order_by(
        LichSuXuLyHD.thoi_diem.desc()
    ).all()
    
    return success_response(data=[{
        'id': ls.id,
        'hanh_dong': ls.hanh_dong,
        'noi_dung': ls.noi_dung,
        'ket_qua': ls.ket_qua,
        'thoi_diem': ls.thoi_diem.isoformat() if ls.thoi_diem else None
    } for ls in lich_su])
