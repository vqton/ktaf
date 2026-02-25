"""Models for AuditLog module.

Provides models for:
- AuditLog: Immutable audit trail with IP, user agent, digital signature

Features:
- IP address + User Agent tracking
- Log every VIEW of financial reports
- Log every EXPORT of files
- Cannot delete or modify (even by admin)
- Digital signature for integrity
- Store minimum 10 years per Điều 41 Luật Kế toán 2015
"""

from datetime import date, datetime
from decimal import Decimal

from sqlalchemy import String, Numeric, Date, DateTime, Text, Integer, ForeignKey, Boolean
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models import Base


class AuditLog(Base):
    """Audit log - Immutable audit trail.
    
    Lưu mọi thao tác trên hệ thống. KHÔNG CHO PHÉP xóa hoặc sửa.
    """
    __tablename__ = 'audit_log'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    
    # User info
    user_id: Mapped[int] = mapped_column(Integer, nullable=True)
    username: Mapped[str] = mapped_column(String(100))
    
    # Request info
    ip_address: Mapped[str] = mapped_column(String(45), nullable=False, comment='IPv4 or IPv6')
    user_agent: Mapped[str] = mapped_column(String(500))
    request_method: Mapped[str] = mapped_column(String(10))
    request_path: Mapped[str] = mapped_column(String(500))
    request_body: Mapped[str] = mapped_column(Text)
    
    # Action details
    action: Mapped[str] = mapped_column(
        String(50),
        nullable=False,
        comment='login, logout, view_report, export_file, create, update, delete, approve...'
    )
    
    resource_type: Mapped[str] = mapped_column(
        String(50),
        comment='chung_tu, hoa_don, bao_cao, tai_lieu...'
    )
    resource_id: Mapped[str] = mapped_column(String(50))
    
    # Details
    description: Mapped[str] = mapped_column(Text)
    old_value: Mapped[str] = mapped_column(Text)
    new_value: Mapped[str] = mapped_column(Text)
    
    # Export tracking (ai, xuat file gi, luc may gio)
    export_file_name: Mapped[str] = mapped_column(String(255))
    export_file_type: Mapped[str] = mapped_column(String(50))
    
    # Response
    status_code: Mapped[int] = mapped_column(Integer)
    error_message: Mapped[str] = mapped_column(Text)
    
    # Digital signature for integrity
    checksum: Mapped[str] = mapped_column(String(64), comment='SHA-256 hash of log entry')
    digital_signature: Mapped[str] = mapped_column(Text, comment='Digital signature')
    signed_at: Mapped[datetime] = mapped_column(DateTime)
    
    # Timestamp - cannot be modified
    created_at: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    
    # Immutable flag - always False
    is_immutable: Mapped[bool] = mapped_column(Boolean, default=True)


class AuditLogSignature(Base):
    """Digital signature key for audit log integrity.
    
    Lưu khóa ký số để đảm bảo tính toàn vẹn của audit log.
    """
    __tablename__ = 'audit_log_signature'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    
    ten_khoa: Mapped[str] = mapped_column(String(255), nullable=False)
    loai_khoa: Mapped[str] = mapped_column(String(20), comment='HSM, USB_Token, file_pfx')
    
    # Certificate info
    so_serie: Mapped[str] = mapped_column(String(100))
    ngay_bat_dau: Mapped[date] = mapped_column(Date)
    ngay_ket_thuc: Mapped[date] = mapped_column(Date)
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='hoat_dong',
        comment='hoat_dong, het_han, thu_hoi'
    )
    
    created_at: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class CauHinhBaoQuan(Base):
    """Cấu hình lưu trữ tài liệu - Document retention config.
    
    Theo Điều 41 Luật Kế toán 88/2015/QH13.
    """
    __tablename__ = 'cau_hinh_bao_quan'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    
    loai_tai_lieu: Mapped[str] = mapped_column(
        String(50),
        nullable=False,
        unique=True,
        comment='chung_tu, so_sach, bao_cao, tai_lieu_quan_ly'
    )
    
    ten_tai_lieu: Mapped[str] = mapped_column(String(255), nullable=False)
    
    so_nam_luu_tru: Mapped[int] = mapped_column(
        Integer,
        nullable=False,
        comment='Số năm lưu trữ (10, 5...)'
    )
    
    # Warning settings
    so_ngay_canh_bao_truoc_het_han: Mapped[int] = mapped_column(
        Integer,
        default=90,
        comment='Số ngày cảnh báo trước khi hết hạn'
    )
    
    # Can delete?
    cho_phep_xoa: Mapped[bool] = mapped_column(
        Boolean,
        default=False,
        comment='Cho phép xóa vật lý sau khi hết hạn'
    )
    
    ghi_chu: Mapped[str] = mapped_column(Text)


class LichSuBaoQuan(Base):
    """Lịch sử bảo quản tài liệu - Document retention history.
    
    Theo dõi việc lưu trữ và hủy tài liệu theo quy định.
    """
    __tablename__ = 'lich_su_bao_quan'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    
    loai_tai_lieu: Mapped[str] = mapped_column(String(50), nullable=False)
    tai_lieu_id: Mapped[str] = mapped_column(String(50), nullable=False)
    ten_tai_lieu: Mapped[str] = mapped_column(String(255))
    
    # Dates
    ngay_tao: Mapped[date] = mapped_column(Date, nullable=False)
    ngay_het_han_du_kien: Mapped[date] = mapped_column(Date)
    ngay_xu_ly: Mapped[date] = mapped_column(Date)
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='luu_tru',
        comment='luu_tru, canh_bao_sap_het_han, het_han, da_huy'
    )
    
    # Action
    hanh_dong: Mapped[str] = mapped_column(
        String(50),
        comment='tao, gia_han, xoa, huy_theo_quy_trinh'
    )
    
    # Approval for destruction
    so_bien_ban_huy: Mapped[str] = mapped_column(String(50))
    ngay_bien_ban_huy: Mapped[date] = mapped_column(Date)
    nguoi_ky_bien_ban: Mapped[str] = mapped_column(String(255))
    
    ghi_chu: Mapped[str] = mapped_column(Text)
    
    created_at: Mapped[datetime] = mapped_column(DateTime, nullable=False)
