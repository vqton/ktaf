from flask import Blueprint

tien_bp = Blueprint('tien', __name__)
cong_no_bp = Blueprint('cong_no', __name__)
hang_ton_kho_bp = Blueprint('hang_ton_kho', __name__)
tai_san_bp = Blueprint('tai_san', __name__)
luong_bp = Blueprint('luong', __name__)
thue_bp = Blueprint('thue', __name__)
bao_cao_bp = Blueprint('bao_cao', __name__)

from app.modules.tien import routes
from app.modules.cong_no import routes
from app.modules.hang_ton_kho import routes
from app.modules.tai_san import routes
from app.modules.luong import routes
from app.modules.thue import routes
from app.modules.bao_cao import routes
