from flask import Blueprint

doi_tuong_bp = Blueprint('doi_tuong', __name__)
hang_hoa_bp = Blueprint('hang_hoa', __name__)
ngan_hang_bp = Blueprint('ngan_hang', __name__)
tien_bp = Blueprint('tien', __name__)
cong_no_bp = Blueprint('cong_no', __name__)
hang_ton_kho_bp = Blueprint('hang_ton_kho', __name__)
tai_san_bp = Blueprint('tai_san', __name__)
luong_bp = Blueprint('luong', __name__)
thue_bp = Blueprint('thue', __name__)
bao_cao_bp = Blueprint('bao_cao', __name__)

from app.modules.danh_muc.doi_tuong import routes
from app.modules.danh_muc.hang_hoa import routes
from app.modules.danh_muc.ngan_hang import routes
from app.modules.tien import routes
from app.modules.cong_no import routes
from app.modules.hang_ton_kho import routes
from app.modules.tai_san import routes
from app.modules.luong import routes
from app.modules.thue import routes
from app.modules.bao_cao import routes
