from datetime import datetime
from flask_sqlalchemy import SQLAlchemy
from flask import current_app
from app.extensions import db
from app.utils.so_hieu import generate_document_number


class ChungTu(db.Model):
    """
    Chứng từ kế toán
    """

    __tablename__ = "chung_tu"

    id = db.Column(db.BigInteger, primary_key=True, autoincrement=True)
    so_ct = db.Column(db.String(30), unique=True, nullable=False)
    loai_ct = db.Column(
        db.Enum(
            "PC", "PT", "BN", "BC", "PNK", "PXK", "HDMH", "HDBL", name="document_types"
        )
    )
    ngay_ct = db.Column(db.Date, nullable=False)
    ngay_hach_toan = db.Column(db.Date)
    dien_giai = db.Column(db.Text)
    doi_tuong_id = db.Column(db.BigInteger, db.ForeignKey("doi_tuong.id"))
    trang_thai = db.Column(db.Enum("nhap", "da_duyet", "da_huy"), default="nhap")
    nguoi_tao = db.Column(db.Integer, db.ForeignKey("nguoi_dung.id"))
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    updated_at = db.Column(
        db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow
    )

    # Relationships
    doi_tuong = db.relationship("DoiTuong", backref=db.backref("chung_tus", lazy=True))

    def __repr__(self):
        return f"<ChungTu {self.so_ct}>"


class DinhKhoan(db.Model):
    """
    Định khoản chi tiết
    """

    __tablename__ = "dinh_khoan"

    id = db.Column(db.BigInteger, primary_key=True, autoincrement=True)
    stt = db.Column(db_SMALLINT, nullable=False)
    tk_no = db.Column(
        db.String(10), db.ForeignKey("he_thong_tai_khoan.ma_tk"), nullable=False
    )
    tk_co = db.Column(db.String(10), db.ForeignKey("he_thong_tai_khoan.ma_tk"))
    so_tien = db.Column(db.DECIMAL(18, 2), nullable=False)
    so_tien_ngoai_te = db.Column(db.DECIMAL(18, 2))
    ma_ngoai_te = db.Column(db.String(3))
    ty_gia = db.Column(db.DECIMAL(10, 4))
    doi_tuong_id = db.Column(db.BigInteger, db.ForeignKey("doi_tuong.id"))
    hang_hoa_id = db.Column(db.BigInteger, db.ForeignKey("hang_hoa.id"))
    dvt = db.Column(db.String(20))
    so_luong = db.Column(db.DECIMAL(15, 4))
    don_gia = db.Column(db.DECIMAL(18, 2))
    dang_gia = db.Column(db.Enum("no", "co", "luong_tinh"))
    dien_giai = db.Column(db.Text)
    chung_tu_id = db.Column(db.BigInteger, db.ForeignKey("chung_tu.id"), nullable=False)

    # Relationships
    tk_no_rel = db.relationship(
        "HeThongTaiKhoan",
        foreign_keys=[tk_no],
        backref=db.backref("no_dinh_khoans", lazy=True),
    )
    tk_co_rel = db.relationship(
        "HeThongTaiKhoan",
        foreign_keys=[tk_co],
        backref=db.backref("co_dinh_khoans", lazy=True),
    )

    def __repr__(self):
        return f"<DinhKhoan {self.stt} - {self.tk_no} {self.tk_co}>"
