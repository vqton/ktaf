from sqlalchemy.orm import Mapped, mapped_column
from sqlalchemy import String, Date, SmallInteger, CheckConstraint, UniqueConstraint
from app.models import Base, AuditMixin


class KyKeToan(Base):
    __tablename__ = 'ky_ke_toan'
    __table_args__ = (
        UniqueConstraint("nam", "thang", name="uq_kyketoan_nam_thang"),
        CheckConstraint(
            "trang_thai IN ('mo','khoa')",
            name="ck_kyketoan_trangthai"
        ),
        CheckConstraint(
            "thang BETWEEN 1 AND 12",
            name="ck_kyketoan_thang"
        ),
        {"schema": "accounting"}
    )

    id: Mapped[int] = mapped_column(primary_key=True)
    nam: Mapped[int] = mapped_column(SmallInteger, nullable=False)
    thang: Mapped[int] = mapped_column(SmallInteger, nullable=False)
    tu_ngay: Mapped[Date] = mapped_column(Date, nullable=False)
    den_ngay: Mapped[Date] = mapped_column(Date, nullable=False)
    trang_thai: Mapped[str] = mapped_column(String(10), default='mo')
