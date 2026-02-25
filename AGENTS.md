# AGENTS.md - Accounting Application (Thông tư 99/2025)

Flask-based accounting app following Vietnamese regulations (Thông tư 99/2025/TT-BTC).

**Tech Stack:** Python 3.11+, Flask 3.0.3, SQLAlchemy 2.0, PostgreSQL 15+, Flask-Migrate, Flask-JWT-Extended, Marshmallow, Celery + Redis

---

## Build / Lint / Test Commands

### Setup (Windows PowerShell)
```powershell
python -m venv .venv
.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

### Running the Application
```powershell
$env:FLASK_APP = "run.py"
$env:FLASK_ENV = "development"
flask run --host=0.0.0.0 --port=5000
```

### Database Migrations
```powershell
flask db migrate -m "migration message"
flask db upgrade
flask db downgrade
```

### Running Tests
```powershell
pytest tests/ -v
pytest tests/test_nhat_ky.py -v
pytest tests/test_nhat_ky.py::test_validate_but_toan_can_bang -v
pytest tests/ --cov=app --cov-report=html
```

### Celery Worker (Windows)
```powershell
celery -A app.celery worker --loglevel=info --pool=solo
```

### Docker Services
```powershell
docker compose -f docker-compose.dev.yml up -d
```

---

## Code Style Guidelines

### Project Structure
```
.
├── app/
│   ├── __init__.py           # Application Factory
│   ├── extensions.py         # db, jwt, migrate, celery
│   ├── config.py             # Config class (Dev/Prod/Test)
│   ├── models/
│   │   └── __init__.py      # Base + AuditMixin
│   ├── modules/
│   │   ├── auth/            # Authentication (models.py, routes.py)
│   │   ├── danh_muc/
│   │   │   ├── doi_tuong/  # Customers/Suppliers
│   │   │   ├── hang_hoa/   # Goods/Services
│   │   │   └── ngan_hang/  # Bank accounts
│   │   ├── he_thong_tk/     # Chart of accounts
│   │   ├── nhat_ky/         # Journal entries (ChungTu, DinhKhoan)
│   │   ├── ky_ke_toan/      # Accounting periods
│   │   ├── so_cai/          # Ledger (SoCai, SoDuDauKy)
│   │   ├── tien/            # Cash/bank accounting
│   │   ├── cong_no/         # Receivables/payables
│   │   ├── hang_ton_kho/    # Inventory
│   │   ├── tai_san/         # Fixed assets
│   │   ├── luong/           # Payroll
│   │   ├── thue/            # Taxes
│   │   ├── bao_cao/         # Financial reports (MauBaoCao)
│   │   └── thanh_tra/       # Inspection & Audit (QuyetDinh, KienNghi)
│   │   └── hoa_don_dien_tu/ # E-Invoice (ND123/2020, TT78/2021)
│   │   └── audit_log/        # Audit trail, document retention
│   └── utils/
│       ├── so_hieu.py       # Document number generation
│       ├── ky_ke_toan.py    # Accounting period handling
│       └── validators.py    # Validation helpers
├── migrations/              # Flask-Migrate
├── scripts/
│   └── dev.ps1            # PowerShell dev commands
├── tests/                  # Test files
├── alembic.ini            # Alembic config
├── docker-compose.dev.yml # Redis + PostgreSQL
├── run.py                 # Entry point
└── requirements.txt
```

### Naming Conventions
| Type | Convention | Example |
|------|------------|---------|
| Models | PascalCase | `ChungTu`, `DinhKhoan` |
| Tables | snake_case | `chung_tu`, `dinh_khoan` |
| Routes/Blueprints | snake_case | `/api/v1/nhat-ky/` |
| Variables | snake_case | `so_ct`, `ngay_ct` |
| Constants | UPPER_SNAKE_CASE | `MAX_AMOUNT` |

### Import Order (PEP 8)
1. Standard library
2. Third-party packages
3. Local application imports

```python
from datetime import datetime
from typing import Optional

from flask import Blueprint, request, jsonify
from sqlalchemy.orm import Mapped, mapped_column, relationship
from marshmallow import Schema, fields

from app.extensions import db
from app.modules.nhat_ky.models import ChungTu
```

### SQLAlchemy 2.0 Style
```python
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship
from sqlalchemy import String, Numeric, ForeignKey
from sqlalchemy.dialects.postgresql import JSONB, TIMESTAMPTZ
from sqlalchemy import text

class Base(DeclarativeBase):
    pass

class AuditMixin:
    created_at: Mapped[datetime] = mapped_column(
        TIMESTAMPTZ, server_default=text("NOW()"), nullable=False
    )
    updated_at: Mapped[datetime] = mapped_column(
        TIMESTAMPTZ, server_default=text("NOW()"),
        onupdate=text("NOW()"), nullable=False
    )

class MyModel(AuditMixin, Base):
    __tablename__ = "my_model"
    __table_args__ = {"schema": "accounting"}
    
    id: Mapped[int] = mapped_column(primary_key=True)
    name: Mapped[str] = mapped_column(String(255))
```

### API Response Format
```json
{
  "success": true,
  "data": {},
  "message": "",
  "pagination": {"page": 1, "per_page": 20, "total": 100}
}
```

### API URL Conventions
```
GET    /api/v1/{module}/           # List
POST   /api/v1/{module}/           # Create
GET    /api/v1/{module}/<id>       # Detail
PUT    /api/v1/{module}/<id>       # Update
DELETE /api/v1/{module}/<id>       # Delete
POST   /api/v1/chung-tu/<id>/duyet  # Approve
POST   /api/v1/chung-tu/<id>/huy     # Cancel
GET    /api/v1/so-cai?tk=111&tu=...&den=...  # Ledger
GET    /api/v1/thanh-tra/ho-so/xuat-chung-tu  # Export documents for inspection
GET    /api/v1/thanh-tra/kiem-tra/hoa-don-tien-mat  # Pre-inspection checks
GET    /api/v1/hoa-don-dien-tu/ban-ra  # E-invoices for sales
POST   /api/v1/hoa-don-dien-tu/ban-ra  # Create e-invoice
POST   /api/v1/hoa-don-dien-tu/ban-ra/<id>/ky-so  # Sign & send to CQT
GET    /api/v1/audit-log/logs           # Query audit trail
POST   /api/v1/audit-log/bao-cao-view  # Log view financial report
POST   /api/v1/audit-log/bao-quan/khong-cho-phep-xoa  # Check delete allowed
```

### Database Conventions
- Use PostgreSQL schema: `accounting`
- Use BIGSERIAL for auto-increment
- Use VARCHAR with CHECK constraint instead of ENUM
- Use JSONB for flexible metadata
- Use `server_default=text("NOW()")` instead of `default=datetime.now`
- Always define indexes for query performance

### Error Handling
```python
class AppException(Exception):
    def __init__(self, message: str, status_code: int = 400):
        self.message = message
        self.status_code = status_code
        super().__init__(message)

@app.errorhandler(AppException)
def handle_app_exception(e):
    return jsonify({"success": False, "message": e.message}), e.status_code

raise AppException("Kỳ kế toán đã khóa", 400)
```

### Accounting-Specific Rules
1. **Balance Validation**: Total Debit (Nợ) = Total Credit (Có) for every document
2. **Period Locking**: Check `ky_ke_toan.trang_thai = 'khoa'` before INSERT/UPDATE/DELETE
3. **Document Numbers**: Format `[LoạiCT][YYYY][MM]-[00001]` (e.g., PC202501-00001)
4. **Use pg_advisory_lock()** for concurrent document number generation

---

## Database Models

### Core Models
| Model | Table | Description |
|-------|-------|-------------|
| `HeThongTaiKhoan` | `he_thong_tai_khoan` | Chart of accounts (TT99) |
| `ChungTu` | `chung_tu` | Journal documents |
| `DinhKhoan` | `dinh_khoan` | Journal entries (debit/credit) |
| `KyKeToan` | `ky_ke_toan` | Accounting periods |
| `SoCai` | `so_cai` | General ledger |
| `SoDuDauKy` | `so_du_dau_ky` | Opening balances |
| `DoiTuong` | `doi_tuong` | Customers/Suppliers |
| `HangHoa` | `hang_hoa` | Goods/Services |
| `NganHang` | `ngan_hang` | Bank accounts |
| `MauBaoCao` | `mau_bao_cao` | Financial report templates |
| `QuyetDinhThanhTra` | `quyet_dinh_thanh_tra` | Inspection decisions |
| `KienNghiThanhTra` | `kien_nghi_thanh_tra` | Inspection recommendations |
| `PhieuThuThueTruyThu` | `phieu_thu_thue_truy_thu` | Tax penalty payments |
| `CauHinhHoaDon` | `cau_hinh_hoa_don` | E-invoice configuration |
| `MauSoKyHieu` | `mau_so_ky_hieu` | Invoice templates |
| `HoaDonBanRa` | `hoa_don_ban_ra` | E-invoices for sales |
| `HoaDonMuaVao` | `hoa_don_mua_vao` | E-invoices for purchases |
| `HoaDonDieuChinh` | `hoa_don_dieu_chinh` | Adjustment invoices |
| `LichSuXuLyHD` | `lich_su_xu_ly_hd` | Invoice processing history |
| `AuditLog` | `audit_log` | Immutable audit trail with IP, digital signature |
| `CauHinhBaoQuan` | `cau_hinh_bao_quan` | Document retention config (10 years) |
| `LichSuBaoQuan` | `lich_su_bao_quan` | Document retention history |
| `User` | `users` | Authentication (public schema) |

---

## Git Workflow
```
main        ← production-ready
develop     ← integration branch
feature/*   ← new features
hotfix/*    ← urgent fixes
```

### Commit Message Format
```
feat(module):     new feature
fix(module):      bug fix
chore:            configuration, setup
refactor(module): code restructuring
test(module):     test additions
docs:             documentation
db(module):       migration, schema changes
```

---

## Environment Variables (.env)
```env
DATABASE_URL=postgresql+psycopg2://postgres:password@localhost:5432/accounting_db
REDIS_URL=redis://localhost:6379/0
FLASK_ENV=development
SECRET_KEY=your-secret-key
JWT_SECRET_KEY=your-jwt-secret
```

**Key References:** `ARCHITECTURE.md`, `requirements.txt`, `.env.example`
