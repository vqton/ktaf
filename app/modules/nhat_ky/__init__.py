from flask import Blueprint

nhat_ky_bp = Blueprint('nhat_ky', __name__)

from app.modules.nhat_ky import routes
