"""Schemas for HoaDonDienTu (E-Invoice) module."""

from marshmallow import Schema, fields, validate, ValidationError


class HoaDonChiTietSchema(Schema):
    stt = fields.Int(required=True)
    ten_hang = fields.Str(required=True)
    ma_hang = fields.Str(required=False, allow_none=True)
    don_vi_tinh = fields.Str(required=False, allow_none=True)
    so_luong = fields.Decimal(required=False, places=4)
    don_gia = fields.Decimal(required=False, places=2)
    thue_suat = fields.Decimal(required=False, places=2)
    hang_hoa_id = fields.Int(required=False, allow_none=True)


class HoaDonBanRaSchema(Schema):
    mau_so_ky_hieu_id = fields.Int(required=True)
    ngay_lap_hoa_don = fields.Date(required=False)
    ten_mua = fields.Str(required=True)
    ma_so_thue_mua = fields.Str(required=False, allow_none=True)
    dia_chi_mua = fields.Str(required=False, allow_none=True)
    dien_thoai_mua = fields.Str(required=False, allow_none=True)
    email_mua = fields.Str(required=False, allow_none=True)
    nguoi_mua = fields.Str(required=False, allow_none=True)
    hinh_thuc_thanh_toan = fields.Str(required=False, validate=validate.OneOf(['TM', 'CK', 'TM/CK', 'khac']))
    ma_khach_hang = fields.Str(required=False, allow_none=True)
    thue_suat = fields.Decimal(required=False, places=2)
    tong_giam_gia = fields.Decimal(required=False, places=2)
    loai_hoa_don = fields.Str(required=False, validate=validate.OneOf(['ban', 'dich_vu', 'hop_dong', 'khac']))
    ghi_chu = fields.Str(required=False, allow_none=True)
    chi_tiet = fields.List(fields.Nested(HoaDonChiTietSchema), required=True)


class MauSoKyHieuSchema(Schema):
    cau_hinh_id = fields.Int(required=True)
    mau_so = fields.Str(required=True)
    ky_hieu = fields.Str(required=True)
    so_quyet_dinh = fields.Str(required=False, allow_none=True)
    ngay_phat_hanh = fields.Date(required=False)
    ngay_het_han = fields.Date(required=False)
    prefix = fields.Str(required=False)
    suffix = fields.Str(required=False)


class CauHinhHoaDonSchema(Schema):
    ma_so_thue = fields.Str(required=True)
    ten_doanh_nghiep = fields.Str(required=True)
    dia_chi = fields.Str(required=False, allow_none=True)
    cqt_username = fields.Str(required=False, allow_none=True)
    cqt_password = fields.Str(required=False, allow_none=True)
    cqt_api_url = fields.Str(required=False)
    smtp_host = fields.Str(required=False, allow_none=True)
    smtp_port = fields.Int(required=False)
    smtp_username = fields.Str(required=False, allow_none=True)
    smtp_password = fields.Str(required=False, allow_none=True)
    email_from = fields.Str(required=False, allow_none=True)


class HuyHoaDonSchema(Schema):
    ly_do = fields.Str(required=True)


hoa_don_ban_schema = HoaDonBanRaSchema()
mau_so_schema = MauSoKyHieuSchema()
cau_hinh_schema = CauHinhHoaDonSchema()
huy_schema = HuyHoaDonSchema()
