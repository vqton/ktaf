from datetime import datetime
from flask_sqlalchemy import SQLAlchemy
from flask import current_app
from app.extensions import db


class HeThongTaiKhoan(db.Model):
    """
    Hệ thống tài khoản kế toán
    """

    __tablename__ = "he_thong_tai_khoan"

    ma_tk = db.Column(db.String(10), primary_key=True)
    ten_tk = db.Column(db.String(255), nullable=False)
    loai_tk = db.Column(
        db.Enum("tai_san", "nguon_von", "doanh_thu", "chi_phi", "ngoai_bang"),
        nullable=False,
    )
    cap_tk = db.Column(db.Integer, nullable=False)
    ma_tk_cha = db.Column(db.String(10), db.ForeignKey("he_thong_tai_khoan.ma_tk"))
    tinh_chat = db.Column(db.Enum("du", "co", "luong_tinh"), nullable=False)
    co_the_dinh_khoan = db.Column(db.Boolean, default=True)
    is_active = db.Column(db.Boolean, default=True)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    updated_at = db.Column(
        db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow
    )

    # Relationships
    con = db.relationship(
        "HeThongTaiKhoan", backref=db.backref("cha", remote_side=[ma_tk])
    )

    def __repr__(self):
        return f"<HeThongTaiKhoan {self.ma_tk} - {self.ten_tk}>"
