## 🗺️ AMS System Design - ERD, API, Design Decisions v1.0
**Version:** 1.0 | **Updated:** 24/03/2026 | **.NET:** 10 | **EF Core:** 10 | **PostgreSQL:** 16

## 🗺️ ERD-Overview
| Unnamed: 0 | AMS — Entity Relationship Overview (Schema Map) | Unnamed: 2 | Unnamed: 3 | Unnamed: 4 | Unnamed: 5 | Unnamed: 6 | Unnamed: 7 | Unnamed: 8 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| NaN | Table (PascalCase) | Schema | PK Type | Row count est. | Key FKs | Description | Indexes | Constraints |
| NaN | ChartOfAccounts | acc | UUID | ~800 | ParentAccountId (self) | TKKT theo TT 99/2025. Cấu trúc cây đa cấp. | AccountCode (UNIQUE), ParentAccountId, IsActive | CHECK AccountLevel IN (1,2,3,4) |
| NaN | Customers | acc | UUID | ~5,000 | – | Danh mục khách hàng. Kiểm tra MST. | TaxCode (UNIQUE), CustomerGroup | NaN |
| NaN | Vendors | acc | UUID | ~2,000 | – | Danh mục nhà cung cấp. | TaxCode, IsForeignContractor | NaN |
| NaN | Products | acc | UUID | ~3,000 | – | Hàng hoá/dịch vụ. | ProductCode (UNIQUE), ProductGroup, IsExciseTaxItem | NaN |
| NaN | Warehouses | acc | UUID | ~20 | – | Kho hàng. Phương pháp giá xuất (FIFO/BQGQ). | WarehouseCode (UNIQUE) | CHECK PricingMethod IN ('FIFO','AVCO') |
| NaN | Employees | acc | UUID | ~500 | – | Nhân viên. Liên kết AD username. | ADUsername (UNIQUE), DepartmentId | NaN |
| NaN | Banks | acc | UUID | ~30 | – | Tài khoản ngân hàng. Multi-currency. | AccountNumber (UNIQUE), CurrencyCode | NaN |
| NaN | ExchangeRates | acc | INT IDENTITY | ~10,000/year | – | Tỷ giá ngoại tệ theo ngày. | (CurrencyCode, RateDate) UNIQUE | NaN |
| NaN | FiscalPeriods | acc | UUID | ~120 (10yr) | FiscalYearId | Kỳ kế toán tháng. | (Year, Month) UNIQUE | CHECK Status IN ('OPEN','CLOSED','LOCKED') |
| NaN | Vouchers | acc | UUID | ~50,000/year | FiscalPeriodId | Chứng từ kế toán – bảng trung tâm. | VoucherDate, Status, VoucherType | CHECK Status IN (5 values); TotalDebit=TotalCredit khi APPROVED |
| NaN | VoucherLines | acc | UUID | ~200,000/year | VoucherId, AccountId, CustomerId?, VendorId? | Dòng định khoản của CT. | VoucherId (FK+IDX), AccountId (IDX) | CHECK DebitAmt>=0, CreditAmt>=0 |
| NaN | VoucherAttachments | acc | UUID | ~30,000/year | VoucherId | File đính kèm CT (UNC path). | VoucherId (IDX) | NaN |
| NaN | LedgerEntries | acc | UUID | ~250,000/year | VoucherId, AccountId, FiscalPeriodId | Sổ cái – append only sau Post. | AccountId+Date (IDX), FiscalPeriodId (IDX) | NO UPDATE/DELETE enforced |
| NaN | InventoryTransactions | acc | UUID | ~100,000/year | VoucherId, ProductId, WarehouseId | NXK kho. | ProductId+Date (IDX) | CHECK TransactionType IN ('IN','OUT','ADJ') |
| NaN | InventoryBalance | acc | UUID | ~3,000 | ProductId, WarehouseId | Số dư tồn kho real-time. | (ProductId,WarehouseId) UNIQUE | NaN |
| NaN | FixedAssets | acc | UUID | ~500 | – | Tài sản cố định. | AssetCode (UNIQUE), AssetGroup, Status | CHECK Status IN ('ACTIVE','REPAIR','IDLE','DISPOSED') |
| NaN | DepreciationSchedule | acc | UUID | ~6,000 | AssetId | Lịch khấu hao theo tháng. | AssetId+Period (UNIQUE) | NaN |
| NaN | TaxRates | tax | INT IDENTITY | ~50 | – | Tham số thuế suất – KHÔNG hardcode. Key bảng tham số. | TaxRateKey (UNIQUE), EffectiveFrom (IDX) | CHECK Rate BETWEEN 0 AND 1 |
| NaN | PITBrackets | tax | INT IDENTITY | ~10 (2 versions) | – | Biểu thuế TNCN. 5 bậc từ 01/01/2026. | (BracketNo, EffectiveFrom) UNIQUE | CHECK TaxRate BETWEEN 0 AND 1 |
| NaN | PITAllowances | tax | UUID | ~500 | EmployeeId | GTGC bản thân + NPT theo NV. | EmployeeId (IDX) | NaN |
| NaN | ExciseTaxItems | tax | UUID | ~50 | ProductId (FK) | Hàng TTĐB + lộ trình tăng TS. | ProductId (UNIQUE FK) | CHECK CurrentRate >= 0 |
| NaN | VATInputRegister | tax | UUID | ~50,000/year | VoucherId, VendorId | Sổ thuế GTGT mua vào (BK 02/GTGT). | (FiscalPeriod, VendorId) IDX | NaN |
| NaN | VATOutputRegister | tax | UUID | ~50,000/year | VoucherId, CustomerId | Sổ thuế GTGT bán ra (BK 01/GTGT). | (FiscalPeriod, CustomerId) IDX | NaN |
| NaN | TaxDeclarations | tax | UUID | ~100/year | – | Tờ khai thuế + XML lưu trữ. | (TaxType, Period) UNIQUE | CHECK Status IN ('DRAFT','SUBMITTED','ACCEPTED','AMENDED') |
| NaN | TaxPayments | tax | UUID | ~100/year | TaxDeclarationId, VoucherId | Theo dõi nộp thuế. | TaxDeclarationId (IDX) | NaN |
| NaN | TaxCalendars | tax | INT IDENTITY | ~50/year | – | Lịch deadline khai thuế. | (TaxType, PeriodType, Year) IDX | NaN |
| NaN | CITAdjustments | tax | UUID | ~10,000/year | VoucherLineId | Điều chỉnh CP không được trừ TNDN. | VoucherLineId (FK), FiscalYear (IDX) | NaN |
| NaN | CITLossCarryForward | tax | INT IDENTITY | ~10/year | – | Chuyển lỗ TNDN tối đa 5 năm. | FiscalYear (UNIQUE) | NaN |
| NaN | WithholdingTax | tax | UUID | ~500/year | VendorId | Thuế nhà thầu NT (Optional module). | VendorId (IDX), FiscalPeriod (IDX) | NaN |
| NaN | AuditLogs | audit | BIGINT IDENTITY | ~500,000/year | – | Nhật ký bất biến. DENY UPDATE/DELETE. | EventTime (IDX), UserName (IDX), TableName+RecordId (IDX) | Append-only. DENY UPDATE,DELETE TO PUBLIC |
| NaN | AppSettings | cfg | VARCHAR(100) | ~50 | – | Cấu hình hệ thống. DatabaseProvider switch. | SettingKey (PK UNIQUE) | NaN |
| NaN | NumberSequences | cfg | UUID | ~20 | – | Số thứ tự tự sinh theo loại CT/kỳ. | (SequenceType, FiscalPeriodId) UNIQUE | NaN |
| NaN | OutboxMessages | cfg | UUID | ~5,000/day | – | Outbox Pattern cho Domain Events reliable delivery. | ProcessedAt IS NULL (IDX for polling) | NaN |

## 📐 DDL-Key-Tables
| Unnamed: 0 | AMS — DDL Song Song: SQL Server 2022 vs PostgreSQL 16 | Unnamed: 2 | Unnamed: 3 | Unnamed: 4 | Unnamed: 5 | Unnamed: 6 | Unnamed: 7 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| NaN | Table.Column | SS DataType | PG DataType | NULL? | Default | Mô tả | Ghi chú kỹ thuật |
| NaN | Vouchers.VoucherId | UNIQUEIDENTIFIER | UUID | NOT NULL | SS: NEWSEQUENTIALID()\nPG: gen\_random\_uuid() | PK chứng từ | EF Core: [DatabaseGenerated(DatabaseGeneratedOption.Identity)] |
| NaN | Vouchers.VoucherNo | NVARCHAR(25) | VARCHAR(25) | NOT NULL | – | Số CT: PT-2026-01-00001 | UNIQUE constraint (per FiscalPeriod). App generates via NumberSequence. |
| NaN | Vouchers.VoucherType | NVARCHAR(10) | VARCHAR(10) | NOT NULL | – | Loại: PT/PC/BC/BN/… | CHECK (11 values). EF Core: enum mapping. |
| NaN | Vouchers.VoucherDate | DATE | DATE | NOT NULL | – | Ngày CT | IDX: non-clustered/BTREE |
| NaN | Vouchers.Status | NVARCHAR(15) | VARCHAR(15) | NOT NULL | 'DRAFT' | State machine status | CHECK 5 values. EF Core: HasConversion<string>() |
| NaN | Vouchers.TotalDebit | DECIMAL(18,0) | NUMERIC(18,0) | NOT NULL | 0 | Tổng Nợ VNĐ | App enforce TotalDebit=TotalCredit khi Submit |
| NaN | Vouchers.CurrencyCode | CHAR(3) | CHAR(3) | NOT NULL | 'VND' | ISO 4217 | FK → lookup table currencies |
| NaN | Vouchers.ExchangeRate | DECIMAL(18,4) | NUMERIC(18,4) | NOT NULL | 1 | Tỷ giá quy đổi | DEFAULT 1 nếu VND |
| NaN | Vouchers.RowVersion | ROWVERSION | (xmin system col) | NOT NULL | auto | Optimistic concurrency | SS: EF Core [Timestamp]. PG: EF Core IsRowVersion() on xmin |
| NaN | Vouchers.IsDeleted | BIT | BOOLEAN | NOT NULL | SS: 0\nPG: FALSE | Soft delete | EF Core Global Query Filter: HasQueryFilter(v=>!v.IsDeleted) |
| NaN | Vouchers.CreatedAt | datetime2(7) | TIMESTAMPTZ | NOT NULL | SS: SYSUTCDATETIME()\nPG: now() | Audit – UTC | EF Core: ValueGeneratedOnAdd |
| NaN | Vouchers.CreatedBy | NVARCHAR(100) | VARCHAR(100) | NOT NULL | – | AD username creator | Set by AuditInterceptor from HttpContext.User |
| NaN | VoucherLines.VoucherId | UNIQUEIDENTIFIER | UUID | NOT NULL | – | FK → Vouchers | ON DELETE CASCADE (lines go with voucher) |
| NaN | VoucherLines.AccountId | UNIQUEIDENTIFIER | UUID | NOT NULL | – | FK → ChartOfAccounts | Only IsDetailAccount=true accounts allowed |
| NaN | VoucherLines.DebitAmount | DECIMAL(18,0) | NUMERIC(18,0) | NOT NULL | 0 | Số tiền Nợ VNĐ | CHECK DebitAmount >= 0 |
| NaN | VoucherLines.CreditAmount | DECIMAL(18,0) | NUMERIC(18,0) | NOT NULL | 0 | Số tiền Có VNĐ | CHECK CreditAmount >= 0 |
| NaN | VoucherLines.IsExciseTaxLine | BIT | BOOLEAN | NOT NULL | SS:0/PG:FALSE | Dòng thuế TTĐB | When true, linked to ExciseTaxRegister |
| NaN | VoucherLines.CITAdjFlag | NVARCHAR(50) | VARCHAR(50) | NaN | NaN | Loại CP không được trừ | GIFT/MEAL/PERSONAL/INTEREST\_EXCESS/… |
| NaN | TaxRates.TaxRateKey | NVARCHAR(50) | VARCHAR(50) | NOT NULL | – | GTGT\_STANDARD, CIT\_SME… | UNIQUE. App reads by key + date. |
| NaN | TaxRates.Rate | DECIMAL(5,4) | NUMERIC(5,4) | NOT NULL | – | 0.08 = 8% | CHECK BETWEEN 0 AND 1. Never hardcode in code. |
| NaN | TaxRates.EffectiveFrom | DATE | DATE | NOT NULL | – | Ngày bắt đầu áp dụng | IDX. GetRateAt(key,date): WHERE key=? AND EffectiveFrom<=date AND (EffectiveTo IS NULL OR EffectiveTo>date) |
| NaN | PITBrackets.FromAmount | BIGINT | BIGINT | NOT NULL | – | Thu nhập từ (VNĐ/tháng) | Bậc 1: 0. Bậc 2: 10\_000\_000. v.v. |
| NaN | PITBrackets.ToAmount | BIGINT | BIGINT | NaN | NaN | Thu nhập đến – NULL=bậc cao nhất | Bậc 5: NULL (>200M) |
| NaN | PITBrackets.QuickDeduction | BIGINT | BIGINT | NOT NULL | – | Số tiền trừ nhanh | NaN |
| NaN | ExciseTaxItems.ScheduledRates | NVARCHAR(MAX) | JSONB | NaN | NaN | Lộ trình tăng TS | Format: [{"year":2027,"rate":0.70},{"year":2028,"rate":0.75}]. PG: JSONB indexable. |
| NaN | AuditLogs.AuditLogId | BIGINT IDENTITY(1,1) | BIGINT GENERATED ALWAYS AS IDENTITY | NOT NULL | auto | PK – thứ tự thời gian | BIGINT thay GUID để đảm bảo chronological sort. 1 tỷ rows ~ 1000 năm. |
| NaN | AuditLogs.OldValues | NVARCHAR(MAX) | JSONB | NaN | NaN | JSON giá trị cũ | SS: JSON\_VALUE() để query. PG: JSONB operators @>, ->> trực tiếp. |
| NaN | AuditLogs.NewValues | NVARCHAR(MAX) | JSONB | NaN | NaN | JSON giá trị mới | PG advantage: can query 'show all changes to Rate field across history' với JSONB. |

## 🔌 API-Register
| Unnamed: 0 | AMS — REST API Register v1.0 (Danh Sách Endpoint + HTTP Method) | Unnamed: 2 | Unnamed: 3 | Unnamed: 4 | Unnamed: 5 | Unnamed: 6 | Unnamed: 7 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| NaN | Method | Endpoint | Mô tả | Request Body / Params | Response | Auth Role | Tags |
| NaN | GET | /api/v1/vouchers | Danh sách CT với filter | ?type=PT&status=POSTED&from=2026-01-01&to=2026-01-31&page=1&size=50 | 200: {items:[], total, page} | KeToanVien+ | Vouchers |
| NaN | GET | /api/v1/vouchers/{id} | Chi tiết CT + Lines + Attachments | – | 200: VoucherDetailDto | KeToanVien+ | Vouchers |
| NaN | POST | /api/v1/vouchers | Tạo CT mới (DRAFT) | CreateVoucherCommand {type, date, description, lines[]} | 201: {id, voucherNo} | KeToanVien, ThuQuy | Vouchers |
| NaN | PUT | /api/v1/vouchers/{id} | Cập nhật CT (DRAFT only) | UpdateVoucherCommand {description, lines[]} | 200: VoucherDto | KeToanVien, ThuQuy | Vouchers |
| NaN | POST | /api/v1/vouchers/{id}/submit | Submit CT → PENDING | – | 200: {status:'PENDING'} | KeToanVien, ThuQuy | Vouchers |
| NaN | POST | /api/v1/vouchers/{id}/approve | Duyệt CT → APPROVED | – | 200: {status:'APPROVED'} | KeToanTruong | Vouchers |
| NaN | POST | /api/v1/vouchers/{id}/reject | Từ chối CT → DRAFT | {reason: string} | 200: {status:'DRAFT'} | KeToanTruong | Vouchers |
| NaN | POST | /api/v1/vouchers/{id}/post | Post CT → POSTED (ghi sổ) | – | 200: {status:'POSTED', ledgerEntriesCount} | KeToanTruong | Vouchers |
| NaN | POST | /api/v1/vouchers/{id}/reverse | Tạo CT đảo ngược | – | 201: {newVoucherId, newVoucherNo} | KeToanTruong | Vouchers |
| NaN | POST | /api/v1/vouchers/{id}/clone | Clone CT → CT mới DRAFT | – | 201: {newVoucherId, newVoucherNo} | KeToanVien | Vouchers |
| NaN | POST | /api/v1/vouchers/import | Import hàng loạt từ Excel | multipart/form-data: file | 200: {imported, errors[]} | KeToanVien | Vouchers |
| NaN | DELETE | /api/v1/vouchers/{id} | Soft-delete CT (DRAFT only) | – | 204 No Content | KeToanVien | Vouchers |
| NaN | GET | /api/v1/vouchers/{id}/audit | Audit trail của CT | – | 200: AuditLogEntry[] | KeToanTruong, KiemToanNB | Vouchers,Audit |
| NaN | GET | /api/v1/ledger/accounts | Danh mục TKKT (có filter) | ?level=3&type=BALANCE\_SHEET&active=true | 200: AccountDto[] | KeToanVien+ | Ledger |
| NaN | GET | /api/v1/ledger/general-ledger/{accountId} | Sổ Cái một TK | ?from=2026-01-01&to=2026-01-31 | 200: LedgerDto | KeToanVien+ | Ledger |
| NaN | GET | /api/v1/ledger/journal | Sổ Nhật Ký Chung | ?from=2026-01-01&to=2026-01-31&page=1 | 200: JournalDto | KeToanVien+ | Ledger |
| NaN | GET | /api/v1/ledger/trial-balance | Bảng CĐTK | ?period=2026-01 | 200: TrialBalanceDto | KeToanVien+ | Ledger |
| NaN | GET | /api/v1/ledger/periods | Danh sách kỳ kế toán | ?year=2026 | 200: FiscalPeriodDto[] | KeToanVien+ | Ledger |
| NaN | POST | /api/v1/ledger/periods/{id}/close | Khoá kỳ tháng (8 bước) | – | 200: {status:'CLOSED', steps[]} | KeToanTruong | Ledger |
| NaN | GET | /api/v1/reports/bctc/b01/{period} | Bảng CĐKT B01-DN | – | 200: B01Dto | KeToanTruong, GiamDoc | Reports |
| NaN | GET | /api/v1/reports/bctc/b02/{period} | BC KQKD B02-DN | – | 200: B02Dto | KeToanTruong, GiamDoc | Reports |
| NaN | GET | /api/v1/reports/bctc/b03/{year} | BC LCTT B03-DN | – | 200: B03Dto | KeToanTruong | Reports |
| NaN | GET | /api/v1/reports/dashboard | Dashboard BI aggregates | ?period=2026-Q1 | 200: DashboardDto | KeToanTruong, GiamDoc, TaxOfficer | Reports |
| NaN | GET | /api/v1/reports/{type}/export | Export PDF/Excel/Word | ?format=pdf&period=2026-01 | 200: file binary | KeToanVien+ | Reports |
| NaN | GET | /api/v1/tax/gtgt/input-register | Sổ GTGT mua vào | ?period=2026-01 | 200: VATRegisterDto[] | TaxOfficer, KeToanVien | Tax,GTGT |
| NaN | GET | /api/v1/tax/gtgt/output-register | Sổ GTGT bán ra | ?period=2026-01 | 200: VATRegisterDto[] | TaxOfficer, KeToanVien | Tax,GTGT |
| NaN | POST | /api/v1/tax/gtgt/declaration | Tạo tờ khai 01/GTGT | {period:'2026-01'} | 201: TaxDeclarationDto | TaxOfficer | Tax,GTGT |
| NaN | GET | /api/v1/tax/gtgt/declaration/{period}/xml | Xuất XML iTaxViewer | – | 200: XML file | TaxOfficer, KeToanTruong | Tax,GTGT |
| NaN | GET | /api/v1/tax/cit/provisional/{quarter} | Tạm tính TNDN | – | 200: CITProvisionalDto | TaxOfficer | Tax,TNDN |
| NaN | POST | /api/v1/tax/cit/finalization/{year} | Quyết toán TNDN năm | – | 200: CITFinalizationDto | TaxOfficer, KeToanTruong | Tax,TNDN |
| NaN | GET | /api/v1/tax/pit/monthly/{period} | Khấu trừ TNCN tháng | – | 200: PITMonthlyDto[] | TaxOfficer, KeToanVien | Tax,TNCN |
| NaN | POST | /api/v1/tax/pit/finalization/{year} | Quyết toán TNCN năm | – | 201: PITFinalizationDto | TaxOfficer | Tax,TNCN |
| NaN | GET | /api/v1/tax/excise/register/{period} | Sổ thuế TTĐB | ?period=2026-01 | 200: ExciseRegisterDto[] | TaxOfficer | Tax,TTDB |
| NaN | POST | /api/v1/tax/excise/declaration/{period} | Tờ khai 01/TTĐB | {period:'2026-01'} | 201: TaxDeclarationDto | TaxOfficer | Tax,TTDB |
| NaN | GET | /api/v1/tax/calendar | Lịch khai báo thuế | ?year=2026 | 200: TaxCalendarDto[] | TaxOfficer, KeToanTruong | Tax |
| NaN | GET | /api/v1/tax/rates | Tất cả thuế suất hiện tại | – | 200: TaxRateDto[] | TaxOfficer, KeToanTruong | Tax,Config |
| NaN | GET | /api/v1/inventory/balance | Tồn kho hiện tại | ?productId=…&warehouseId=… | 200: InventoryBalanceDto[] | KeToanVien+ | Inventory |
| NaN | GET | /api/v1/inventory/nxt | Báo cáo NXT kho | ?period=2026-01&warehouseId=… | 200: NXTReportDto | KeToanVien+ | Inventory |
| NaN | POST | /api/v1/inventory/stocktake | Tạo phiên kiểm kê | {warehouseId, date} | 201: StockTakeId | KeToanVien, KeToanTruong | Inventory |
| NaN | GET | /api/v1/assets | Danh sách TSCĐ | ?status=ACTIVE&group=… | 200: AssetDto[] | KeToanVien+ | Assets |
| NaN | GET | /api/v1/assets/{id}/depreciation | Lịch khấu hao | – | 200: DepreciationScheduleDto[] | KeToanVien+ | Assets |
| NaN | GET | /api/v1/assets/depreciation-report/{period} | Bảng KH tháng (Mẫu 06-TSCĐ) | – | 200: DepreciationReportDto | KeToanVien+ | Assets |
| NaN | GET | /api/v1/ar/aging | Aging report phải thu | ?asOf=2026-03-23 | 200: AgingReportDto | KeToanTruong | AR |
| NaN | GET | /api/v1/ap/aging | Aging report phải trả | ?asOf=2026-03-23 | 200: AgingReportDto | KeToanTruong | AP |
| NaN | GET | /api/v1/ar/ledger/{customerId} | Sổ chi tiết KH | ?from=…&to=… | 200: CustomerLedgerDto | KeToanVien+ | AR |
| NaN | GET | /api/v1/config/tax-rates | Tham số thuế suất | – | 200: TaxRateConfigDto[] | KeToanTruong, ITAdmin | Config |
| NaN | PUT | /api/v1/config/tax-rates/{key} | Cập nhật thuế suất | {rate, effectiveFrom, legalBasis} | 200: TaxRateDto | KeToanTruong, ITAdmin | Config |
| NaN | GET | /api/v1/config/pit-brackets | Biểu thuế TNCN | ?effectiveDate=2026-01-01 | 200: PITBracketDto[] | KeToanTruong, ITAdmin | Config |
| NaN | PUT | /api/v1/config/pit-brackets | Cập nhật biểu TNCN | {brackets[], effectiveFrom, legalBasis} | 200: PITBracketDto[] | KeToanTruong, ITAdmin | Config |
| NaN | GET | /api/v1/audit-logs | Query audit log | ?user=…&table=…&from=…&action=POST | 200: AuditLogDto[] | ITAdmin, KeToanTruong | Audit |
| NaN | GET | /api/v1/health/db | DB health check | – | 200: {status, responseMs, dbVersion, connections} | ITAdmin, DBAdmin | Health |
| NaN | GET | /api/v1/health/full | Full health (DB+SMTP+FileServer) | – | 200: {components:{}} | ITAdmin | Health |
| NaN | POST | /api/v1/jobs/trigger/{jobName} | Trigger Hangfire job thủ công | – | 202 Accepted: {jobId} | ITAdmin, DBAdmin | Jobs |
| NaN | GET | /api/v1/jobs/history | Lịch sử Hangfire jobs | ?from=…&status=Succeeded | 200: HangfireJobDto[] | ITAdmin, DBAdmin | Jobs |
| NaN | GET | /api/v1/users/me | Thông tin user hiện tại (từ AD) | – | 200: {username, displayName, roles[]} | All authenticated | Users |

## ⚙️ StoredProcs
| Unnamed: 0 | AMS — Stored Procedures / Functions (T-SQL + PL/pgSQL) | Unnamed: 2 | Unnamed: 3 | Unnamed: 4 |
| --- | --- | --- | --- | --- |
| NaN | SP / Function Name | Lang | Mô tả & Thuật toán | Input → Output |
| NaN | SP\_PostVoucher / fn\_post\_voucher | Both | Ghi CT lên sổ cái trong 1 transaction: (1) validate Status=APPROVED + Period=OPEN, (2) INSERT LedgerEntries batch, (3) UPDATE account balances, (4) SET Status=POSTED, (5) INSERT AuditLog. Rollback toàn bộ nếu bất kỳ bước nào fail. | (voucherId UUID) → void / raises exception |
| NaN | SP\_CloseMonth / fn\_close\_month | Both | 8 bước khoá tháng. Có thể chạy từng bước độc lập. (1) Check 0 PENDING, (2) SP\_AllocatePrepaid, (3) SP\_DepreciateAssets, (4) SP\_CalcInventoryCost, (5) SP\_FxRevaluation, (6) SP\_CloseTransfer, (7) SELECT trial balance for review, (8) UPDATE FiscalPeriod.Status=CLOSED. | (periodId UUID, step INT = 0) → {step\_results[]} |
| NaN | SP\_AutoSwitchTaxRates / fn\_auto\_switch\_tax\_rates | Both | Idempotent. Đọc TaxRates WHERE EffectiveFrom = CURRENT\_DATE. UPDATE: set previous record EffectiveTo = yesterday. Key use: switch GTGT 8%→10% ngày 01/01/2027. Gửi signal cho Outbox Event. | () → {switched\_count, keys\_switched[]} |
| NaN | SP\_DepreciateAssets / fn\_depreciate\_assets | Both | Tính KH tháng cho mọi TS Status=ACTIVE. 3 phương pháp: (1) Đường thẳng: rate = 1/UsefulLife, (2) Sản lượng: output/totalOutput × cost, (3) Số dư giảm dần: BookValue × rate. INSERT DepreciationSchedule + tạo VoucherLines DRAFT. | (periodId UUID) → {assets\_processed, total\_depreciation\_amount} |
| NaN | SP\_CalcInventoryCost / fn\_calc\_inventory\_cost | Both | Tính giá xuất kho cuối kỳ. FIFO: truy xuất theo batch chronological. BQGQ: SUM(value)/SUM(qty) cuối tháng. UPDATE InventoryTransactions.UnitCost. INSERT VoucherLines giá vốn. | (periodId UUID, method VARCHAR) → {items\_processed} |
| NaN | SP\_FxRevaluation / fn\_fx\_revaluation | Both | Đánh giá lại TK ngoại tệ (112x, 131x, 331x). Lấy tỷ giá cuối kỳ, tính chênh lệch, hạch toán TK 515 (lãi) hoặc TK 635 (lỗ). Tạo CT PB DRAFT. | (periodId UUID) → {accounts\_revalued, total\_gain, total\_loss} |
| NaN | SP\_GenTrialBalance / fn\_gen\_trial\_balance | Both | Tổng hợp số dư Nợ/Có cho mọi TK trong kỳ. JOIN LedgerEntries. Dùng cho: (1) UI Trial Balance, (2) Weekly backup verify, (3) Month-end check. | (periodId UUID) → TABLE(accountCode, accountName, openingDr, openingCr, movementDr, movementCr, closingDr, closingCr) |
| NaN | SP\_CalcPIT\_Monthly / fn\_calc\_pit\_monthly | Both | Tính khấu trừ TNCN tháng: (1) Load PITBrackets theo date, (2) Load PITAllowances, (3) foreach employee: TNTT = Salary - BHXH - GTGC, (4) Apply biểu lũy tiến 5 bậc với QuickDeduction, (5) INSERT VoucherLines TK3335 DRAFT. | (periodId UUID) → {employees\_processed, total\_pit\_withheld} |
| NaN | SP\_CITFinalization / fn\_cit\_finalization | Both | QT TNDN năm: (1) Tổng DT năm → xác định TS 15%/20%, (2) Lấy CITAdjustments (CP không được trừ), (3) TNCT = DT - CP + CITAdj, (4) Trừ chuyển lỗ năm trước, (5) Thuế = TNCT × TS, (6) Return reconciliation data. | (fiscalYear INT) → TABLE(description, amount) |
| NaN | SP\_AllocatePrepaid / fn\_allocate\_prepaid | Both | Phân bổ CP trả trước (TK 142, 242): (1) Load danh sách khoản trả trước còn hiệu lực, (2) Tính phần phân bổ tháng = total / months, (3) INSERT VoucherLines CP DRAFT. | (periodId UUID) → {items\_allocated, total\_allocated} |
| NaN | SP\_OutboxProcess / fn\_outbox\_process | Both | Polling job mỗi 30 giây: (1) SELECT TOP 100 OutboxMessages WHERE ProcessedAt IS NULL ORDER BY CreatedAt, (2) For each: deserialize, publish to Hangfire event handler, (3) UPDATE ProcessedAt = now(). Idempotent via idempotencyKey. | () → {messages\_processed} |

## 🧠 Design-Decisions
| Unnamed: 0 | AMS — Architecture Decision Records (ADR) | Unnamed: 2 | Unnamed: 3 | Unnamed: 4 |
| --- | --- | --- | --- | --- |
| NaN | Decision | Lý do chọn | Phương án đã từ chối | Hệ quả / Trade-off |
| NaN | Clean Architecture thay Layered Architecture | Dependency rule: Domain không phụ thuộc EF/HTTP. Test domain logic không cần DB. Dễ swap DB provider (SS↔PG). | Traditional 3-tier (UI→BL→DB) | Project references phức tạp hơn. Cần abstract interface cho mọi infrastructure concern. |
| NaN | Feature Folders thay Type Folders | Mọi code của 1 feature (CT) nằm cùng thư mục: Commands, Queries, Handlers, Validators. Dễ navigate. | Type folders: /Controllers, /Services, /Repositories | Một số developer không quen. Cần convention rõ ràng. |
| NaN | CQRS (light) – không Event Sourcing | Command (write) và Query (read) tách riêng handler. Query có thể optimize read (join nhiều bảng). Không implement full Event Sourcing vì complexity không cần thiết. | Full CQRS + Event Sourcing | Không có full audit từ events. AuditLog thay thế. Không cần rebuild read models. |
| NaN | EF Core + Repository pattern thay Dapper | EF Core: type safety, migration, LINQ, global filters (soft-delete). Repository abstract DB provider. | Dapper (raw SQL) | EF Core N+1 query risk → cần careful Include(). Stored Procs vẫn dùng cho complex logic. |
| NaN | Outbox Pattern cho Domain Events | Reliable event delivery. VoucherPosted → SignalR + Email guaranteed, ngay cả khi app crash sau SaveChanges. | In-memory event bus (MediatR only) | Thêm bảng OutboxMessages. Eventual consistency (~30s delay). Hangfire poller overhead nhỏ. |
| NaN | Dual DB (SS + PG) qua provider switch | Zero code change khi switch. EF Core handles DDL difference. Admin chọn phù hợp license/infra. | Chỉ support SQL Server | PL/pgSQL khác T-SQL cho SP → cần test 2 lần. Migration phải ANSI SQL compatible. |
| NaN | Windows Auth + AD Groups thay Custom Auth | Không quản lý password. SSO trong intranet. Tích hợp với IT infrastructure sẵn có. | JWT + Identity Server | Chỉ hoạt động trong Windows domain. Không phù hợp nếu sau này cần public internet access. |
| NaN | Hangfire on DB thay Windows Service | Job state persist trong DB (SS/PG). Dashboard UI. Retry tự động. Không cần Windows Service riêng. | Windows Scheduler / Custom Service | Job store thêm tables trong DB. Hangfire dashboard cần bảo mật riêng. |
| NaN | Parameterized tax rates (DB) thay hardcode | Thay đổi luật thuế → chỉ update DB. Không cần deploy code. Support lộ trình tăng TS 2027-2031 tự động. | Hardcode trong appsettings.json | Cần UI để admin cập nhật. Cần validate input khi update (LegalBasis bắt buộc). |
| NaN | IMemoryCache thay Redis | On-premise, single server. Redis overkill cho ~50 users. IMemoryCache đủ performance cho load thực tế. | Redis distributed cache | Scale-out không được nếu sau này multi-server. Acceptable: BRD scope là single server. |

## ✅ NFR-Validation
| Unnamed: 0 | AMS — NFR → Design Decision Traceability | Unnamed: 2 | Unnamed: 3 | Unnamed: 4 | Unnamed: 5 |
| --- | --- | --- | --- | --- | --- |
| NaN | NFR-ID | Yêu cầu | Giải pháp thiết kế | Measurement | Status |
| NaN | NFR-P-001 | Page load < 2s / 50 users | EF Core query optimization. Global Query Filter. IDX trên FK, Date, Status. IMemoryCache cho TaxRates. Async/await mọi DB call. | Load test: 50 VU concurrent, P95 < 2s | Design OK |
| NaN | NFR-P-002 | BCTC năm < 10s | SSRS Snapshot reports. SP\_GenTrialBalance với indexed query. Pre-aggregation trong rpt schema snapshots. | Benchmark test với 1 năm data (50K vouchers) | Design OK |
| NaN | NFR-S-001 | Windows Auth SSPI only | IIS Windows Auth + Anonymous Auth disabled. Connection string: Integrated Security=SSPI. App startup check: reject SQL Login connections. | Test: thử SQL Login phải fail. Test: AD token bypass phải fail. | Design OK |
| NaN | NFR-S-003 | AuditLog bất biến | EF Core ISaveChangesInterceptor: auto-append AuditLog. DB DENY UPDATE/DELETE on audit.AuditLogs. | Test: attempt UPDATE via SQL → permission error. Check 0 update API exists. | Design OK |
| NaN | NFR-S-004 | HTTPS/TLS 1.2+ | IIS config: require TLS 1.2+. Disable TLS 1.0/1.1. HSTS header. HTTP → HTTPS 301. | SSL Labs test (internal): A rating. TLS 1.0 connection → refused. | Design OK |
| NaN | NFR-M-001 | Zero hardcoded tax rates | TaxRates / PITBrackets / ExciseTaxItems bảng DB. TaxEngine inject ITaxRateRepo. SP\_AutoSwitchTaxRates Hangfire. | Code review: grep cho hardcoded 0.08, 0.20, 0.15, 0.35 → 0 results. | Design OK |
| NaN | NFR-M-002 | RDLC template tách rời | SSRS .rdlc files lưu trong Windows File Server. ReportEngine load từ path (AppSettings). Không compile vào .dll. | Test: sửa RDLC, upload → report thay đổi không cần redeploy. | Design OK |
| NaN | NFR-DB-001 | Connection pool 5-100 | EF Core: Min Pool Size=5, Max Pool Size=100 trong connection string. | Monitor: sys.dm\_exec\_connections (SS) / pg\_stat\_activity (PG). | Design OK |
| NaN | NFR-DB-002 | SS vs PG < 20% variance | Identical EF Core queries. Identical indexes. Benchmark both providers same data. | Automated benchmark test suite run against both DBs. | Needs testing |
| NaN | NFR-DB-004 | Backup verify 100% | pgAgent/SQL Agent + RESTORE VERIFYONLY. Weekly sandbox restore test. BackupTestLog. | Alert nếu verify fail. DBAdmin check log hàng tuần. | Design OK |
| NaN | NFR-C-002 | FiscalPeriod lock blocks writes | FiscalPeriod.Status check trong SP\_PostVoucher + VoucherService. Middleware check FiscalPeriod trước mọi write API. | Test: attempt POST voucher khi period CLOSED → 403. | Design OK |
| NaN | NFR-C-003 | XML iTaxViewer valid | XMLSchema validation trong TaxDeclarationService trước khi xuất. Test với sample schema từ TCT. | Validate against official iTaxViewer XSD schema. 0 errors. | Design OK |