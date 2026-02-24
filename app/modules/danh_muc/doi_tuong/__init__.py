from flask import Blueprint

doi_tuong_bp = Blueprint('doi_tuong', __name__)

from app.modules.danh_muc.doi_tuong import routes
