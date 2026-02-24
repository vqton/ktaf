from flask import Blueprint

luong_bp = Blueprint('luong', __name__)

from app.modules.luong import routes
