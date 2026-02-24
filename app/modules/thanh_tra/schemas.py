"""Schemas for ThanhTra (Inspection & Audit) module."""

from marshmallow import Schema, fields, validate, ValidationError


class QuyetDinhThanhTraSchema(Schema):
    so_quyet_dinh = fields.Str(required=True)
    ngay_quyet_dinh = fields.Date(required=True)
    loai_thanh_tra = fields.Str(required=True, validate=validate.OneOf(['thue', 'kt', 'noi_bo']))
    co_quan_thanh_tra = fields.Str(required=True)
    can_bo_thanh_tra = fields.Str(required=False, allow_none=True)
    tu_ngay = fields.Date(required=True)
    den_ngay = fields.Date(required=True)
    pham_vi = fields.Str(required=False, allow_none=True)
    ghi_chu = fields.Str(required=False, allow_none=True)


class KienNghiThanhTraSchema(Schema):
    quyet_dinh_id = fields.Int(required=True)
    so_kien_nghi = fields.Str(required=False, allow_none=True)
    ngay_kien_nghi = fields.Date(required=False, allow_none=True)
    noi_dung = fields.Str(required=True)
    muc_do = fields.Str(required=False, validate=validate.OneOf(['nghiem_trong', 'binh_thuong', 'nhe']))
    trang_thai = fields.Str(required=False, validate=validate.OneOf(['cho_xu_ly', 'dang_xu_ly', 'da_xu_ly', 'khong_xu_duoc']))
    han_xu_ly = fields.Date(required=False, allow_none=True)
    chung_tu_dinh_kem = fields.Str(required=False, allow_none=True)
    ghi_chu = fields.Str(required=False, allow_none=True)


class PhieuThuThueTruyThuSchema(Schema):
    kien_nghi_id = fields.Int(required=True)
    ngay_nop = fields.Date(required=True)
    thue_gtgt = fields.Decimal(required=False, places=2)
    thue_tndn = fields.Decimal(required=False, places=2)
    thue_tncn = fields.Decimal(required=False, places=2)
    thue_khac = fields.Decimal(required=False, places=2)
    tien_phat = fields.Decimal(required=False, places=2)
    tien_cham_nop = fields.Decimal(required=False, places=2)
    chung_tu_goc = fields.Str(required=False, allow_none=True)
    ghi_chu = fields.Str(required=False, allow_none=True)


class BienBanLamViecSchema(Schema):
    quyet_dinh_id = fields.Int(required=True)
    so_bien_ban = fields.Str(required=True)
    ngay_lap = fields.Date(required=True)
    noi_dung = fields.Str(required=True)
    ket_luan = fields.Str(required=False, allow_none=True)
    dai_dien_co_quan = fields.Str(required=False, allow_none=True)
    dai_dien_doanh_nghiep = fields.Str(required=False, allow_none=True)


quyet_dinh_schema = QuyetDinhThanhTraSchema()
kien_nghi_schema = KienNghiThanhTraSchema()
phieu_thu_schema = PhieuThuThueTruyThuSchema()
bien_ban_schema = BienBanLamViecSchema()
