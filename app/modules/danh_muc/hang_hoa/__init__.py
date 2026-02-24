from flask import Blueprint

hang_hoa_bp = Blueprint('hang_hoa', __name__)

from app.modules.danh_muc.hang_hoa import routes
