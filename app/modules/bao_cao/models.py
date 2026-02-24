from sqlalchemy.orm import Mapped, mapped_column
from sqlalchemy import String, Boolean, Text
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
