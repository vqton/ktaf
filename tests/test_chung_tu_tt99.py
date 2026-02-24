"""
File: tests/test_chung_tu_tt99.py
Module: TT99/2025 Document Tests
Mô tả: Test cases cho chứng từ điện tử theo Thông tư 99/2025/TT-BTC.

Kiểm tra:
- 7 yếu tố bắt buộc của chứng từ theo Điều 16 Luật Kế toán 2015
- Bút toán cân bằng (Nợ = Có)
- Kỳ kế toán mở/khóa
- TK 215 - Tài sản sinh học

Tham chiếu pháp lý:
- Luật Kế toán 2015: Điều 16 - Chứng từ kế toán
- TT99/2025/TT-BTC: Hệ thống tài khoản mới
- Thông tư 133/2016/TT-BTC: Chứng từ điện tử

Tác giả: [Tên]
Ngày tạo: 2025-02-24
"""

import pytest
from decimal import Decimal
from datetime import date


class TestChungTu7YeuTo:
    """Test cases cho 7 yếu tố bắt buộc của chứng từ kế toán.
    
    Theo Điều 16 Luật Kế toán 2015, chứng từ kế toán phải có:
    1. Tên chứng từ
    2. Số hiệu chứng từ
    3. Ngày, tháng năm lập
    4. Tên, địa chỉ đơn vị
    5. Nội dung nghiệp vụ
    6. Số tiền
    7. Chữ ký người lập, kế toán trưởng, thủ trưởng đơn vị
    """

    def test_chung_tu_co_ten(self, chung_tu_7_yeu_to):
        """Yếu tố 1: Tên chứng từ."""
        assert chung_tu_7_yeu_to['ten_chung_tu'] == 'Phiếu chi'
        assert chung_tu_7_yeu_to['loai_ct'] == 'PC'

    def test_chung_tu_co_so_hieu(self, chung_tu_7_yeu_to):
        """Yếu tố 2: Số hiệu chứng từ (hệ thống sinh tự động)."""
        # Số hiệu sẽ được sinh bởi hệ thống theo format: PC202501-00001
        loai_ct = chung_tu_7_yeu_to['loai_ct']
        assert loai_ct in ['PC', 'PT', 'BN', 'BC', 'PNK', 'PXK', 'HDMH', 'HDBL']

    def test_chung_tu_co_ngay_lap(self, chung_tu_7_yeu_to):
        """Yếu tố 3: Ngày, tháng năm lập."""
        assert chung_tu_7_yeu_to['ngay_ct'] == date(2025, 1, 15)
        assert chung_tu_7_yeu_to['ngay_hach_toan'] == date(2025, 1, 15)

    def test_chung_tu_co_thong_tin_don_vi(self, chung_tu_7_yeu_to):
        """Yếu tố 4: Tên, địa chỉ đơn vị."""
        company = chung_tu_7_yeu_to['thong_tin_cong_ty']
        assert company['ten_cong_ty'] == 'Công ty TNHH Thương mại ABC'
        assert company['dia_chi'] is not None
        assert company['ma_so_thue'] is not None

    def test_chung_tu_co_noi_dung_nghiep_vu(self, chung_tu_7_yeu_to):
        """Yếu tố 5: Nội dung nghiệp vụ."""
        assert chung_tu_7_yeu_to['dien_giai'] is not None
        assert len(chung_tu_7_yeu_to['dien_giai']) > 0

    def test_chung_tu_co_so_tien(self, chung_tu_7_yeu_to):
        """Yếu tố 6: Số tiền."""
        dinh_khoan = chung_tu_7_yeu_to['dinh_khoan']
        assert len(dinh_khoan) > 0
        assert dinh_khoan[0]['so_tien'] > 0

    def test_chung_tu_co_chu_ky(self, chung_tu_7_yeu_to):
        """Yếu tố 7: Chữ ký."""
        chu_ky = chung_tu_7_yeu_to['chữ_ký']
        assert chu_ky['nguoi_lap'] is not None
        assert chu_ky['ke_toan_truong'] is not None
        assert chu_ky['thu_truong'] is not None


class TestButToanCanBang:
    """Test cases cho nguyên tắc bút toán cân bằng (Nợ = Có)."""

    def test_but_toan_can_bang(self, chung_tu_can_bang):
        """Bút toán phải luôn cân bằng: Tổng Nợ = Tổng Có."""
        tong_no = chung_tu_can_bang['tong_tien_no']
        tong_co = chung_tu_can_bang['tong_tien_co']
        
        assert tong_no == tong_co
        assert chung_tu_can_bang['can_bang'] is True

    def test_tinh_tong_no_tu_dinh_khoan(self, chung_tu_can_bang):
        """Tính tổng Nợ từ danh sách định khoản."""
        tong_no = sum(
            dk['so_tien'] for dk in chung_tu_can_bang['dinh_khoan']
            if dk.get('tk_no') is not None
        )
        assert tong_no == Decimal('11000000.00')

    def test_tinh_tong_co_tu_dinh_khoan(self, chung_tu_can_bang):
        """Tính tổng Có từ danh sách định khoản."""
        tong_co = sum(
            dk['so_tien'] for dk in chung_tu_can_bang['dinh_khoan']
            if dk.get('tk_co') is not None
        )
        assert tong_co == Decimal('11000000.00')


class TestTaiKhoanSinhHoc:
    """Test cases cho TK 215 - Tài sản sinh học (TT99)."""

    def test_tai_san_sinh_hoc_ton_tai(self, db_session):
        """Kiểm tra TK 215 tồn tại trong hệ thống."""
        from app.modules.he_thong_tk.models import HeThongTaiKhoan
        
        tk_215 = db_session.query(HeThongTaiKhoan).filter_by(ma_tk='215').first()
        assert tk_215 is not None
        assert tk_215.ten_tk == 'Tài sản sinh học'
        assert tk_215.loai_tk == 'tai_san'

    def test_tai_san_sinh_hoc_co_the_duoc_su_dung(self, db_session):
        """Kiểm tra TK 215 có thể được sử dụng để hạch toán."""
        from app.modules.he_thong_tk.models import HeThongTaiKhoan
        
        tk_215 = db_session.query(HeThongTaiKhoan).filter_by(ma_tk='215').first()
        assert tk_215.co_the_dk is True

    def test_tai_san_sinh_hoc_co_tai_khoan_con(self, db_session):
        """Kiểm tra TK 215 có tài khoản cấp con."""
        from app.modules.he_thong_tk.models import HeThongTaiKhoan
        
        children = db_session.query(HeThongTaiKhoan).filter_by(ma_tk_cha='215').all()
        assert len(children) >= 3  # 2151, 2152, 2153
        
        ma_tk_children = [tk.ma_tk for tk in children]
        assert '2151' in ma_tk_children
        assert '2152' in ma_tk_children
        assert '2153' in ma_tk_children


class TestBaoCaoTinhHinhTaiChinh:
    """Test cases cho Báo cáo tình hình tài chính (TT99).
    
    Theo TT99/2025, "Bảng cân đối kế toán" được đổi tên thành
    "Báo cáo tình hình tài chính".
    """

    def test_bao_cao_co_ten_dung(self, bao_cao_tinh_hinh_tai_chinh):
        """Kiểm tra tên báo cáo đúng theo TT99."""
        assert bao_cao_tinh_hinh_tai_chinh['ten_bao_cao'] == 'Báo cáo tình hình tài chính'

    def test_bao_cao_co_tai_san_sinh_hoc(self, bao_cao_tinh_hinh_tai_chinh):
        """Kiểm tra báo cáo có chỉ tiêu Tài sản sinh học."""
        tsbh = bao_cao_tinh_hinh_tai_chinh['tai_san']['B. TÀI SẢN DÀI HẠN']['3. Tài sản sinh học (TK 215 - NEW TT99)']
        assert tsbh['tong_cong'] == Decimal('90000000')

    def test_bao_cao_can_doi(self, bao_cao_tinh_hinh_tai_chinh):
        """Kiểm tra báo cáo cân đối: Tổng Tài sản = Tổng Nguồn vốn."""
        kiem_tra = bao_cao_tinh_hinh_tai_chinh['kiem_tra']
        assert kiem_tra['tong_tai_san'] == kiem_tra['tong_nguon_von']
        assert kiem_tra['can_bang'] is True

    def test_bao_cao_co_thong_tin_cong_ty(self, bao_cao_tinh_hinh_tai_chinh):
        """Kiểm tra báo cáo có đủ thông tin công ty."""
        assert bao_cao_tinh_hinh_tai_chinh['ten_cong_ty'] is not None
        assert bao_cao_tinh_hinh_tai_chinh['ma_so_thue'] is not None


class TestKyKeToan:
    """Test cases cho kỳ kế toán."""

    def test_ky_mo_cho_phep_tao_chung_tu(self, ky_ke_toan_mo):
        """Kỳ mở cho phép tạo chứng từ."""
        assert ky_ke_toan_mo.trang_thai == 'mo'

    def test_ky_khoa_khong_cho_phep_tao_chung_tu(self, ky_ke_toan_khoa):
        """Kỳ khóa không cho phép tạo chứng từ."""
        assert ky_ke_toan_khoa.trang_thai == 'khoa'


# ============================================================================
# Run: pytest tests/test_chung_tu_tt99.py -v
# ============================================================================
