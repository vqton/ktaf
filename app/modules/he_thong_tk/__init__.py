from flask import Blueprint

he_thong_tk_bp = Blueprint('he_thong_tk', __name__)

from app.modules.he_thong_tk import routes
