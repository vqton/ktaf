from flask import Blueprint

cong_no_bp = Blueprint('cong_no', __name__)

from app.modules.cong_no import routes
