# MASTER PROMPT — Khung ứng dụng kế toán Thông tư 99/2025
> Techstack: Python · Flask · PostgreSQL | Môi trường: Windows 10

---

```
Bạn là một senior software architect chuyên về hệ thống kế toán doanh nghiệp Việt Nam.
Hãy xây dựng bộ khung (scaffold) đầy đủ cho ứng dụng kế toán tuân thủ Thông tư 99/2025/TT-BTC
của Bộ Tài chính (chế độ kế toán doanh nghiệp vừa và nhỏ), với techstack:
- Backend  : Python 3.11+, Flask (Application Factory pattern)
- ORM      : SQLAlchemy 2.x
- Database : PostgreSQL 15+
- Migration: Flask-Migrate (Alembic)
- Auth     : Flask-JWT-Extended
- Validation: Marshmallow / Pydantic v2
- Task Queue: Celery + Redis (export báo cáo nền)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
## MÔI TRƯỜNG PHÁT TRIỂN: WINDOWS 10

Toàn bộ quá trình dev/code thực hiện trên Windows 10.
Mọi hướng dẫn, lệnh, cấu hình phải tương thích Windows 10.
Không dùng lệnh Linux/macOS thuần túy (chmod, chown, sudo, apt, brew...).

### Công cụ bắt buộc dùng trên Windows:
- Terminal  : PowerShell 7+ hoặc Windows Terminal (không dùng CMD)
- Python    : Cài qua python.org hoặc Chocolatey, dùng venv
- PostgreSQL: Cài bản Windows installer từ postgresql.org (hoặc Docker Desktop)
- Redis     : Chạy qua Docker Desktop (Redis không có bản native Windows ổn định)
- Docker Desktop: Dùng cho Redis + có thể containerize PostgreSQL nếu muốn
- Git       : Git for Windows (Git Bash đi kèm có thể dùng thay PowerShell)
- IDE       : VSCode với extension Python, PostgreSQL (ms-ossdata.vscode-postgresql)

### Cấu hình môi trường ảo (Virtual Environment):
```powershell
# Tạo môi trường ảo
python -m venv .venv

# Kích hoạt trên PowerShell
.venv\Scripts\Activate.ps1

# Nếu bị lỗi Execution Policy, chạy lệnh này trước:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Cài dependencies
pip install -r requirements.txt
```

### File .env cho Windows:
```env
DATABASE_URL=postgresql+psycopg2://postgres:password@localhost:5432/accounting_db
REDIS_URL=redis://localhost:6379/0
FLASK_ENV=development
FLASK_DEBUG=1
SECRET_KEY=your-secret-key-here
JWT_SECRET_KEY=your-jwt-secret-key-here
```

### Chạy Celery trên Windows:
```powershell
# Windows không hỗ trợ fork mode, bắt buộc dùng --pool=solo
celery -A app.celery worker --loglevel=info --pool=solo
```

### Docker Compose cho Redis + PostgreSQL (khuyến nghị):
```yaml
# docker-compose.dev.yml
version: '3.8'
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  db:
    image: postgres:15-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
      POSTGRES_DB: accounting_db
    ports:
      - "5432:5432"
    volumes:
      - pg_data:/var/lib/postgresql/data

volumes:
  redis_data:
  pg_data:
```

```powershell
# Khởi động services
docker compose -f docker-compose.dev.yml up -d
```

### Setup PostgreSQL thủ công (nếu không dùng Docker):
```powershell
& "C:\Program Files\PostgreSQL\15\bin\psql.exe" -U postgres

-- Chạy trong psql:
CREATE DATABASE accounting_db ENCODING 'UTF8';
\c accounting_db
CREATE SCHEMA accounting;
CREATE USER accounting_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON SCHEMA accounting TO accounting_user;
```

### Chạy Flask trên Windows (PowerShell):
```powershell
$env:FLASK_APP = "run.py"
$env:FLASK_ENV = "development"
flask run --host=0.0.0.0 --port=5000
```

### PowerShell script thay thế Makefile:
```powershell
# scripts\dev.ps1
param([string]$Command)
switch ($Command) {
    "run"     { flask run --host=0.0.0.0 --port=5000 }
    "migrate" { flask db upgrade }
    "seed"    { flask seed-data }
    "worker"  { celery -A app.celery worker --pool=solo --loglevel=info }
    "test"    { pytest tests/ -v }
    "docker"  { docker compose -f docker-compose.dev.yml up -d }
    default   { Write-Host "Commands: run | migrate | seed | worker | test | docker" }
}

# Cách dùng:
# .\scripts\dev.ps1 run
# .\scripts\dev.ps1 migrate
```

### requirements.txt (tối ưu cho Windows):
```
Flask==3.0.3
SQLAlchemy==2.0.30
Flask-SQLAlchemy==3.1.1
Flask-Migrate==4.0.7
Flask-JWT-Extended==4.6.0
marshmallow==3.21.3
psycopg2-binary==2.9.9      # Bắt buộc dùng binary trên Windows (tránh lỗi compile C)
celery==5.3.6
redis==5.0.4
python-dotenv==1.0.1
waitress==3.0.0              # WSGI server thay gunicorn cho Windows dev
gunicorn==22.0.0             # Chỉ dùng khi deploy lên Linux server
```

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
## YÊU CẦU CẤU TRÚC THƯ MỤC

Tạo cấu trúc project theo blueprint pattern:

accounting_app/
├── app/
│   ├── __init__.py              # Application Factory
│   ├── extensions.py            # db, jwt, migrate, celery
│   ├── config.py                # Config class (Dev/Prod/Test)
│   │
│   ├── modules/
│   │   ├── auth/                # Xác thực, phân quyền
│   │   ├── danh_muc/            # Danh mục dùng chung
│   │   │   ├── doi_tuong/       # Khách hàng, nhà cung cấp
│   │   │   ├── hang_hoa/        # Hàng hóa, dịch vụ
│   │   │   └── ngan_hang/       # Tài khoản ngân hàng
│   │   ├── he_thong_tk/         # Hệ thống tài khoản kế toán
│   │   ├── nhat_ky/             # Module nhật ký chung
│   │   ├── tien/                # Kế toán tiền mặt, tiền gửi
│   │   ├── cong_no/             # Công nợ phải thu, phải trả
│   │   ├── hang_ton_kho/        # Hàng tồn kho
│   │   ├── tai_san/             # TSCĐ, khấu hao
│   │   ├── luong/               # Tiền lương, BHXH
│   │   ├── thue/                # Thuế GTGT, TNDN, TNCN
│   │   └── bao_cao/             # Báo cáo tài chính
│   │
│   └── utils/
│       ├── so_hieu.py           # Sinh số chứng từ tự động
│       ├── ky_ke_toan.py        # Xử lý kỳ kế toán
│       └── validators.py
│
├── migrations/
├── tests/
├── scripts/
│   └── dev.ps1                  # PowerShell dev script (thay Makefile)
├── docker-compose.dev.yml
├── requirements.txt
├── .env.example
└── run.py

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
## YÊU CẦU DATABASE (PostgreSQL 15+)

Thiết kế schema tuân thủ TT99/2025. Lưu ý các đặc thù PostgreSQL:
- Dùng BIGSERIAL thay AUTO_INCREMENT
- Dùng VARCHAR có CHECK constraint thay ENUM (dễ migrate hơn)
- Tận dụng JSONB cho metadata linh hoạt
- Dùng schema riêng: CREATE SCHEMA accounting;
- Encoding: UTF8
- Tận dụng Partial Index, Expression Index cho hiệu năng
- Computed column (GENERATED ALWAYS AS ... STORED) cho số dư cuối kỳ

### [1] Hệ thống tài khoản kế toán
```sql
CREATE TABLE accounting.he_thong_tai_khoan (
    ma_tk           VARCHAR(10) PRIMARY KEY,
    ten_tk          VARCHAR(255) NOT NULL,
    loai_tk         VARCHAR(20) NOT NULL
                    CHECK (loai_tk IN ('tai_san','nguon_von','doanh_thu','chi_phi','ngoai_bang')),
    cap_tk          SMALLINT NOT NULL CHECK (cap_tk BETWEEN 1 AND 4),
    ma_tk_cha       VARCHAR(10) REFERENCES accounting.he_thong_tai_khoan(ma_tk),
    tinh_chat       VARCHAR(15) CHECK (tinh_chat IN ('du','co','luong_tinh')),
    co_the_dk       BOOLEAN DEFAULT FALSE,   -- Chỉ TK cấp cuối mới được định khoản
    is_active       BOOLEAN DEFAULT TRUE,
    metadata        JSONB DEFAULT '{}'
);
```

### [2] Chứng từ & Định khoản
```sql
CREATE TABLE accounting.chung_tu (
    id              BIGSERIAL PRIMARY KEY,
    so_ct           VARCHAR(30) UNIQUE NOT NULL,   -- PC202501-00001
    loai_ct         VARCHAR(10) NOT NULL
                    CHECK (loai_ct IN ('PC','PT','BN','BC','PNK','PXK','HDMH','HDBL')),
    ngay_ct         DATE NOT NULL,
    ngay_hach_toan  DATE NOT NULL,
    dien_giai       TEXT,
    doi_tuong_id    BIGINT REFERENCES accounting.doi_tuong(id),
    trang_thai      VARCHAR(10) DEFAULT 'nhap'
                    CHECK (trang_thai IN ('nhap','da_duyet','da_huy')),
    nguoi_tao       INTEGER REFERENCES public.users(id),
    created_at      TIMESTAMPTZ DEFAULT NOW(),
    updated_at      TIMESTAMPTZ DEFAULT NOW(),
    created_by      INTEGER,
    updated_by      INTEGER
);

CREATE TABLE accounting.dinh_khoan (
    id              BIGSERIAL PRIMARY KEY,
    chung_tu_id     BIGINT NOT NULL REFERENCES accounting.chung_tu(id) ON DELETE CASCADE,
    stt             SMALLINT NOT NULL,
    tk_no           VARCHAR(10) REFERENCES accounting.he_thong_tai_khoan(ma_tk),
    tk_co           VARCHAR(10) REFERENCES accounting.he_thong_tai_khoan(ma_tk),
    so_tien         NUMERIC(18,2) NOT NULL CHECK (so_tien > 0),
    so_tien_nt      NUMERIC(18,2),           -- Số tiền ngoại tệ
    ma_nt           CHAR(3) DEFAULT 'VND',
    ty_gia          NUMERIC(10,4) DEFAULT 1,
    doi_tuong_id    BIGINT,
    hang_hoa_id     BIGINT,
    dvt             VARCHAR(20),
    so_luong        NUMERIC(15,4),
    don_gia         NUMERIC(18,2),
    dien_giai       TEXT,
    UNIQUE (chung_tu_id, stt)
);

-- Index hiệu năng
CREATE INDEX idx_chung_tu_ngay        ON accounting.chung_tu(ngay_hach_toan);
CREATE INDEX idx_chung_tu_doi_tuong   ON accounting.chung_tu(doi_tuong_id);
CREATE INDEX idx_chung_tu_nhap        ON accounting.chung_tu(trang_thai)
    WHERE trang_thai = 'nhap';                    -- Partial index chứng từ chưa duyệt
CREATE INDEX idx_dinh_khoan_tk        ON accounting.dinh_khoan(tk_no, tk_co);
```

### [3] Kỳ kế toán & Sổ cái
```sql
CREATE TABLE accounting.ky_ke_toan (
    id          SERIAL PRIMARY KEY,
    nam         SMALLINT NOT NULL,
    thang       SMALLINT NOT NULL CHECK (thang BETWEEN 1 AND 12),
    tu_ngay     DATE NOT NULL,
    den_ngay    DATE NOT NULL,
    trang_thai  VARCHAR(10) DEFAULT 'mo'
                CHECK (trang_thai IN ('mo','khoa')),
    UNIQUE (nam, thang)
);

CREATE TABLE accounting.so_cai (
    id              BIGSERIAL PRIMARY KEY,
    ma_tk           VARCHAR(10) NOT NULL REFERENCES accounting.he_thong_tai_khoan(ma_tk),
    ky_ke_toan_id   INTEGER NOT NULL REFERENCES accounting.ky_ke_toan(id),
    so_du_dau       NUMERIC(18,2) DEFAULT 0,
    phat_sinh_no    NUMERIC(18,2) DEFAULT 0,
    phat_sinh_co    NUMERIC(18,2) DEFAULT 0,
    so_du_cuoi      NUMERIC(18,2) GENERATED ALWAYS AS
                    (so_du_dau + phat_sinh_no - phat_sinh_co) STORED,
    UNIQUE (ma_tk, ky_ke_toan_id)
);

CREATE TABLE accounting.so_du_dau_ky (
    id              BIGSERIAL PRIMARY KEY,
    ma_tk           VARCHAR(10) NOT NULL,
    doi_tuong_id    BIGINT,
    nam_tai_chinh   SMALLINT NOT NULL,
    so_du_no        NUMERIC(18,2) DEFAULT 0,
    so_du_co        NUMERIC(18,2) DEFAULT 0,
    so_du_no_nt     NUMERIC(18,2) DEFAULT 0,
    so_du_co_nt     NUMERIC(18,2) DEFAULT 0
);
```

### [4] Báo cáo tài chính (TT99)
```sql
CREATE TABLE accounting.mau_bao_cao (
    id          SERIAL PRIMARY KEY,
    ma_bc       VARCHAR(20) UNIQUE,   -- B01-DN, B02-DN, B03-DN, B09-DN
    ten_bc      VARCHAR(255),
    mo_ta       TEXT,
    is_active   BOOLEAN DEFAULT TRUE
);

CREATE TABLE accounting.chi_tiet_mau_bc (
    id              SERIAL PRIMARY KEY,
    mau_bc_id       INTEGER REFERENCES accounting.mau_bao_cao(id),
    ma_chi_tieu     VARCHAR(10),
    ten_chi_tieu    VARCHAR(255),
    cong_thuc       JSONB,   -- {"no": ["111","112"], "co": [], "tinh_chat": "du"}
    stt_hien_thi    SMALLINT,
    la_tieu_de      BOOLEAN DEFAULT FALSE
);
```

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
## YÊU CẦU MODEL SQLALCHEMY

Với mỗi module, tạo file models.py dùng SQLAlchemy 2.x declarative:
- Dùng Mapped[] và mapped_column() (style mới)
- Khai báo relationship() đầy đủ với back_populates
- Dùng postgresql.JSONB, NUMERIC đúng dialect
- Khai báo __table_args__ với schema="accounting"
- Dùng server_default=text("NOW()") thay vì default=datetime.now
- Audit Mixin dùng TIMESTAMPTZ

```python
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship
from sqlalchemy import String, Boolean, Numeric, SmallInteger, Text, ForeignKey
from sqlalchemy import CheckConstraint, Index, UniqueConstraint, text
from sqlalchemy.dialects.postgresql import JSONB, TIMESTAMPTZ
from datetime import datetime
from typing import Optional

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
    created_by: Mapped[Optional[int]] = mapped_column(nullable=True)
    updated_by: Mapped[Optional[int]] = mapped_column(nullable=True)

class ChungTu(AuditMixin, Base):
    __tablename__ = "chung_tu"
    __table_args__ = (
        CheckConstraint("trang_thai IN ('nhap','da_duyet','da_huy')", name="ck_chungtu_trangthai"),
        Index("idx_chung_tu_ngay", "ngay_hach_toan"),
        Index("idx_chung_tu_doi_tuong", "doi_tuong_id"),
        {"schema": "accounting"}
    )
    id:     Mapped[int] = mapped_column(primary_key=True)
    so_ct:  Mapped[str] = mapped_column(String(30), unique=True)
    # ... các field khác
    dinh_khoan: Mapped[list["DinhKhoan"]] = relationship(
        back_populates="chung_tu", cascade="all, delete-orphan"
    )
```

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
## YÊU CẦU API (Flask Blueprint)

### Convention URL:
GET    /api/v1/{module}/           -- Danh sách (phân trang, filter)
POST   /api/v1/{module}/           -- Tạo mới
GET    /api/v1/{module}/<id>       -- Chi tiết
PUT    /api/v1/{module}/<id>       -- Cập nhật
DELETE /api/v1/{module}/<id>       -- Xoá (soft delete)

### Endpoint đặc thù kế toán:
POST   /api/v1/chung-tu/<id>/duyet
POST   /api/v1/chung-tu/<id>/huy
GET    /api/v1/so-cai?tk=111&tu=2025-01-01&den=2025-01-31
GET    /api/v1/bao-cao/bcdkt       -- B01-DN Bảng cân đối kế toán
GET    /api/v1/bao-cao/kqkd        -- B02-DN Kết quả kinh doanh
GET    /api/v1/bao-cao/lctt        -- B03-DN Lưu chuyển tiền tệ
GET    /api/v1/bao-cao/bcdkps      -- B09-DN Bảng cân đối số phát sinh

### Response format chuẩn:
```json
{
  "success": true,
  "data": {},
  "message": "",
  "pagination": {
    "page": 1,
    "per_page": 20,
    "total": 100
  }
}
```

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
## YÊU CẦU NGHIỆP VỤ KẾ TOÁN (TT99/2025)

### Nguyên tắc bắt buộc implement:

1. **Bút toán cân bằng**
   Tổng Nợ = Tổng Có trong mỗi chứng từ.
   Validate ở service layer + PostgreSQL trigger làm lớp bảo vệ thứ hai.

2. **Khoá kỳ**
   Check ky_ke_toan.trang_thai = 'khoa' trước mọi thao tác INSERT/UPDATE/DELETE.
   Raise lỗi rõ ràng nếu chứng từ thuộc kỳ đã khoá.

3. **Cập nhật sổ cái tự động**
   Dùng PostgreSQL trigger AFTER INSERT/UPDATE/DELETE ON dinh_khoan
   để cập nhật phat_sinh_no / phat_sinh_co trên bảng so_cai.

4. **Kết chuyển cuối kỳ**
   Service tự động sinh bút toán kết chuyển sang TK 911 cuối mỗi kỳ.

5. **Hệ thống tài khoản**
   Seed đầy đủ danh mục TK theo TT99/2025 (TK 111 → 911)
   bằng Alembic data migration, không hardcode trong code.

6. **Ngoại tệ**
   Xử lý chênh lệch tỷ giá hạch toán vào TK 413.
   Lưu cả so_tien (VND) và so_tien_nt + ty_gia.

7. **Sinh số chứng từ an toàn**
   Dùng pg_advisory_lock() để tránh trùng lặp khi nhiều user
   tạo chứng từ đồng thời.
   Format: [LoạiCT][YYYY][MM]-[00001]
   Ví dụ: PC202501-00001 | PT202501-00001 | BN202501-00001

### PostgreSQL-specific optimizations:
- pg_advisory_lock() khi sinh số chứng từ
- CTE (WITH clause) cho query báo cáo phức tạp
- WINDOW FUNCTION tính số dư lũy kế trên sổ cái
- Materialized View cho báo cáo nặng, refresh định kỳ qua Celery

Ví dụ query sổ cái dùng Window Function:
```sql
SELECT
    ngay_ct,
    so_ct,
    dien_giai,
    so_tien_no,
    so_tien_co,
    SUM(so_tien_no - so_tien_co) OVER (
        PARTITION BY ma_tk
        ORDER BY ngay_ct, id
        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    ) AS so_du_luy_ke
FROM accounting.v_so_cai_chi_tiet
WHERE ma_tk = :ma_tk
  AND ngay_ct BETWEEN :tu_ngay AND :den_ngay;
```

### Báo cáo tài chính theo TT99/2025:
- B01-DN : Bảng cân đối kế toán
- B02-DN : Báo cáo kết quả hoạt động kinh doanh
- B03-DN : Báo cáo lưu chuyển tiền tệ (trực tiếp + gián tiếp)
- B09-DN : Bảng cân đối số phát sinh
- Sổ cái tài khoản
- Sổ chi tiết công nợ

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
## YÊU CẦU OUTPUT — THỰC HIỆN THEO THỨ TỰ

**BƯỚC 1 — Cấu trúc thư mục**
Tạo toàn bộ thư mục và file rỗng bằng lệnh PowerShell
(không dùng mkdir -p hay lệnh Linux).

**BƯỚC 2 — Core files (code đầy đủ, không placeholder)**
- app/__init__.py             (Application Factory)
- app/extensions.py
- app/config.py              (DATABASE_URL dạng postgresql+psycopg2://...)
- app/modules/he_thong_tk/models.py
- app/modules/nhat_ky/models.py      (ChungTu + DinhKhoan)
- app/modules/nhat_ky/routes.py      (CRUD + duyệt + huỷ)
- app/modules/nhat_ky/services.py    (validate bút toán + advisory lock)
- app/modules/nhat_ky/schemas.py     (Marshmallow)
- app/utils/so_hieu.py               (pg_advisory_lock để sinh số CT)

**BƯỚC 3 — Migration & Seed**
- Alembic migration tạo schema + toàn bộ bảng
- Seed đầy đủ hệ thống tài khoản TT99/2025
- PostgreSQL trigger cập nhật sổ cái tự động

**BƯỚC 4 — Config files**
- requirements.txt  (psycopg2-binary, waitress cho Windows dev)
- .env.example
- docker-compose.dev.yml
- scripts/dev.ps1   (PowerShell thay Makefile)

**BƯỚC 5 — Hướng dẫn khởi chạy trên Windows 10**
Viết từng bước bằng lệnh PowerShell:
1. Khởi động Docker Desktop → chạy docker-compose
2. Tạo virtualenv và kích hoạt
3. Cài requirements
4. Chạy flask db upgrade
5. Chạy flask seed-data
6. Chạy flask run (dùng waitress cho Windows)
7. Chạy Celery worker với --pool=solo

Yêu cầu code: clean, có docstring tiếng Việt, tuân thủ PEP8,
sẵn sàng production khi deploy lên Linux server.
```

---

## 🔀 QUY TẮC GIT WORKFLOW (WINDOWS 10)

Git credential đã được setup sẵn trên máy, không cần nhập username/password.
Áp dụng nguyên tắc: **làm xong phần nào → commit + push ngay phần đó**.
Không gộp nhiều phần vào một commit lớn.

### Cấu trúc nhánh:
```
main        ← production-ready, chỉ merge từ develop
develop     ← nhánh tích hợp chính
feature/*   ← mỗi module/tính năng một nhánh riêng
hotfix/*    ← sửa lỗi khẩn
```

### Khởi tạo repo lần đầu (PowerShell):
```powershell
git init
git add .gitignore README.md
git commit -m "chore: khoi tao project"
git branch -M main
git remote add origin https://github.com/<username>/<repo>.git
git push -u origin main

git checkout -b develop
git push -u origin develop
```

### File .gitignore bắt buộc có:
```gitignore
# Python
.venv/
__pycache__/
*.pyc
*.pyo
*.egg-info/
dist/
build/

# Môi trường
.env
.env.local
.env.*.local

# IDE
.vscode/settings.json
.idea/

# OS Windows
Thumbs.db
Desktop.ini
$RECYCLE.BIN/

# Migration cache
migrations/__pycache__/
```

### Quy tắc commit message (Conventional Commits):
```
feat(module):     thêm tính năng mới
fix(module):      sửa lỗi
chore:            cấu hình, setup, không ảnh hưởng logic
refactor(module): tái cấu trúc code
test(module):     thêm/sửa test
docs:             cập nhật tài liệu
db(module):       thêm migration, thay đổi schema
```

### Workflow chuẩn cho từng module (PowerShell):
```powershell
# 1. Tạo nhánh feature từ develop
git checkout develop
git checkout -b feature/he-thong-tai-khoan

# 2. Làm từng phần nhỏ → commit ngay sau khi xong
git add app/modules/he_thong_tk/models.py
git commit -m "feat(he_thong_tk): them model HeThongTaiKhoan SQLAlchemy 2.x"

git add app/modules/he_thong_tk/routes.py
git commit -m "feat(he_thong_tk): them CRUD API routes"

git add app/modules/he_thong_tk/schemas.py
git commit -m "feat(he_thong_tk): them Marshmallow schema validation"

git add migrations/versions/xxx_he_thong_tk.py
git commit -m "db(he_thong_tk): migration tao bang he_thong_tai_khoan"

# 3. Push nhánh feature
git push -u origin feature/he-thong-tai-khoan

# 4. Hoàn thành module → merge vào develop
git checkout develop
git merge --no-ff feature/he-thong-tai-khoan
git push origin develop

# 5. Xoá nhánh feature
git branch -d feature/he-thong-tai-khoan
git push origin --delete feature/he-thong-tai-khoan
```

### Lịch trình git push theo tiến độ dự án:
```
[BƯỚC 1]  Scaffold xong          → "chore: tao cau truc thu muc project"
[BƯỚC 2]  extensions + config    → "chore: cau hinh Flask app factory PostgreSQL Celery"
[BƯỚC 3]  Auth module xong       → "feat(auth): JWT login phan quyen"
[BƯỚC 4]  Danh mục xong          → "feat(danh_muc): doi tuong hang hoa ngan hang"
[BƯỚC 5]  Hệ thống TK xong       → "feat(he_thong_tk): model API seed TT99"
[BƯỚC 6]  Migration + seed xong  → "db: migration toan bo schema seed tai khoan TT99"
[BƯỚC 7]  Nhật ký xong           → "feat(nhat_ky): chung tu dinh khoan duyet huy"
[BƯỚC 8]  Mỗi module tiếp theo   → commit ngay sau khi module pass test cơ bản
[BƯỚC 9]  Báo cáo xong           → "feat(bao_cao): B01 B02 B03 B09 theo TT99"
[BƯỚC 10] Hotfix bất kỳ lúc nào  → nhánh hotfix/* → merge cả main lẫn develop
```

### Cập nhật dev.ps1 tích hợp git shortcuts:
```powershell
# scripts\dev.ps1
param([string]$Command, [string]$Message = "")
switch ($Command) {
    # Dev commands
    "run"     { flask run --host=0.0.0.0 --port=5000 }
    "migrate" { flask db upgrade }
    "seed"    { flask seed-data }
    "worker"  { celery -A app.celery worker --pool=solo --loglevel=info }
    "test"    { pytest tests/ -v }
    "docker"  { docker compose -f docker-compose.dev.yml up -d }

    # Git shortcuts — làm tới đâu push tới đó
    "save"    {
                if ($Message -eq "") { Write-Host "Dung: .\dev.ps1 save 'commit message'"; return }
                git add .
                git commit -m $Message
                git push
                Write-Host "✅ Pushed: $Message"
              }
    "sync"    { git pull origin develop }
    "status"  { git status }
    "log"     { git log --oneline -10 }

    default   {
        Write-Host "Dev : run | migrate | seed | worker | test | docker"
        Write-Host "Git : save '<message>' | sync | status | log"
    }
}

# Ví dụ dùng hàng ngày:
# .\scripts\dev.ps1 save "feat(nhat_ky): them validate but toan can bang"
# .\scripts\dev.ps1 sync
# .\scripts\dev.ps1 log
```

---

## 📝 Gợi ý chia nhỏ khi AI bị giới hạn token

| Lần | Nội dung gửi thêm |
|-----|-------------------|
| 1 | Toàn bộ prompt trên → nhận scaffold + core modules |
| 2 | "Tiếp tục sinh module tien, cong_no, hang_ton_kho theo đúng pattern đã tạo" |
| 3 | "Sinh module tai_san (TSCĐ + khấu hao) và module luong (lương + BHXH)" |
| 4 | "Sinh logic báo cáo B01-DN, B02-DN, B03-DN với công thức ánh xạ tài khoản TT99/2025" |
| 5 | "Viết test cases cho module nhat_ky: validate cân bằng Nợ-Có, khoá kỳ, sinh số CT" |

## ⚠️ 3 lưu ý quan trọng nhất khi dev trên Windows 10

1. **Celery** bắt buộc chạy `--pool=solo` — Windows không hỗ trợ `fork()`
2. **psycopg2-binary** thay vì `psycopg2` — tránh lỗi compile C, không cần Visual C++ Build Tools
3. **Redis qua Docker Desktop** — Redis không còn maintain bản native Windows
