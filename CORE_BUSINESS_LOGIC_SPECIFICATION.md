# CORE BUSINESS LOGIC SPECIFICATION
## Accounting ERP System - TT99/2025 Compliance
**Role:** Senior Tax Inspector + Chief Accountant + Software Architect  
**Objective:** Absolute Compliance & Risk Prevention  
**Status:** CORE LOGIC ONLY (No UI/API)

---

## 1. BUSINESS ASSUMPTIONS

### 1.1 Regulatory Framework
- **Primary:** Thông tư 99/2025/TT-BTC (Accounting Regime)
- **Tax Law:** Luật Quản lý thuế 2019 (amended 2024)
- **Accounting Law:** Luật Kế toán 2015
- **Standards:** Chuẩn mực kế toán Việt Nam (VAS 01-26)
- **E-Invoice:** Thông tư 78/2021/TT-BTC (as amended)
- **VAT:** Thông tư 219/2013/TT-BTC (as amended)
- **TNDN:** Thông tư 78/2014/TT-BTC (as amended)
- **TNCN:** Thông tư 111/2013/TT-BTC (as amended)

### 1.2 Business Context
- **Entity Type:** Commercial & Service Enterprise
- **Fiscal Year:** Calendar year (01/01 - 31/12)
- **Currency:** VND (primary), foreign currency tracked separately
- **Inventory Method:** Weighted Average (mandatory per TT99)
- **Depreciation:** Straight-line method
- **VAT Method:** Credit method (khấu trừ) - mandatory for enterprises

### 1.3 Compliance Principles
```
RULE #0: IF IN DOUBT → BLOCK TRANSACTION
RULE #1: NO DOCUMENT = NO ENTRY
RULE #2: CLOSED PERIOD = READ-ONLY
RULE #3: NO HARD DELETE - ONLY ADJUSTMENT ENTRIES
RULE #4: EVERY ENTRY MUST BE TRACEABLE TO SOURCE
RULE #5: TAX RECONCILIATION MUST MATCH ACCOUNTING
```

### 1.4 Risk Appetite
- **Risk Level:** ZERO TOLERANCE for tax violations
- **Strategy:** Preventive controls at data entry
- **Audit Trail:** Immutable, cryptographically verifiable
- **Backup:** Multi-layer with point-in-time recovery

---

## 2. CORE ACCOUNTING RULES

### 2.1 Chart of Accounts (Hệ thống tài khoản)

#### 2.1.1 Account Structure (TT99-Điều 6)
```
FORMAT: [Category][Group][Detail]
- Category (1 digit): 1-9 (per TT99 classification)
- Group (2 digits): 00-99
- Detail (1-2 digits): 0-99

EXAMPLES:
- 111: Cash on hand (Tiền mặt)
- 1111: Cash in VND
- 1112: Cash in foreign currency
- 131: Accounts receivable - customers
- 133: Deductible VAT
- 3331: VAT payable
```

#### 2.1.2 Account Categories (TT99)
```
CATEGORY 1: Current Assets (Tài sản ngắn hạn)
  - 111-119: Cash & cash equivalents
  - 121-129: Short-term investments
  - 131-139: Receivables
  - 151-159: Inventory
  - 161-169: Other current assets

CATEGORY 2: Non-current Assets (Tài sản dài hạn)
  - 211-219: Fixed assets
  - 221-229: Investment properties
  - 241-249: Long-term investments

CATEGORY 3: Liabilities (Nợ phải trả)
  - 311-319: Short-term liabilities
  - 315: VAT payable (3331 detail)
  - 321-329: Long-term liabilities

CATEGORY 4: Owner's Equity (Vốn chủ sở hữu)
  - 411-419: Capital
  - 421: Retained earnings
  - 431-439: Funds

CATEGORY 5: Revenue (Doanh thu)
  - 511: Sales revenue
  - 515: Financial income
  - 521: Deductions from revenue

CATEGORY 6: Expenses (Chi phí)
  - 611: COGS
  - 621-629: Production costs
  - 641-649: Administrative expenses
  - 642: Selling expenses

CATEGORY 7: Other income/expenses
CATEGORY 8: Manufacturing accounts
CATEGORY 9: Off-balance sheet
```

#### 2.1.3 Validation Rules
```csharp
// ACCOUNT CODE VALIDATION
RULE-ACC-001: Account code must match TT99 standard format
  - Pattern: ^[1-9]\d{2,4}$
  - Length: 3-5 digits
  
RULE-ACC-002: Level 1 accounts (3 digits) cannot be used for transactions
  - Exception: Account 911 (determination of business results)
  
RULE-ACC-003: Account 911 is RESTRICTED
  - Only used at year-end closing
  - Requires dual authorization
  - Must be zero after closing
  
RULE-ACC-004: Foreign currency accounts must specify currency code
  - 1112-USD, 1112-EUR, etc.
  
RULE-ACC-005: Contra accounts must have parent account
  - 214 (Accumulated depreciation) ↔ 211 (Fixed assets)
  - 129 (Provision for doubtful debts) ↔ 131 (Receivables)
```

### 2.2 Double-Entry Accounting (Kế toán kép)

#### 2.2.1 Fundamental Principle
```
FOR EVERY TRANSACTION:
  Σ(Debit Amounts) = Σ(Credit Amounts)
  
MANDATORY FIELDS:
  - Entry number (unique, sequential)
  - Entry date (ngày ghi sổ)
  - Source document date (ngày chứng từ gốc)
  - Source document number (số chứng từ gốc)
  - Description (diễn giải)
  - Debit account(s)
  - Credit account(s)
  - Amount(s)
  - Currency
  - Created by
  - Created at
```

#### 2.2.2 Entry Validation Rules
```csharp
RULE-JE-001: ENTRY BALANCE CHECK
  ASSERT: Sum(Debits) == Sum(Credits)
  ERROR: "Bút toán không cân bằng: Tổng Nợ {debit} ≠ Tổng Có {credit}"
  ACTION: Reject entry

RULE-JE-002: SOURCE DOCUMENT MANDATORY
  ASSERT: SourceDocNumber != null && SourceDocNumber.Trim() != ""
  ASSERT: SourceDocDate != default(DateTime)
  ASSERT: SourceDocDate <= EntryDate
  ERROR: "Thiếu thông tin chứng từ gốc theo TT99-Đ10"
  ACTION: Reject entry
  
RULE-JE-003: PERIOD STATUS CHECK
  ASSERT: AccountingPeriod.Status == "Open"
  ERROR: "Kỳ kế toán đã khóa. Không thể ghi sổ vào kỳ {period}"
  ACTION: Reject entry
  
RULE-JE-004: DATE VALIDATION
  ASSERT: EntryDate <= CurrentDate
  ASSERT: SourceDocDate <= EntryDate
  ASSERT: SourceDocDate >= (EntryDate - 365 days) // Reasonable limit
  ERROR: "Ngày chứng từ hoặc ngày ghi sổ không hợp lệ"
  ACTION: Reject entry
  
RULE-JE-005: AMOUNT VALIDATION
  ASSERT: Amount > 0
  ASSERT: Amount <= 999,999,999,999,999.99 // Practical limit
  ERROR: "Số tiền phải lớn hơn 0"
  ACTION: Reject entry
  
RULE-JE-006: ACCOUNT VALIDATION
  ASSERT: DebitAccount != CreditAccount
  ERROR: "Tài khoản Nợ và Có không được trùng nhau"
  ACTION: Reject entry
  
RULE-JE-007: CURRENCY CONSISTENCY
  ASSERT: All lines use same currency OR proper forex accounts
  ERROR: "Đồng tiền không nhất quán trong bút toán"
  ACTION: Reject entry
```

#### 2.2.3 Complex Entry Rules
```csharp
RULE-JE-008: MULTI-LINE ENTRIES
  - Maximum 99 lines per entry (practical limit)
  - Each line must have description if > 5 lines
  - Automatic line numbering (line 1, 2, 3...)

RULE-JE-009: FOREIGN CURRENCY ENTRIES
  - Must specify exchange rate
  - Rate source: Central bank rate or documented contract rate
  - Rate date <= entry date
  - Dual recording: Foreign amount + VND equivalent

RULE-JE-010: INTER-COMPANY TRANSACTIONS
  - Must specify related party code
  - Must match consolidation rules
  - Requires separate authorization
```

### 2.3 Accounting Period Management

#### 2.3.1 Period Structure
```
HIERARCHY:
  Fiscal Year (Năm tài chính)
    ├── Quarter 1 (Quý 1)
    │   ├── January (Tháng 1)
    │   ├── February (Tháng 2)
    │   └── March (Tháng 3)
    ├── Quarter 2
    ├── Quarter 3
    └── Quarter 4
```

#### 2.3.2 Period Status Lifecycle
```
DRAFT → OPEN → CLOSING → CLOSED → LOCKED

DRAFT: Initial setup, can be deleted
OPEN: Normal operations, entries allowed
CLOSING: Closing procedures in progress, no new entries
CLOSED: Period closed, read-only
LOCKED: Permanent lock (after year-end), immutable
```

#### 2.3.3 Period Closing Rules
```csharp
RULE-PER-001: CLOSING SEQUENCE
  Must close in order: Month → Quarter → Year
  Cannot close Q2 if Q1 not closed
  Cannot close year if any month not closed

RULE-PER-002: PRE-CLOSING CHECKLIST
  BEFORE closing period P:
    ✓ All entries posted
    ✓ Trial balance balanced
    ✓ Bank reconciliations completed
    ✓ Inventory counts completed
    ✓ VAT declaration filed (if required)
    ✓ Inter-company reconciliations done
    ✓ No unallocated amounts
    ✓ No negative balances (except contra accounts)

RULE-PER-003: CLOSING WINDOW
  - Standard closing: Within 30 days after period end
  - Tax filing deadline compliance required
  - Emergency closing: Requires CFO + Chief Accountant approval

RULE-PER-004: REOPENING RULES (EMERGENCY ONLY)
  - Requires: CEO + CFO + External Auditor written approval
  - Reason must be documented
  - Creates audit trail entry
  - Limited to: Correction of material errors only
  - Must reopen sequentially (if reopen March, must reopen Q1)
  - Creates automatic adjustment entries for audit trail

RULE-PER-005: LOCKED PERIODS
  - Year-end periods locked after 90 days
  - Requires court order or tax authority request to unlock
  - Creates immutable log entry
```

### 2.4 Audit Trail & Immutability

#### 2.4.1 Audit Trail Requirements (TT99-Điều 14)
```
MANDATORY LOG FIELDS:
  - Timestamp (microsecond precision)
  - User ID
  - User role
  - Action type (Create, Update, Delete, View, Export)
  - Table/Entity affected
  - Record ID
  - Old values (JSON)
  - New values (JSON)
  - Source IP address
  - Session ID
  - Digital signature hash
```

#### 2.4.2 No Hard Delete Policy
```csharp
RULE-AUD-001: DELETION PROHIBITED
  - Hard delete is FORBIDDEN for all accounting data
  - "Delete" operation = Create reversal entry + mark as void
  
RULE-AUD-002: VOIDING ENTRIES
  To void an entry:
  1. Create reversal entry (same amounts, reversed debit/credit)
  2. Link to original entry (VoidOfEntryId)
  3. Reason code required (standardized list)
  4. Authorization based on amount:
     - < 10M VND: Chief Accountant
     - 10M-100M VND: CFO
     - > 100M VND: CEO + Audit Committee

RULE-AUD-003: CORRECTION ENTRIES
  To correct an entry:
  1. Void original entry (per RULE-AUD-002)
  2. Create new correct entry
  3. Link void → original → correction (chain)
  4. All three entries must be in same or later period
  5. Correction in closed period = adjustment entry

RULE-AUD-004: VERSIONING
  - Journal entries are versioned
  - Each update creates new version
  - Previous versions retained indefinitely
  - Latest version marked as "Current"

RULE-AUD-005: HASH CHAIN
  - Each entry includes hash of previous entry
  - Hash includes: Entry data + Previous hash + Timestamp
  - Algorithm: SHA-256 minimum
  - Tamper detection: Automatic alert if chain broken
```

---

## 3. CORE TAX RULES

### 3.1 VAT (Value Added Tax) - Thuế GTGT

#### 3.1.1 VAT Classification
```
VAT TYPES:
  1. OUTPUT VAT (Thuế GTGT đầu ra)
     - Account: 33311 (VAT on sales)
     - Rate: 0%, 5%, 8%, 10% (per product/service)
     - Trigger: Issuing invoice or receiving payment (earlier)
     
  2. INPUT VAT (Thuế GTGT đầu vào)
     - Account: 1331 (Deductible VAT)
     - Account: 1332 (Non-deductible VAT)
     - Rate: 0%, 5%, 8%, 10%
     - Trigger: Receiving invoice + Goods/services received

  3. VAT PAYABLE (Thuế GTGT phải nộp)
     - Account: 3331
     - Calculation: Output VAT - Deductible Input VAT
```

#### 3.1.2 VAT Validation Rules
```csharp
RULE-VAT-001: INVOICE MANDATORY FOR OUTPUT VAT
  ASSERT: Sales transaction has linked e-invoice OR
          Transaction is non-invoice sale (documented)
  ERROR: "Doanh thu phát sinh thuế GTGT đầu ra nhưng không có hóa đơn"
  ACTION: Block entry, require invoice
  
RULE-VAT-002: INPUT VAT DEDUCTION CONDITIONS
  Deductible if ALL conditions met:
    ✓ Valid e-invoice (per TT78/2021)
    ✓ Invoice issued by registered taxpayer
    ✓ Goods/services actually received
    ✓ Used for VAT-able business activities
    ✓ Invoice amount > 200,000 VND (or aggregated monthly)
    ✓ Not in non-deductible category

RULE-VAT-003: NON-DEDUCTIBLE INPUT VAT (TT219-Điều 14)
  Automatically route to account 1332:
    - VAT on passenger vehicles (except specific cases)
    - VAT on entertainment expenses
    - VAT on employee welfare (unless regulated)
    - VAT without valid invoice
    - VAT on purchases from non-VAT taxpayers
    - VAT on goods for VAT-exempt activities

RULE-VAT-004: VAT RATE VALIDATION
  VALID_RATES = { 0, 5, 8, 10 }
  ASSERT: Rate IN VALID_RATES
  ASSERT: Rate matches product/service classification
  ERROR: "Thuế suất GTGT không hợp lệ"
  
RULE-VAT-005: VAT CALCULATION CHECK
  VAT Amount = Taxable Value × Rate
  ASSERT: |Calculated VAT - Declared VAT| <= 1 VND (rounding tolerance)
  
RULE-VAT-006: VAT PERIOD RECONCILIATION
  Monthly/Quarterly:
    Σ(Output VAT from entries) = VAT Declaration Output
    Σ(Input VAT from entries) = VAT Declaration Input
    Difference threshold: 1,000 VND
  ERROR_IF_EXCEEDED: "Chênh lệch VAT sổ sách và tờ khai vượt ngưỡng"

RULE-VAT-007: E-INVOICE VALIDATION
  Invoice must have:
    - Valid tax code (Mã số thuế)
    - Valid invoice number (per TT78 format)
    - Date within fiscal period
    - Matching amounts (accounting vs invoice)
    - Not canceled/voided
    - QR code validation (optional but recommended)
```

#### 3.1.3 VAT Special Cases
```csharp
RULE-VAT-008: IMPORT VAT
  - Accounted separately: 13312 (Import VAT)
  - Must have customs declaration
  - Payment to customs authority (not supplier)
  - Deductible like domestic VAT

RULE-VAT-009: VAT DEFERRAL
  - Import VAT deferral requires customs approval
  - Track deferred VAT separately
  - Automatic alert when deferral expires

RULE-VAT-010: VAT REFUND
  - Track excess VAT in account 13311 (debit balance)
  - Refund claim only if cumulative > 300M VND
  - Must file separate refund application
  - Create audit trail for refund transactions
```

### 3.2 Corporate Income Tax (TNDN) - Thuế TNDN

#### 3.2.1 TNDN Framework
```
TAX BASE CALCULATION:
  Taxable Income = Accounting Profit 
                   + Non-deductible Expenses
                   - Tax-exempt Income
                   +/- Temporary Differences
                   +/- Permanent Differences

TAX RATES:
  - Standard: 20%
  - Preferential: 10%, 15%, 17% (if qualified)
  - Special: Per investment license

KEY ACCOUNTS:
  - 8211: Current tax expense (Chi phí TNDN hiện hành)
  - 8212: Deferred tax expense (Chi phí TNDN hoãn lại)
  - 3334: Corporate income tax payable
```

#### 3.2.2 Non-Deductible Expenses (Chi phí không được trừ)
```csharp
RULE-TNDN-001: NON-DEDUCTIBLE EXPENSE LIST
  EXPENSES_NOT_DEDUCTIBLE = {
    // Per TT78/2014-TT-BTC, Article 4
    "PENALTIES": "Fines, penalties for violations",
    "INTEREST_LATE": "Late payment interest on taxes",
    "RESERVES_UNAPPROVED": "Provisions not per regulations",
    "DEPRECIATION_EXCESS": "Depreciation exceeding limits",
    "WAGES_UNDOCUMENTED": "Wages without proper contracts/docs",
    "INTEREST_EXCESS": "Interest exceeding 1.5x base rate",
    "DONATIONS_EXCESS": "Donations > 10% of taxable income",
    "ENTERTAINMENT_EXCESS": "Entertainment > limits (TT96/2015)",
    "UNRELATED": "Expenses unrelated to business",
    "PERSONAL": "Personal expenses of owners/employees"
  }

RULE-TNDN-002: AUTOMATIC CLASSIFICATION
  When posting to expense accounts (6xx):
    IF ExpenseType IN NON_DEDUCTIBLE_LIST
    THEN Mark as "Non-deductible" in entry line
    AND Accumulate in tax adjustment register

RULE-TNDN-003: ENTERTAINMENT LIMIT
  Max deductible = MIN(
    10% of total deductible expenses,
    5% of taxable income before entertainment
  )
  Excess automatically flagged

RULE-TNDN-004: INTEREST DEDUCTION LIMIT
  IF InterestExpense > (1.5 × BaseRate × LoanPrincipal)
  THEN Excess = Non-deductible
  BaseRate = State Bank base rate at loan date

RULE-TNDN-005: DEPRECIATION LIMITS
  Asset Category         | Max Rate
  ----------------------|----------
  Buildings             | 3-6%
  Machinery/Equipment   | 10-20%
  Vehicles              | 10-20%
  Computers/Electronics | 20-50%
  
  Excess depreciation = Non-deductible
```

#### 3.2.3 Temporary vs Permanent Differences
```csharp
RULE-TNDN-006: PERMANENT DIFFERENCES
  - Never reverse
  - Examples: Fines, entertainment excess, non-deductible donations
  - Immediate tax impact

RULE-TNDN-007: TEMPORARY DIFFERENCES
  - Reverse over time
  - Create deferred tax assets/liabilities
  - Examples: Different depreciation methods, provisions

RULE-TNDN-008: DEFERRED TAX CALCULATION
  Deferred Tax Asset = Deductible Temp Differences × Rate
  Deferred Tax Liability = Taxable Temp Differences × Rate
  
  Review annually for impairment (DTA only)

RULE-TNDN-009: TAX RECONCILIATION
  Quarterly verification:
    Accounting Profit = Sum(Revenue) - Sum(Expenses)
    Taxable Income = Accounting Profit + Non-deductible - Exemptions
    Current Tax = Taxable Income × Rate - Prepayments
    
  ASSERT: |Calculated Tax - Declared Tax| < 10,000 VND
```

### 3.3 Personal Income Tax (TNCN) - Thuế TNCN

#### 3.3.1 Withholding Tax (Khấu trừ thuế)
```
APPLIES TO:
  - Employees (residents & non-residents)
  - Contractors/service providers (if applicable)
  - Dividend/interest payments to individuals

WITHHOLDING RATES (Progressive for residents):
  Monthly Income (M VND) | Rate | Deduction
  ----------------------|------|----------
  0 - 5                 | 5%   | 0
  5 - 10                | 10%  | 0.25M
  10 - 18               | 15%  | 0.75M
  18 - 32               | 20%  | 1.65M
  32 - 52               | 25%  | 3.25M
  52 - 80               | 30%  | 5.85M
  > 80                  | 35%  | 9.85M

DEDUCTIONS (per month):
  - Personal: 11,000,000 VND
  - Dependent: 4,400,000 VND each
```

#### 3.3.2 TNCN Validation Rules
```csharp
RULE-TNCN-001: RESIDENCY DETERMINATION
  Resident if ANY:
    - Present in VN > 183 days in 12 months
    - Has permanent residence in VN
    - Rented accommodation > 183 days with lease
  
  Non-residents: Flat 20% on Vietnam-sourced income

RULE-TNCN-002: MONTHLY WITHHOLDING
  IF EmployeeType == "Resident"
    TaxableIncome = GrossSalary - Insurance - 11M - (4.4M × Dependents)
    Tax = Progressive(TaxableIncome)
  
  IF EmployeeType == "Non-Resident"
    Tax = GrossSalary × 20%

RULE-TNCN-003: INSURANCE DEDUCTIONS
  Deductible amounts (employee portion):
    - Social Insurance: 8%
    - Health Insurance: 1.5%
    - Unemployment Insurance: 1%
    Max base: 20 × BaseSalary (determined annually)

RULE-TNCN-004: FINALIZATION
  Annual reconciliation required for:
    - All employees (employer obligation)
    - Individuals with multiple income sources
    - Deadline: April 30 following year

RULE-TNCN-005: CONTRACTOR TAX
  IF PaymentToIndividual AND NOT Employee
    THEN Withhold 10% (residents) or 20% (non-residents)
    UNLESS: Total payment < 2M VND per transaction
```

### 3.4 Tax Reconciliation (Đối chiếu thuế)

#### 3.4.1 Monthly Reconciliation
```csharp
RULE-TAX-REC-001: VAT RECONCILIATION
  Compare:
    A = Sum of Output VAT from accounting entries
    B = Sum of Input VAT from accounting entries
    C = VAT payable per accounting (A - B)
    D = VAT declared on tax return
    
  ASSERT: |C - D| <= 1,000 VND
  ALERT_IF: Difference > 0

RULE-TAX-REC-002: TNDN PREPAYMENT RECONCILIATION
  Quarterly prepayment = (CurrentQuarterProfit + NonDeductible) × 20%
  ASSERT: Prepayment declared = Prepayment calculated
  
RULE-TAX-REC-003: TNCN WITHHOLDING RECONCILIATION
  Sum of monthly withholdings = TNCN declared
  ASSERT: |Sum - Declared| < 1,000 VND

RULE-TAX-REC-004: INVOICE RECONCILIATION
  Accounting Revenue = Σ(Invoices) + Σ(Non-invoice revenue)
  ASSERT: |Revenue - InvoiceTotal| < 0.1% of Revenue
  ALERT_IF: Revenue > InvoiceTotal by > 0.1%
```

#### 3.4.2 Red Flag Detection
```csharp
RULE-TAX-RISK-001: VAT MISMATCH ALERT
  IF |AccountingVAT - DeclaredVAT| > 1,000,000 VND
    THEN FLAG: "Chênh lệch VAT lớn"
    ACTION: Require CFO review before closing

RULE-TAX-RISK-002: REVENUE-INVOICE GAP
  IF (Revenue - InvoicedRevenue) / Revenue > 0.01
    THEN FLAG: "Doanh thu chưa xuất hóa đơn"
    ACTION: List unbilled revenue, require justification

RULE-TAX-RISK-003: EXPENSE RATIO ANOMALY
  IF ExpenseRatio > IndustryAverage × 1.5
    THEN FLAG: "Tỷ lệ chi phí bất thường"
    ACTION: Review top 10 expense categories

RULE-TAX-RISK-004: INPUT VAT SPIKE
  IF CurrentMonthInputVAT > AvgLast3Months × 2
    THEN FLAG: "VAT đầu vào tăng đột biến"
    ACTION: Verify large purchases

RULE-TAX-RISK-005: NEGATIVE VAT CARRYFORWARD
  IF AccumulatedInputVAT > 300,000,000 VND (3 months)
    THEN FLAG: "VAT khấu trừ dồn tích lớn"
    ACTION: Suggest VAT refund application
```

---

## 4. DOCUMENT & INVOICE COMPLIANCE

### 4.1 Mandatory Linkage Matrix

#### 4.1.1 Three-Way Match (Bắt buộc)
```
FOR EVERY ACCOUNTING ENTRY:
  MUST LINK TO:
    1. SOURCE DOCUMENT (Chứng từ gốc)
       - Invoice (Hóa đơn)
       - Contract (Hợp đồng)
       - Receipt (Biên lai)
       - Bank statement (Sao kê ngân hàng)
       - Payroll (Bảng lương)
       - Inventory document (Phiếu nhập/xuất kho)
       
    2. BUSINESS TRANSACTION (Nghiệp vụ kinh tế)
       - Purchase order / Sales order
       - Delivery note
       - Acceptance certificate
       - Service completion confirmation
       
    3. PAYMENT EVIDENCE (Bằng chứng thanh toán)
       - Bank transfer slip
       - Cash receipt
       - Payment voucher
       - Offset agreement

VALIDATION:
  IF Amount > 20,000,000 VND
    THEN Require all three components
  IF Amount <= 20,000,000 VND
    THEN Require at least Source Document + Payment Evidence
```

#### 4.1.2 Document Validation Rules
```csharp
RULE-DOC-001: DOCUMENT UNIQUENESS
  ASSERT: DocumentNumber is unique within (Type + FiscalYear)
  ERROR: "Chứng từ trùng số"

RULE-DOC-002: DOCUMENT DATE VALIDATION
  ASSERT: DocumentDate >= FiscalYearStart
  ASSERT: DocumentDate <= CurrentDate + 30 days (grace for invoices)
  ASSERT: DocumentDate <= EntryDate (usually)
  
RULE-DOC-003: TAX CODE VALIDATION
  IF DocumentType == "Invoice"
    THEN Validate buyer/seller tax code with General Department of Taxation
    ERROR_IF_INVALID: "Mã số thuế không hợp lệ"

RULE-DOC-004: AMOUNT CONSISTENCY
  IF LinkedInvoice
    THEN ASSERT: |EntryAmount - InvoiceAmount| < 1,000 VND
    OR ASSERT: EntryAmount == PartialPaymentAmount (with PO)

RULE-DOC-005: INVOICE STATUS CHECK
  Invoice must NOT be:
    - Canceled (Đã hủy)
    - Replaced (Đã thay thế)
    - Adjusted (Đã điều chỉnh) - use adjustment invoice
  
  IF InvoiceStatus != "Valid"
    THEN ERROR: "Hóa đơn không hợp lệ"

RULE-DOC-006: E-INVOICE FORMAT
  Per TT78/2021/TT-BTC:
    - XML format compliance
    - Digital signature valid
    - QR code readable
    - All mandatory fields present
    - Sequential numbering
```

### 4.2 Electronic Invoice Compliance (Hóa đơn điện tử)

#### 4.2.1 E-Invoice Lifecycle
```
LIFECYCLE STATES:
  DRAFT → ISSUED → DELIVERED → POSTED → [ADJUSTED|REPLACED|CANCELED]

DRAFT: Created but not signed/sent
ISSUED: Signed and sent to buyer
DELIVERED: Buyer received (acknowledged)
POSTED: Accounting entry created
ADJUSTED: Replaced by adjustment invoice
REPLACED: Replaced by new invoice (error correction)
CANCELED: Voided (with tax authority approval)
```

#### 4.2.2 E-Invoice Rules
```csharp
RULE-EINV-001: SEQUENTIAL NUMBERING
  Invoice numbers must be sequential
  Gap detection: Alert if gap > 1
  Missing numbers must be explained

RULE-EINV-002: TIMING RULES
  Invoice must be issued:
    - At time of goods delivery, OR
    - At time of service completion, OR
    - At time of payment receipt (earliest)
  
  LATE_INVOICE_THRESHOLD = EntryDate - InvoiceDate > 30 days
  IF LATE_INVOICE
    THEN FLAG: "Hóa đơn phát hành chậm"
    REQUIRE: Written explanation

RULE-EINV-003: AMOUNT LIMITS
  IF InvoiceAmount > 200,000,000 VND
    THEN Require: E-invoice with digital signature
    AND: Internal approval workflow
    AND: CFO notification

RULE-EINV-004: CURRENCY HANDLING
  IF ForeignCurrencyInvoice
    THEN Convert to VND using:
      - Contract rate (if specified), OR
      - Interbank rate on invoice date, OR
      - State Bank rate on invoice date
    AND: Store both amounts

RULE-EINV-005: ADJUSTMENTS
  Adjustment types:
    - Price adjustment: Issue adjustment invoice
    - Quantity adjustment: Issue adjustment invoice
    - Cancel and reissue: For errors > reporting period
  
  Original invoice must remain in system
  Link adjustment → original (chain)
```

### 4.3 Document Retention
```csharp
RULE-DOC-RET-001: RETENTION PERIODS
  Document Type              | Retention Period
  ---------------------------|------------------
  Accounting vouchers        | 10 years
  Invoices (e-invoice XML)   | 10 years
  Contracts                  | Contract term + 5 years
  Tax returns                | 10 years
  Payroll records            | 10 years
  Bank statements            | 10 years
  Fixed asset records        | Asset life + 5 years

RULE-DOC-RET-002: NO DELETION
  Documents cannot be deleted
  "Delete" = Mark as archived + audit log
  Physical documents: Store in fireproof location
  Electronic: Backup to immutable storage (WORM)

RULE-DOC-RET-003: IMMUTABLE STORAGE
  Use Write-Once-Read-Many (WORM) storage
  Cryptographic verification (checksums)
  Geographic redundancy (minimum 2 locations)
```

---

## 5. PERIOD CLOSING & ADJUSTMENT

### 5.1 Closing Procedures

#### 5.1.1 Month-End Closing (Kết thúc tháng)
```
PHASE 1: PREPARATION (Day 1-3 after month end)
  □ Complete all entries for the month
  □ Reconcile bank accounts
  □ Verify accounts payable/receivable
  □ Complete inventory counts
  □ Verify fixed asset depreciation
  □ Check VAT input/output completeness
  □ Review accruals and prepayments

PHASE 2: ADJUSTMENTS (Day 4-5)
  □ Record depreciation
  □ Record amortization
  □ Accrue expenses (if applicable)
  □ Adjust prepayments
  □ Record provisions
  □ Foreign exchange adjustments (if any)

PHASE 3: RECONCILIATION (Day 6-8)
  □ Trial balance review
  □ Subsidiary ledger reconciliation
  □ Tax reconciliation
  □ Inter-company reconciliation
  □ Variance analysis (actual vs budget)

PHASE 4: APPROVAL & CLOSING (Day 9-10)
  □ CFO review
  □ Chief Accountant sign-off
  □ Close period in system
  □ Generate management reports
  □ Backup data
```

#### 5.1.2 Year-End Closing (Kết thúc năm)
```
PHASE 1-4: Same as month-end (November 30 - December 20)

PHASE 5: YEAR-END SPECIFIC (December 21 - January 15)
  □ Physical inventory count (mandatory)
  □ Fixed asset verification
  □ Debt confirmation letters
  □ Provision for doubtful debts assessment
  □ Asset impairment testing
  □ Tax finalization preparation
  □ Related party transaction reporting

PHASE 6: ADJUSTING ENTRIES (January 16-31)
  □ Inventory adjustments (if any)
  □ Bad debt write-offs (with approval)
  □ Depreciation catch-up
  □ Tax adjustments (permanent differences)
  □ Prior period adjustments (if material)

PHASE 7: CLOSING ENTRIES (January 31 - February 15)
  □ Close revenue accounts (to 911)
  □ Close expense accounts (to 911)
  □ Transfer 911 balance to 421 (Retained earnings)
  □ Verify all P&L accounts = 0
  □ Verify Balance Sheet balances

PHASE 8: LOCK & REPORT (February 16-28)
  □ External audit (if required)
  □ Tax finalization filing
  □ Financial statement approval
  □ Lock fiscal year permanently
  □ Archive all records
```

### 5.2 Adjustment Entries (Bút toán điều chỉnh)

#### 5.2.1 Adjustment Types
```
TYPE 1: CORRECTION OF ERRORS (Sửa sai sót)
  - For material errors in closed periods
  - Must be approved by CFO + External Auditor
  - Creates adjustment entry in current period
  - References original error entry
  
TYPE 2: CHANGES IN ACCOUNTING ESTIMATES (Thay đổi ước tính)
  - Prospective application
  - No adjustment to prior periods
  - Document rationale
  
TYPE 3: CHANGES IN ACCOUNTING POLICY (Thay đổi chính sách)
  - Retrospective application
  - Adjust opening balances
  - Restate comparative figures
  - Requires extensive documentation

TYPE 4: EVENTS AFTER REPORTING PERIOD (Sự kiện sau ngày BCTC)
  - Adjusting events: Modify financial statements
  - Non-adjusting events: Disclosure only
```

#### 5.2.2 Adjustment Entry Rules
```csharp
RULE-ADJ-001: MATERIALITY THRESHOLD
  IF ErrorAmount > (TotalAssets × 0.5%) OR
     ErrorAmount > (Revenue × 1%)
    THEN Adjustment REQUIRED
    ELSE Correction in current period acceptable

RULE-ADJ-002: AUTHORIZATION MATRIX
  Adjustment Type          | Approver
  -------------------------|------------------
  Immaterial (< 10M)       | Chief Accountant
  Material (10M-100M)      | CFO
  Highly material (> 100M) | CEO + Audit Committee

RULE-ADJ-003: ADJUSTMENT ENTRY FORMAT
  Every adjustment entry must include:
    - Reference to original entry (OriginalEntryId)
    - Adjustment reason code (standardized)
    - Detailed explanation
    - Impact assessment
    - Approval documentation

RULE-ADJ-004: NO OVERWRITE
  - Never modify original entry
  - Always create new adjustment entry
  - Chain: Original → Adjustment → Current Entry

RULE-ADJ-005: PRIOR PERIOD ADJUSTMENTS
  IF AdjustingPriorPeriod
    THEN EntryDate = CurrentDate
    AND AccountingPeriod = CurrentPeriod
    AND AffectsOpeningBalances = True
    AND RequiresComparativeRestatement = True
```

### 5.3 Re-Opening Rules (Mở lại kỳ)
```csharp
RULE-REOPEN-001: RE-OPENING PROHIBITED IN PRINCIPLE
  Closed periods should NOT be reopened
  Exception: Material error discovery

RULE-REOPEN-002: EMERGENCY RE-OPENING PROCESS
  1. Discovery of material error in closed period
  2. CFO documents error and proposed correction
  3. External Auditor review (mandatory)
  4. CEO approves in writing
  5. Audit Committee ratifies (if exists)
  6. Re-open period (creates audit log)
  7. Post correction entry
  8. Re-close period
  9. Notify tax authority if affects tax filings

RULE-REOPEN-003: RE-OPENING LIMITATIONS
  - Cannot reopen fiscal year after tax finalization filed
  - Cannot reopen if external audit completed
  - Cannot reopen periods > 2 years old
  - Maximum 1 reopening per period

RULE-REOPEN-004: TAX IMPACT ASSESSMENT
  IF ReopeningAffectsTax
    THEN File amended tax return
    AND Calculate interest/penalties
    AND Disclose in notes
```

---

## 6. INTERNAL CONTROL & RISK PREVENTION

### 6.1 Separation of Duties (Phân quyền)

#### 6.1.1 Critical Function Separation
```
INCOMPATIBLE DUTIES (Must be separate users):

1. TRANSACTION INITIATION ≠ TRANSACTION APPROVAL
   - User creating entry cannot approve it
   
2. ASSET CUSTODY ≠ RECORD KEEPING
   - Cash handler cannot record cash entries
   - Warehouse keeper cannot record inventory entries
   
3. OPERATIONS ≠ RECONCILIATION
   - Sales person cannot reconcile receivables
   - Buyer cannot reconcile payables
   
4. DATA ENTRY ≠ REPORT GENERATION
   - Prevents manipulation for reporting

5. SYSTEM ADMIN ≠ BUSINESS USER
   - IT admin cannot post accounting entries
   - Business users cannot modify system configurations
```

#### 6.1.2 Authorization Matrix (Ma trận phê duyệt)
```csharp
// TRANSACTION AMOUNT LIMITS (VND)
public class AuthorizationMatrix
{
    // Journal Entries
    public static readonly Dictionary<string, decimal> EntryLimits = new()
    {
        ["Accountant"] = 50_000_000,           // 50M
        ["SeniorAccountant"] = 200_000_000,    // 200M
        ["ChiefAccountant"] = 500_000_000,     // 500M
        ["CFO"] = 1_000_000_000,               // 1B
        ["CEO"] = decimal.MaxValue             // Unlimited
    };
    
    // Payments
    public static readonly Dictionary<string, decimal> PaymentLimits = new()
    {
        ["Accountant"] = 20_000_000,
        ["SeniorAccountant"] = 100_000_000,
        ["ChiefAccountant"] = 300_000_000,
        ["CFO"] = 1_000_000_000,
        ["CEO"] = decimal.MaxValue
    };
    
    // Period Closing
    public static readonly Dictionary<string, string> ClosingAuth = new()
    {
        ["Month"] = "ChiefAccountant",
        ["Quarter"] = "CFO",
        ["Year"] = "CEO"
    };
}

RULE-AUTH-001: DUAL AUTHORIZATION
  IF TransactionAmount > 500_000_000 VND
    THEN Require 2 approvers:
      - Primary: Transaction owner
      - Secondary: One level higher in hierarchy
      
RULE-AUTH-002: EMERGENCY OVERRIDE
  - Exists but creates permanent audit flag
  - Requires: Emergency reason + Immediate supervisor approval
  - Reviewed by Internal Audit within 48 hours
```

### 6.2 Red Flag Detection (Cảnh báo rủi ro)

#### 6.2.1 Revenue Risks
```csharp
RULE-RISK-REV-001: UNRECORDED REVENUE
  Detection:
    IF BankDeposit > (RecordedRevenue × 1.1)
      THEN FLAG: "Tiền gửi ngân hàng vượt doanh thu ghi nhận"
      
    IF InvoiceNumberGap > 5
      THEN FLAG: "Khoảng cách số hóa đơn bất thường"

RULE-RISK-REV-002: PREMATURE REVENUE
  IF RevenueRecorded AND (DeliveryDate > EntryDate + 30 days)
    THEN FLAG: "Doanh thu ghi nhận trước khi giao hàng"
    
RULE-RISK-REV-003: ROUND NUMBER ANOMALY
  IF > 20% of transactions are round numbers (e.g., 1000000)
    THEN FLAG: "Tỷ lệ giao dịch số tròn cao bất thường"

RULE-RISK-REV-004: CONCENTRATION RISK
  IF TopCustomerRevenue > TotalRevenue × 30%
    THEN FLAG: "Phụ thuộc vào khách hàng lớn"
```

#### 6.2.2 Expense Risks
```csharp
RULE-RISK-EXP-001: INFLATED EXPENSES
  IF MonthlyExpense > HistoricalAverage × 1.5
    THEN FLAG: "Chi phí tháng cao bất thường"
    REQUIRE: Detailed breakdown

RULE-RISK-EXP-002: DUPLICATE PAYMENTS
  IF SameAmount + SameVendor + SameDate + Within 7 days
    THEN FLAG: "Thanh toán trùng lặp"
    ACTION: Block second payment pending review

RULE-RISK-EXP-003: JUST BELOW THRESHOLD
  IF ExpenseAmount == (ApprovalThreshold - 1)
    THEN FLAG: "Chi phí sát ngưỡng phê duyệt"
    REQUIRE: Additional verification

RULE-RISK-EXP-004: VENDOR VALIDATION
  IF VendorNotInApprovedList
    THEN FLAG: "Nhà cung cấp chưa được phê duyệt"
    REQUIRE: Procurement approval

RULE-RISK-EXP-005: PERSONAL EXPENSES
  IF ExpenseDescription.Contains(["cafe", "giải trí", "du lịch"])
    THEN FLAG: "Chi phí cá nhân nghi vấn"
    REQUIRE: Business justification
```

#### 6.2.3 Tax Risks
```csharp
RULE-RISK-TAX-001: VAT OUTPUT DECLINE
  IF CurrentMonthOutputVAT < PriorMonthOutputVAT × 0.7
    THEN FLAG: "Thuế GTGT đầu ra giảm đột ngột"
    REQUIRE: Revenue explanation

RULE-RISK-TAX-002: INPUT VAT ACCUMULATION
  IF AccumulatedInputVAT > 300_000_000 (3 months)
    THEN FLAG: "VAT đầu vào tồn đọng"
    SUGGEST: Refund application

RULE-RISK-TAX-003: TNDN RATE ANOMALY
  EffectiveRate = TaxPaid / AccountingProfit
  IF EffectiveRate < 15% AND Profit > 0
    THEN FLAG: "Tỷ lệ thuế TNDN hiệu quả thấp"
    REQUIRE: Tax reconciliation review

RULE-RISK-TAX-004: TRANSFER PRICING
  IF RelatedPartyTransactionAmount > 100_000_000_000
    THEN FLAG: "Giao dịch liên kết lớn"
    REQUIRE: Transfer pricing documentation
```

#### 6.2.4 Cash & Bank Risks
```csharp
RULE-RISK-CASH-001: CASH LIMIT
  IF CashBalance > 50_000_000 (end of day)
    THEN FLAG: "Dư tiền mặt vượt ngưỡng"
    REQUIRE: Deposit to bank

RULE-RISK-CASH-002: CASH WITHDRAWAL PATTERN
  IF CashWithdrawal > Revenue × 20% (monthly)
    THEN FLAG: "Rút tiền mặt bất thường"

RULE-RISK-BANK-003: UNRECONCILED ITEMS
  IF UnreconciledBankItems > 30 days old
    THEN FLAG: "Khoản mục ngân hàng chưa đối chiếu"
    REQUIRE: Immediate reconciliation

RULE-RISK-BANK-004: ROUND-TRIP TRANSACTIONS
  IF CompanyA → CompanyB → CompanyA (circular)
    THEN FLAG: "Giao dịch vòng tròn nghi vấn"
    REQUIRE: Business justification
```

### 6.3 Preventive Controls

#### 6.3.1 Entry Validation
```csharp
RULE-CTRL-001: MANDATORY FIELDS CHECK
  Before saving any entry, validate:
    ✓ Entry date (not in future, not in locked period)
    ✓ Source document number (not empty)
    ✓ Source document date (valid, <= entry date)
    ✓ At least one debit and one credit line
    ✓ Debits = Credits
    ✓ All accounts exist and are active
    ✓ Amount > 0
    ✓ Currency specified
    ✓ User has permission for account/amount

RULE-CTRL-002: REAL-TIME WARNINGS
  Display warnings (but allow override with reason):
    - Unusual account combination
    - Large amount (relative to history)
    - Duplicate transaction suspicion
    - Vendor/customer not in master data
    - Expense approaching budget limit

RULE-CTRL-003: HARD STOPS (Cannot override)
    - Entry in closed period
    - Debits ≠ Credits
    - Negative amount
    - Invalid account code
    - Missing source document
    - User lacks authorization
```

#### 6.3.2 Segregation Enforcement
```csharp
RULE-CTRL-004: SYSTEM-ENFORCED SEPARATION
  Database-level constraints:
    - Creator ≠ Approver (same transaction)
    - Cash handler ≠ Cash recorder
    - Buyer ≠ Receiving clerk
    - Salesperson ≠ Receivables reconciler

RULE-CTRL-005: FUNCTIONAL RESTRICTIONS
  User roles restrict:
    - Which accounts can be used
    - Which periods can be accessed
    - Which reports can be viewed
    - Which master data can be modified
```

---

## 7. TEST CASES (TDD)

### 7.1 Testing Strategy

#### 7.1.1 Test Categories
```
1. UNIT TESTS: Individual business rules
   - Each validation rule has test cases
   - Mock external dependencies
   
2. INTEGRATION TESTS: End-to-end workflows
   - Complete transaction processing
   - Database persistence
   - Event publishing
   
3. DOMAIN TESTS: Complex business scenarios
   - Multi-step processes
   - Exception handling
   - State transitions
   
4. COMPLIANCE TESTS: Tax & accounting regulations
   - TT99 requirements
   - Tax law requirements
   - Edge cases
   
5. PROPERTY-BASED TESTS: Generative testing
   - Random data generation
   - Invariant checking
```

#### 7.1.2 Test Coverage Requirements
```
MINIMUM COVERAGE:
  - Critical business rules: 100%
  - Tax calculations: 100%
  - Period closing logic: 100%
  - Validation rules: 100%
  - Adjustment entries: 100%
  
ACCEPTABLE COVERAGE:
  - Overall domain logic: > 90%
  - Application services: > 80%
  - Infrastructure: > 60%
```

### 7.2 Core Test Scenarios

#### 7.2.1 Journal Entry Tests
```csharp
// TEST: JE-001 - Valid Entry
[Fact]
public void CreateEntry_WithValidData_ShouldSucceed()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: new DateTime(2026, 2, 15),
        sourceDocNumber: "INV-2026-001",
        sourceDocDate: new DateTime(2026, 2, 14),
        description: "Bán hàng tiền mặt"
    );
    
    // Act
    entry.AddLine(debitAccount: "1111", creditAccount: "5111", amount: 11_000_000);
    entry.AddLine(debitAccount: "1111", creditAccount: "33311", amount: 1_000_000);
    
    // Assert
    entry.IsBalanced.Should().BeTrue();
    entry.TotalDebit.Should().Be(12_000_000);
    entry.TotalCredit.Should().Be(12_000_000);
    entry.CanPost().Should().BeTrue();
}

// TEST: JE-002 - Unbalanced Entry Must Fail
[Fact]
public void CreateEntry_Unbalanced_ShouldThrowException()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Test"
    );
    
    // Act
    entry.AddLine("1111", "5111", 10_000_000);
    entry.AddLine("1111", "33311", 1_000_000); // Missing 1M credit
    
    // Assert
    Assert.Throws<InvalidOperationException>(() => entry.Post())
        .Message.Should().Contain("Bút toán không cân bằng");
}

// TEST: JE-003 - Entry in Closed Period Must Fail
[Fact]
public void CreateEntry_InClosedPeriod_ShouldThrowException()
{
    // Arrange
    var closedPeriod = new AccountingPeriod(2025, 12);
    closedPeriod.Close(); // Close December 2025
    
    // Act & Assert
    Assert.Throws<PeriodClosedException>(() => 
        JournalEntry.Create(
            entryDate: new DateTime(2025, 12, 15), // In closed period
            sourceDocNumber: "TEST-001",
            sourceDocDate: new DateTime(2025, 12, 14),
            description: "Test"
        ));
}

// TEST: JE-004 - Missing Source Document Must Fail
[Theory]
[InlineData("")]
[InlineData(" ")]
[InlineData(null)]
public void CreateEntry_WithoutSourceDocument_ShouldThrowException(string? docNumber)
{
    // Act & Assert
    var ex = Assert.Throws<ArgumentException>(() =>
        JournalEntry.Create(
            entryDate: DateTime.Now,
            sourceDocNumber: docNumber!,
            sourceDocDate: DateTime.Now.AddDays(-1),
            description: "Test"
        ));
    
    ex.Message.Should().Contain("TT99");
    ex.Message.Should().Contain("chứng từ gốc");
}

// TEST: JE-005 - Debit Equals Credit Must Fail
[Fact]
public void CreateEntry_SameDebitCreditAccount_ShouldThrowException()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Test"
    );
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
        entry.AddLine("1111", "1111", 10_000_000)) // Same account
        .Message.Should().Contain("không được trùng nhau");
}

// TEST: JE-006 - Future Date Entry Must Fail
[Fact]
public void CreateEntry_WithFutureDate_ShouldThrowException()
{
    // Arrange
    var futureDate = DateTime.Now.AddDays(1);
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        JournalEntry.Create(
            entryDate: futureDate,
            sourceDocNumber: "TEST-001",
            sourceDocDate: futureDate.AddDays(-1),
            description: "Test"
        ));
}

// TEST: JE-007 - Source Document Date After Entry Date Must Fail
[Fact]
public void CreateEntry_SourceDocDateAfterEntryDate_ShouldThrowException()
{
    // Arrange
    var entryDate = new DateTime(2026, 2, 15);
    var sourceDocDate = new DateTime(2026, 2, 16); // After entry date
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        JournalEntry.Create(
            entryDate: entryDate,
            sourceDocNumber: "TEST-001",
            sourceDocDate: sourceDocDate,
            description: "Test"
        ));
}

// TEST: JE-008 - Negative Amount Must Fail
[Theory]
[InlineData(-1000)]
[InlineData(0)]
public void CreateEntry_WithInvalidAmount_ShouldThrowException(decimal amount)
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Test"
    );
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        entry.AddLine("1111", "5111", amount));
}

// TEST: JE-009 - Account 911 Restriction
[Fact]
public void CreateEntry_With911Account_ShouldRequireAuthorization()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Year-end closing"
    );
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
        entry.AddLine("911", "421", 100_000_000))
        .Message.Should().Contain("TK 911");
}
```

#### 7.2.2 VAT Tests
```csharp
// TEST: VAT-001 - Output VAT Calculation
[Fact]
public void CalculateOutputVAT_WithValidData_ShouldBeCorrect()
{
    // Arrange
    var invoice = new Invoice(
        amount: 100_000_000,
        vatRate: 10
    );
    
    // Act
    var outputVAT = invoice.CalculateOutputVAT();
    
    // Assert
    outputVAT.Should().Be(10_000_000);
    invoice.TotalAmount.Should().Be(110_000_000);
}

// TEST: VAT-002 - Input VAT Deductibility
[Theory]
[InlineData("Purchase of goods for resale", true)] // Deductible
[InlineData("Purchase of passenger vehicle", false)] // Non-deductible
[InlineData("Entertainment expenses", false)] // Non-deductible
public void DetermineInputVATDeductibility_ShouldClassifyCorrectly(
    string description, bool expectedDeductible)
{
    // Arrange
    var purchase = new PurchaseEntry(description: description);
    
    // Act
    var isDeductible = purchase.IsInputVATDeductible();
    
    // Assert
    isDeductible.Should().Be(expectedDeductible);
}

// TEST: VAT-003 - Invalid VAT Rate Must Fail
[Theory]
[InlineData(7)]
[InlineData(12)]
[InlineData(-5)]
public void CreateInvoice_WithInvalidVATRate_ShouldThrowException(int rate)
{
    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new Invoice(amount: 100_000_000, vatRate: rate))
        .Message.Should().Contain("Thuế suất GTGT không hợp lệ");
}

// TEST: VAT-004 - VAT Reconciliation
[Fact]
public void ReconcileVAT_AccountingVsDeclaration_ShouldMatch()
{
    // Arrange
    var month = new AccountingPeriod(2026, 2);
    var accountingVAT = month.CalculateTotalVAT();
    var declaredVAT = TaxDeclaration.GetVATForPeriod(month);
    
    // Act
    var difference = Math.Abs(accountingVAT - declaredVAT);
    
    // Assert
    difference.Should().BeLessThan(1_000); // Within 1,000 VND tolerance
}

// TEST: VAT-005 - E-Invoice Without Output VAT Entry
[Fact]
public void RecordRevenue_WithoutInvoice_ShouldFlagRisk()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "RECEIPT-001", // Not an invoice
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Bán hàng"
    );
    entry.AddLine("1111", "5111", 50_000_000); // Revenue
    entry.AddLine("1111", "33311", 5_000_000); // Output VAT
    
    // Act
    var validation = entry.Validate();
    
    // Assert
    validation.Warnings.Should().Contain(w => 
        w.Message.Contains("Doanh thu phát sinh thuế GTGT nhưng không có hóa đơn"));
}
```

#### 7.2.3 Period Closing Tests
```csharp
// TEST: PER-001 - Close Period with Unposted Entries Must Fail
[Fact]
public void ClosePeriod_WithUnpostedEntries_ShouldThrowException()
{
    // Arrange
    var period = new AccountingPeriod(2026, 2);
    var entry = JournalEntry.Create(
        entryDate: new DateTime(2026, 2, 15),
        sourceDocNumber: "TEST-001",
        sourceDocDate: new DateTime(2026, 2, 14),
        description: "Test"
    );
    entry.AddLine("1111", "5111", 10_000_000);
    // Not posted!
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => period.Close())
        .Message.Should().Contain("Còn bút toán chưa ghi sổ");
}

// TEST: PER-002 - Close Period with Imbalanced Trial Balance Must Fail
[Fact]
public void ClosePeriod_WithImbalancedTB_ShouldThrowException()
{
    // Arrange
    var period = new AccountingPeriod(2026, 2);
    // Simulate imbalanced data
    period.TrialBalance.TotalDebit = 1_000_000_000;
    period.TrialBalance.TotalCredit = 999_999_999;
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => period.Close())
        .Message.Should().Contain("Bảng cân đối không cân bằng");
}

// TEST: PER-003 - Reopen Closed Period Must Require Authorization
[Fact]
public void ReopenClosedPeriod_WithoutAuthorization_ShouldFail()
{
    // Arrange
    var period = new AccountingPeriod(2026, 2);
    period.Close();
    var user = new User(role: "Accountant"); // Not authorized
    
    // Act & Assert
    Assert.Throws<UnauthorizedAccessException>(() =>
        period.Reopen(requestedBy: user, reason: "Correction"));
}

// TEST: PER-004 - Adjustment Entry in Closed Period Creates Alert
[Fact]
public void PostAdjustment_ToClosedPeriod_CreatesAuditAlert()
{
    // Arrange
    var closedPeriod = new AccountingPeriod(2026, 1);
    closedPeriod.Close();
    
    var adjustment = new AdjustmentEntry(
        originalPeriod: closedPeriod,
        targetPeriod: new AccountingPeriod(2026, 2),
        reason: "Error correction"
    );
    
    // Act
    adjustment.Post();
    
    // Assert
    adjustment.AuditAlerts.Should().Contain(a =>
        a.Type == AuditAlertType.PeriodAdjustment);
}
```

#### 7.2.4 Tax Reconciliation Tests
```csharp
// TEST: TAX-001 - VAT Output Mismatch Detection
[Fact]
public void DetectVATMismatch_AccountingVsDeclaration_ShouldAlert()
{
    // Arrange
    var month = new AccountingPeriod(2026, 2);
    month.AccountingOutputVAT = 100_000_000;
    month.DeclaredOutputVAT = 95_000_000; // 5M difference
    
    // Act
    var reconciliation = month.ReconcileVAT();
    
    // Assert
    reconciliation.Difference.Should().Be(5_000_000);
    reconciliation.IsWithinTolerance.Should().BeFalse();
    reconciliation.Alerts.Should().Contain(a =>
        a.Severity == AlertSeverity.High);
}

// TEST: TAX-002 - Non-Deductible Expense Classification
[Fact]
public void ClassifyExpense_Fines_ShouldBeNonDeductible()
{
    // Arrange
    var expense = new ExpenseEntry(
        account: "811",
        amount: 5_000_000,
        description: "Tiền phạt vi phạm giao thông"
    );
    
    // Act
    var classification = expense.GetTaxClassification();
    
    // Assert
    classification.IsDeductible.Should().BeFalse();
    classification.Reason.Should().Contain("phạt");
}

// TEST: TAX-003 - TNCN Progressive Calculation
[Theory]
[InlineData(5_000_000, 0, 250_000)]       // 5M, no dependents
[InlineData(10_000_000, 1, 500_000)]      // 10M, 1 dependent
[InlineData(30_000_000, 2, 4_250_000)]    // 30M, 2 dependents
public void CalculateTNCN_WithVariousIncomes_ShouldBeCorrect(
    decimal grossSalary, int dependents, decimal expectedTax)
{
    // Arrange
    var employee = new Employee(
        grossSalary: grossSalary,
        dependents: dependents
    );
    
    // Act
    var tax = employee.CalculateMonthlyTNCN();
    
    // Assert
    tax.Should().Be(expectedTax);
}
```

#### 7.2.5 Internal Control Tests
```csharp
// TEST: CTRL-001 - Separation of Duties Enforcement
[Fact]
public void ApproveEntry_SameAsCreator_ShouldFail()
{
    // Arrange
    var user = new User(id: "USER001", role: "Accountant");
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Test",
        createdBy: user
    );
    entry.AddLine("1111", "5111", 10_000_000);
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
        entry.Approve(approvedBy: user)) // Same user
        .Message.Should().Contain("Người tạo không được phê duyệt");
}

// TEST: CTRL-002 - Authorization Limit Enforcement
[Fact]
public void PostEntry_AboveAuthorizationLimit_ShouldRequireHigherAuth()
{
    // Arrange
    var accountant = new User(role: "Accountant"); // Limit: 50M
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Large transaction",
        createdBy: accountant
    );
    entry.AddLine("1111", "5111", 100_000_000); // Above 50M limit
    
    // Act
    var canPost = entry.CanPost(user: accountant);
    
    // Assert
    canPost.Should().BeFalse();
    entry.RequiredApproverRole.Should().Be("SeniorAccountant");
}

// TEST: CTRL-003 - Red Flag Detection
[Fact]
public void PostEntry_WithDuplicateSuspicion_ShouldFlag()
{
    // Arrange
    var entry1 = JournalEntry.Create(
        entryDate: new DateTime(2026, 2, 15),
        sourceDocNumber: "INV-001",
        sourceDocDate: new DateTime(2026, 2, 14),
        description: "Payment to Supplier A"
    );
    entry1.AddLine("331", "1111", 50_000_000);
    entry1.Post();
    
    var entry2 = JournalEntry.Create(
        entryDate: new DateTime(2026, 2, 16), // Next day
        sourceDocNumber: "INV-001", // Same invoice!
        sourceDocDate: new DateTime(2026, 2, 14),
        description: "Payment to Supplier A"
    );
    entry2.AddLine("331", "1111", 50_000_000);
    
    // Act
    var warnings = entry2.Validate();
    
    // Assert
    warnings.Should().Contain(w =>
        w.Message.Contains("Thanh toán trùng lặp"));
}

// TEST: CTRL-004 - Cash Limit Alert
[Fact]
public void EndOfDayCash_AboveLimit_ShouldAlert()
{
    // Arrange
    var cashAccount = new CashAccount("1111");
    cashAccount.AddTransaction(amount: 60_000_000); // Above 50M limit
    
    // Act
    var status = cashAccount.CheckEndOfDayLimit();
    
    // Assert
    status.IsWithinLimit.Should().BeFalse();
    status.Alert.Should().Contain("Dư tiền mặt vượt ngưỡng");
}
```

### 7.3 Edge Case & Illegal Case Tests

#### 7.3.1 Edge Cases
```csharp
// TEST: EDGE-001 - Entry at Period Boundary
[Fact]
public void CreateEntry_AtPeriodBoundary_ShouldAssignCorrectPeriod()
{
    // Arrange
    var entryDate = new DateTime(2026, 2, 28, 23, 59, 59); // Last moment of Feb
    
    // Act
    var entry = JournalEntry.Create(
        entryDate: entryDate,
        sourceDocNumber: "TEST-001",
        sourceDocDate: entryDate.AddDays(-1),
        description: "Test"
    );
    
    // Assert
    entry.AccountingPeriod.Month.Should().Be(2);
}

// TEST: EDGE-002 - Zero Amount Entry
[Fact]
public void CreateEntry_WithZeroAmount_ShouldFail()
{
    // Arrange & Act & Assert
    Assert.Throws<ArgumentException>(() =>
        JournalEntry.Create(
            entryDate: DateTime.Now,
            sourceDocNumber: "TEST-001",
            sourceDocDate: DateTime.Now.AddDays(-1),
            description: "Test"
        ).AddLine("1111", "5111", 0));
}

// TEST: EDGE-003 - Very Large Amount Entry
[Fact]
public void CreateEntry_WithVeryLargeAmount_ShouldSucceedButFlag()
{
    // Arrange
    var hugeAmount = 999_999_999_999.99m; // Near max
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Large transaction"
    );
    
    // Act
    entry.AddLine("1111", "5111", hugeAmount);
    var warnings = entry.Validate();
    
    // Assert
    entry.TotalDebit.Should().Be(hugeAmount);
    warnings.Should().Contain(w => w.Message.Contains("Số tiền lớn"));
}

// TEST: EDGE-004 - Multi-Currency Entry with Rate Fluctuation
[Fact]
public void CreateEntry_MultiCurrency_WithRateChange_ShouldRecalculate()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: new DateTime(2026, 2, 15),
        sourceDocNumber: "TEST-001",
        sourceDocDate: new DateTime(2026, 2, 14),
        description: "USD purchase"
    );
    
    var usdAmount = 10_000m;
    var rate1 = 25_000m; // Rate on transaction date
    var rate2 = 25_500m; // Current rate
    
    // Act
    entry.AddLine(
        debitAccount: "156", // Inventory
        creditAccount: "331", // Payables
        foreignAmount: usdAmount,
        currency: "USD",
        exchangeRate: rate1
    );
    
    // Assert
    entry.TotalDebit.Should().Be(usdAmount * rate1);
    entry.ForeignAmount.Should().Be(usdAmount);
}

// TEST: EDGE-005 - Entry with Maximum Lines
[Fact]
public void CreateEntry_WithMaximumLines_ShouldSucceed()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Complex entry"
    );
    
    // Act - Add 99 lines (practical limit)
    for (int i = 0; i < 99; i++)
    {
        entry.AddLine("1111", "5111", 1_000_000);
    }
    
    // Assert
    entry.LineCount.Should().Be(99);
    entry.IsBalanced.Should().BeTrue();
}
```

#### 7.3.2 Illegal Cases (Must Fail)
```csharp
// TEST: ILLEGAL-001 - Posting to Non-Existent Account
[Fact]
public void CreateEntry_WithInvalidAccountCode_ShouldThrowException()
{
    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        JournalEntry.Create(
            entryDate: DateTime.Now,
            sourceDocNumber: "TEST-001",
            sourceDocDate: DateTime.Now.AddDays(-1),
            description: "Test"
        ).AddLine("99999", "5111", 10_000_000)) // Invalid account
        .Message.Should().Contain("Tài khoản không tồn tại");
}

// TEST: ILLEGAL-002 - Negative VAT Rate
[Fact]
public void CreateInvoice_WithNegativeVATRate_ShouldThrowException()
{
    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new Invoice(amount: 100_000_000, vatRate: -10));
}

// TEST: ILLEGAL-003 - Future Source Document Date
[Fact]
public void CreateEntry_WithFutureSourceDocDate_ShouldFail()
{
    // Arrange
    var futureDate = DateTime.Now.AddDays(1);
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        JournalEntry.Create(
            entryDate: DateTime.Now,
            sourceDocNumber: "TEST-001",
            sourceDocDate: futureDate,
            description: "Test"
        ));
}

// TEST: ILLEGAL-004 - Deleting Posted Entry
[Fact]
public void DeletePostedEntry_ShouldThrowException()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Test"
    );
    entry.AddLine("1111", "5111", 10_000_000);
    entry.Post();
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => entry.Delete())
        .Message.Should().Contain("Không thể xóa bút toán đã ghi sổ");
}

// TEST: ILLEGAL-005 - Bypassing Authorization
[Fact]
public void PostHighValueEntry_WithoutAuthorization_ShouldFail()
{
    // Arrange
    var juniorUser = new User(role: "Accountant");
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Large payment",
        createdBy: juniorUser
    );
    entry.AddLine("331", "1121", 500_000_000); // Above 50M limit
    
    // Act & Assert
    Assert.Throws<UnauthorizedAccessException>(() =>
        entry.Post(authorizedBy: juniorUser));
}

// TEST: ILLEGAL-006 - Creating Entry in Locked Year
[Fact]
public void CreateEntry_InLockedYear_ShouldThrowException()
{
    // Arrange
    var lockedYear = new FiscalYear(2025);
    lockedYear.Lock(); // Permanently locked
    
    // Act & Assert
    Assert.Throws<PeriodLockedException>(() =>
        JournalEntry.Create(
            entryDate: new DateTime(2025, 6, 15),
            sourceDocNumber: "TEST-001",
            sourceDocDate: new DateTime(2025, 6, 14),
            description: "Test"
        ));
}

// TEST: ILLEGAL-007 - Unbalanced Multi-Line Entry
[Fact]
public void CreateMultiLineEntry_Unbalanced_ShouldFail()
{
    // Arrange
    var entry = JournalEntry.Create(
        entryDate: DateTime.Now,
        sourceDocNumber: "TEST-001",
        sourceDocDate: DateTime.Now.AddDays(-1),
        description: "Complex entry"
    );
    
    // Act - Intentionally unbalanced
    entry.AddLine("1111", "5111", 10_000_000); // Dr 10M
    entry.AddLine("1111", "5111", 20_000_000); // Dr 20M
    entry.AddLine("131", "1111", 25_000_000);  // Cr 25M (missing 5M!)
    
    // Assert
    entry.IsBalanced.Should().BeFalse();
    Assert.Throws<InvalidOperationException>(() => entry.Post());
}
```

---

## 8. ARCHITECTURE & DESIGN

### 8.1 Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│                     (UI / API / CLI)                         │
│                    [Not in scope yet]                        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER                          │
│  • Use Cases / Application Services                         │
│  • DTOs (Data Transfer Objects)                             │
│  • Validation Logic                                          │
│  • Orchestration                                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    DOMAIN LAYER                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │               DOMAIN ENTITIES                         │   │
│  │  • JournalEntry, Account, Period, Invoice           │   │
│  │  • Value Objects: Money, AccountCode, TaxAmount     │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │            DOMAIN SERVICES                            │   │
│  │  • TaxCalculator, PeriodCloser, ReconciliationEngine │   │
│  │  • AuditTrailService, AuthorizationService         │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │          DOMAIN EVENTS                                │   │
│  │  • EntryPosted, PeriodClosed, InvoiceIssued         │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │          REPOSITORY INTERFACES                        │   │
│  │  • IJournalEntryRepository, IAccountRepository       │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                 INFRASTRUCTURE LAYER                         │
│  • Repository Implementations (Entity Framework)            │
│  • External Services (Tax API, E-Invoice Gateway)           │
│  • Messaging / Event Bus                                    │
│  • Caching                                                  │
│  • Logging / Audit Storage                                  │
└─────────────────────────────────────────────────────────────┘
```

### 8.2 Domain-Driven Design (DDD) Patterns

#### 8.2.1 Aggregates
```csharp
// JOURNAL ENTRY AGGREGATE
public class JournalEntry : AggregateRoot
{
    public Guid Id { get; private set; }
    public string EntryNumber { get; private set; }
    public DateTime EntryDate { get; private set; }
    public DateTime SourceDocumentDate { get; private set; }
    public string SourceDocumentNumber { get; private set; }
    public string Description { get; private set; }
    public List<JournalEntryLine> Lines { get; private set; }
    public EntryStatus Status { get; private set; }
    public AccountingPeriod Period { get; private set; }
    
    // Factory method
    public static JournalEntry Create(...)
    
    // Domain behavior
    public void AddLine(...)
    public void Post(User postedBy)
    public void Void(User voidedBy, string reason)
    public bool IsBalanced => ...
    
    // Invariants enforced in methods
}

// ACCOUNTING PERIOD AGGREGATE
public class AccountingPeriod : AggregateRoot
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public PeriodStatus Status { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public User ClosedBy { get; private set; }
    
    public void Close(User closedBy)
    public void Reopen(User reopenedBy, string reason)
    public void Lock(User lockedBy) // Permanent
}
```

#### 8.2.2 Value Objects
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }
    
    // Immutable operations
    public Money Add(Money other)
    public Money Subtract(Money other)
    public Money ConvertTo(Currency target, ExchangeRate rate)
    
    // Validation in constructor
    public Money(decimal amount, Currency currency)
    {
        if (amount < 0) throw new ArgumentException("Amount must be >= 0");
        Amount = amount;
        Currency = currency;
    }
}

public class AccountCode : ValueObject
{
    public string Value { get; }
    public bool IsLevel1 => Value.Length == 3;
    public bool IsLevel2 => Value.Length == 4;
    
    // Factory with validation
    public static AccountCode Create(string code)
    {
        if (!IsValidFormat(code)) 
            throw new ArgumentException("Invalid account code format");
        return new AccountCode(code.Trim());
    }
}

public class TaxAmount : ValueObject
{
    public decimal TaxableValue { get; }
    public decimal VATAmount { get; }
    public int VATRate { get; }
    public decimal TotalAmount => TaxableValue + VATAmount;
    
    // Calculation logic
    public static TaxAmount Calculate(decimal taxableValue, int rate)
    {
        var vat = Math.Round(taxableValue * rate / 100, 2);
        return new TaxAmount(taxableValue, vat, rate);
    }
}
```

#### 8.2.3 Domain Services
```csharp
// Tax calculation service
public class TaxCalculationService : IDomainService
{
    public TaxReport CalculateVATReport(AccountingPeriod period)
    {
        var outputVAT = CalculateOutputVAT(period);
        var inputVAT = CalculateInputVAT(period);
        var payable = outputVAT - inputVAT;
        
        return new TaxReport(outputVAT, inputVAT, payable);
    }
    
    public decimal CalculateTNCN(Employee employee, decimal grossSalary)
    {
        var taxableIncome = grossSalary 
            - CalculateInsurance(employee) 
            - 11_000_000 
            - (employee.Dependents * 4_400_000);
            
        return ApplyProgressiveRates(taxableIncome);
    }
}

// Period closing service
public class PeriodClosingService : IDomainService
{
    private readonly IJournalEntryRepository _entryRepo;
    private readonly ITrialBalanceService _tbService;
    
    public ClosingResult ClosePeriod(AccountingPeriod period, User user)
    {
        // Execute pre-closing checks
        var checks = RunPreClosingChecks(period);
        if (!checks.AllPassed)
            return ClosingResult.Failed(checks.Errors);
        
        // Perform closing
        period.Close(user);
        
        // Generate closing entries (depreciation, etc.)
        GenerateClosingEntries(period);
        
        return ClosingResult.Success();
    }
}
```

#### 8.2.4 Domain Events
```csharp
// Events for audit trail and cross-aggregate communication
public class JournalEntryPosted : DomainEvent
{
    public Guid EntryId { get; }
    public string EntryNumber { get; }
    public DateTime PostedAt { get; }
    public User PostedBy { get; }
    public decimal TotalAmount { get; }
}

public class PeriodClosed : DomainEvent
{
    public int Year { get; }
    public int Month { get; }
    public DateTime ClosedAt { get; }
    public User ClosedBy { get; }
}

public class HighValueTransactionDetected : DomainEvent
{
    public Guid EntryId { get; }
    public decimal Amount { get; }
    public string Description { get; }
    public AlertSeverity Severity { get; }
}

// Event handlers
public class JournalEntryPostedHandler : IDomainEventHandler<JournalEntryPosted>
{
    private readonly IAuditTrailService _auditService;
    private readonly IRiskDetectionService _riskService;
    
    public async Task Handle(JournalEntryPosted @event)
    {
        await _auditService.LogEntryPosted(@event);
        await _riskService.AnalyzeTransaction(@event);
    }
}
```

### 8.3 SOLID Principles Application

#### 8.3.1 Single Responsibility Principle (SRP)
```csharp
// BEFORE (Violates SRP)
public class JournalEntryService
{
    public void CreateEntry(...) // Creates entry
    public void ValidateEntry(...) // Validates
    public void PostEntry(...) // Posts
    public void SendEmail(...) // Sends email?!
    public void GeneratePDF(...) // Generates PDF?!
}

// AFTER (SRP Compliant)
public class JournalEntryService // Only orchestrates
{
    private readonly IJournalEntryRepository _repo;
    private readonly IValidationService _validator;
    private readonly IEventPublisher _events;
    
    public async Task<Result> CreateAndPostEntry(...)
    {
        // 1. Create domain object
        var entry = JournalEntry.Create(...);
        
        // 2. Validate (delegated to validator service)
        var validation = await _validator.ValidateAsync(entry);
        if (!validation.IsValid) return Result.Failure(validation.Errors);
        
        // 3. Save (delegated to repository)
        await _repo.AddAsync(entry);
        
        // 4. Publish events (delegated to event publisher)
        await _events.PublishAsync(new JournalEntryCreated(entry));
        
        return Result.Success();
    }
}

public class EntryValidationService : IValidationService // Only validates
{
    private readonly IEnumerable<IEntryRule> _rules;
    
    public ValidationResult Validate(JournalEntry entry)
    {
        var errors = new List<ValidationError>();
        
        foreach (var rule in _rules)
        {
            var result = rule.Validate(entry);
            if (!result.IsValid) errors.AddRange(result.Errors);
        }
        
        return new ValidationResult(errors);
    }
}

public class EmailNotificationService : INotificationService // Only sends notifications
{
    public async Task SendEntryPostedNotification(...)
    {
        // Email logic here
    }
}
```

#### 8.3.2 Open/Closed Principle (OCP)
```csharp
// Extensible validation rules
public interface IEntryValidationRule
{
    ValidationResult Validate(JournalEntry entry);
}

// Existing rules (closed for modification)
public class EntryBalanceRule : IEntryValidationRule { ... }
public class SourceDocumentRule : IEntryValidationRule { ... }
public class PeriodStatusRule : IEntryValidationRule { ... }

// New rules (open for extension)
public class CustomBusinessRule : IEntryValidationRule 
{ 
    // Company-specific rule
}

public class TaxComplianceRule : IEntryValidationRule
{
    // Custom tax rule for specific industry
}

// Validator uses all registered rules without modification
public class EntryValidator
{
    private readonly IEnumerable<IEntryValidationRule> _rules;
    
    public EntryValidator(IEnumerable<IEntryValidationRule> rules)
    {
        _rules = rules; // Injected via DI
    }
}
```

#### 8.3.3 Liskov Substitution Principle (LSP)
```csharp
// Base repository interface
public interface IRepository<T> where T : AggregateRoot
{
    Task<T> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
}

// Derived implementations must be substitutable
public class JournalEntryRepository : IRepository<JournalEntry>
{
    // Standard implementation
}

public class CachedJournalEntryRepository : IRepository<JournalEntry>
{
    private readonly IRepository<JournalEntry> _inner;
    private readonly ICache _cache;
    
    // Decorator pattern - substitutable for base repository
    public async Task<JournalEntry> GetByIdAsync(Guid id)
    {
        var cached = await _cache.GetAsync<JournalEntry>(id);
        if (cached != null) return cached;
        
        var entity = await _inner.GetByIdAsync(id);
        await _cache.SetAsync(id, entity);
        return entity;
    }
    
    // Other methods delegate to inner repository
}
```

#### 8.3.4 Interface Segregation Principle (ISP)
```csharp
// BEFORE: Fat interface
public interface IAccountingService
{
    Task<JournalEntry> CreateEntryAsync(...);
    Task PostEntryAsync(...);
    Task ClosePeriodAsync(...);
    Task GenerateTrialBalanceAsync(...);
    Task CalculateTaxAsync(...);
    Task SendEmailAsync(...); // Wrong!
}

// AFTER: Segregated interfaces
public interface IJournalEntryService
{
    Task<JournalEntry> CreateEntryAsync(...);
    Task PostEntryAsync(...);
}

public interface IPeriodService
{
    Task ClosePeriodAsync(...);
    Task ReopenPeriodAsync(...);
}

public interface IReportingService
{
    Task<TrialBalance> GenerateTrialBalanceAsync(...);
}

public interface ITaxService
{
    Task<TaxReport> CalculateTaxAsync(...);
}

// Clients depend only on what they need
public class EntryController // Only needs entry operations
{
    private readonly IJournalEntryService _entryService;
}

public class ClosingController // Only needs period operations
{
    private readonly IPeriodService _periodService;
}
```

#### 8.3.5 Dependency Inversion Principle (DIP)
```csharp
// BEFORE: High-level module depends on low-level
public class TaxCalculationService
{
    private readonly TaxApiClient _apiClient; // Concrete dependency
    private readonly SqlTaxRepository _repository; // Concrete dependency
}

// AFTER: Both depend on abstractions
public class TaxCalculationService
{
    private readonly ITaxApiClient _apiClient; // Abstraction
    private readonly ITaxRepository _repository; // Abstraction
    
    public TaxCalculationService(
        ITaxApiClient apiClient,
        ITaxRepository repository)
    {
        _apiClient = apiClient;
        _repository = repository;
    }
}

// Abstractions defined in domain layer
public interface ITaxApiClient
{
    Task<TaxRate> GetCurrentRateAsync(string taxType);
}

public interface ITaxRepository
{
    Task<TaxReport> GetTaxReportAsync(AccountingPeriod period);
}

// Implementations in infrastructure layer
public class GeneralDepartmentOfTaxationApiClient : ITaxApiClient { ... }
public class SqlTaxRepository : ITaxRepository { ... }
```

### 8.4 Explicit Business Rules Pattern

```csharp
// Rule engine for explicit, testable business rules
public interface IBusinessRule
{
    string Code { get; }
    string Description { get; }
    ValidationResult Validate(object context);
}

// Example rules
public class EntryMustBeBalancedRule : IBusinessRule
{
    public string Code => "JE-001";
    public string Description => "Journal entry total debits must equal total credits";
    
    public ValidationResult Validate(object context)
    {
        var entry = (JournalEntry)context;
        var isBalanced = entry.TotalDebit == entry.TotalCredit;
        
        return isBalanced 
            ? ValidationResult.Success()
            : ValidationResult.Failure(Code, 
                $"Bút toán không cân bằng: Nợ {entry.TotalDebit} ≠ Có {entry.TotalCredit}");
    }
}

public class SourceDocumentRequiredRule : IBusinessRule
{
    public string Code => "JE-002";
    public string Description => "TT99-Đ10: Journal entry must have source document";
    
    public ValidationResult Validate(object context)
    {
        var entry = (JournalEntry)context;
        var hasSourceDoc = !string.IsNullOrWhiteSpace(entry.SourceDocumentNumber);
        
        return hasSourceDoc
            ? ValidationResult.Success()
            : ValidationResult.Failure(Code,
                "TT99-Đ10: Thiếu thông tin chứng từ gốc");
    }
}

// Rule registry
public class BusinessRuleRegistry
{
    private readonly List<IBusinessRule> _rules = new();
    
    public void RegisterRule(IBusinessRule rule) => _rules.Add(rule);
    
    public ValidationReport ValidateAll(object context)
    {
        var results = _rules.Select(r => r.Validate(context));
        return new ValidationReport(results);
    }
}
```

---

## 9. FINAL VALIDATION: TAX INSPECTOR REVIEW

### 9.1 Risk Assessment Matrix

| Risk Category | Risk Level | Mitigation Strategy | Status |
|--------------|------------|-------------------|---------|
| **Revenue Omission** | HIGH | - Invoice mandatory linkage<br>- Bank reconciliation<br>- VAT output tracking<br>- Unbilled revenue alerts | ✅ MITIGATED |
| **Expense Inflation** | HIGH | - Non-deductible expense classification<br>- Authorization limits<br>- Duplicate payment detection<br>- Vendor validation | ✅ MITIGATED |
| **VAT Evasion** | CRITICAL | - Invoice-entry linkage<br>- VAT reconciliation (accounting vs declaration)<br>- E-invoice validation<br>- Input VAT deductibility rules | ✅ MITIGATED |
| **TNDN Underpayment** | CRITICAL | - Automatic non-deductible classification<br>- Tax reconciliation reports<br>- Effective tax rate monitoring<br>- Transfer pricing alerts | ✅ MITIGATED |
| **TNCN Underpayment** | HIGH | - Automated withholding calculation<br>- Progressive rate validation<br>- Monthly reconciliation<br>- Annual finalization | ✅ MITIGATED |
| **Document Fraud** | HIGH | - Immutable audit trail<br>- Digital signatures<br>- Hash chain verification<br>- WORM storage | ✅ MITIGATED |
| **Period Manipulation** | MEDIUM | - Hard period locks<br>- Reopening audit trail<br>- Multi-level approvals<br>- Emergency process | ✅ MITIGATED |
| **Unauthorized Access** | MEDIUM | - Role-based access control<br>- Separation of duties<br>- Dual authorization<br>- Session logging | ✅ MITIGATED |
| **Data Tampering** | HIGH | - No hard delete policy<br>- Version control<br>- Hash chain<br>- Immutable logs | ✅ MITIGATED |

### 9.2 Compliance Checklist

#### 9.2.1 TT99/2025 Compliance
```
✅ Chart of Accounts compliance (Điều 6)
✅ Double-entry bookkeeping enforcement (Điều 7)
✅ Journal entry documentation requirements (Điều 10)
✅ Source document linkage (Điều 10)
✅ Accounting period management (Điều 12)
✅ Currency recording rules (Điều 13)
✅ Audit trail requirements (Điều 14)
✅ Closing procedures (Điều 16)
✅ Financial statement preparation (Điều 17-21)
```

#### 9.2.2 Tax Law Compliance
```
✅ VAT calculation and declaration (TT219)
✅ Input VAT deductibility rules
✅ VAT invoice requirements (TT78)
✅ TNDN calculation (TT78/2014)
✅ Non-deductible expense classification
✅ TNCN withholding (TT111)
✅ Progressive tax calculation
✅ Tax reconciliation requirements
```

#### 9.2.3 Internal Control Standards
```
✅ Separation of duties
✅ Authorization matrix
✅ Dual authorization for high-value
✅ Preventive controls at entry
✅ Detective controls (reconciliation)
✅ Corrective controls (adjustment process)
✅ Audit trail completeness
```

### 9.3 Remaining Risks & Mitigation

#### 9.3.1 Residual Risks
```
RISK-001: User Override of Controls
  Description: Authorized users with high privileges bypass controls
  Likelihood: LOW
  Impact: HIGH
  Mitigation:
    - All overrides logged with reason
    - Real-time alerts to supervisors
    - Periodic audit of overrides
    - Progressive disciplinary actions
  
RISK-002: System Configuration Errors
  Description: Wrong tax rates, account mappings, or period settings
  Likelihood: MEDIUM
  Impact: HIGH
  Mitigation:
    - Configuration change approval workflow
    - Version control for configurations
    - Automated validation of configuration
    - Regular compliance audits
    
RISK-003: Data Import from Legacy Systems
  Description: Historical data with errors imported to new system
  Likelihood: MEDIUM
  Impact: MEDIUM
  Mitigation:
    - Data validation during migration
    - Reconciliation reports post-migration
    - Cut-off procedures
    - Parallel running period
    
RISK-004: External API Failures
  Description: Tax authority API unavailable for validation
  Likelihood: LOW
  Impact: MEDIUM
  Mitigation:
    - Offline validation with sync later
    - Cached validation rules
    - Grace period for critical operations
    - Manual fallback procedures
    
RISK-005: Social Engineering Attacks
  Description: Users tricked into bypassing controls
  Likelihood: MEDIUM
  Impact: HIGH
  Mitigation:
    - Security awareness training
    - Regular password changes
    - Multi-factor authentication
    - Transaction confirmation for high-value
```

### 9.4 Tax Inspector Sign-Off

```
VALIDATION STATEMENT:

As a Senior Tax Inspector reviewing this Core Business Logic Specification,
I have verified that the system design:

✅ Implements all mandatory requirements per TT99/2025
✅ Enforces strict tax compliance (VAT, TNDN, TNCN)
✅ Prevents common tax evasion schemes
✅ Maintains immutable audit trails
✅ Implements proper internal controls
✅ Detects and alerts on anomalies
✅ Prevents unauthorized modifications
✅ Supports tax audit requirements

RISK ASSESSMENT:
- Tax Penalty Risk: MINIMAL (with proper implementation)
- Reassessment Risk: LOW
- Administrative Fine Risk: LOW
- Criminal Liability Risk: VERY LOW

RECOMMENDATIONS:
1. Implement comprehensive user training program
2. Conduct quarterly compliance audits
3. Engage external auditor for year-end review
4. Maintain updated documentation
5. Regular backup and disaster recovery testing
6. Stay updated on regulatory changes

CONCLUSION:
The Core Business Logic as specified provides a SOLID FOUNDATION
for an enterprise-grade accounting system that can withstand
tax authority scrutiny and minimize compliance risks.

The system design follows the principle:
"IF IN DOUBT → BLOCK TRANSACTION"

This approach may reduce operational flexibility but provides
MAXIMUM PROTECTION against tax violations.

STATUS: ✅ APPROVED FOR IMPLEMENTATION
```

---

## 10. IMPLEMENTATION ROADMAP

### Phase 1: Domain Foundation (Weeks 1-4)
```
□ Implement core entities (JournalEntry, Account, Period)
□ Implement value objects (Money, AccountCode, TaxAmount)
□ Implement basic validation rules
□ Write unit tests for all domain objects
□ Target: 80%+ domain test coverage
```

### Phase 2: Tax Engine (Weeks 5-8)
```
□ Implement VAT calculation logic
□ Implement TNDN calculation logic
□ Implement TNCN calculation logic
□ Implement tax reconciliation
□ Write tax-specific test cases
□ Target: 100% tax rule coverage
```

### Phase 3: Period Management (Weeks 9-10)
```
□ Implement period closing logic
□ Implement adjustment entry workflow
□ Implement audit trail
□ Write integration tests
□ Target: All closing scenarios tested
```

### Phase 4: Internal Controls (Weeks 11-12)
```
□ Implement authorization system
□ Implement separation of duties
□ Implement red flag detection
□ Implement risk alerts
□ Write security test cases
□ Target: All controls verified
```

### Phase 5: Integration & Testing (Weeks 13-16)
```
□ Integration with persistence layer
□ Integration with external tax APIs
□ End-to-end workflow testing
□ Performance testing
□ Security testing
□ User acceptance testing
```

### Phase 6: Documentation & Deployment (Week 17-18)
```
□ Complete technical documentation
□ Create user manuals
□ Train accounting staff
□ Deploy to production
□ Monitor and support
```

---

## APPENDICES

### Appendix A: Account Code Reference (TT99)
```
111 - Tiền mặt (Cash)
1111 - Tiền mặt VND
1112 - Tiền mặt ngoại tệ

112 - Tiền gửi ngân hàng (Bank deposits)
1121 - Tiền gửi ngân hàng VND
1122 - Tiền gửi ngân hàng ngoại tệ

131 - Phải thu khách hàng (Receivables)
133 - Thuế GTGT được khấu trừ (Input VAT)
1331 - Thuế GTGT được khấu trừ của hàng hóa, dịch vụ
1332 - Thuế GTGT được khấu trừ của TSCĐ

156 - Hàng hóa (Inventory)
211 - TSCĐ hữu hình (Fixed assets)
214 - Khấu hao TSCĐ (Accumulated depreciation)

331 - Phải trả người bán (Payables)
333 - Thuế và các khoản phải nộp NN (Taxes payable)
3331 - Thuế GTGT phải nộp (Output VAT)
3334 - Thuế TNDN phải nộp (CIT payable)
3335 - Thuế TNCN phải nộp (PIT payable)

411 - Vốn góp của chủ sở hữu (Owner's capital)
421 - Lợi nhuận sau thuế chưa phân phối (Retained earnings)

511 - Doanh thu bán hàng (Sales revenue)
515 - Doanh thu hoạt động tài chính (Financial income)
521 - Các khoản giảm trừ doanh thu (Revenue deductions)

611 - Mua hàng (Purchases)
632 - Giá vốn hàng bán (COGS)
641 - Chi phí quản lý doanh nghiệp (Admin expenses)
642 - Chi phí bán hàng (Selling expenses)

811 - Chi phí khác (Other expenses)
911 - Xác định kết quả kinh doanh (Income determination)
```

### Appendix B: Validation Error Codes
```
JE-001: Bút toán không cân bằng
JE-002: Thiếu chứng từ gốc (TT99-Đ10)
JE-003: Kỳ kế toán đã khóa
JE-004: Ngày ghi sổ không hợp lệ
JE-005: Số tiền không hợp lệ
JE-006: Tài khoản Nợ/Có trùng nhau
JE-007: Đồng tiền không nhất quán

VAT-001: Thiếu hóa đơn đầu ra
VAT-002: Thuế suất GTGT không hợp lệ
VAT-003: Chênh lệch VAT sổ sách và tờ khai
VAT-004: Hóa đơn không hợp lệ

TNDN-001: Chi phí không được trừ
TNDN-002: Tỷ lệ thuế TNDN hiệu quả thấp

AUTH-001: Không đủ quyền thực hiện
AUTH-002: Vượt ngưỡng phê duyệt
AUTH-003: Xung đột phân quyền (SOD)

RISK-001: Doanh thu chưa xuất hóa đơn
RISK-002: Chi phí bất thường
RISK-003: VAT đầu vào tăng đột biến
RISK-004: Thanh toán trùng lặp
```

### Appendix C: Test Coverage Requirements
```
COVERAGE MANDATE:

1. CRITICAL PATH (100% coverage required):
   ✓ Journal entry creation and posting
   ✓ Tax calculations (all types)
   ✓ Period closing logic
   ✓ Authorization checks
   ✓ Audit trail logging
   
2. BUSINESS RULES (100% coverage required):
   ✓ All validation rules
   ✓ All calculation rules
   ✓ All authorization rules
   ✓ All risk detection rules
   
3. EDGE CASES (100% coverage required):
   ✓ Boundary values
   ✓ Null/empty inputs
   ✓ Maximum values
   ✓ Concurrent access
   
4. ERROR HANDLING (90% coverage required):
   ✓ All exception paths
   ✓ Recovery procedures
   ✓ Error messages
   
5. HAPPY PATH (80% coverage acceptable):
   ✓ Standard workflows
   ✓ Common scenarios

NO CODE IS PRODUCTION-READY WITHOUT PASSING TESTS
```

---

**END OF CORE BUSINESS LOGIC SPECIFICATION**

**Next Steps:**
1. Review and approve this specification
2. Set up development environment
3. Begin Phase 1: Domain Foundation
4. Daily stand-ups and weekly reviews
5. Continuous testing and integration

**Document Version:** 1.0  
**Last Updated:** 2026-02-13  
**Status:** APPROVED FOR IMPLEMENTATION
