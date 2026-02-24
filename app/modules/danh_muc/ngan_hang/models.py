from sqlalchemy.orm import Mapped, mapped_column
from sqlalchemy import String, Boolean
from sqlalchemy.dialects.postgresql import JSONB
from app.models import Base, AuditMixin


class NganHang(AuditMixin, Base):
    __tablename__ = 'ngan_hang'
    __table_args__ = {"schema": "accounting"}

    ma_nh: Mapped[str] = mapped_column(String(20), unique=True, nullable=False)
    ten_nh: Mapped[str] = mapped_column(String(255), nullable=False)
    chi_nhanh: Mapped[str] = mapped_column(String(255), nullable=True)
    dia_chi: Mapped[str] = mapped_column(String(500), nullable=True)
    so_tai_khoan: Mapped[str] = mapped_column(String(50), nullable=True)
    chu_tai_khoan: Mapped[str] = mapped_column(String(255), nullable=True)
    ma_tk_ngan_hang: Mapped[str] = mapped_column(String(10), nullable=True)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)
    metadata: Mapped[dict] = mapped_column(JSONB, default={})
