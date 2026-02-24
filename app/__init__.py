import os
from flask import Flask, jsonify

from app.extensions import db, migrate, jwt, init_celery
from app.config import config


class AppException(Exception):
    def __init__(self, message: str, status_code: int = 400):
        self.message = message
        self.status_code = status_code
        super().__init__(message)


def create_app(config_name=None):
    if config_name is None:
        config_name = os.environ.get('FLASK_ENV', 'development')

    app = Flask(__name__)
    app.config.from_object(config[config_name])

    config[config_name].init_app(app)

    db.init_app(app)
    migrate.init_app(app, db)
    jwt.init_app(app)
    init_celery(app)

    from app.models import Base
    Base.query = db.session.query_property()

    register_error_handlers(app)
    register_blueprints(app)
    register_shell_context(app)

    return app


def register_error_handlers(app):
    @app.errorhandler(AppException)
    def handle_app_exception(e):
        return jsonify({
            'success': False,
            'message': e.message
        }), e.status_code

    @app.errorhandler(404)
    def handle_not_found(e):
        return jsonify({
            'success': False,
            'message': 'Resource not found'
        }), 404

    @app.errorhandler(500)
    def handle_server_error(e):
        return jsonify({
            'success': False,
            'message': 'Internal server error'
        }), 500


def register_blueprints(app):
    from app.modules.auth import auth_bp
    from app.modules.he_thong_tk import he_thong_tk_bp
    from app.modules.nhat_ky import nhat_ky_bp
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

    app.register_blueprint(auth_bp, url_prefix='/api/v1/auth')
    app.register_blueprint(he_thong_tk_bp, url_prefix='/api/v1/he-thong-tk')
    app.register_blueprint(nhat_ky_bp, url_prefix='/api/v1/nhat-ky')
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


def register_shell_context(app):
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
