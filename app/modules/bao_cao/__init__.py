from flask import Blueprint

bao_cao_bp = Blueprint('bao_cao', __name__)

from app.modules.bao_cao import routes
