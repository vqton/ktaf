from flask import Blueprint

hang_ton_kho_bp = Blueprint('hang_ton_kho', __name__)

from app.modules.hang_ton_kho import routes
