"""Journal entry (Nhật ký) models.

Provides:
- ChungTu: Journal document with header information
- DinhKhoan: Individual debit/credit line items

Document types (loai_ct): PC (Phiếu chi), PT (Phiếu thu), BN (Báo nợ),
BC (Báo có), PNK (Phiếu nhập kho), PXK (Phiếu xuất kho),
HDMH (Hóa đơn mua hàng), HDBL (Hóa đơn bán lẻ)

Status (trang_thai): nhap, da_duyet, da_huy
"""

from sqlalchemy.orm import Mapped, mapped_column, relationship
from sqlalchemy import String, Date, Text, ForeignKey, CheckConstraint, Index, UniqueConstraint
from sqlalchemy.dialects.postgresql import NUMERIC, BIGINT
from datetime import datetime
from typing import Optional, List
from app.models import Base, AuditMixin


class ChungTu(AuditMixin, Base):
    """Journal document (Chứng từ) model.

    Represents a single accounting document with multiple booking entries.
    """

    __tablename__ = 'chung_tu'
    __table_args__ = (
        CheckConstraint(
            "loai_ct IN ('PC','PT','BN','BC','PNK','PXK','HDMH','HDBL')",
            name="ck_chungtu_loaict"
        ),
        CheckConstraint(
            "trang_thai IN ('nhap','da_duyet','da_huy')",
            name="ck_chungtu_trangthai"
        ),
        Index("idx_chung_tu_ngay", "ngay_hach_toan"),
        Index("idx_chung_tu_doi_tuong", "doi_tuong_id"),
        Index("idx_chung_tu_nhap", "trang_thai"),
        {"schema": "accounting"}
    )

    id: Mapped[int] = mapped_column(BIGINT, primary_key=True)
    so_ct: Mapped[str] = mapped_column(String(30), unique=True, nullable=False)
    loai_ct: Mapped[str] = mapped_column(String(10), nullable=False)
    ngay_ct: Mapped[datetime] = mapped_column(Date, nullable=False)
    ngay_hach_toan: Mapped[datetime] = mapped_column(Date, nullable=False)
    dien_giai: Mapped[Optional[str]] = mapped_column(Text, nullable=True)
    doi_tuong_id: Mapped[Optional[int]] = mapped_column(BIGINT, nullable=True)
    trang_thai: Mapped[str] = mapped_column(String(10), default='nhap')

    dinh_khoan: Mapped[List["DinhKhoan"]] = relationship(
        back_populates="chung_tu",
        cascade="all, delete-orphan",
        order_by="DinhKhoan.stt"
    )


class DinhKhoan(Base):
    """Journal entry line item (Định khoản) model.

    Represents a single debit/credit line within a ChungTu.
    """

    __tablename__ = 'dinh_khoan'
    __table_args__ = (
        UniqueConstraint("chung_tu_id", "stt", name="uq_dinhkhoan_ct_stt"),
        Index("idx_dinhkhoan_tk", "tk_no", "tk_co"),
        {"schema": "accounting"}
    )

    id: Mapped[int] = mapped_column(BIGINT, primary_key=True)
    chung_tu_id: Mapped[int] = mapped_column(
        BIGINT,
        ForeignKey("accounting.chung_tu.id", ondelete="CASCADE"),
        nullable=False
    )
    stt: Mapped[int] = mapped_column(nullable=False)
    tk_no: Mapped[Optional[str]] = mapped_column(String(10), nullable=True)
    tk_co: Mapped[Optional[str]] = mapped_column(String(10), nullable=True)
    so_tien: Mapped[float] = mapped_column(NUMERIC(18, 2), nullable=False)
    so_tien_nt: Mapped[Optional[float]] = mapped_column(NUMERIC(18, 2), nullable=True)
    ma_nt: Mapped[str] = mapped_column(String(3), default='VND')
    ty_gia: Mapped[float] = mapped_column(NUMERIC(10, 4), default=1.0)
    doi_tuong_id: Mapped[Optional[int]] = mapped_column(BIGINT, nullable=True)
    hang_hoa_id: Mapped[Optional[int]] = mapped_column(BIGINT, nullable=True)
    dvt: Mapped[Optional[str]] = mapped_column(String(20), nullable=True)
    so_luong: Mapped[Optional[float]] = mapped_column(NUMERIC(15, 4), nullable=True)
    don_gia: Mapped[Optional[float]] = mapped_column(NUMERIC(18, 2), nullable=True)
    dien_giai: Mapped[Optional[str]] = mapped_column(Text, nullable=True)

    chung_tu: Mapped["ChungTu"] = relationship(back_populates="dinh_khoan")
    __tablename__ = 'chung_tu'
    __table_args__ = (
        CheckConstraint(
            "loai_ct IN ('PC','PT','BN','BC','PNK','PXK','HDMH','HDBL')",
            name="ck_chungtu_loaict"
        ),
        CheckConstraint(
            "trang_thai IN ('nhap','da_duyet','da_huy')",
            name="ck_chungtu_trangthai"
        ),
        Index("idx_chung_tu_ngay", "ngay_hach_toan"),
        Index("idx_chung_tu_doi_tuong", "doi_tuong_id"),
        Index("idx_chung_tu_nhap", "trang_thai"),
        {"schema": "accounting"}
    )

    id: Mapped[int] = mapped_column(BIGINT, primary_key=True)
    so_ct: Mapped[str] = mapped_column(String(30), unique=True, nullable=False)
    loai_ct: Mapped[str] = mapped_column(String(10), nullable=False)
    ngay_ct: Mapped[datetime] = mapped_column(Date, nullable=False)
    ngay_hach_toan: Mapped[datetime] = mapped_column(Date, nullable=False)
    dien_giai: Mapped[Optional[str]] = mapped_column(Text, nullable=True)
    doi_tuong_id: Mapped[Optional[int]] = mapped_column(BIGINT, nullable=True)
    trang_thai: Mapped[str] = mapped_column(String(10), default='nhap')

    dinh_khoan: Mapped[List["DinhKhoan"]] = relationship(
        back_populates="chung_tu",
        cascade="all, delete-orphan",
        order_by="DinhKhoan.stt"
    )


class DinhKhoan(Base):
    __tablename__ = 'dinh_khoan'
    __table_args__ = (
        UniqueConstraint("chung_tu_id", "stt", name="uq_dinhkhoan_ct_stt"),
        Index("idx_dinhkhoan_tk", "tk_no", "tk_co"),
        {"schema": "accounting"}
    )

    id: Mapped[int] = mapped_column(BIGINT, primary_key=True)
    chung_tu_id: Mapped[int] = mapped_column(
        BIGINT,
        ForeignKey("accounting.chung_tu.id", ondelete="CASCADE"),
        nullable=False
    )
    stt: Mapped[int] = mapped_column(nullable=False)
    tk_no: Mapped[Optional[str]] = mapped_column(String(10), nullable=True)
    tk_co: Mapped[Optional[str]] = mapped_column(String(10), nullable=True)
    so_tien: Mapped[float] = mapped_column(NUMERIC(18, 2), nullable=False)
    so_tien_nt: Mapped[Optional[float]] = mapped_column(NUMERIC(18, 2), nullable=True)
    ma_nt: Mapped[str] = mapped_column(String(3), default='VND')
    ty_gia: Mapped[float] = mapped_column(NUMERIC(10, 4), default=1.0)
    doi_tuong_id: Mapped[Optional[int]] = mapped_column(BIGINT, nullable=True)
    hang_hoa_id: Mapped[Optional[int]] = mapped_column(BIGINT, nullable=True)
    dvt: Mapped[Optional[str]] = mapped_column(String(20), nullable=True)
    so_luong: Mapped[Optional[float]] = mapped_column(NUMERIC(15, 4), nullable=True)
    don_gia: Mapped[Optional[float]] = mapped_column(NUMERIC(18, 2), nullable=True)
    dien_giai: Mapped[Optional[str]] = mapped_column(Text, nullable=True)

    chung_tu: Mapped["ChungTu"] = relationship(back_populates="dinh_khoan")
