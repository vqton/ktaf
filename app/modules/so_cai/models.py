from sqlalchemy.orm import Mapped, mapped_column
from sqlalchemy import String, ForeignKey, UniqueConstraint
from sqlalchemy.dialects.postgresql import NUMERIC
from app.models import Base


class SoCai(Base):
    __tablename__ = 'so_cai'
    __table_args__ = (
        UniqueConstraint("ma_tk", "ky_ke_toan_id", name="uq_socai_ma_tk_ky"),
        {"schema": "accounting"}
    )

    id: Mapped[int] = mapped_column(primary_key=True)
    ma_tk: Mapped[str] = mapped_column(String(10), nullable=False)
    ky_ke_toan_id: Mapped[int] = mapped_column(nullable=False)
    so_du_dau: Mapped[float] = mapped_column(NUMERIC(18, 2), default=0)
    phat_sinh_no: Mapped[float] = mapped_column(NUMERIC(18, 2), default=0)
    phat_sinh_co: Mapped[float] = mapped_column(NUMERIC(18, 2), default=0)


class SoDuDauKy(Base):
    __tablename__ = 'so_du_dau_ky'
    __table_args__ = (
        UniqueConstraint("ma_tk", "doi_tuong_id", "nam_tai_chinh", name="uq_sodudauky_ma_dt_nam"),
        {"schema": "accounting"}
    )

    id: Mapped[int] = mapped_column(primary_key=True)
    ma_tk: Mapped[str] = mapped_column(String(10), nullable=False)
    doi_tuong_id: Mapped[int] = mapped_column(nullable=True)
    nam_tai_chinh: Mapped[int] = mapped_column(nullable=False)
    so_du_no: Mapped[float] = mapped_column(NUMERIC(18, 2), default=0)
    so_du_co: Mapped[float] = mapped_column(NUMERIC(18, 2), default=0)
    so_du_no_nt: Mapped[float] = mapped_column(NUMERIC(18, 2), default=0)
    so_du_co_nt: Mapped[float] = mapped_column(NUMERIC(18, 2), default=0)
