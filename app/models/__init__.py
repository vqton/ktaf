"""Database models base classes and mixins.

Provides:
- Base: SQLAlchemy declarative base for all models
- AuditMixin: Automatic created_at, updated_at, created_by, updated_by fields
"""

from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column
from sqlalchemy.dialects.postgresql import TIMESTAMPTZ
from sqlalchemy import text
from datetime import datetime
from typing import Optional


class Base(DeclarativeBase):
    """SQLAlchemy declarative base for all database models."""
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
