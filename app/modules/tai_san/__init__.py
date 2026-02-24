from flask import Blueprint

tai_san_bp = Blueprint('tai_san', __name__)

from app.modules.tai_san import routes
