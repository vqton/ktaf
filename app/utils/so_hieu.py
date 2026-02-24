"""Document number (Số chứng từ) generation utilities.

Provides functions for:
- Generating unique sequential document numbers
- Parsing document numbers to extract components

Uses pg_advisory_lock for concurrent number generation to prevent duplicates.
Format: [LoạiCT][YYYY][MM]-[00001] e.g., PC202501-00001
"""

from datetime import datetime
from typing import Optional
from app.extensions import db
from sqlalchemy import text


def generate_so_chung_tu(loai_ct: str, connection=None) -> str:
    """Generate unique sequential document number.

    Uses pg_advisory_lock(1) to ensure uniqueness in concurrent scenarios.

    Args:
        loai_ct: Document type prefix (PC, PT, BN, etc.)
        connection: Optional database connection (uses db.session if None)

    Returns:
        Generated document number in format: LOAICTYYYYMM-00001

    Example:
        >>> generate_so_chung_tu('PC')
        'PC202501-00001'
    """
    if connection is None:
        connection = db.session.connection()

    year = datetime.now().year
    month = datetime.now().month

    with connection.execute(text("SELECT pg_advisory_lock(1)")):
        result = connection.execute(
            text("""
                SELECT so_ct FROM accounting.chung_tu 
                WHERE so_ct LIKE :prefix || to_char(NOW(), 'YYYYMM') || '-%'
                ORDER BY so_ct DESC 
                LIMIT 1
            """),
            {"prefix": loai_ct}
        ).fetchone()

        if result:
            last_num = int(result[0].split('-')[1]) + 1
        else:
            last_num = 1

        so_ct = f"{loai_ct}{year:04d}{month:02d}-{last_num:05d}"

        connection.execute(text("SELECT pg_advisory_unlock(1)"))

    return so_ct


def parse_so_chung_tu(so_ct: str) -> dict:
    """Parse document number into components.

    Args:
        so_ct: Document number to parse

    Returns:
        Dict with loai_ct, nam, thang, so_tu

    Raises:
        ValueError: If format is invalid

    Example:
        >>> parse_so_chung_tu('PC202501-00001')
        {'loai_ct': 'PC', 'nam': 2025, 'thang': 1, 'so_tu': 1}
    """
    parts = so_ct.split('-')
    if len(parts) != 2:
        raise ValueError("Invalid so_chung_tu format")

    prefix = parts[0]
    num = int(parts[1])

    return {
        'loai_ct': prefix[:-6],
        'nam': int(prefix[-6:-2]),
        'thang': int(prefix[-2:]),
        'so_tu': num
    }
    if connection is None:
        connection = db.session.connection()

    year = datetime.now().year
    month = datetime.now().month

    with connection.execute(text("SELECT pg_advisory_lock(1)")):
        result = connection.execute(
            text("""
                SELECT so_ct FROM accounting.chung_tu 
                WHERE so_ct LIKE :prefix || to_char(NOW(), 'YYYYMM') || '-%'
                ORDER BY so_ct DESC 
                LIMIT 1
            """),
            {"prefix": loai_ct}
        ).fetchone()

        if result:
            last_num = int(result[0].split('-')[1]) + 1
        else:
            last_num = 1

        so_ct = f"{loai_ct}{year:04d}{month:02d}-{last_num:05d}"

        connection.execute(text("SELECT pg_advisory_unlock(1)"))

    return so_ct


def parse_so_chung_tu(so_ct: str) -> dict:
    parts = so_ct.split('-')
    if len(parts) != 2:
        raise ValueError("Invalid so_chung_tu format")

    prefix = parts[0]
    num = int(parts[1])

    return {
        'loai_ct': prefix[:-6],
        'nam': int(prefix[-6:-2]),
        'thang': int(prefix[-2:]),
        'so_tu': num
    }
