"""
File: app/__init__.py
Module: Application Factory
Mô tả: Khởi tạo Flask app theo Application Factory pattern.
       Đăng ký tất cả extensions, blueprints và error handlers.

Tham chiếu pháp lý:
    - TT99/2025/TT-BTC: Chế độ kế toán doanh nghiệp vừa và nhỏ

Tác giả: [Tên]
Ngày tạo: 2025-01-xx
Cập nhật: [Ngày] — [Mô tả thay đổi]
"""

import os
import logging
from flask import Flask

from app.extensions import db, migrate, jwt, init_celery
from app.config import config
from app.utils.exceptions import KeToanBaseError


class AppException(Exception):
    """Legacy exception for backward compatibility."""

    def __init__(self, message: str, status_code: int = 400):
        self.message = message
        self.status_code = status_code
        super().__init__(message)
from app.utils.exceptions import KeToanBaseError
from app.utils.response import error_response

logger = logging.getLogger(__name__)


def create_app(config_name: str = None) -> Flask:
    """Tạo và cấu hình Flask application instance.

    Args:
        config_name: Tên môi trường ('development', 'production', 'testing').
                    Nếu None, lấy từ biến môi trường FLASK_ENV.

    Returns:
        Flask application đã được cấu hình đầy đủ.

    Raises:
        ValueError: Nếu config_name không hợp lệ.
    """
    if config_name is None:
        config_name = os.environ.get('FLASK_ENV', 'development')

    if config_name not in config:
        raise ValueError(f"Invalid config_name: {config_name}")

    app = Flask(__name__)
    app.config.from_object(config[config_name])

    config[config_name].init_app(app)

    db.init_app(app)
    migrate.init_app(app, db)
    jwt.init_app(app)
    init_celery(app)

    from app.models import Base
    Base.query = db.session.query_property()

    logger.info(
        "App khởi động",
        extra={"config": config_name, "env": os.getenv("FLASK_ENV")}
    )

    register_error_handlers(app)
    register_blueprints(app)
    register_shell_context(app)
    register_cli_commands(app)

    return app


def register_error_handlers(app: Flask) -> None:
    """Đăng ký handlers cho các loại HTTP errors và custom exceptions.

    Args:
        app: Flask application instance.
    """

    @app.errorhandler(KeToanBaseError)
    def handle_ke_toan_error(e: KeToanBaseError):
        return error_response(
            message=e.message,
            code=e.code,
            status_code=e.status_code
        )

    @app.errorhandler(400)
    def handle_bad_request(e):
        return error_response(
            message="Yêu cầu không hợp lệ",
            code="BAD_REQUEST",
            status_code=400
        )

    @app.errorhandler(401)
    def handle_unauthorized(e):
        return error_response(
            message="Chưa xác thực. Vui lòng đăng nhập",
            code="UNAUTHORIZED",
            status_code=401
        )

    @app.errorhandler(403)
    def handle_forbidden(e):
        return error_response(
            message="Không có quyền truy cập tài nguyên này",
            code="FORBIDDEN",
            status_code=403
        )

    @app.errorhandler(404)
    def handle_not_found(e):
        return error_response(
            message="Tài nguyên không tìm thấy",
            code="NOT_FOUND",
            status_code=404
        )

    @app.errorhandler(500)
    def handle_server_error(e):
        logger.error("Internal server error", exc_info=True)
        return error_response(
            message="Lỗi server nội bộ",
            code="INTERNAL_SERVER_ERROR",
            status_code=500
        )


def register_blueprints(app: Flask) -> None:
    """Đăng ký tất cả blueprints vào application.

    Args:
        app: Flask application instance.
    """
    from app.modules.auth import auth_bp
    from app.modules.he_thong_tk import he_thong_tk_bp
    from app.modules.nhat_ky import nhat_ky_bp
    from app.modules.ky_ke_toan import ky_ke_toan_bp
    from app.modules.danh_muc.doi_tuong import doi_tuong_bp
    from app.modules.danh_muc.hang_hoa import hang_hoa_bp
    from app.modules.danh_muc.ngan_hang import ngan_hang_bp
    from app.modules.tien import tien_bp
    from app.modules.cong_no import cong_no_bp
    from app.modules.hang_ton_kho import hang_ton_kho_bp
    from app.modules.tai_san import tai_san_bp
    from app.modules.luong import luong_bp
    from app.modules.thue import thue_bp
    from app.modules.bao_cao import bao_cao_bp
    from app.modules.thanh_tra import thanh_tra_bp
    from app.modules.hoa_don_dien_tu import hoa_don_bp

    app.register_blueprint(auth_bp, url_prefix='/api/v1/auth')
    app.register_blueprint(he_thong_tk_bp, url_prefix='/api/v1/he-thong-tk')
    app.register_blueprint(nhat_ky_bp, url_prefix='/api/v1/nhat-ky')
    app.register_blueprint(ky_ke_toan_bp, url_prefix='/api/v1/ky-ke-toan')
    app.register_blueprint(doi_tuong_bp, url_prefix='/api/v1/doi-tuong')
    app.register_blueprint(hang_hoa_bp, url_prefix='/api/v1/hang-hoa')
    app.register_blueprint(ngan_hang_bp, url_prefix='/api/v1/ngan-hang')
    app.register_blueprint(tien_bp, url_prefix='/api/v1/tien')
    app.register_blueprint(cong_no_bp, url_prefix='/api/v1/cong-no')
    app.register_blueprint(hang_ton_kho_bp, url_prefix='/api/v1/hang-ton-kho')
    app.register_blueprint(tai_san_bp, url_prefix='/api/v1/tai-san')
    app.register_blueprint(luong_bp, url_prefix='/api/v1/luong')
    app.register_blueprint(thue_bp, url_prefix='/api/v1/thue')
    app.register_blueprint(bao_cao_bp, url_prefix='/api/v1/bao-cao')
    app.register_blueprint(thanh_tra_bp, url_prefix='/api/v1/thanh-tra')
    app.register_blueprint(hoa_don_bp, url_prefix='/api/v1/hoa-don-dien-tu')


def register_shell_context(app: Flask) -> None:
    """Đăng ký shell context cho flask shell command.

    Args:
        app: Flask application instance.
    """

    @app.shell_context_processor
    def make_shell_context():
        from app.models import Base
        from app.modules.he_thong_tk.models import HeThongTaiKhoan
        from app.modules.nhat_ky.models import ChungTu, DinhKhoan
        from app.modules.danh_muc.doi_tuong.models import DoiTuong
        from app.modules.danh_muc.hang_hoa.models import HangHoa
        from app.modules.danh_muc.ngan_hang.models import NganHang
        return {
            'db': db,
            'Base': Base,
            'HeThongTaiKhoan': HeThongTaiKhoan,
            'ChungTu': ChungTu,
            'DinhKhoan': DinhKhoan,
            'DoiTuong': DoiTuong,
            'HangHoa': HangHoa,
            'NganHang': NganHang,
        }


def register_cli_commands(app: Flask) -> None:
    """Đăng ký Flask CLI commands.

    Args:
        app: Flask application instance.
    """

    @app.cli.command("seed-data")
    def seed_data_command():
        """Seed dữ liệu mẫu vào database."""
        from app.utils.seed_tt99 import seed_tt99_accounts
        with app.app_context():
            seed_tt99_accounts()
        print("Seeded data successfully")

    @app.cli.command("create-admin")
    def create_admin_command():
        """Tạo tài khoản admin mặc định."""
        from app.modules.auth.services import create_default_admin
        with app.app_context():
            create_default_admin()
        print("Admin user created")
