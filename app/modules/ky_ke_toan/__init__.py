from flask import Blueprint

ky_ke_toan_bp = Blueprint('ky_ke_toan', __name__)

from app.modules.ky_ke_toan import routes
