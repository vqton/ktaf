from flask import Blueprint

ngan_hang_bp = Blueprint('ngan_hang', __name__)

from app.modules.danh_muc.ngan_hang import routes
