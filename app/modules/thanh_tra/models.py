"""Models for ThanhTra (Inspection & Audit) module.

Provides models for:
- QuyetDinhThanhTra: Inspection decision records
- BienBanLamViec: Working minutes
- KienNghiThanhTra: Inspection recommendations
- PhieuThuThueTruyThu: Tax penalty payment records

References:
- Luật Thanh tra 11/2022/QH15
- Luật Quản lý Thuế 38/2019/QH14 Chương X
"""

from datetime import date, datetime
from decimal import Decimal

from sqlalchemy import String, Numeric, Date, DateTime, Text, Integer, ForeignKey, JSON
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models import Base, AuditMixin


class QuyetDinhThanhTra(Base, AuditMixin):
    """Quyết định thanh tra - Inspection decision record.
    
    Lưu thông tin quyết định thanh tra thuế hoặc kiểm toán.
    """
    __tablename__ = 'quyet_dinh_thanh_tra'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    so_quyet_dinh: Mapped[str] = mapped_column(String(50), unique=True, nullable=False)
    ngay_quyet_dinh: Mapped[date] = mapped_column(Date, nullable=False)
    
    loai_thanh_tra: Mapped[str] = mapped_column(
        String(20),
        nullable=False,
        comment='thue: thanh tra thuế, kt: kiểm toán độc lập, noi_bo: kiểm tra nội bộ'
    )
    
    co_quan_thanh_tra: Mapped[str] = mapped_column(String(255), nullable=False)
    can_bo_thanh_tra: Mapped[str] = mapped_column(String(255))
    
    tu_ngay: Mapped[date] = mapped_column(Date, nullable=False)
    den_ngay: Mapped[date] = mapped_column(Date, nullable=False)
    
    pham_vi: Mapped[str] = mapped_column(
        Text,
        comment='Phạm vi thanh tra: thuế GTGT, TNDN, hải quan...'
    )
    
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='dang_dien_ra',
        comment='dang_dien_ra, hoan_thanh, huy'
    )
    
    ghi_chu: Mapped[str] = mapped_column(Text)
    
    bien_ban: Mapped[list['BienBanLamViec']] = relationship(
        'BienBanLamViec',
        back_populates='quyet_dinh',
        cascade='all, delete-orphan'
    )
    kien_nghi: Mapped[list['KienNghiThanhTra']] = relationship(
        'KienNghiThanhTra',
        back_populates='quyet_dinh',
        cascade='all, delete-orphan'
    )


class BienBanLamViec(Base, AuditMixin):
    """Biên bản làm việc - Working minutes during inspection."""
    __tablename__ = 'bien_ban_lam_viec'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    quyet_dinh_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.quyet_dinh_thanh_tra.id'),
        nullable=False
    )
    
    so_bien_ban: Mapped[str] = mapped_column(String(50), unique=True, nullable=False)
    ngay_lap: Mapped[date] = mapped_column(Date, nullable=False)
    noi_dung: Mapped[str] = mapped_column(Text, nullable=False)
    ket_luan: Mapped[str] = mapped_column(Text)
    
    dai_dien_co_quan: Mapped[str] = mapped_column(String(255))
    dai_dien_doanh_nghiep: Mapped[str] = mapped_column(String(255))
    
    quyet_dinh: Mapped['QuyetDinhThanhTra'] = relationship(
        'QuyetDinhThanhTra',
        back_populates='bien_ban'
    )


class KienNghiThanhTra(Base, AuditMixin):
    """Kiến nghị thanh tra - Inspection recommendations.
    
    Theo dõi từng kiến nghị từ quyết định thanh tra.
    """
    __tablename__ = 'kien_nghi_thanh_tra'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    quyet_dinh_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.quyet_dinh_thanh_tra.id'),
        nullable=False
    )
    
    so_kien_nghi: Mapped[str] = mapped_column(String(50))
    ngay_kien_nghi: Mapped[date] = mapped_column(Date)
    
    noi_dung: Mapped[str] = mapped_column(Text, nullable=False)
    muc_do: Mapped[str] = mapped_column(
        String(20),
        comment='nghiem_trong: nghiêm trọng, binh_thuong: bình thường, nhe: nhẹ'
    )
    
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='cho_xu_ly',
        comment='cho_xu_ly, dang_xu_ly, da_xu_ly, khong_xu_duoc'
    )
    
    han_xu_ly: Mapped[date] = mapped_column(Date)
    ngay_xu_ly: Mapped[date] = mapped_column(Date)
    
    chung_tu_dinh_kem: Mapped[str] = mapped_column(Text)
    ghi_chu: Mapped[str] = mapped_column(Text)
    
    quyet_dinh: Mapped['QuyetDinhThanhTra'] = relationship(
        'QuyetDinhThanhTra',
        back_populates='kien_nghi'
    )
    phieu_thu: Mapped[list['PhieuThuThueTruyThu']] = relationship(
        'PhieuThuThueTruyThu',
        back_populates='kien_nghi',
        cascade='all, delete-orphan'
    )


class PhieuThuThueTruyThu(Base, AuditMixin):
    """Phiếu nộp thuế truy thu - Tax penalty payment record.
    
    Lưu chứng từ nộp thuế truy thu, tiền phạt, tiền chậm nộp.
    """
    __tablename__ = 'phieu_thu_thue_truy_thu'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    kien_nghi_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.kien_nghi_thanh_tra.id'),
        nullable=False
    )
    
    so_phieu: Mapped[str] = mapped_column(String(50), unique=True)
    ngay_nop: Mapped[date] = mapped_column(Date, nullable=False)
    
    thue_gtgt: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    thue_tndn: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    thue_tncn: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    thue_khac: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    
    tien_phat: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tien_cham_nop: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    tong_tien: Mapped[Decimal] = mapped_column(Numeric(18, 2), default=0)
    
    chung_tu_goc: Mapped[str] = mapped_column(String(255))
    ghi_chu: Mapped[str] = mapped_column(Text)
    
    kien_nghi: Mapped['KienNghiThanhTra'] = relationship(
        'KienNghiThanhTra',
        back_populates='phieu_thu'
    )


class MauKiemToan(Base, AuditMixin):
    """Mẫu kiểm toán - Audit templates/lead schedules.
    
    Lưu các mẫu báo cáo kiểm toán: lead schedule, reconciliation...
    """
    __tablename__ = 'mau_kiem_toan'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    ma_mau: Mapped[str] = mapped_column(String(50), unique=True, nullable=False)
    ten_mau: Mapped[str] = mapped_column(String(255), nullable=False)
    loai_mau: Mapped[str] = mapped_column(
        String(30),
        nullable=False,
        comment='lead_schedule, reconciliation, roll_forward, bank_recon'
    )
    
    mo_ta: Mapped[str] = mapped_column(Text)
    template: Mapped[dict] = mapped_column(JSON, default={})


class DuLieuKiemToan(Base, AuditMixin):
    """Dữ liệu kiểm toán - Generated audit data.
    
    Lưu dữ liệu kiểm toán đã tạo cho từng kỳ.
    """
    __tablename__ = 'du_lieu_kiem_toan'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    mau_kiem_toan_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.mau_kiem_toan.id'),
        nullable=False
    )
    
    ky_ke_toan_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.ky_ke_toan.id'),
        nullable=False
    )
    
    tai_khoan_id: Mapped[str] = mapped_column(String(20))
    doi_tuong_id: Mapped[int] = mapped_column(ForeignKey('accounting.doi_tuong.id'))
    
    du_lieu: Mapped[dict] = mapped_column(JSON, default={})
    file_dinh_kem: Mapped[str] = mapped_column(String(500))
    
    mau: Mapped['MauKiemToan'] = relationship('MauKiemToan')
    ky_ke_toan: Mapped['KyKeToan'] = relationship('KyKeToan')
