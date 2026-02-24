from sqlalchemy.orm import Mapped, mapped_column, relationship
from sqlalchemy import String, Boolean, SmallInteger, CheckConstraint, Index, text
from sqlalchemy.dialects.postgresql import JSONB
from app.models import Base, AuditMixin


class HeThongTaiKhoan(AuditMixin, Base):
    __tablename__ = 'he_thong_tai_khoan'
    __table_args__ = (
        CheckConstraint(
            "loai_tk IN ('tai_san','nguon_von','doanh_thu','chi_phi','ngoai_bang')",
            name="ck_hethongtk_loaitk"
        ),
        CheckConstraint(
            "tinh_chat IN ('du','co','luong_tinh')",
            name="ck_hethongtk_tinhchat"
        ),
        CheckConstraint(
            "cap_tk BETWEEN 1 AND 4",
            name="ck_hethongtk_captk"
        ),
        {"schema": "accounting"}
    )

    ma_tk: Mapped[str] = mapped_column(String(10), primary_key=True)
    ten_tk: Mapped[str] = mapped_column(String(255), nullable=False)
    loai_tk: Mapped[str] = mapped_column(String(20), nullable=False)
    cap_tk: Mapped[int] = mapped_column(SmallInteger, nullable=False)
    ma_tk_cha: Mapped[str] = mapped_column(String(10), nullable=True)
    tinh_chat: Mapped[str] = mapped_column(String(15), nullable=False)
    co_the_dk: Mapped[bool] = mapped_column(Boolean, default=False)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)
    metadata: Mapped[dict] = mapped_column(JSONB, default={})

    tai_khoan_cha: Mapped["HeThongTaiKhoan"] = relationship(
        "HeThongTaiKhoan",
        remote_side=[ma_tk_cha],
        backref="tai_khoan_con",
        foreign_keys=[ma_tk_cha]
    )
