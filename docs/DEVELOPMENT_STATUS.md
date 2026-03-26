# AMS Development Status

**Last Updated:** 2026-03-25 18:00  
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
| AMS.Web | ⚠️ Warning | 1 (Newtonsoft.Json vulnerability) |

**Last Build:** ✅ PASSED - 2026-03-25 17:30 (1 warning, 0 errors)

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

### Business Rules Implemented
- Voucher workflow: Draft → Pending → Approved → Posted → Reversed
- FiscalPeriod lifecycle: Open → Closed → Locked
- PIT progressive tax calculation (7 brackets per TT 99/2025)
- Inventory balance tracking with FIFO/AVCO
- Fixed asset straight-line depreciation

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
4. **Missing Entity Properties** - DbContext updated to match entity definitions

---

## Database

- **Host:** localhost:15432
- **Database:** ams_db
- **User:** postgres / 123456
- **Schema:** acc, tax, audit, cfg, rpt
- **Tables:** Created via manual SQL (EF migrations had timeout issues)

---

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
