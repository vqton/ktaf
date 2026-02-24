"""Models for HoaDonDienTu (E-Invoice) module.

Provides models for:
- CauHinhHoaDon: E-invoice configuration
- MauSoKyHieu: Invoice templates (mẫu số, ký hiệu)
- HoaDonBanRa: E-invoices for sales
- HoaDonMuaVao: E-invoices for purchases
- HoaDonDieuChinh: Adjustment invoices
- LichSuXuLy: Processing history log

References:
- Nghị định 123/2020/NĐ-CP
- Thông tư 78/2021/TT-BTC
- Quyết định 1830/QĐ-TCT (schema XML HĐĐT)
"""

from datetime import date, datetime
from decimal import Decimal

from sqlalchemy import String, Numeric, Date, DateTime, Text, Integer, ForeignKey, JSON, Boolean
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models import Base, AuditMixin


class CauHinhHoaDon(Base, AuditMixin):
    """Cấu hình hóa đơn điện tử - E-invoice configuration.
    
    Lưu thông tin đăng ký HĐĐT với CQT và credentials API.
    """
    __tablename__ = 'cau_hinh_hoa_don'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    
    ma_so_thue: Mapped[str] = mapped_column(String(20), nullable=False, unique=True)
    ten_doanh_nghiep: Mapped[str] = mapped_column(String(255), nullable=False)
    dia_chi: Mapped[str] = mapped_column(String(500))
    
    # CQT credentials
    cqt_username: Mapped[str] = mapped_column(String(100))
    cqt_password: Mapped[str] = mapped_column(String(255))
    cqt_api_url: Mapped[str] = mapped_column(String(500), default='https://hdinvoice.gdt.gov.vn/api')
    
    # Certificate info
    so_serie_cert: Mapped[str] = mapped_column(String(50))
    ngay_het_han_cert: Mapped[date] = mapped_column(Date)
    cert_file_path: Mapped[str] = mapped_column(String(500))
    
    # HSM/Token config
    hsm_endpoint: Mapped[str] = mapped_column(String(500))
    hsm_token_id: Mapped[str] = mapped_column(String(100))
    
    # Email/SMS config
    smtp_host: Mapped[str] = mapped_column(String(100))
    smtp_port: Mapped[int] = mapped_column(default=587)
    smtp_username: Mapped[str] = mapped_column(String(100))
    smtp_password: Mapped[str] = mapped_column(String(255))
    email_from: Mapped[str] = mapped_column(String(100))
    
    # Trạng thái
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='cho_duyet',
        comment='cho_duyet, hoat_dong, tam_ngung'
    )
    
    ngay_dang_ky: Mapped[date] = mapped_column(Date)
    
    mau_so: Mapped[list['MauSoKyHieu']] = relationship(
        'MauSoKyHieu',
        back_populates='cau_hinh',
        cascade='all, delete-orphan'
    )


class MauSoKyHieu(Base, AuditMixin):
    """Mẫu số, ký hiệu hóa đơn - Invoice template.
    
    Lưu thông tin mẫu số, ký hiệu đã được CQT chấp thuận.
    """
    __tablename__ = 'mau_so_ky_hieu'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    cau_hinh_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.cau_hinh_hoa_don.id'),
        nullable=False
    )
    
    mau_so: Mapped[str] = mapped_column(String(10), nullable=False, comment='Mẫu số (01, 02...)')
    ky_hieu: Mapped[str] = mapped_column(String(20), nullable=False, comment='Ký hiệu (AA, AB...)')
    
    # CQT approval info
    so_quyet_dinh: Mapped[str] = mapped_column(String(50), comment='Số quyết định phê duyệt')
    ngay_phat_hanh: Mapped[date] = mapped_column(Date)
    ngay_het_han: Mapped[date] = mapped_column(Date)
    
    # Sequence
    so_hien_tai: Mapped[int] = mapped_column(Integer, default=0)
    prefix: Mapped[str] = mapped_column(String(10), default='')
    suffix: Mapped[str] = mapped_column(String(10), default='')
    
    # Trạng thái
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='hoat_dong',
        comment='hoat_dong, ngung'
    )
    
    hoa_don_ban: Mapped[list['HoaDonBanRa']] = relationship(
        'HoaDonBanRa',
        back_populates='mau_so_ky_hieu'
    )
    
    cau_hinh: Mapped['CauHinhHoaDon'] = relationship(
        'CauHinhHoaDon',
        back_populates='mau_so'
    )


class HoaDonBanRa(Base, AuditMixin):
    """Hóa đơn điện tử bán ra - E-invoice for sales.
    
    Lưu thông tin HĐ bán ra theo ND123/2020.
    """
    __tablename__ = 'hoa_don_ban_ra'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    mau_so_ky_hieu_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.mau_so_ky_hieu.id'),
        nullable=False
    )
    
    # Invoice identification
    mau_so: Mapped[str] = mapped_column(String(10), nullable=False)
    ky_hieu: Mapped[str] = mapped_column(String(20), nullable=False)
    so_hoa_don: Mapped[str] = mapped_column(String(20), nullable=False)
    ky_hieu_ma: Mapped[str] = mapped_column(String(20), comment='Ký hiệu mã (nếu có)')
    
    # CQT reference
    ma_hoa_don_cqt: Mapped[str] = mapped_column(
        String(50),
        unique=True,
        comment='Mã hóa đơn từ CQT (21 ký tự)'
    )
    so_hoa_don_thay_the: Mapped[str] = mapped_column(String(20))
    
    # Date info
    ngay_lap_hoa_don: Mapped[date] = mapped_column(Date, nullable=False)
    ngay_xac_nhan: Mapped[datetime] = mapped_column(DateTime)
    
    # Seller info (from config)
    ma_so_thue_ban: Mapped[str] = mapped_column(String(20), nullable=False)
    ten_ban: Mapped[str] = mapped_column(String(255), nullable=False)
    dia_chi_ban: Mapped[str] = mapped_column(String(500))
    dien_thoai_ban: Mapped[str] = mapped_column(String(20))
    fax_ban: Mapped[str] = mapped_column(String(20))
    
    # Buyer info
    ma_so_thue_mua: Mapped[str] = mapped_column(String(20))
    ten_mua: Mapped[str] = mapped_column(String(255), nullable=False)
    dia_chi_mua: Mapped[str] = mapped_column(String(500))
    dien_thoai_mua: Mapped[str] = mapped_column(String(20))
    email_mua: Mapped[str] = mapped_column(String(100))
    nguoi_mua: Mapped[str] = mapped_column(String(100))
    ten_don_vi_mua: Mapped[str] = mapped_column(String(255))
    
    # Payment info
    hinh_thuc_thanh_toan: Mapped[str] = mapped_column(
        String(20),
        comment='TM, CK, TM/CK, khac'
    )
    ma_khach_hang: Mapped[str] = mapped_column(String(50))
    
    # Money fields
    tong_tien_hang: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tong_chi_phi: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tong_giam_gia: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tong_tien_thue: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tong_tien_thanh_toan: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    
    # VAT breakdown
    thue_suat: Mapped[Decimal] = mapped_column(Numeric(5, 2), default=10)
    tien_thue_0: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tien_thue_5: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tien_thue_10: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    
    # Signature
    chu_ky_so: Mapped[str] = mapped_column(Text)
    chu_ky_ma: Mapped[str] = mapped_column(Text)
    thoi_diem_ky: Mapped[datetime] = mapped_column(DateTime)
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='nhap',
        comment='nhap, cho_ky, da_ky, gui_cqt, da_xac_nhan, loi, huy, thay_the, dieuchinh'
    )
    
    # Loại hóa đơn
    loai_hoa_don: Mapped[str] = mapped_column(
        String(20),
        default='ban',
        comment='ban, dich_vu, hop_dong, khac'
    )
    
    # File attachments
    file_xml: Mapped[str] = mapped_column(String(500))
    file_pdf: Mapped[str] = mapped_column(String(500))
    
    # Link to original if adjusted
    hoa_don_goc_id: Mapped[int] = mapped_column(ForeignKey('accounting.hoa_don_ban_ra.id'))
    
    # Notes
    ghi_chu: Mapped[str] = mapped_column(Text)
    
    # Payment tracking
    da_thanh_toan: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    ngay_thanh_toan: Mapped[date] = mapped_column(Date)
    
    chi_tiet: Mapped[list['HoaDonChiTiet']] = relationship(
        'HoaDonChiTiet',
        back_populates='hoa_don',
        cascade='all, delete-orphan'
    )
    lich_su: Mapped[list['LichSuXuLyHD']] = relationship(
        'LichSuXuLyHD',
        back_populates='hoa_don',
        cascade='all, delete-orphan'
    )
    
    mau_so_ky_hieu: Mapped['MauSoKyHieu'] = relationship(
        'MauSoKyHieu',
        back_populates='hoa_don_ban'
    )


class HoaDonChiTiet(Base, AuditMixin):
    """Chi tiết hóa đơn - Invoice line items."""
    __tablename__ = 'hoa_don_chi_tiet'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    hoa_don_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.hoa_don_ban_ra.id'),
        nullable=False
    )
    
    stt: Mapped[int] = mapped_column(Integer, nullable=False)
    
    # Product info
    ten_hang: Mapped[str] = mapped_column(String(500), nullable=False)
    ma_hang: Mapped[str] = mapped_column(String(50))
    don_vi_tinh: Mapped[str] = mapped_column(String(20))
    
    # Quantity & Price
    so_luong: Mapped[Decimal] = mapped_column(Numeric(18, 4), default=1)
    don_gia: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    don_gia_thanhtoan: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    
    # Money
    thue_suat: Mapped[Decimal] = mapped_column(Numeric(5, 2), default=10)
    tien_hang: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tien_thue: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    giam_gia: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    
    # Link to hang_hoa
    hang_hoa_id: Mapped[int] = mapped_column(ForeignKey('accounting.hang_hoa.id'))
    
    hoa_don: Mapped['HoaDonBanRa'] = relationship(
        'HoaDonBanRa',
        back_populates='chi_tiet'
    )


class HoaDonMuaVao(Base, AuditMixin):
    """Hóa đơn điện tử mua vào - E-invoice for purchases.
    
    Lưu thông tin HĐ mua vào từ nhà cung cấp.
    """
    __tablename__ = 'hoa_don_mua_vao'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    
    # Invoice identification from supplier
    ma_hoa_don_cqt: Mapped[str] = mapped_column(String(50), unique=True)
    so_hoa_don: Mapped[str] = mapped_column(String(20), nullable=False)
    mau_so: Mapped[str] = mapped_column(String(10))
    ky_hieu: Mapped[str] = mapped_column(String(20))
    
    # Supplier info
    ma_so_thue_ban: Mapped[str] = mapped_column(String(20), nullable=False)
    ten_ban: Mapped[str] = mapped_column(String(255), nullable=False)
    dia_chi_ban: Mapped[str] = mapped_column(String(500))
    
    # Our company as buyer
    ma_so_thue_mua: Mapped[str] = mapped_column(String(20))
    ten_mua: Mapped[str] = mapped_column(String(255))
    
    ngay_lap_hoa_don: Mapped[date] = mapped_column(Date, nullable=False)
    ngay_nhan: Mapped[date] = mapped_column(Date)
    
    # Money
    tong_tien_hang: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tong_tien_thue: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tong_tien_thanh_toan: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='chua_xu_ly',
        comment='chua_xu_ly, da_xac_nhan, da_ke_khai, bi_huy, bi_thay_the'
    )
    
    # Link to accounting
    chung_tu_id: Mapped[int] = mapped_column(ForeignKey('accounting.chung_tu.id'))
    doi_tuong_id: Mapped[int] = mapped_column(ForeignKey('accounting.doi_tuong.id'))
    
    # Warnings
    canh_bao: Mapped[str] = mapped_column(String(500), comment='Cảnh báo từ CQT')
    
    # File
    file_xml: Mapped[str] = mapped_column(String(500))
    file_pdf: Mapped[str] = mapped_column(String(500))


class HoaDonDieuChinh(Base, AuditMixin):
    """Hóa đơn điều chỉnh/thay thế/hủy - Adjustment invoices.
    
    Lưu thông tin HĐ điều chỉnh, thay thế hoặc hủy.
    """
    __tablename__ = 'hoa_don_dieu_chinh'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    hoa_don_goc_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.hoa_don_ban_ra.id'),
        nullable=False
    )
    
    # Adjustment info
    loai_dieu_chinh: Mapped[str] = mapped_column(
        String(20),
        nullable=False,
        comment='dieuchinh_tang, dieuchinh_giam, thay_the, huy'
    )
    
    # Reference to adjustment invoice
    hoa_don_dieu_chinh_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.hoa_don_ban_ra.id')
    )
    
    # Original invoice details before adjustment
    tong_tien_goc: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    tong_thue_goc: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    
    # Adjusted values
    tong_tien_dieu_chinh: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    tong_thue_dieu_chinh: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    
    # Reason
    ly_do: Mapped[str] = mapped_column(Text, nullable=False)
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='cho_duyet',
        comment='cho_duyet, da_duyet, bi_tu_choi'
    )
    
    # CQT confirmation
    ma_hoa_don_moi: Mapped[str] = mapped_column(String(50))
    ngay_xac_nhan_cqt: Mapped[datetime] = mapped_column(DateTime)


class LichSuXuLyHD(Base, AuditMixin):
    """Lịch sử xử lý hóa đơn - Invoice processing history.
    
    Log mọi thao tác trên HĐ (không thể xóa).
    """
    __tablename__ = 'lich_su_xu_ly_hd'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    hoa_don_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.hoa_don_ban_ra.id'),
        nullable=False
    )
    
    hanh_dong: Mapped[str] = mapped_column(
        String(50),
        nullable=False,
        comment='tao, sua, ky_so, gui_cqt, xac_nhan, huy, dieuchinh, thaythe'
    )
    
    noi_dung: Mapped[str] = mapped_column(Text)
    ket_qua: Mapped[str] = mapped_column(String(20), comment='thanh_cong, loi')
    loi_chi_tiet: Mapped[str] = mapped_column(Text)
    
    # CQT response
    ma_loi_cqt: Mapped[str] = mapped_column(String(20))
    thong_bao_cqt: Mapped[str] = mapped_column(Text)
    
    # Timestamp
    thoi_diem: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    
    hoa_don: Mapped['HoaDonBanRa'] = relationship(
        'HoaDonBanRa',
        back_populates='lich_su'
    )
