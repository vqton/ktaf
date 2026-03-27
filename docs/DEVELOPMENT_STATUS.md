# AMS Development Status

**Last Updated:** 2026-03-27 16:15  
**Project:** Accounting Management System (AMS)  
**Framework:** .NET 10 + Bootstrap 5.3 + jQuery  
**Database:** PostgreSQL 16

---

## Build Status

| Project | Status | Errors |
|---------|--------|--------|
| AMS.Domain | ✅ Pass | 0 |
| AMS.Application | ✅ Pass | 0 |
| AMS.Infrastructure | ✅ Pass | 0 |
| AMS.Web | ✅ Pass | 0 |
| AMS.Domain.Tests | ✅ Pass | 0 |

**Last Build:** ✅ PASSED - 2026-03-27 16:15 (4 warnings, 0 errors)
- Warnings: Package version constraints (ClosedXML requires DocumentFormat.OpenXml 2.x, resolved 3.x)

---

## Core Voucher Logic (2026-03-27)

### Implemented Features
1. **Voucher Workflow** - Full lifecycle: Draft → Pending → Approved → Posted → Reversed
2. **Voucher Validation** - Minimum 2 lines, Total Debit = Total Credit (ΣNợ = ΣCó)
3. **Ledger Posting** - Creates ledger entries when voucher is posted
4. **Account Balance Tracking** - Updates balances on voucher post
5. **FiscalPeriod Management** - Open/Close/Lock methods

### Working Pages
| Page | Status | Notes |
|------|--------|-------|
| Vouchers | ✅ HTTP 200 | Full workflow: Create → Submit → Approve → Post → Ledger |
| Customers | ✅ HTTP 200 | CRUD operations |
| ChartOfAccounts | ✅ HTTP 200 | Account listing |
| Products | ✅ HTTP 200 | Product management |
| Warehouses | ✅ HTTP 200 | CRUD operations |
| FiscalPeriods | ❌ HTTP 404 | Missing Controller/View |

### Database Updates
- Added FiscalPeriod.ClosedAt, ClosedBy properties
- Added AccountBalance audit columns (IsDeleted, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy)
- Updated DbCreate to include all entity properties
- Added PIT Brackets (5 bậc: 5%/10%/20%/30%/35% từ 01/01/2026)
- Added PIT Allowances (Giảm trừ bản thân 15.5tr, người phụ thuộc 6.2tr)
- Added Tax Rates (GTGT 8%/10%, CIT 15%/20%, EXCISE)
- Added Sample Chart of Accounts (TT 99/2025: 111, 112, 131, 141, 152, 211, 331, 333, 411, 511, 632, 641, 642)

---

## Recommended Next Steps (Priority Order)

1. **Fix Warehouses View** - ✅ DONE - Created Views/Warehouses/Index.cshtml
2. **Seed Tax Rates** - ✅ DONE - Inserted GTGT, TNDN, TNCN, EXCISE rates into tax.tax_rates
3. **Create FiscalPeriods View** - ⏳ PENDING - Views/FiscalPeriods/Index.cshtml needed
4. **Add Sample Chart of Accounts** - ✅ DONE - Seeded TT 99/2025 account structure (13 accounts)

---

## Database Schema Fixes (2026-03-27)

### Issues Resolved
1. **`chart_of_accounts.account_number` column missing**
   - Root cause: Entity had `AccountNumber` property but database script didn't include it
   - Fix: Added `account_number INT` column and additional ChartOfAccounts columns to DbCreate script

2. **`vouchers.RowVersion` column mapping issue**
   - Root cause: Entity had `RowVersion` property, database had `row_version`, but DbContext didn't map it
   - Fix: Added `entity.Property(e => e.RowVersion).HasColumnName("row_version").IsConcurrencyToken()` to Voucher configuration

3. **`fixed_assets` table missing**
   - Root cause: Table was not created in database script
   - Fix: Added complete `fixed_assets` table creation to DbCreate script

4. **`vat_input_registers` table missing**
   - Root cause: Table was not created and DbContext had no configuration for VATInputRegister
   - Fix: Added table creation to DbCreate script and full entity configuration to DbContext

### Changes Made
- `tools/dbcreate/DbCreate/Program.cs`:
  - Added database drop/recreate functionality
  - Added `account_number` and additional columns to `chart_of_accounts`
  - Added `fixed_assets` table with all columns
  - Added `vat_input_registers` table with all columns
- `src/AMS.Infrastructure/Data/AMSDbContext.cs`:
  - Added `RowVersion` mapping for Voucher entity
  - Added complete `VATInputRegister` entity configuration
  - Enhanced `FixedAsset` entity configuration with all property mappings

### Database Recreation
The database was dropped and recreated with all schema fixes applied:
```
✓ Database 'ams_db' dropped
✓ Database 'ams_db' created
✓ Schemas: acc, tax, audit, cfg, rpt
✓ Tables: fiscal_periods, chart_of_accounts, vouchers, voucher_lines, fixed_assets, vat_input_registers, audit_logs
✓ Indexes: voucher_no, voucher_date, status, fiscal_period, voucher_line relations
```

---

## Completed Modules

### Domain Layer
| Entity | Status | Notes |
|--------|--------|-------|
| BaseEntity | ✅ Done | |
| BaseAuditEntity | ✅ Done | |
| BaseLookupEntity | ✅ Done | |
| Voucher | ✅ Done | With workflow methods |
| VoucherLine | ✅ Done | |
| VoucherAttachment | ✅ Done | |
| FiscalPeriod | ✅ Done | With lifecycle methods |
| LedgerEntry | ✅ Done | |
| LedgerSummary | ✅ Done | DTO-like class |
| ChartOfAccounts | ✅ Done | |
| Customer | ✅ Done | |
| Vendor | ✅ Done | |
| Employee | ✅ Done | HR/Employee.cs |
| Product | ✅ Done | Inventory/Product.cs |
| FixedAsset | ✅ Done | Assets/FixedAsset.cs |
| DepreciationSchedule | ✅ Done | Assets/DepreciationSchedule.cs |
| TaxRate | ✅ Done | Tax/TaxRate.cs |
| PITBracket | ✅ Done | Tax/PITBracket.cs |
| PITAllowance | ✅ Done | Tax/PITAllowance.cs |
| ExciseTaxItem | ✅ Done | Tax/ExciseTaxItem.cs |
| TaxDeclaration | ✅ Done | Tax/TaxDeclaration.cs |
| AuditLog | ✅ Done | Audit/AuditLog.cs |
| Warehouse | ✅ Done | DM/Warehouse.cs |
| Bank | ✅ Done | DM/Bank.cs |
| ExchangeRate | ✅ Done | DM/ExchangeRate.cs |
| Bank | ✅ Done | DM/Bank.cs |
| BankAccount | ✅ Done | DM/Bank.cs |
| BankTransaction | ✅ Done | DM/Bank.cs |
| CashBook | ✅ Done | DM/CashBook.cs |
| CashBookEntry | ✅ Done | DM/CashBook.cs |
| BankReconciliation | ✅ Done | Entities/BankReconciliation.cs |
| Receivable | ✅ Done | DM/ReceivablePayable.cs |
| Payable | ✅ Done | DM/ReceivablePayable.cs |
| ReceivablePayment | ✅ Done | DM/ReceivablePayable.cs |
| PayablePayment | ✅ Done | DM/ReceivablePayable.cs |
| AgingReport | ✅ Done | DM/ReceivablePayable.cs |
| InventoryTransaction | ✅ Done | Inventory/InventoryTransaction.cs |
| InventoryBalance | ✅ Done | Inventory/InventoryBalance.cs |
| NumberSequence | ✅ Done | Cfg/NumberSequence.cs |
| AppSetting | ✅ Done | Cfg/AppSetting.cs |
| OutboxMessage | ✅ Done | Cfg/OutboxMessage.cs |
| ProductGroup | ✅ Done | Inventory/ProductGroup.cs |
| AssetGroup | ✅ Done | Assets/AssetGroup.cs |
| Department | ✅ Done | HR/Department.cs |
| VATInputRegister | ✅ Done | Tax/VATInputRegister.cs |
| VATOutputRegister | ✅ Done | Tax/VATOutputRegister.cs |
| TaxPayment | ✅ Done | Tax/TaxPayment.cs |
| TaxCalendar | ✅ Done | Tax/TaxCalendar.cs |
| CITAdjustment | ✅ Done | Tax/CITAdjustment.cs |
| CITLossCarryForward | ✅ Done | Tax/CITLossCarryForward.cs |
| WithholdingTax | ✅ Done | Tax/WithholdingTax.cs |
| CostCenter | ✅ Done | DM/CostAccounting.cs |
| CostAllocation | ✅ Done | DM/CostAccounting.cs |
| CostAllocationDetail | ✅ Done | DM/CostAccounting.cs |
| CostReport | ✅ Done | DM/CostAccounting.cs |
| Revenue | ✅ Done | DM/Revenue.cs |
| RevenueRecognition | ✅ Done | DM/Revenue.cs |
| RevenueReport | ✅ Done | DM/Revenue.cs |

### Enums
All enums defined in `Enums.cs`: VoucherType, VoucherStatus, FiscalPeriodStatus, AccountType, TaxType, ProductType, InventoryTransactionType, PricingMethod, AssetStatus, CITAdjFlag, TransactionType, TaxDeclarationStatus

### Exceptions
All exceptions in `DomainExceptions.cs`: DomainException, BusinessRuleException, ConcurrencyException, ValidationException

### Application Layer
| Component | Status | Notes |
|-----------|--------|-------|
| ServiceResult<T> | ✅ Done | |
| PagedResult<T> | ✅ Done | |
| VoucherDto | ✅ Done | |
| IVoucherService | ✅ Done | |
| VoucherService | ✅ Done | |
| ChartOfAccountsDto | ✅ Done | |
| IChartOfAccountsService | ✅ Done | |
| ChartOfAccountsService | ✅ Done | |
| CustomerDto | ✅ Done | |
| ICustomerService | ✅ Done | |
| CustomerService | ✅ Done | |
| VendorDto | ✅ Done | |
| IVendorService | ✅ Done | |
| VendorService | ✅ Done | |
| LedgerEntryDto | ✅ Done | |
| LedgerSummaryDto | ✅ Done | |
| ILedgerService | ✅ Done | |
| LedgerService | ✅ Done | |

### Infrastructure
| Component | Status | Notes |
|-----------|--------|-------|
| AMSDbContext | ✅ Done | All DbSets configured |
| UnitOfWork | ✅ Done | Transaction management |

### Repository Implementations
| Repository | Status | Notes |
|------------|--------|-------|
| VoucherRepository | ✅ Done | |
| ChartOfAccountsRepository | ✅ Done | |
| CustomerRepository | ✅ Done | |
| VendorRepository | ✅ Done | |
| LedgerRepository | ✅ Done | |
| FiscalPeriodRepository | ✅ Done | |
| TaxRepository | ✅ Done | |
| InventoryRepository | ✅ Done | |
| BankRepository | ✅ Done | QT Module |
| BankAccountRepository | ✅ Done | QT Module |
| BankTransactionRepository | ✅ Done | QT Module |
| CashBookRepository | ✅ Done | QT Module |
| CashBookEntryRepository | ✅ Done | QT Module |
| BankReconciliationRepository | ✅ Done | QT Module |
| ReceivableRepository | ✅ Done | CN Module |
| PayableRepository | ✅ Done | CN Module |
| ReceivablePaymentRepository | ✅ Done | CN Module |
| PayablePaymentRepository | ✅ Done | CN Module |
| AgingReportRepository | ✅ Done | CN Module |
| ProductRepository | ✅ Done | HH Module |
| WarehouseRepository | ✅ Done | HH Module |
| CostCenterRepository | ✅ Done | CP Module |
| CostAllocationRepository | ✅ Done | CP Module |
| CostAllocationDetailRepository | ✅ Done | CP Module |
| CostReportRepository | ✅ Done | CP Module |
| RevenueRepository | ✅ Done | DT Module |
| RevenueRecognitionRepository | ✅ Done | DT Module |
| RevenueReportRepository | ✅ Done | DT Module |

### Application Services
| Service | Status | Notes |
|---------|--------|-------|
| VoucherService | ✅ Done | + ReverseAsync, uses domain methods |
| ChartOfAccountsService | ✅ Done | |
| CustomerService | ✅ Done | |
| VendorService | ✅ Done | |
| LedgerService | ✅ Done | |
| FiscalPeriodService | ✅ Done | Open/Close/Lock periods |
| TaxService | ✅ Done | PIT calculation, brackets |
| InventoryService | ✅ Done | Products, transactions, balances |
| AssetService | ✅ Done | Depreciation calculation |
| TrialBalanceService | ✅ Done | B01-DN Trial Balance |
| MonthEndClosingService | ✅ Done | Month-end closing (8 steps) |
| BankReconciliationService | ✅ Done | QT - Bank statement reconciliation |
| CashFlowReportService | ✅ Done | QT - B03-DN Cash Flow Statement |
| ReceivablePayableService | ✅ Done | CN - AR/AP management |
| InventoryReportService | ✅ Done | HH - Inventory reports |
| CostAccountingService | ✅ Done | CP - Cost accounting |
| RevenueService | ✅ Done | DT - Revenue/Income |
| FinancialReportService | ✅ Done | BC - Financial Reports (B01-DN, B02-DN, B03-DN) |

### Business Rules Implemented
- Voucher workflow: Draft → Pending → Approved → Posted → Reversed
- FiscalPeriod lifecycle: Open → Closed → Locked
- PIT progressive tax calculation (7 brackets per TT 99/2025)
- Inventory balance tracking with FIFO/AVCO
- Fixed asset straight-line depreciation

### P0 Implementations (2026-03-26)

#### 1. Authorization (RBAC) - AD Security Groups Mapping
| Component | Status | Notes |
|-----------|--------|-------|
| User Entity | ✅ Done | Security/User.cs |
| Role Entity | ✅ Done | Security/Role.cs |
| Permission Entity | ✅ Done | Security/Permission.cs |
| ADGroup Entity | ✅ Done | Security/ADGroup.cs |
| UserRole Entity | ✅ Done | Many-to-many User-Role |
| ADGroupRole Entity | ✅ Done | Many-to-many ADGroup-Role |
| UserADGroup Entity | ✅ Done | Many-to-many User-ADGroup |
| RolePermission Entity | ✅ Done | Many-to-many Role-Permission |
| IUserRepository | ✅ Done | Security/ISecurityRepository.cs |
| IRoleRepository | ✅ Done | |
| IADGroupRepository | ✅ Done | |
| IPermissionRepository | ✅ Done | |
| IUserRoleRepository | ✅ Done | |
| IUserADGroupRepository | ✅ Done | |
| IADGroupRoleRepository | ✅ Done | |
| IRolePermissionRepository | ✅ Done | |
| UserRepository | ✅ Done | Infrastructure/Repositories/SecurityRepository.cs |
| RoleRepository | ✅ Done | |
| ADGroupRepository | ✅ Done | |
| PermissionRepository | ✅ Done | |
| UserRoleRepository | ✅ Done | |
| UserADGroupRepository | ✅ Done | |
| ADGroupRoleRepository | ✅ Done | |
| RolePermissionRepository | ✅ Done | |
| IAuthorizationService | ✅ Done | Application/Interfaces/IAuthorizationService.cs |
| AuthorizationService | ✅ Done | Application/Services/AuthorizationService.cs |
| SecurityDto | ✅ Done | Application/DTOs/SecurityDto.cs |
| DbContext Config | ✅ Done | User, Role, ADGroup, Permission entities |

#### 2. Tax Module UI
| Component | Status | Notes |
|-----------|--------|-------|
| TaxController | ✅ Done | Web/Controllers/TaxController.cs |
| Index.cshtml | ✅ Done | Views/Tax/Index.cshtml |
| GTGT.cshtml | ✅ Done | Views/Tax/GTGT.cshtml |
| TNDN.cshtml | ✅ Done | Views/Tax/TNDN.cshtml |
| TNCN.cshtml | ✅ Done | Views/Tax/TNCN.cshtml |
| ITaxService Extensions | ✅ Done | Declaration CRUD, VAT registers |
| TaxService Extensions | ✅ Done | Implementation |
| TaxDeclarationDto | ✅ Done | DTOs/TaxDto.cs |
| CreateTaxDeclarationDto | ✅ Done | |
| VATRegisterDto | ✅ Done | |

#### 3. Report Export (Excel/PDF)
| Component | Status | Notes |
|-----------|--------|-------|
| ExportTrialBalance | ✅ Done | CSV export for Trial Balance |
| ExportBalanceSheet | ✅ Done | CSV export for Balance Sheet |
| ExportIncomeStatement | ✅ Done | CSV export for Income Statement |
| ExportCashFlowStatement | ✅ Done | CSV export for Cash Flow Statement |

### Web Layer
| Component | Status | Notes |
|-----------|--------|-------|
| VouchersController | ✅ Done | |
| Index.cshtml | ✅ Done | |
| _Layout.cshtml | ✅ Done | |

---

## Pending Work

### Repository Interfaces (Required)
- [x] IChartOfAccountsRepository
- [x] ICustomerRepository
- [x] IVendorRepository
- [x] ILedgerRepository
- [x] IFiscalPeriodRepository

### Application Services (Required)
- [x] ChartOfAccountsService
- [x] CustomerService
- [x] VendorService
- [x] LedgerService

### Infrastructure (Required)
- [x] Repository implementations
  - [x] VoucherRepository
  - [x] ChartOfAccountsRepository
  - [x] CustomerRepository
  - [x] VendorRepository
  - [x] LedgerRepository
  - [x] FiscalPeriodRepository

### DI Registration (Required)
- [x] All repositories registered in Program.cs

---

## Known Issues

1. ~~**VoucherStatus Ambiguity**~~ - ✅ FIXED
2. ~~**IsActive Hide Warnings**~~ - ✅ FIXED (by updating DbContext to use correct property names)
3. **Newtonsoft.Json Vulnerability** - Need to upgrade to 13.x in Web project
4. ~~**Missing Entity Properties**~~ - ✅ FIXED (DbContext updated to match entity definitions)
5. ~~**Database Schema Mismatches**~~ - ✅ FIXED (2026-03-27)
   - `chart_of_accounts.account_number` column added
   - `vouchers.row_version` column mapping fixed
   - `fixed_assets` table created
   - `vat_input_registers` table created with DbContext configuration

---

## Database

- **Host:** localhost:15432
- **Database:** ams_db
- **User:** postgres / 123456
- **Schema:** acc, tax, audit, cfg, rpt
- **Tables:** Created via manual SQL (EF migrations had timeout issues)

---

## Priority 3 Implementation Summary (2026-03-27)

### Files Created
1. `Web/Filters/RequirePermissionAttribute.cs` - Permission-based authorization filter
2. `Web/Controllers/AuthorizationController.cs` - RBAC management API
3. `Web/Controllers/ExportController.cs` - Data export API (Excel/PDF/CSV)
4. `Web/Controllers/DataTablesController.cs` - DataTables server-side processing API
5. `Web/Hubs/NotificationHub.cs` - SignalR real-time notification hub
6. `Web/Services/NotificationService.cs` - SignalR notification service
7. `Web/Services/ExportModels.cs` - Export request/response models
8. `Web/Services/ExportService.cs` - Excel/PDF/CSV export implementation
9. `Web/Common/DataTablesModels.cs` - DataTables request/response models

### Files Modified
1. `Web/Program.cs` - Added:
   - Windows Negotiate authentication
   - Authorization policies (Admin, Accountant, Viewer)
   - Hangfire scheduler with PostgreSQL storage
   - SignalR configuration with custom UserIdProvider
   - Export service registration
   - Sample background job
2. `Web/AMS.Web.csproj` - Added packages:
   - Hangfire.AspNetCore, Hangfire.Core, Hangfire.PostgreSql
   - ClosedXML, QuestPDF, DocumentFormat.OpenXml
   - Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File
3. `Infrastructure/AMS.Infrastructure.csproj` - Added packages:
   - Hangfire.Core, Hangfire.PostgreSql
   - ClosedXML, QuestPDF
   - Serilog packages

### Configuration Changes
- Authentication: Windows Negotiate for AD integration
- Authorization: Role-based policies (RequireAdminRole, RequireAccountantRole, RequireViewerRole)
- Hangfire: Dashboard at `/hangfire` with PostgreSQL persistence
- SignalR: Hub endpoint at `/hubs/notifications`
- Logging: Serilog with Console and File sinks

## Recent Fixes (2026-03-25)

1. Fixed `VoucherStatus` namespace ambiguity in IVoucherRepository.cs
2. Fixed `VoucherType` reference in VoucherService.cs (changed from Domain.Entities.VoucherType to VoucherType)
3. Fixed DbContext FiscalPeriod configuration (removed non-existent ClosedAt/ClosedBy)
4. Fixed DbContext ChartOfAccounts configuration (mapped to correct property names: Code, Name, ParentId, IsDetail)
5. All projects now build successfully
6. Implemented all 5 repository interfaces (IChartOfAccountsRepository, ICustomerRepository, IVendorRepository, ILedgerRepository, IFiscalPeriodRepository)
7. Implemented all 4 Application Services:
   - ChartOfAccountsService (interface + implementation + DTOs)
   - CustomerService (interface + implementation + DTOs)
   - VendorService (interface + implementation + DTOs)
   - LedgerService (interface + implementation + DTOs)
8. Created FiscalPeriodRepository implementation
9. Registered all repositories in Program.cs DI container
10. Created UnitOfWork for transaction management
11. Created FiscalPeriodService with Open/Close/Lock business logic
12. Enhanced VoucherService to use domain entity methods (Submit, Approve, Reject, Post, Reverse)
13. Created TaxService with PIT calculation (7 progressive brackets per TT 99/2025)
14. Created InventoryService with product, transaction, and balance management
15. Created AssetService with depreciation calculation
16. Implemented TaxRepository and InventoryRepository
17. Added InventoryBalance navigation property for Product
18. Added DefaultPersonalDeduction constant (11,000,000 VND)
19. Added XML documentation to Infrastructure layer (AMSDbContext, DbContextFactory, UnitOfWork, 8 Repositories)
20. Added XML documentation to Web layer (VouchersController, BaseController, GlobalExceptionFilter, Program.cs)
21. Fixed VoucherService.PostAsync to create ledger entries via LedgerService.CreateFromVoucherAsync
22. Added fiscal period validation before posting (must be Open)
24. Fixed VoucherService.ReverseAsync to create reversal ledger entries
25. Added LedgerService.CreateReversalEntriesAsync method (swaps debit/credit, marks as adjustment)
26. Added fiscal period validation before reversing (must be Open)
27. Added fiscal period validation to SubmitAsync, ApproveAsync, RejectAsync (must be Open)
28. Added FiscalPeriod eager loading in VoucherRepository (GetById, GetByIdWithLines)
29. Fixed LedgerEntry.AccountCode population when creating from voucher
30. Added ChartOfAccountsRepository.GetByIdsAsync for bulk account lookup
31. Implemented Account Balance tracking - updates balances on voucher post
32. Implemented Trial Balance with opening balances from prior periods
33. Implemented Month-end closing service (8-step process per TT 99/2025):
   - Bước 1: Kiểm tra và hạch toán chứng từ
   - Bước 2: Tính giá xuất kho
   - Bước 3: Tính khấu hao TSCĐ
   - Bước 4: Phân bổ chi phí trả trước
   - Bước 5: Hoàn nhập doanh thu chưa thực hiện
   - Bước 6: Kết chuyển doanh thu, chi phí
   - Bước 7: Lập bảng cân đối thử
   - Bước 8: Khoá sổ kế toán
34. Implemented Opening balance carry-forward from previous periods
35. Created AccountBalance entity for persistent balance tracking
36. Created TrialBalanceService (GetTrialBalance, UpdateAccountBalances, CarryForwardBalances)
37. Created MonthEndClosingService (ExecuteMonthEndClosingAsync)
38. Added transaction wrapper for posting voucher and creating ledger entries

## QT Module Implementation (2026-03-25)

### Domain Entities
- Bank entity with BankAccount and BankTransaction entities
- BankTransactionType enum (Deposit, Withdrawal, TransferIn, TransferOut, Fee)
- BankTransactionStatus enum (Pending, Completed, Cancelled)
- CashBook entity with CashBookEntry
- BankReconciliation entity with BankReconciliationStatus (Draft, InProgress, Completed, Cancelled)

### Repository Interfaces
- IBankRepository, IBankAccountRepository, IBankTransactionRepository
- ICashBookRepository, ICashBookEntryRepository, IBankReconciliationRepository

### Repository Implementations
- BankRepository (Bank)
- BankAccountRepository (BankAccount)
- BankTransactionRepository (BankTransaction)
- CashBookRepository (CashBook)
- CashBookEntryRepository (CashBookEntry)
- BankReconciliationRepository (BankReconciliation)

### Application Services
- IBankReconciliationService + BankReconciliationService
  - CreateReconciliationAsync, GetReconciliationByIdAsync
  - ReconcileAsync, ApproveReconciliationAsync, CancelReconciliationAsync
  - GetReconciliationReportAsync, UpdateStatementBalanceAsync, CompleteReconciliationAsync
- ICashFlowReportService + CashFlowReportService (B03-DN Cash Flow Statement)
  - GetCashFlowReportAsync (by year/month)
  - GetCashFlowReportByDateRangeAsync
  - GetCashFlowReportByCashBookAsync
  - GetCashFlowReportByBankAccountAsync

### DI Registration
- All repositories and services registered in Program.cs

## CN Module Implementation (2026-03-25)

### Domain Entities
- Receivable entity (Phải thu khách hàng - 131)
- Payable entity (Phải trả người bán - 331)
- ReceivablePayment entity (Phiếu thu)
- PayablePayment entity (Phiếu chi)
- AgingReport entity (Báo cáo công nợ)
- ReceivableType enum (TradeReceivable, AdvancePayment, OtherReceivable)
- PayableType enum (TradePayable, AdvancePayment, OtherPayable)

### Repository Interfaces
- IReceivableRepository, IPayableRepository
- IReceivablePaymentRepository, IPayablePaymentRepository
- IAgingReportRepository

### Repository Implementations
- ReceivableRepository, PayableRepository
- ReceivablePaymentRepository, PayablePaymentRepository
- AgingReportRepository

### Application Services
- IReceivablePayableService + ReceivablePayableService
  - CreateReceivableAsync, UpdateReceivableAsync, GetReceivableByIdAsync
  - CreatePayableAsync, UpdatePayableAsync, GetPayableByIdAsync
  - RecordReceivablePaymentAsync, RecordPayablePaymentAsync
  - GetReceivablesByCustomerAsync, GetPayablesByVendorAsync
  - GetUnpaidReceivablesAsync, GetUnpaidPayablesAsync
  - GetReceivableAgingReportAsync, GetPayableAgingReportAsync
  - GenerateAgingReportAsync (Current, 1-30, 31-60, 61-90, Over 90 days)

### DI Registration
- All repositories and service registered in Program.cs

## HH Module Implementation (2026-03-25)

### Domain Entities
- Enhanced InventoryTransactionType enum:
  - PurchaseIn (Nhập mua), SaleOut (Xuất bán), ReturnOut (Xuất trả)
  - ReturnIn (Nhập trả), Transfer (Chuyển kho), Adjustment (Điều chỉnh)
  - SampleOut (Xuất mẫu), InternalOut (Xuất nội bộ)

### Repository Interfaces
- IProductRepository, IWarehouseRepository

### Repository Implementations
- ProductRepository, WarehouseRepository
- Added GetTransactionsByDateRangeAsync to IInventoryRepository
- Added GetAllBalancesAsync to IInventoryRepository

### Application Services
- IInventoryReportService + InventoryReportService
  - GetStockBalanceReportAsync (Báo cáo tồn kho)
  - GetInventoryMovementReportAsync (Báo cáo luân chuyển)
  - GetInventoryValuationReportAsync (Báo cáo định giá hàng tồn kho)
  - GetInventoryTurnoverReportAsync (Báo cáo vòng quay hàng tồn kho)
  - GetInventoryAgingReportAsync (Báo cáo tuổi hàng tồn kho)

### DI Registration
- All repositories and service registered in Program.cs

## CP Module Implementation (2026-03-25)

### Domain Entities
- CostCenter, CostAllocation, CostAllocationDetail, CostReport entities
- CostAllocationMethod enum (Direct, Percentage, SquareMeter, etc.)

### Repository Interfaces
- ICostCenterRepository, ICostAllocationRepository
- ICostAllocationDetailRepository, ICostReportRepository

### Repository Implementations
- CostCenterRepository, CostAllocationRepository
- CostAllocationDetailRepository, CostReportRepository

### Application Services
- ICostAccountingService + CostAccountingService
  - Create/Update/Get CostCenter, Create/Approve CostAllocation
  - GetCostReportAsync, GetCostVarianceReportAsync

### DI Registration
- All repositories and service registered in Program.cs

## DT Module Implementation (2026-03-25)

### Domain Entities
- Revenue, RevenueRecognition, RevenueReport entities
- RevenueType enum (SalesRevenue, ServiceRevenue, etc.)

### Repository Interfaces
- IRevenueRepository, IRevenueRecognitionRepository
- IRevenueReportRepository

### Repository Implementations
- RevenueRepository, RevenueRecognitionRepository
- RevenueReportRepository

### Application Services
- IRevenueService + RevenueService
  - Create/Update/Get Revenue, RecognizeRevenueAsync
  - GetRevenueSummaryAsync, GenerateRevenueReportAsync

### DI Registration
- All repositories and service registered in Program.cs

## BC Module Implementation (2026-03-25)

### Domain Entities
- BalanceSheet entity (Bảng cân đối kế toán - B01-DN)
- IncomeStatement entity (Báo cáo kết quả KD - B02-DN)
- CashFlowStatement entity (Báo cáo lưu chuyển tiền tệ - B03-DN)
- FinancialReport entity (Báo cáo tài chính)

### Repository Interfaces
- IFinancialReportRepository

### Repository Implementations
- FinancialReportRepository

### Application Services
- IFinancialReportService + FinancialReportService
  - GetBalanceSheetAsync (B01-DN)
  - GetIncomeStatementAsync (B02-DN)
  - GetCashFlowStatementAsync (B03-DN)
  - Generate methods for all reports
  - GetAllReportsAsync

### DI Registration
- All repositories and service registered in Program.cs

## Domain Entities Complete (2026-03-24)

All 30 domain entities completed:
- Base: BaseEntity, BaseAuditEntity, BaseLookupEntity
- Voucher: Voucher, VoucherLine, VoucherAttachment
- Ledger: FiscalPeriod, LedgerEntry, LedgerSummary
- DM: ChartOfAccounts, Customer, Vendor, Warehouse, Bank, ExchangeRate
- HR: Employee
- Inventory: Product, InventoryTransaction, InventoryBalance
- Assets: FixedAsset, DepreciationSchedule
- Tax: TaxRate, PITBracket, PITAllowance, ExciseTaxItem, TaxDeclaration
- Audit: AuditLog
- Config: NumberSequence, AppSetting, OutboxMessage

## Repository Interfaces Complete (2026-03-25)

All 5 repository interfaces implemented:
- IChartOfAccountsRepository
- ICustomerRepository
- IVendorRepository
- ILedgerRepository
- IFiscalPeriodRepository

## XML Documentation (2026-03-25)

All domain entities, enums, exceptions, interfaces, and Application layer have XML documentation:
- Base classes: `<summary>` for class and all properties
- Enums: `<summary>` for enum and each value
- Exceptions: `<summary>` and `<param>` tags
- Interfaces: `<summary>` and `<returns>` tags
- Entities: `<summary>` for class, all properties, and methods with `<exception>` tags
- Application DTOs: `<summary>` for class and all properties
- Application Services: `<summary>`, `<param>`, and `<returns>` tags
- Application Constants: `<summary>` for class and constants
- **Infrastructure layer (2026-03-25):** Added `<summary>` to AMSDbContext, DbContextFactory, UnitOfWork, and all 8 Repositories
- **Web layer (2026-03-25):** Added `<summary>` to Controllers, BaseController, GlobalExceptionFilter, Program.cs
- **P0 Implementations (2026-03-26):** Added XML documentation to:
  - AuthorizationService (class, constructor, methods, private methods)
  - IAuthorizationService (interface, all methods)
  - SecurityDto (all DTOs and properties)
  - ISecurityRepository (all interfaces and methods)
  - SecurityRepository (all repository classes and methods)
  - TaxController (class, constructor, all action methods, view model)
  - ITaxService (interface, all methods)
  - TaxDto (all DTOs and properties)

## Test Infrastructure (2026-03-27)

### Test Projects Created
- `tests/AMS.Domain.Tests/AMS.Domain.Tests.csproj` - Unit test project for Domain layer
- `tests/AMS.Application.Tests/AMS.Application.Tests.csproj` - Unit test project for Application layer

### Test Frameworks & Packages
- xUnit 2.6.2 - Test framework
- Moq 4.20.70 - Mocking library
- FluentAssertions 6.12.0 - Assertion library
- coverlet.collector 6.0.0 - Code coverage
- Microsoft.NET.Test.Sdk 17.8.0

### Test Data Management
- Created `TestDataFactory` helper class following TEST_STRATEGY.md patterns:
  - Factory pattern for creating test entities
  - Object Mother pattern via static factory methods
  - Supports configuration via Action<T> delegates
  - Added factory methods for: User, Role, ADGroup, Permission, UserRole, UserADGroup, ADGroupRole, RolePermission

### Unit Tests Implemented
| Test Class | Tests | Coverage |
|------------|-------|----------|
| ChartOfAccountsServiceTests | 25 | CRUD operations, validation, hierarchy |
| AuthorizationServiceTests | 10 | CRUD operations, validation, edge cases |

### Test Results
```
Passed! - Failed: 0, Passed: 35, Skipped: 0, Total: 35, Duration: 450 ms
```

### Test Naming Convention (per TEST_STRATEGY.md)
- `[MethodName]_[Scenario]_[ExpectedResult]`
- Example: `CreateAsync_WithValidData_ReturnsSuccess`
- Class naming: `[ClassName]Tests`

### Test Patterns Implemented
- Arrange-Act-Assert pattern
- Moq for dependency mocking
- FluentAssertions for readable assertions
- Parameterized tests with `[Theory]` and `[InlineData]`

### Test Coverage Status
| Layer | Target | Current | Priority |
|-------|--------|---------|----------|
| Domain | 90%+ | ~18% (ChartOfAccountsService + AuthorizationService) | Cao nhất |
| Application | 80%+ | ~15% (ChartOfAccountsService + AuthorizationService) | Cao |
| Infrastructure | 70%+ | 0% | Trung bình |
| Web | 60%+ | 0% | Thấp |

### Pending Test Classes
- [x] AuthorizationServiceTests
- [ ] TaxServiceTests  
- [ ] VoucherServiceTests
- [ ] LedgerServiceTests
- [ ] CustomerServiceTests
- [ ] VendorServiceTests

## UI/UX Implementation (2026-03-26)

### Theme & Layout
- Updated `_Layout.cshtml` with full Bootstrap 5.3 + jQuery + Bootstrap Icons
- Implemented App Shell layout: Topbar + Sidenav + Main Content
- Added comprehensive navigation menu per UI/UX spec
- Created `accounting-theme.css` with design tokens per SPEC
- Created `accounting-app.js` with:
  - Toastr notifications (ACC.toast)
  - Flatpickr date pickers
  - Select2 dropdowns (accounts, customers, vendors, products)
  - Keyboard shortcuts (Ctrl+K search, Ctrl+S save)
  - AJAX global handlers
  - Confirmation dialogs
  - DataTables helper (ACC.initDataTable)
  - Currency/date formatters (ACC.format)
  - Voucher form helpers (ACC.voucher)

### Views Implemented
| Controller | Index View | Status |
|------------|------------|--------|
| Vouchers | ✅ | Complete with filters, workflow buttons |
| ChartOfAccounts | ✅ | With account type filter. Details, Create, Edit views added. |
| Customers | ✅ | With status filter. Details, Create, Edit views added. |
| Vendors | ✅ | With status filter. Details, Create, Edit views added. |
| Products | ✅ | With type filter. Details, Create, Edit views added. |
| Home/Dashboard | ✅ | Enhanced with workflow integration, drill-down capabilities, accessibility features, real-time data loading, and visual workflow indicators |
| Tax | 4 views | GTGT, TNDN, TNCN, Index |
| Reports | ✅ | TrialBalance, BalanceSheet, IncomeStatement, CashFlowStatement views added. |

### CDN Libraries Added
- Bootstrap 5.3.3
- Bootstrap Icons 1.11.3
- DataTables 2.0.8 + Bootstrap 5 theme
- Select2 4.1.0-rc.0 + Bootstrap 5 theme
- Flatpickr 4.6.x
- Chart.js 4.4.3
- Toastr 2.1.4
- jQuery Validate + Unobtrusive

## Next Steps (2026-03-26)

### Priority 1 - Missing Controllers
- [x] FiscalPeriodsController - Period management
- [x] BankAccountsController - Bank/Cash management
- [x] WarehousesController - Inventory locations
- [x] DashboardController - API endpoints for dashboard widgets (KPIs, charts, pending vouchers, recent transactions)

### Priority 2 - Views Needed
- Details views for all entities ✅ (ChartOfAccounts, Customers, Vendors, Products)
- Create views for ChartOfAccounts, Customers, Vendors, Products ✅
- Edit views for all entities ✅ (ChartOfAccounts, Customers, Vendors, Products)
- Reports views (TrialBalance, BalanceSheet, IncomeStatement, CashFlow) ✅

### Priority 3 - Features (2026-03-27)

#### 1. Authorization (RBAC with AD groups) - Enhanced
| Component | Status | Notes |
|-----------|--------|-------|
| RequirePermissionAttribute | ✅ Done | Web/Filters/RequirePermissionAttribute.cs - Custom permission-based auth |
| AuthorizationController | ✅ Done | Web/Controllers/AuthorizationController.cs - User/Role/ADGroup management API |
| Authentication Config | ✅ Done | Windows Negotiate authentication in Program.cs |
| Authorization Policies | ✅ Done | RequireAdminRole, RequireAccountantRole, RequireViewerRole |

**API Endpoints:**
- `GET /api/authorization/me` - Get current user
- `GET /api/authorization/users/{userId}/roles` - Get user roles
- `GET /api/authorization/users/{userId}/permissions` - Get user permissions
- `POST /api/authorization/users` - Create user
- `POST /api/authorization/users/{userId}/roles` - Assign role
- `GET /api/authorization/users/{userId}/has-permission` - Check permission
- `GET /api/authorization/ad-groups` - Get mapped AD groups
- `POST /api/authorization/ad-groups/{adGroupId}/roles` - Map AD group to role
- `POST /api/authorization/sync-ad-groups` - Sync AD groups for user

#### 2. Hangfire Scheduler Setup
| Component | Status | Notes |
|-----------|--------|-------|
| Hangfire Configuration | ✅ Done | PostgreSQL storage configured in Program.cs |
| Hangfire Dashboard | ✅ Done | Available at `/hangfire` with authentication |
| DashboardAuthorizationFilter | ✅ Done | Custom auth filter for Hangfire dashboard |
| SampleBackgroundJob | ✅ Done | Demo job with notification integration |
| Recurring Jobs Registration | ✅ Done | Sample daily job configured |

**Features:**
- Dashboard with real-time job monitoring
- PostgreSQL persistence for job storage
- Authorization-protected dashboard access
- Support for fire-and-forget, delayed, recurring, and continuous jobs

#### 3. SignalR for Real-time Notifications
| Component | Status | Notes |
|-----------|--------|-------|
| NotificationHub | ✅ Done | Web/Hubs/NotificationHub.cs - SignalR hub |
| INotificationService | ✅ Done | Web/Services/NotificationService.cs - Service interface |
| SignalRNotificationService | ✅ Done | Implementation with SendToAll/User/Group |
| NameUserIdProvider | ✅ Done | Custom user ID provider for SignalR |
| SignalR Configuration | ✅ Done | Hub endpoint at `/hubs/notifications` |

**Hub Methods:**
- `JoinGroup(groupName)` - Join notification group
- `LeaveGroup(groupName)` - Leave notification group
- `SendMessageToAll(message)` - Broadcast message
- `SendMessageToUser(userId, message)` - Send to specific user
- `SendMessageToGroup(groupName, message)` - Send to group

**Notification Types:**
- Voucher approval notifications
- System job completion notifications
- Custom group/user broadcasts

#### 4. Export Functionality (Excel/PDF/CSV)
| Component | Status | Notes |
|-----------|--------|-------|
| IExportService | ✅ Done | Web/Services/ExportModels.cs - Interface and models |
| ExportService | ✅ Done | Web/Services/ExportService.cs - Implementation |
| ExportController | ✅ Done | Web/Controllers/ExportController.cs - API controller |
| Excel Export | ✅ Done | Using ClosedXML library |
| PDF Export | ✅ Done | Using QuestPDF library |
| CSV Export | ✅ Done | Native implementation |

**API Endpoints:**
- `POST /api/export` - Export with auto-detect format
- `POST /api/export/excel` - Export to Excel (.xlsx)
- `POST /api/export/pdf` - Export to PDF
- `POST /api/export/csv` - Export to CSV

**Features:**
- Dynamic column configuration
- Currency and date formatting
- Professional PDF layouts with headers/footers
- CSV with proper escaping

#### 5. API Endpoints for DataTables
| Component | Status | Notes |
|-----------|--------|-------|
| DataTablesRequest | ✅ Done | Web/Common/DataTablesModels.cs - Request model |
| DataTablesResponse<T> | ✅ Done | Response model with draw, recordsTotal, recordsFiltered |
| DataTablesHelper | ✅ Done | Helper class for server-side processing |
| DataTablesController | ✅ Done | Web/Controllers/DataTablesController.cs - API endpoints |

**API Endpoints:**
- `POST /api/datatables/vouchers` - Vouchers for DataTables
- `POST /api/datatables/chart-of-accounts` - Chart of accounts for DataTables
- `POST /api/datatables/customers` - Customers for DataTables
- `POST /api/datatables/vendors` - Vendors for DataTables

**Features:**
- Server-side pagination
- Search/filter support
- Column ordering
- Draw counter for async request matching

### Packages Added
| Package | Version | Purpose |
|---------|---------|---------|
| Hangfire.AspNetCore | 1.8.14 | Job scheduling framework |
| Hangfire.Core | 1.8.14 | Core Hangfire functionality |
| Hangfire.PostgreSql | 1.21.1 | PostgreSQL storage for Hangfire |
| ClosedXML | 0.102.3 | Excel file generation |
| QuestPDF | 2024.3.0 | PDF generation |
| DocumentFormat.OpenXml | 3.2.0 | Open XML support (ClosedXML dependency) |
| Serilog.AspNetCore | 8.0.3 | Structured logging |
| Serilog.Sinks.Console | 6.0.0 | Console log sink |
| Serilog.Sinks.File | 6.0.0 | File log sink |
