"""
File: tests/conftest.py
Module: Pytest Configuration - TT99/2025 Test Base
Mô tả: Fixtures và configuration cho pytest kiểm thử Thông tư 99/2025/TT-BTC.

Fixtures cung cấp:
- Mock database với hệ thống tài khoản TT99 (bao gồm TK 215 - Tài sản sinh học)
- Fixtures kiểm tra chứng từ điện tử (7 yếu tố bắt buộc theo Luật Kế toán)
- Fixture cho Báo cáo tình hình tài chính (Financial Situation Report)

Tham chiếu pháp lý:
- Luật Kế toán 2015: Điều 16 - Chứng từ kế toán (7 yếu tố)
- TT99/2025/TT-BTC: Hệ thống tài khoản mới, Báo cáo tài chính mới

Tác giả: [Tên]
Ngày tạo: 2025-01-xx
Cập nhật: 2025-02-24 — Thêm TK 215 và fixtures báo cáo
"""

import os
import pytest
from datetime import date, datetime
from decimal import Decimal

from app import create_app
from app.extensions import db
from app.modules.he_thong_tk.models import HeThongTaiKhoan
from app.modules.nhat_ky.models import ChungTu, DinhKhoan
from app.modules.ky_ke_toan.models import KyKeToan


# =============================================================================
# TT99 CHART OF ACCOUNTS - Mock Data for Testing
# =============================================================================

TT99_ACCOUNTS_TEST = [
    # CLASS 1: TÀI SẢN (Assets) - Loại tài sản
    {'ma_tk': '11', 'ten_tk': 'Tiền', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '111', 'ten_tk': 'Tiền mặt', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '11', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1111', 'ten_tk': 'Tiền Việt Nam', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '111', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '112', 'ten_tk': 'Tiền gửi ngân hàng', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '11', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1121', 'ten_tk': 'Tiền Việt Nam', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '112', 'tinh_chat': 'du', 'co_the_dk': True},
    
    {'ma_tk': '13', 'ten_tk': 'Phải thu của khách hàng', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '131', 'ten_tk': 'Phải thu của khách hàng', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '13', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '133', 'ten_tk': 'Thuế GTGT được khấu trừ', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '13', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1331', 'ten_tk': 'Thuế GTGT được khấu trừ', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '133', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '15', 'ten_tk': 'Hàng tồn kho', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '151', 'ten_tk': 'Mua hàng', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '152', 'ten_tk': 'Nguyên vật liệu', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '156', 'ten_tk': 'Hàng hóa', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '16', 'ten_tk': 'Tài sản cố định', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '211', 'ten_tk': 'Tài sản cố định hữu hình', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '16', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '214', 'ten_tk': 'Hao mòn tài sản cố định', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '16', 'tinh_chat': 'co', 'co_the_dk': True},

    # TK 215 - TÀI SẢN SINH HỌC (Biological Assets) - NEW in TT99
    {'ma_tk': '215', 'ten_tk': 'Tài sản sinh học', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '2151', 'ten_tk': 'Tài sản sinh học tăng trưởng', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '215', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '2152', 'ten_tk': 'Tài sản sinh học giảm trừ', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '215', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '2153', 'ten_tk': 'Chênh lệch đánh giá TSBĐ', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '215', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '18', 'ten_tk': 'Phải thu khác', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '138', 'ten_tk': 'Phải thu khác', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '18', 'tinh_chat': 'du', 'co_the_dk': True},

    # CLASS 2: NGUỒN VỐN (Liabilities + Equity)
    {'ma_tk': '31', 'ten_tk': 'Phải trả người bán', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '331', 'ten_tk': 'Phải trả người bán', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '31', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '33', 'ten_tk': 'Thuế và các khoản phải nộp', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '333', 'ten_tk': 'Thuế và các khoản phải nộp', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '33', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3331', 'ten_tk': 'Thuế GTGT đầu ra', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '333', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3332', 'ten_tk': 'Thuế tiêu thụ đặc biệt', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '333', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '34', 'ten_tk': 'Phải trả người lao động', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '334', 'ten_tk': 'Phải trả người lao động', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '34', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '35', 'ten_tk': 'Chi phí phải trả', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '335', 'ten_tk': 'Chi phí phải trả', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '35', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '36', 'ten_tk': 'Phải trả khác', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '338', 'ten_tk': 'Phải trả khác', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '36', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '37', 'ten_tk': 'Vay và nợ thuê tài chính', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '341', 'ten_tk': 'Vay ngắn hạn', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '37', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '342', 'ten_tk': 'Vay dài hạn', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '37', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '41', 'ten_tk': 'Vốn chủ sở hữu', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '411', 'ten_tk': 'Vốn chủ sở hữu', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '41', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '4111', 'ten_tk': 'Vốn đầu tư của chủ sở hữu', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '411', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '412', 'ten_tk': 'Lợi nhuận sau thuế chưa phân phối', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '41', 'tinh_chat': 'co', 'co_the_dk': True},

    # CLASS 5: DOANH THU (Revenue)
    {'ma_tk': '51', 'ten_tk': 'Doanh thu hoạt động kinh doanh', 'loai_tk': 'doanh_thu', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '511', 'ten_tk': 'Doanh thu bán hàng và cung cấp dịch vụ', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '51', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '5111', 'ten_tk': 'Doanh thu bán hàng', 'loai_tk': 'doanh_thu', 'cap_tk': 3, 'ma_tk_cha': '511', 'tinh_chat': 'co', 'co_the_dk': True},

    # CLASS 6: CHI PHÍ (Expenses)
    {'ma_tk': '61', 'ten_tk': 'Giá vốn hàng bán', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '611', 'ten_tk': 'Giá vốn hàng bán', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '61', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '62', 'ten_tk': 'Chi phí tài chính', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '632', 'ten_tk': 'Chi phí tài chính', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '62', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '64', 'ten_tk': 'Chi phí bán hàng', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '641', 'ten_tk': 'Chi phí bán hàng', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '64', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '65', 'ten_tk': 'Chi phí quản lý doanh nghiệp', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '642', 'ten_tk': 'Chi phí quản lý doanh nghiệp', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '65', 'tinh_chat': 'du', 'co_the_dk': True},
]


# =============================================================================
# FIXTURES: Flask App & Database
# =============================================================================

@pytest.fixture(scope='session')
def app():
    """Tạo Flask app cho testing với TT99."""
    os.environ['FLASK_ENV'] = 'testing'
    app = create_app('testing')
    app.config['TESTING'] = True
    app.config['SQLALCHEMY_DATABASE_URI'] = os.environ.get(
        'TEST_DATABASE_URL',
        'postgresql+psycopg2://postgres:123456@127.0.0.1:15432/gl_tt99_test'
    )
    app.config['JWT_SECRET_KEY'] = 'test-secret-key'
    return app


@pytest.fixture(scope='function')
def client(app):
    """Tạo test client."""
    return app.test_client()


@pytest.fixture(scope='function')
def db_session(app):
    """Tạo database session cho mỗi test với TT99 accounts."""
    with app.app_context():
        # Import all models to ensure they're registered with SQLAlchemy
        from app.models import Base
        from app.modules.he_thong_tk.models import HeThongTaiKhoan
        from app.modules.nhat_ky.models import ChungTu, DinhKhoan
        from app.modules.ky_ke_toan.models import KyKeToan
        from app.modules.danh_muc.doi_tuong.models import DoiTuong
        from app.modules.danh_muc.hang_hoa.models import HangHoa
        from app.modules.danh_muc.ngan_hang.models import NganHang
        
        # Check if tables exist, if not create them
        db.create_all()
        
        yield db.session


@pytest.fixture(scope='function')
def authenticated_client(client, app, db_session):
    """Tạo authenticated test client với JWT token."""
    from app.modules.auth.models import User
    
    user = User(
        username='testuser',
        email='test@example.com',
        is_active=True
    )
    user.set_password('testpass123')
    db_session.add(user)
    db_session.commit()

    import jwt as pyjwt
    token = pyjwt.encode(
        {'user_id': user.id},
        app.config['JWT_SECRET_KEY'],
        algorithm='HS256'
    )

    client.headers.update({'Authorization': f'Bearer {token}'})
    return client


# =============================================================================
# FIXTURES: Accounting Period (Kỳ kế toán)
# =============================================================================

@pytest.fixture
def ky_ke_toan_mo(db_session):
    """Tạo kỳ kế toán mở (đang hoạt động)."""
    existing = db_session.query(KyKeToan).filter_by(nam=2025, thang=1).first()
    if existing:
        return existing
    ky = KyKeToan(
        nam=2025,
        thang=1,
        tu_ngay=date(2025, 1, 1),
        den_ngay=date(2025, 1, 31),
        trang_thai='mo'
    )
    db_session.add(ky)
    db_session.commit()
    return ky


@pytest.fixture
def ky_ke_toan_khoa(db_session):
    """Tạo kỳ kế toán đã khóa."""
    existing = db_session.query(KyKeToan).filter_by(nam=2024, thang=12).first()
    if existing:
        return existing
    ky = KyKeToan(
        nam=2024,
        thang=12,
        tu_ngay=date(2024, 12, 1),
        den_ngay=date(2024, 12, 31),
        trang_thai='khoa'
    )
    db_session.add(ky)
    db_session.commit()
    return ky


# =============================================================================
# FIXTURES: Electronic Document Elements (7 yếu tố chứng từ)
# =============================================================================

@pytest.fixture
def company_info():
    """Thông tin công ty - Yếu tố 4 của chứng từ (Điều 16 Luật Kế toán)."""
    return {
        'ten_cong_ty': 'Công ty TNHH Thương mại ABC',
        'dia_chi': '123 Đường Nguyễn Trãi, Quận 1, TP.HCM',
        'ma_so_thue': '0312345678',
        'dien_thoai': '028 1234 5678'
    }


@pytest.fixture
def chung_tu_7_yeu_to(company_info, ky_ke_toan_mo):
    """Fixture tạo chứng từ điện tử đảm bảo 7 yếu tố bắt buộc.
    
    Theo Điều 16 Luật Kế toán 2015, chứng từ kế toán phải có:
    1. Tên chứng từ (Document name)
    2. Số hiệu chứng từ (Document number)
    3. Ngày, tháng năm lập (Date of preparation)
    4. Tên, địa chỉ đơn vị (Unit name, address)
    5. Nội dung nghiệp vụ (Content of transaction)
    6. Số tiền (Amount)
    7. Chữ ký người lập, kế toán trưởng, thủ trưởng đơn vị (Signatures)
    
    Theo TT99/2025:
    - Chứng từ điện tử phải đảm bảo tính toàn vẹn, xác thực
    - Có mã số chứng từ, thời điểm lập, ký số
    """
    return {
        # Yếu tố 1: Tên chứng từ
        'ten_chung_tu': 'Phiếu chi',
        
        # Yếu tố 2: Số hiệu chứng từ (sẽ được hệ thống sinh tự động)
        'loai_ct': 'PC',
        
        # Yếu tố 3: Ngày, tháng năm lập
        'ngay_ct': date(2025, 1, 15),
        'ngay_hach_toan': date(2025, 1, 15),
        
        # Yếu tố 4: Tên, địa chỉ đơn vị
        'thong_tin_cong_ty': company_info,
        
        # Yếu tố 5: Nội dung nghiệp vụ
        'dien_giai': 'Chi tiền mua vật liệu văn phòng',
        
        # Yếu tố 6: Số tiền
        'dinh_khoan': [
            {
                'stt': 1,
                'tk_no': '6411',  # Chi phí văn phòng
                'tk_co': '1111',  # Tiền mặt VND
                'so_tien': Decimal('5000000.00'),
                'ma_nt': 'VND',
                'ty_gia': Decimal('1.0'),
                'dien_giai': 'Mua giấy, bút, văn phòng phẩm'
            }
        ],
        
        # Yếu tố 7: Chữ ký (trong hệ thống điện tử sẽ là chữ ký số)
        # Lưu ý: Trong test, chúng ta giả định đã có chữ ký
        'chữ_ký': {
            'nguoi_lap': 'Nguyễn Văn A',
            'ke_toan_truong': 'Trần Thị B',
            'thu_truong': 'Lê Văn C',
            'ngay_ky': date(2025, 1, 15),
            'chữ_ký_số': 'SIGNED_HASH_ABC123'
        }
    }


@pytest.fixture
def chung_tu_can_bang(company_info, ky_ke_toan_mo):
    """Tạo chứng từ có bút toán cân bằng (Nợ = Có).
    
    Theo nguyên tắc kế toán, mọi nghiệp vụ phải có:
    - Tổng số tiền Nợ = Tổng số tiền Có
    """
    return {
        'loai_ct': 'PC',
        'ngay_ct': date(2025, 1, 20),
        'ngay_hach_toan': date(2025, 1, 20),
        'dien_giai': 'Thu tiền bán hàng từ khách hàng X',
        'thong_tin_cong_ty': company_info,
        'dinh_khoan': [
            {
                'stt': 1,
                'tk_no': '1121',  # Tiền gửi ngân hàng
                'tk_co': None,
                'so_tien': Decimal('11000000.00'),
                'ma_nt': 'VND',
                'ty_gia': Decimal('1.0'),
                'dien_giai': 'Thu tiền'
            },
            {
                'stt': 2,
                'tk_no': None,
                'tk_co': '5111',  # Doanh thu bán hàng
                'so_tien': Decimal('10000000.00'),
                'ma_nt': 'VND',
                'ty_gia': Decimal('1.0'),
                'dien_giai': 'Doanh thu chưa VAT'
            },
            {
                'stt': 3,
                'tk_no': None,
                'tk_co': '3331',  # Thuế GTGT đầu ra
                'so_tien': Decimal('1000000.00'),
                'ma_nt': 'VND',
                'ty_gia': Decimal('1.0'),
                'dien_giai': 'VAT 10%'
            }
        ],
        'tong_tien_no': Decimal('11000000.00'),
        'tong_tien_co': Decimal('11000000.00'),
        'can_bang': True
    }


# =============================================================================
# FIXTURE: Financial Situation Report (Báo cáo tình hình tài chính)
# =============================================================================

@pytest.fixture
def bao_cao_tinh_hinh_tai_chinh(company_info):
    """Fixture tạo Báo cáo tình hình tài chính theo TT99.
    
    Theo TT99/2025/TT-BTC, "Bảng cân đối kế toán" được đổi tên thành
    "Báo cáo tình hình tài chính" (Financial Situation Report).
    
    Cấu trúc báo cáo:
    - Tài sản (Assets)
    - Nguồn vốn (Liabilities + Equity)
    
    Điểm mới trong TT99:
    - Thêm TK 215 - Tài sản sinh học
    - Trình bày theo tính thanh khoản tăng dần hoặc giảm dần
    """
    return {
        'ten_bao_cao': 'Báo cáo tình hình tài chính',
        'ten_cong_ty': company_info['ten_cong_ty'],
        'dia_chi': company_info['dia_chi'],
        'ma_so_thue': company_info['ma_so_thue'],
        
        'ky_ke_toan': {
            'nam': 2025,
            'thang': 1,
            'tu_ngay': date(2025, 1, 1),
            'den_ngay': date(2025, 1, 31)
        },
        
        'don_vi_tinh': 'VND',
        
        'tai_san': {
            'A. TÀI SẢN NGẮN HẠN': {
                '1. Tiền và tương đương tiền': {
                    '111': Decimal('500000000'),
                    '112': Decimal('800000000'),
                    'tong_cong': Decimal('1300000000')
                },
                '2. Đầu tư tài chính ngắn hạn': {
                    '121': Decimal('200000000'),
                    'tong_cong': Decimal('200000000')
                },
                '3. Phải thu ngắn hạn': {
                    '131': Decimal('300000000'),
                    '136': Decimal('50000000'),
                    'tong_cong': Decimal('350000000')
                },
                '4. Hàng tồn kho': {
                    '151': Decimal('100000000'),
                    '152': Decimal('150000000'),
                    '156': Decimal('250000000'),
                    'tong_cong': Decimal('500000000')
                },
                '5. Tài sản ngắn hạn khác': {
                    '138': Decimal('30000000'),
                    'tong_cong': Decimal('30000000')
                },
                'Tổng cộng A': Decimal('2380000000')
            },
            
            'B. TÀI SẢN DÀI HẠN': {
                '1. Phải thu dài hạn': {
                    '131': Decimal('0'),
                    'tong_cong': Decimal('0')
                },
                '2. Tài sản cố định': {
                    '211': Decimal('1500000000'),
                    '214': Decimal('-200000000'),
                    'tong_cong': Decimal('1300000000')
                },
                '3. Tài sản sinh học (TK 215 - NEW TT99)': {
                    '2151': Decimal('100000000'),
                    '2152': Decimal('-10000000'),
                    '2153': Decimal('0'),
                    'tong_cong': Decimal('90000000')
                },
                '4. Bất động sản đầu tư': {
                    '217': Decimal('0'),
                    'tong_cong': Decimal('0')
                },
                '5. Đầu tư tài chính dài hạn': {
                    '228': Decimal('100000000'),
                    'tong_cong': Decimal('100000000')
                },
                '6. Tài sản dài hạn khác': {
                    '241': Decimal('50000000'),
                    'tong_cong': Decimal('50000000')
                },
                'Tổng cộng B': Decimal('1540000000')
            },
            
            'TỔNG TÀI SẢN (A + B)': Decimal('3920000000')
        },
        
        'nguon_von': {
            'A. NỢ PHẢI TRẢ': {
                '1. Nợ ngắn hạn': {
                    '311': Decimal('0'),
                    '331': Decimal('200000000'),
                    '333': Decimal('50000000'),
                    '334': Decimal('100000000'),
                    '335': Decimal('30000000'),
                    '338': Decimal('20000000'),
                    '341': Decimal('150000000'),
                    'tong_cong': Decimal('550000000')
                },
                '2. Nợ dài hạn': {
                    '342': Decimal('300000000'),
                    '343': Decimal('0'),
                    'tong_cong': Decimal('300000000')
                },
                'Tổng cộng A': Decimal('850000000')
            },
            
            'B. VỐN CHỦ SỞ HỮU': {
                '1. Vốn chủ sở hữu': {
                    '4111': Decimal('2500000000'),
                    '4112': Decimal('0'),
                    '4113': Decimal('0'),
                    'tong_cong': Decimal('2500000000')
                },
                '2. Lợi nhuận sau thuế': {
                    '412': Decimal('520000000'),
                    '417': Decimal('50000000'),
                    '418': Decimal('0'),
                    'tong_cong': Decimal('570000000')
                },
                '3. Quỹ': {
                    '418': Decimal('0'),
                    '419': Decimal('0'),
                    'tong_cong': Decimal('0')
                },
                'Tổng cộng B': Decimal('3070000000')
            },
            
            'TỔNG NGUỒN VỐN (A + B)': Decimal('3920000000')
        },
        
        'kiem_tra': {
            'tong_tai_san': Decimal('3920000000'),
            'tong_nguon_von': Decimal('3920000000'),
            'can_bang': True
        },
        
        'cac_chỉ_tiêu_đặc_thù_tt99': {
            'tai_san_sinh_hoc_tong': Decimal('90000000'),
            'chenh_lech_ty_gia_hop_ly': 0,
            'tai_san_thue_thu_nhap_hoan_lai': 0
        },
        
        'ngay_lap': date(2025, 1, 31),
        'nguoi_lap_bieu': 'Nguyễn Văn A',
        'ke_toan_truong': 'Trần Thị B',
        'thu_truong_don_vi': 'Lê Văn C'
    }


# =============================================================================
# FIXTURE: TK 215 - Biological Assets Test Data
# =============================================================================

@pytest.fixture
def tai_san_sinh_hoc_data(db_session):
    """Fixture test data cho TK 215 - Tài sản sinh học.
    
    TK 215 được thêm mới trong TT99/2025 để hạch toán:
    - Động vật (gia súc, gia cầm, thuỷ sản nuôi trồng)
    - Thực vật (cây trồng, rừng trồng)
    
    Đặc điểm:
    - Biến động theo sinh trưởng, suy thoái
    - Đánh giá theo giá trị hợp lý trừ chi phí bán
    """
    return {
        '2151': {
            'ten': 'Tài sản sinh học tăng trưởng',
            'loai': 'tsbd_tang_truong',
            'so_luong': 100,  # 100 con bò
            'don_gia': Decimal('15000000'),
            'gia_tri': Decimal('1500000000'),
            'mo_ta': '�àn bò sữa nuôi tại trang trại'
        },
        '2152': {
            'ten': 'Tài sản sinh học giảm trừ',
            'loai': 'tsbd_giam_tru',
            'so_luong': 10,
            'don_gia': Decimal('12000000'),
            'gia_tri': Decimal('120000000'),
            'mo_ta': 'Bò đã thanh lý trong kỳ'
        },
        '2153': {
            'ten': 'Chênh lệch đánh giá TSBĐ',
            'loai': 'chenh_lech',
            'gia_tri': Decimal('0'),
            'mo_ta': 'Chênh lệch đánh giá lại cuối năm'
        }
    }


@pytest.fixture
def chung_tu_tai_san_sinh_hoc(company_info, ky_ke_toan_mo, tai_san_sinh_hoc_data):
    """Tạo chứng từ liên quan đến Tài sản sinh học (TK 215).
    
    Ví dụ: Mua 10 con bò sữa trị giá 150 triệu
    """
    return {
        'loai_ct': 'PNK',
        'ngay_ct': date(2025, 1, 10),
        'ngay_hach_toan': date(2025, 1, 10),
        'dien_giai': 'Mua 10 con bò sữa sinh sản',
        'thong_tin_cong_ty': company_info,
        'dinh_khoan': [
            {
                'stt': 1,
                'tk_no': '2151',  # Tài sản sinh học tăng trưởng
                'tk_co': '331',   # Phải trả người bán
                'so_tien': Decimal('150000000.00'),
                'ma_nt': 'VND',
                'ty_gia': Decimal('1.0'),
                'dien_giai': 'Mua 10 con bò sữa sinh sản'
            }
        ],
        'tk_su_dung': ['2151', '331'],
        'loai_tai_san': 'sinh_hoc'
    }
