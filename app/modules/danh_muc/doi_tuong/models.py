from sqlalchemy.orm import Mapped, mapped_column
from sqlalchemy import String, Boolean, CheckConstraint
from sqlalchemy.dialects.postgresql import JSONB
from app.models import Base, AuditMixin


class DoiTuong(AuditMixin, Base):
    __tablename__ = 'doi_tuong'
    __table_args__ = (
        CheckConstraint(
            "loai_dt IN ('khach_hang','nha_cung_cap','ca_nhan','to_chuc')",
            name="ck_doituong_loaidt"
        ),
        {"schema": "accounting"}
    )

    ma_dt: Mapped[str] = mapped_column(String(50), unique=True, nullable=False)
    ten_dt: Mapped[str] = mapped_column(String(255), nullable=False)
    loai_dt: Mapped[str] = mapped_column(String(20), nullable=False)
    dia_chi: Mapped[str] = mapped_column(String(500), nullable=True)
    dien_thoai: Mapped[str] = mapped_column(String(20), nullable=True)
    email: Mapped[str] = mapped_column(String(100), nullable=True)
    ma_so_thue: Mapped[str] = mapped_column(String(20), nullable=True)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)
    metadata: Mapped[dict] = mapped_column(JSONB, default={})
