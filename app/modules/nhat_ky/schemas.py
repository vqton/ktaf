"""Marshmallow schemas for NhatKy (Journal) module.

Provides validation schemas for:
- ChungTu: Journal documents
- DinhKhoan: Booking entries
"""

from marshmallow import Schema, fields, validate, ValidationError, post_load


class DinhKhoanSchema(Schema):
    stt = fields.Int(required=True)
    tk_no = fields.Str(required=False, allow_none=True)
    tk_co = fields.Str(required=False, allow_none=True)
    so_tien = fields.Decimal(required=True, places=2, validate=validate.Range(min=0.01))
    so_tien_nt = fields.Decimal(required=False, allow_none=True, places=2)
    ma_nt = fields.Str(required=False, load_default='VND')
    ty_gia = fields.Decimal(required=False, load_default=1.0, places=4)
    doi_tuong_id = fields.Int(required=False, allow_none=True)
    hang_hoa_id = fields.Int(required=False, allow_none=True)
    dvt = fields.Str(required=False, allow_none=True)
    so_luong = fields.Decimal(required=False, allow_none=True, places=4)
    don_gia = fields.Decimal(required=False, allow_none=True, places=2)
    dien_giai = fields.Str(required=False, allow_none=True)


class ChungTuSchema(Schema):
    loai_ct = fields.Str(required=True, validate=validate.OneOf(['PC', 'PT', 'BN', 'BC', 'PNK', 'PXK', 'HDMH', 'HDBL']))
    ngay_ct = fields.Date(required=True)
    ngay_hach_toan = fields.Date(required=True)
    dien_giai = fields.Str(required=False, allow_none=True)
    doi_tuong_id = fields.Int(required=False, allow_none=True)
    dinh_khoan = fields.List(fields.Nested(DinhKhoanSchema), required=True, validate=validate.Length(min=1))


class ChungTuUpdateSchema(Schema):
    ngay_ct = fields.Date(required=False)
    ngay_hach_toan = fields.Date(required=False)
    dien_giai = fields.Str(required=False, allow_none=True)
    doi_tuong_id = fields.Int(required=False, allow_none=True)
    trang_thai = fields.Str(required=False, validate=validate.OneOf(['nhap', 'da_duyet', 'da_huy']))


class ChungTuListSchema(Schema):
    id = fields.Int(dump_only=True)
    so_ct = fields.Str(dump_only=True)
    loai_ct = fields.Str(dump_only=True)
    ngay_ct = fields.Date(dump_only=True)
    ngay_hach_toan = fields.Date(dump_only=True)
    dien_giai = fields.Str(dump_only=True, allow_none=True)
    doi_tuong_id = fields.Int(dump_only=True, allow_none=True)
    trang_thai = fields.Str(dump_only=True)
    tong_tien = fields.Decimal(dump_only=True, places=2)


class DinhKhoanDetailSchema(Schema):
    id = fields.Int(dump_only=True)
    stt = fields.Int(dump_only=True)
    tk_no = fields.Str(dump_only=True, allow_none=True)
    tk_co = fields.Str(dump_only=True, allow_none=True)
    so_tien = fields.Decimal(dump_only=True, places=2)
    so_tien_nt = fields.Decimal(dump_only=True, allow_none=True, places=2)
    ma_nt = fields.Str(dump_only=True)
    ty_gia = fields.Decimal(dump_only=True, places=4)
    doi_tuong_id = fields.Int(dump_only=True, allow_none=True)
    hang_hoa_id = fields.Int(dump_only=True, allow_none=True)
    dvt = fields.Str(dump_only=True, allow_none=True)
    so_luong = fields.Decimal(dump_only=True, allow_none=True, places=4)
    don_gia = fields.Decimal(dump_only=True, allow_none=True, places=2)
    dien_giai = fields.Str(dump_only=True, allow_none=True)


class ChungTuDetailSchema(Schema):
    id = fields.Int(dump_only=True)
    so_ct = fields.Str(dump_only=True)
    loai_ct = fields.Str(dump_only=True)
    ngay_ct = fields.Date(dump_only=True)
    ngay_hach_toan = fields.Date(dump_only=True)
    dien_giai = fields.Str(dump_only=True, allow_none=True)
    doi_tuong_id = fields.Int(dump_only=True, allow_none=True)
    trang_thai = fields.Str(dump_only=True)
    created_at = fields.DateTime(dump_only=True)
    updated_at = fields.DateTime(dump_only=True)
    dinh_khoan = fields.List(fields.Nested(DinhKhoanDetailSchema), dump_only=True)


create_schema = ChungTuSchema()
update_schema = ChungTuUpdateSchema()
detail_schema = ChungTuDetailSchema()
list_schema = ChungTuListSchema()
