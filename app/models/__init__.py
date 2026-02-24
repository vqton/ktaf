from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column
from sqlalchemy.dialects.postgresql import TIMESTAMPTZ
from sqlalchemy import text
from datetime import datetime
from typing import Optional


class Base(DeclarativeBase):
    pass


class AuditMixin:
    created_at: Mapped[datetime] = mapped_column(
        TIMESTAMPTZ, server_default=text("NOW()"), nullable=False
    )
    updated_at: Mapped[datetime] = mapped_column(
        TIMESTAMPTZ, server_default=text("NOW()"),
        onupdate=text("NOW()"), nullable=False
    )
    created_by: Mapped[Optional[int]] = mapped_column(nullable=True)
    updated_by: Mapped[Optional[int]] = mapped_column(nullable=True)
