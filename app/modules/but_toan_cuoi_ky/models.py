"""Models for ButToanCuoiKy (End-of-Period Accounting) module.

Provides models for:
- DuPhongPhaiThu: Provision for doubtful debts
- DuPhongHangTonKho: Provision for inventory devaluation
- DuPhongDauTu: Provision for financial investment
- ChiPhiTraTruoc: Prepaid expenses
- ChecklistTruocKhoa: Pre-closing checklist
"""

from datetime import date, datetime
from decimal import Decimal

from sqlalchemy import String, Numeric, Date, DateTime, Text, Integer, ForeignKey, Boolean, JSON
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models import Base, AuditMixin


class DuPhongPhaiThu(AuditMixin, Base):
    """Dự phòng phải thu khó đòi - Provision for doubtful debts.
    
    Theo TT48/2019:
    - Nợ quá hạn 6 tháng → 30%
    - Nợ quá hạn 1 năm → 50%
    - Nợ quá hạn 2 năm → 70%
    - Nợ quá hạn 3 năm → 100%
    """
    __tablename__ = 'du_phong_phai_thu'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    ky_ke_toan_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.ky_ke_toan.id'),
        nullable=False
    )
    
    doi_tuong_id: Mapped[int] = mapped_column(ForeignKey('accounting.doi_tuong.id'))
    
    # Aging info
    so_tien_no: Mapped[Decimal] = mapped_column(Numeric(18, 2), nullable=False)
    so_ngay_qua_han: Mapped[int] = mapped_column(Integer, nullable=False)
    ty_le_du_phong: Mapped[Decimal] = mapped_column(Numeric(5, 2), nullable=False)
    
    # Amount
    so_tien_du_phong: Mapped[Decimal] = mapped_column(Numeric(18, 2), nullable=False)
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='nhap',
        comment='nhap, da_duyet, da_hach_toan'
    )
    
    # Link to chung tu
    chung_tu_id: Mapped[int] = mapped_column(ForeignKey('accounting.chung_tu.id'))


class DuPhongHangTonKho(AuditMixin, Base):
    """Dự phòng giảm giá hàng tồn kho - Provision for inventory devaluation.
    
    Theo VAS 02.
    """
    __tablename__ = 'du_phong_hang_ton_kho'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    ky_ke_toan_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.ky_ke_toan.id'),
        nullable=False
    )
    
    hang_hoa_id: Mapped[int] = mapped_column(ForeignKey('accounting.hang_hoa.id'))
    
    # Info
    ten_hang: Mapped[str] = mapped_column(String(255))
    gia_goc: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    gia_tri_thuan_troi: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    chenh_lech: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    
    # Provision amount
    so_tien_du_phong: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    
    trang_thai: Mapped[str] = mapped_column(String(20), default='nhap')


class DuPhongDauTu(Base):
    """Dự phòng đầu tư tài chính - Provision for financial investment.
    
    Theo TT48/2019.
    """
    __tablename__ = 'du_phong_dau_tu'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    ky_ke_toan_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.ky_ke_toan.id'),
        nullable=False
    )
    
    loai_dau_tu: Mapped[str] = mapped_column(String(20), comment='co_phieu, trai_phieu, von_gop')
    ten_co_phieu: Mapped[str] = mapped_column(String(255))
    
    gia_mua: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    gia_tri_thi_truong: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    
    so_tien_du_phong: Mapped[Decimal] = mapped_column(Numeric(18, 2))
    
    trang_thai: Mapped[str] = mapped_column(String(20), default='nhap')


class ChiPhiTraTruoc(Base):
    """Chi phí trả trước - Prepaid expenses (TK 242).
    
    Validate: số kỳ phân bổ còn lại > 0
    """
    __tablename__ = 'chi_phi_tra_truoc'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    ky_ke_toan_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.ky_ke_toan.id'),
        nullable=False
    )
    
    ten_chi_phi: Mapped[str] = mapped_column(String(255), nullable=False)
    so_tien: Mapped[Decimal] = mapped_column(Numeric(18, 2), nullable=False)
    
    # Amortization info
    so_ky_phan_bo: Mapped[int] = mapped_column(Integer, nullable=False)
    so_ky_da_phan_bo: Mapped[int] = mapped_column(Integer, default=0)
    ngay_bat_dau: Mapped[date] = mapped_column(Date, nullable=False)
    
    # Remaining periods (must > 0 to validate)
    so_ky_con_lai: Mapped[int] = mapped_column(Integer)
    
    trang_thai: Mapped[str] = mapped_column(String(20), default='dang_phan_bo')


class ChecklistTruocKhoa(Base):
    """Checklist kiểm tra trước khoá kỳ - Pre-closing checklist.
    
    Checklist bắt buộc trước khi khóa kỳ kế toán.
    """
    __tablename__ = 'checklist_truoc_khoa'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    ky_ke_toan_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.ky_ke_toan.id'),
        nullable=False
    )
    
    # Checklist items - JSON format
    items: Mapped[dict] = mapped_column(JSON, nullable=False)
    
    # Summary
    tong_item: Mapped[int] = mapped_column(Integer, default=0)
    da_kiem_tra: Mapped[int] = mapped_column(Integer, default=0)
    ti_le_hoan_thanh: Mapped[Decimal] = mapped_column(Numeric(5, 2), default=0)
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='chua_hoan_thanh',
        comment='chua_hoan_thanh, dang_kiem_tra, da_hoan_thanh'
    )
    
    nguoi_kiem_tra_id: Mapped[int] = mapped_column(Integer)
    ngay_kiem_tra: Mapped[datetime] = mapped_column(DateTime)
    
    created_at: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    updated_at: Mapped[datetime] = mapped_column(DateTime, nullable=False)
