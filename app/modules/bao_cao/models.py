from datetime import date, datetime
from decimal import Decimal

from sqlalchemy.orm import Mapped, mapped_column, relationship
from sqlalchemy import String, Boolean, Text, Integer, Numeric, Date, DateTime, ForeignKey, JSON
from sqlalchemy.dialects.postgresql import JSONB
from app.models import Base, AuditMixin


class MauBaoCao(AuditMixin, Base):
    __tablename__ = 'mau_bao_cao'
    __table_args__ = {"schema": "accounting"}

    id: Mapped[int] = mapped_column(primary_key=True)
    ma_bc: Mapped[str] = mapped_column(String(20), unique=True, nullable=False)
    ten_bc: Mapped[str] = mapped_column(String(255), nullable=True)
    mo_ta: Mapped[str] = mapped_column(Text, nullable=True)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)


class ChiTietMauBC(Base):
    __tablename__ = 'chi_tiet_mau_bc'
    __table_args__ = {"schema": "accounting"}

    id: Mapped[int] = mapped_column(primary_key=True)
    mau_bc_id: Mapped[int] = mapped_column(nullable=False)
    ma_chi_tieu: Mapped[str] = mapped_column(String(10), nullable=True)
    ten_chi_tieu: Mapped[str] = mapped_column(String(255), nullable=True)
    cong_thuc: Mapped[dict] = mapped_column(JSONB, nullable=True)
    stt_hien_thi: Mapped[int] = mapped_column(nullable=True)
    la_tieu_de: Mapped[bool] = mapped_column(Boolean, default=False)


class B04DNThuyetMinh(AuditMixin, Base):
    """B04-DN: Bản thuyết minh BCTC - Financial Statements Explanations.
    
    Bắt buộc theo TT99/2025. Tự động điền từ số liệu hệ thống.
    """
    __tablename__ = 'b04dn_thuyet_minh'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    ky_ke_toan_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.ky_ke_toan.id'),
        nullable=False
    )
    
    # I. Chính sách kế toán áp dụng
    chinh_sach_ke_toan: Mapped[str] = mapped_column(Text, comment='Chính sách kế toán áp dụng')
    phuong_phap_khau_hao: Mapped[str] = mapped_column(Text, comment='Phương pháp khấu hao TSCĐ')
    phuong_phap_tinh_gia_tri_hang_ton: Mapped[str] = mapped_column(Text, comment='Phương pháp tính giá trị hàng tồn kho')
    ty_gia_hach_toan: Mapped[str] = mapped_column(String(20), comment='Tỷ giá hạch toán')
    
    # II. Chi tiết TSCĐ
    chi_tiet_tscd: Mapped[dict] = mapped_column(JSON, comment='Chi tiết TSCĐ theo từng loại')
    
    # III. Chi tiết công nợ > 1 năm
    chi_tiet_cong_no_1_nam: Mapped[dict] = mapped_column(JSON, comment='Công nợ > 1 năm chi tiết theo đối tượng')
    
    # IV. Chi tiết vốn chủ sở hữu
    chi_tiet_von_csh: Mapped[dict] = mapped_column(JSON, comment='Chi tiết vốn chủ sở hữu')
    
    # V. Các sự kiện sau ngày kết thúc kỳ kế toán
    su_kien_sau_ky: Mapped[str] = mapped_column(Text, comment='Các sự kiện sau ngày kết thúc kỳ kế toán')
    
    # VI. Thông tin bổ sung
    thong_tin_bo_sung: Mapped[dict] = mapped_column(JSON)
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='nhap',
        comment='nhap, da_duyet, da_ky, da_nop'
    )
    
    # Link to ky_ke_toan
    ky_ke_toan: Mapped['KyKeToan'] = relationship('KyKeToan')


class BCTCSignature(Base):
    """Chữ ký BCTC - BCTC Digital Signatures.
    
    Lưu chữ ký của người lập, Kế toán trưởng, Giám đốc.
    """
    __tablename__ = 'bc_tc_signature'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    b04dn_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.b04dn_thuyet_minh.id'),
        nullable=False
    )
    
    # Người ký
    nguoi_lap_id: Mapped[int] = mapped_column(Integer, comment='User ID người lập')
    nguoi_lap_ten: Mapped[str] = mapped_column(String(255))
    nguoi_lap_chuc_vu: Mapped[str] = mapped_column(String(100))
    nguoi_lap_ky: Mapped[str] = mapped_column(Text, comment='Chữ ký số')
    nguoi_lap_ky_ma: Mapped[str] = mapped_column(String(64), comment='Hash chữ ký')
    nguoi_lap_ky_luc: Mapped[datetime] = mapped_column(DateTime)
    
    ke_toan_truong_id: Mapped[int] = mapped_column(Integer)
    ke_toan_truong_ten: Mapped[str] = mapped_column(String(255))
    ke_toan_truong_chuc_vu: Mapped[str] = mapped_column(String(100))
    ke_toan_truong_ky: Mapped[str] = mapped_column(Text)
    ke_toan_truong_ky_ma: Mapped[str] = mapped_column(String(64))
    ke_toan_truong_ky_luc: Mapped[datetime] = mapped_column(DateTime)
    
    giam_doc_id: Mapped[int] = mapped_column(Integer)
    giam_doc_ten: Mapped[str] = mapped_column(String(255))
    giam_doc_chuc_vu: Mapped[str] = mapped_column(String(100))
    giam_doc_ky: Mapped[str] = mapped_column(Text)
    giam_doc_ky_ma: Mapped[str] = mapped_column(String(64))
    giam_doc_ky_luc: Mapped[datetime] = mapped_column(DateTime)
    
    # File BCTC after signing
    file_bc_tc_hash: Mapped[str] = mapped_column(String(64), comment='SHA-256 hash of BCTC file')
    file_signed_at: Mapped[datetime] = mapped_column(DateTime)
    
    created_at: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class BCTCSubmission(Base):
    """Nộp BCTC điện tử - Electronic BCTC Submission.
    
    Lưu thông tin nộp BCTC lên eTax/HTKK.
    """
    __tablename__ = 'bc_tc_submission'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    b04dn_id: Mapped[int] = mapped_column(
        ForeignKey('accounting.b04dn_thuyet_minh.id'),
        nullable=False
    )
    
    # Submission info
    loai_bao_cao: Mapped[str] = mapped_column(
        String(30),
        nullable=False,
        comment='BCTC, QT_TNDN, QT_TNCN'
    )
    
    ky_lap: Mapped[int] = mapped_column(Integer, comment='Kỳ lập (năm)')
    thang_lap: Mapped[int] = mapped_column(Integer, comment='Tháng lập')
    
    # eTax/HTKK response
    ma_tiep_nhan: Mapped[str] = mapped_column(String(50), comment='Mã tiếp nhận từ eTax')
    ngay_nop: Mapped[datetime] = mapped_column(DateTime, comment='Ngày nộp')
    
    # File
    file_xml: Mapped[str] = mapped_column(String(500), comment='File XML đã nộp')
    file_xml_hash: Mapped[str] = mapped_column(String(64), comment='Hash file XML')
    
    # Status
    trang_thai: Mapped[str] = mapped_column(
        String(20),
        default='cho_nop',
        comment='cho_nop, da_nop, loi'
    )
    
    loi_chi_tiet: Mapped[str] = mapped_column(Text)
    
    created_at: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class DeadlineReminder(Base):
    """Deadline nhắc nhở - Compliance Deadline Reminders.
    
    Lưu các deadline nộp BCTC, quyết toán thuế.
    """
    __tablename__ = 'deadline_reminder'
    __table_args__ = {'schema': 'accounting'}

    id: Mapped[int] = mapped_column(primary_key=True)
    
    loai_deadline: Mapped[str] = mapped_column(
        String(30),
        nullable=False,
        comment='BCTC, QT_TNDN, QT_TNCN, bao_cao_thang, bao_cao_quy'
    )
    
    ten_deadline: Mapped[str] = mapped_column(String(255), nullable=False)
    
    # Deadline rules
    ngay_deadline: Mapped[date] = mapped_column(Date, nullable=False)
    loai_don_vi: Mapped[str] = mapped_column(
        String(20),
        comment='tu_nhan, cong_ty, tat_ca'
    )
    
    # Notification
    so_ngay_nhac_truoc: Mapped[int] = mapped_column(
        Integer,
        default=7,
        comment='Số ngày nhắc trước'
    )
    
    # Status
    da_nhac: Mapped[bool] = mapped_column(Boolean, default=False)
    ngay_nhac_gan_nhat: Mapped[datetime] = mapped_column(DateTime)
    
    ghi_chu: Mapped[str] = mapped_column(Text)
    
    created_at: Mapped[datetime] = mapped_column(DateTime, nullable=False)
