from flask import current_app
from flask_sqlalchemy import SQLAlchemy
from .models import ChungTu, DinhKhoan
from .validators import validate_bu_quy
from app.utils.so_hieu import generate_document_number
from datetime import datetime
import logging

logger = logging.getLogger(__name__)


class ChungTuService:
    def __init__(self):
        self.session = current_app.db.session

    def get_all(self, user_id):
        """Retrieve all journal entries for the given user"""
        return ChungTu.query.filter_by(nguoi_tao=user_id).all()

    def get_by_id(self, ct_id):
        """Retrieve a single journal entry by ID"""
        return ChungTu.query.get(ct_id)

    def create(self, data):
        """Create a new journal entry and its detailing lines"""
        try:
            with self.session.begin():
                # Generate document number
                data['so_ct'] = generate_document_number(data['loai_ct'])
                
                # Create main document
                chung_tu = ChungTu(**data)
                self.session.add(chung_tu)
                
                # Process detailing lines
                self._validate_balanced_entries(data.get('dinh_khoans', []))
                for line in data.get('dinh_khoans', []):
                    dinh_khoan = DinhKhoan(**line)
                    dinh_khoan.chung_tu_id = chung_tu.id
                    self.session.add(dinh_khoan)
                    
                    # Update account balances
                    self._update_account_balance(line['tk_no'], line['so_tien'])
                    if line['tk_co']:
                        self._update_account_balance(line['tk_co'], -line['so_tien'])
                
                return chung_tu
        except Exception as e:
            logger.error(f'Error creating journal entry: {str(e)}')
            raise

    def update(self, ct_id, data):
        """Update an existing journal entry"""
        chung_tu = self.get_by_id(ct_id)
        if not chung_tu:
            raise ValueError('Chứng từ không tồn tại')
        
        with self.session.begin():
            chung_tu.ngay_ct = data.get('ngay_ct', chung_tu.ngay_ct)
            chung_tu.ngay_hach_toan = data.get('ngay_hach_toan', chung_tu.ngay_hach_toan)
            chung_tu.doi_tuong_id = data.get('doi_tuong_id', chung_tu.doi_tuong_id)
            chung_tu.dien_giai = data.get('dien_giai', chung_tu.dien_giai)
            
            # Update detailing lines
            # ... similar to create method but with updates to existing lines
            
        return chung_tu

    def delete(self, ct_id):
        """Delete a journal entry and its detailing lines"""
        try:
            with self.session.begin():
                ChungTu.query.filter_by(id=ct_id).delete()
                DinhKhoan.query.filter_by(chung_tu_id=ct_id).delete()
        except Exception as e:
            logger.error(f'Error deleting journal entry {ct_id}: {str(e)}')
            raise

    def duyet(self, ct_id):
        """Duyệt (approve) a journal entry"""
        chung_tu = self.get_by_id(ct_id)
        if not chung_tu:
            raise ValueError('Chứng từ không tồn tại')
        if chung_tu.trang_thai != 'nhap':
            raise ValueError('Chứng từ đã được duyệt hoặc hủy')
        
        with self.session.begin():
            chung_tu.trang_thai = 'da_duyet'
            chung_tu.ngay_hach_toan = datetime.now()
            
            # Additional validation for approved entries
            self._validate_approved_entry(chung_tu)

    def huy(self, ct_id):
        """Hủy a journal entry"""
        chung_tu = self.get_by_id(ct_id)
        if not chung_tu:
            raise ValueError('Chứng từ không tồn tại')
        if chung_tu.trang_thai == 'da_duyet':
            raise ValueError('Chứng từ đã được duyệt không thể hủy')
        
        with self.session.begin():
            chung_tu.trang_thai = 'da_huy'

    def _validate_balanced_entries(self, lines):
        """Validate that total debit equals total credit"""
        total_no = sum(line.get('so_tien', 0) for line in lines if line.get('tk_no'))
        total_co = sum(line.get('so_tien', 0) for line in lines if line.get('tk_co'))
        
        if abs(total_no - total_co) > 0.01:  # Small tolerance for floating point errors
            raise ValueError('Tổng decrease (...)