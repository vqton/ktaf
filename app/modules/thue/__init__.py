from flask import Blueprint

thue_bp = Blueprint('thue', __name__)

from app.modules.thue import routes
