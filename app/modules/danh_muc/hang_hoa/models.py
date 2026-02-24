from sqlalchemy.orm import Mapped, mapped_column
from sqlalchemy import String, Boolean, CheckConstraint
from sqlalchemy.dialects.postgresql import NUMERIC, JSONB
from app.models import Base, AuditMixin


class HangHoa(AuditMixin, Base):
    __tablename__ = 'hang_hoa'
    __table_args__ = (
        CheckConstraint(
            "loai_hh IN ('hang_hoa','dich_vu','vat_tu')",
            name="ck_hanghoa_loaihh"
        ),
        {"schema": "accounting"}
    )

    ma_hh: Mapped[str] = mapped_column(String(50), unique=True, nullable=False)
    ten_hh: Mapped[str] = mapped_column(String(255), nullable=False)
    loai_hh: Mapped[str] = mapped_column(String(20), nullable=False)
    don_vi_tinh: Mapped[str] = mapped_column(String(20), nullable=True)
    gia_von: Mapped[float] = mapped_column(NUMERIC(18, 2), nullable=True)
    gia_ban: Mapped[float] = mapped_column(NUMERIC(18, 2), nullable=True)
    ma_tk_hh: Mapped[str] = mapped_column(String(10), nullable=True)
    ma_tk_gv: Mapped[str] = mapped_column(String(10), nullable=True)
    thue_gtgt: Mapped[float] = mapped_column(NUMERIC(5, 2), default=10.0)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)
    metadata: Mapped[dict] = mapped_column(JSONB, default={})
