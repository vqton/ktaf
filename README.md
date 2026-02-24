# Accounting Application (Thông tư 99/2025)

Flask-based accounting application following Vietnamese accounting regulations (Thông tư 99/2025/TT-BTC) for small and medium enterprises.

## Tech Stack

- **Backend**: Python 3.11+, Flask 3.0.3
- **ORM**: SQLAlchemy 2.0 (declarative style)
- **Database**: PostgreSQL 15+
- **Migration**: Flask-Migrate (Alembic)
- **Auth**: Flask-JWT-Extended
- **Validation**: Marshmallow
- **Task Queue**: Celery + Redis
- **Environment**: Windows 10 (PowerShell)

## Features

- Chart of Accounts (Hệ thống tài khoản TT99)
- Journal Entries (Nhật ký chung - ChungTu, DinhKhoan)
- Balance Validation (Nợ = Có)
- Period Locking (Khóa kỳ kế toán)
- Document Numbering with pg_advisory_lock
- Accounts Receivable/Payable (Công nợ)
- Inventory (Hàng tồn kho)
- Fixed Assets (Tài sản cố định)
- Payroll (Tiền lương)
- Financial Reports (Báo cáo tài chính B01-B09)

## Quick Start

### 1. Clone and Setup Virtual Environment

```powershell
python -m venv .venv
.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

### 2. Start Docker Services

```powershell
docker compose -f docker-compose.dev.yml up -d
```

### 3. Configure Environment

Copy `.env.example` to `.env` and update:

```env
DATABASE_URL=postgresql+psycopg2://postgres:password@localhost:5432/accounting_db
REDIS_URL=redis://localhost:6379/0
FLASK_ENV=development
SECRET_KEY=your-secret-key
JWT_SECRET_KEY=your-jwt-secret
```

### 4. Initialize Database

```powershell
flask db init
flask db migrate -m "Initial migration"
flask db upgrade
```

### 5. Run Application

```powershell
flask run --host=0.0.0.0 --port=5000
```

### 6. Run Celery Worker (Optional)

```powershell
celery -A app.celery worker --loglevel=info --pool=solo
```

## Development Commands

```powershell
# Using dev.ps1 script
.\scripts\dev.ps1 run        # Run Flask
.\scripts\dev.ps1 migrate   # Run migrations
.\scripts\dev.ps1 test      # Run tests
.\scripts\dev.ps1 docker     # Start Docker
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/v1/he-thong-tk | List accounts |
| POST | /api/v1/he-thong-tk | Create account |
| GET | /api/v1/nhat-ky | List journal entries |
| POST | /api/v1/nhat-ky | Create journal entry |
| POST | /api/v1/nhat-ky/{id}/duyet | Approve entry |
| POST | /api/v1/nhat-ky/{id}/huy | Cancel entry |
| GET | /api/v1/doi-tuong | List customers/suppliers |
| GET | /api/v1/hang-hoa | List goods/services |
| GET | /api/v1/bao-cao/bcdkt | Balance Sheet (B01-DN) |
| GET | /api/v1/bao-cao/kqkd | Income Statement (B02-DN) |

## Project Structure

```
app/
├── __init__.py           # Application Factory
├── extensions.py         # SQLAlchemy, JWT, Celery
├── config.py             # Configuration
├── models/
│   └── __init__.py      # Base, AuditMixin
├── modules/
│   ├── auth/            # Authentication
│   ├── danh_muc/        # Categories
│   ├── he_thong_tk/     # Chart of Accounts
│   ├── nhat_ky/         # Journal Entries
│   ├── tien/            # Cash/Bank
│   ├── cong_no/         # AR/AP
│   ├── hang_ton_kho/    # Inventory
│   ├── tai_san/         # Fixed Assets
│   ├── luong/           # Payroll
│   ├── thue/            # Taxes
│   └── bao_cao/         # Financial Reports
└── utils/
    ├── so_hieu.py       # Document numbering
    ├── ky_ke_toan.py    # Period handling
    └── validators.py    # Validation helpers
```

## Accounting Rules

1. **Balance Validation**: Total Debit (Nợ) = Total Credit (Có) for every document
2. **Period Locking**: Cannot modify entries in locked periods
3. **Document Numbers**: Format `[LoạiCT][YYYY][MM]-[00001]` (e.g., PC202501-00001)
4. **Concurrent Safety**: Use `pg_advisory_lock()` for document number generation

## Testing

```powershell
pytest tests/ -v
pytest tests/test_nhat_ky.py -v
pytest tests/test_nhat_ky.py::test_validate_but_toan_can_bang -v
```

## References

- [Thông tư 99/2025/TT-BTC](https://)
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Detailed architecture
- [AGENTS.md](./AGENTS.md) - Developer guidelines

## License

MIT
