# Use Cases for AMS ERP

**Version:** 1.0  
**Last Updated:** March 2026

---

## Table of Contents

1. [Overview](#1-overview)
2. [Use Case Categories](#2-use-case-categories)
3. [Voucher Management Use Cases](#3-voucher-management-use-cases)
4. [Master Data Use Cases](#4-master-data-use-cases)
5. [Reporting Use Cases](#5-reporting-use-cases)
6. [System Administration Use Cases](#6-system-administration-use-cases)
7. [Use Case Prioritization](#7-use-case-prioritization)

---

## 1. Overview

This document defines the functional requirements for AMS ERP system using use case format. Each use case includes:
- **ID**: Unique identifier
- **Name**: Descriptive name
- **Actor**: Who performs the use case
- **Goal**: What the actor wants to achieve
- **Preconditions**: What must be true before starting
- **Flow**: Step-by-step interaction
- **Postconditions**: What must be true after completion

---

## 2. Use Case Categories

| Category | Description | Priority |
|----------|-------------|----------|
| Voucher Management | Create, edit, approve, post vouchers | Critical |
| Master Data | Products, accounts, customers, vendors | Critical |
| Reporting | Financial reports, exports | High |
| Inventory | Stock movements, balances | High |
| System Admin | Users, roles, settings | High |
| Audit | Trail, compliance | Medium |

---

## 3. Voucher Management Use Cases

### UC-V001: Create Voucher

| Field | Details |
|-------|---------|
| **ID** | UC-V001 |
| **Name** | Create Voucher |
| **Actor** | Clerk, Accountant |
| **Goal** | Input a new accounting voucher |
| **Preconditions** | User logged in, accounting period open |
| **Priority** | Critical |

**Main Flow:**
1. User clicks "Tạo mới" (Create)
2. System displays blank voucher form
3. User enters:
   - Voucher type (PN, PX, PC, NC)
   - Date (default: today)
   - Description
   - Multiple line items (account + debit/credit)
4. System validates:
   - Debit = Credit (must balance)
   - All accounts exist
   - Date within open period
5. User clicks "Lưu" (Save)
6. System saves with status "Nháp" (Draft)
7. System displays success message

**Alternative Flow:**
- **Duplicate**: User clicks "Sao chép" to duplicate previous voucher
- **Cancel**: User clicks "Hủy" to discard changes

**Postconditions:**
- Voucher saved with Draft status
- Created by/date logged

---

### UC-V002: Edit Voucher

| Field | Details |
|-------|---------|
| **ID** | UC-V002 |
| **Name** | Edit Voucher |
| **Actor** | Clerk, Accountant |
| **Goal** | Modify existing voucher |
| **Preconditions** | Voucher exists, status = Draft, user has permission |
| **Priority** | Critical |

**Main Flow:**
1. User searches/selects voucher
2. User clicks "Sửa" (Edit)
3. System displays voucher in edit mode
4. User modifies fields
5. System validates changes
6. User saves
7. System updates and logs changes

**Business Rules:**
- Cannot edit after approval
- Cannot edit in closed period
- Edit creates audit trail with before/after values

---

### UC-V003: Delete Voucher

| Field | Details |
|-------|---------|
| **ID** | UC-V003 |
| **Name** | Delete Voucher |
| **Actor** | Accountant, Manager |
| **Goal** | Remove invalid voucher |
| **Preconditions** | Voucher exists, status = Draft |
| **Priority** | High |

**Main Flow:**
1. User selects voucher
2. User clicks "Xóa"
3. System shows confirmation dialog
4. User confirms
5. System marks as Deleted (soft delete)
6. System logs who deleted and when

**Business Rules:**
- Cannot delete approved vouchers
- Deleted vouchers kept for audit (status = Deleted)

---

### UC-V004: Approve Voucher

| Field | Details |
|-------|---------|
| **ID** | UC-V004 |
| **Name** | Approve Voucher |
| **Actor** | Manager |
| **Goal** | Authorize voucher for posting |
| **Preconditions** | Voucher status = Draft, user has approval permission |
| **Priority** | Critical |

**Main Flow:**
1. User views pending vouchers
2. User reviews voucher details
3. User clicks "Phê duyệt" (Approve)
4. System validates user permission
5. System updates status to "Đã duyệt"
6. System logs approval (user, timestamp)
7. System notifies creator

**Alternative Flow:**
- **Reject**: User clicks "Từ chối", enters reason → status = Từ chối, notify creator

**Postconditions:**
- Status = Đã duyệt
- Approved by + timestamp recorded

---

### UC-V005: Post Voucher

| Field | Details |
|-------|---------|
| **ID** | UC-V005 |
| **Name** | Post Voucher |
| **Actor** | System (automatic) |
| **Goal** | Move voucher to general ledger |
| **Preconditions** | Voucher status = Đã duyệt |
| **Priority** | Critical |

**Main Flow:**
1. User clicks "Ghi sổ" (Post) or system auto-posts
2. System validates:
   - Status = Đã duyệt
   - Period open
3. System creates GL entries
4. System updates voucher status = "Đã ghi"
5. System logs posting

---

## 4. Master Data Use Cases

### UC-MD001: Create Account

| Field | Details |
|-------|---------|
| **ID** | UC-MD001 |
| **Name** | Create Account (Chart of Accounts) |
| **Actor** | Accountant, Admin |
| **Goal** | Add new general ledger account |
| **Preconditions** | User has permission |
| **Priority** | Critical |

**Main Flow:**
1. User navigates to Chart of Accounts
2. User clicks "Thêm mới"
3. User enters:
   - Account code (e.g., 1561)
   - Account name
   - Type (Asset, Liability, Equity, Revenue, Expense)
   - Parent account (optional)
   - Description
4. System validates code is unique
5. User saves
6. System creates account

**Business Rules:**
- Account code must be unique
- Must follow numbering convention
- Cannot delete if has transactions

---

### UC-MD002: Create Product

| Field | Details |
|-------|---------|
| **ID** | UC-MD002 |
| **Name** | Create Product |
| **Actor** | Clerk, Accountant |
| **Goal** | Add new inventory item |
| **Preconditions** | User has permission |
| **Priority** | Critical |

**Main Flow:**
1. User navigates to Products
2. User clicks "Thêm mới"
3. User enters:
   - Product code
   - Product name
   - Unit of measure
   - Category
   - Cost price, selling price
   - VAT rate
4. System validates code unique
5. User saves
6. System creates product

---

### UC-MD003: Manage Warehouse

| Field | Details |
|-------|---------|
| **ID** | UC-MD003 |
| **Name** | Manage Warehouse |
| **Actor** | Admin |
| **Goal** | Create/edit warehouse locations |
| **Preconditions** | User has admin permission |
| **Priority** | High |

---

## 5. Reporting Use Cases

### UC-R001: View Trial Balance

| Field | Details |
|-------|---------|
| **ID** | UC-R001 |
| **Name** | View Trial Balance |
| **Actor** | Accountant, Manager, Auditor |
| **Goal** | View account balances at period end |
| **Preconditions** | User has permission |
| **Priority** | Critical |

**Main Flow:**
1. User selects Reports > Trial Balance
2. User selects period (month/quarter/year)
3. System calculates:
   - Sum of all debits
   - Sum of all credits
   - Per account: debit - credit balance
4. System displays report:
   - Account code, name
   - Debit balance, Credit balance
   - Total row: must balance
5. User can drill down to transactions

---

### UC-R002: View General Ledger

| Field | Details |
|-------|---------|
| **ID** | UC-R002 |
| **Name** | View General Ledger |
| **Actor** | Accountant, Manager, Auditor |
| **Goal** | View detailed transactions per account |
| **Preconditions** | User has permission |
| **Priority** | Critical |

**Main Flow:**
1. User selects account
2. User selects date range
3. System shows all transactions:
   - Date, voucher number
   - Description
   - Debit amount, Credit amount
   - Running balance
4. User can click to view voucher details

---

### UC-R003: Export Report

| Field | Details |
|-------|---------|
| **ID** | UC-R003 |
| **Name** | Export Report |
| **Actor** | Accountant, Manager, Auditor |
| **Goal** | Export data to Excel/PDF |
| **Preconditions** | Report generated |
| **Priority** | High |

**Main Flow:**
1. User views report
2. User clicks Export dropdown
3. User selects format (Excel, PDF)
4. System generates file
5. System prompts download

---

## 6. System Administration Use Cases

### UC-SA001: Create User

| Field | Details |
|-------|---------|
| **ID** | UC-SA001 |
| **Name** | Create User |
| **Actor** | Admin |
| **Goal** | Add new system user |
| **Preconditions** | Admin logged in |
| **Priority** | High |

**Main Flow:**
1. Admin navigates to Users
2. Admin clicks "Thêm người dùng"
3. Admin enters:
   - Username (from AD or manual)
   - Display name
   - Email
   - Department
   - Role(s)
4. System validates username unique
5. Admin saves
6. System creates user + logs action

---

### UC-SA002: Assign Role

| Field | Details |
|-------|---------|
| **ID** | UC-SA002 |
| **Name** | Assign Role to User |
| **Actor** | Admin |
| **Goal** | Grant system permissions |
| **Preconditions** | User exists, role exists |
| **Priority** | High |

**Main Flow:**
1. Admin selects user
2. Admin clicks "Phân quyền"
3. Admin selects role(s) from list
4. Admin saves
5. System updates permissions + logs

---

### UC-SA003: View Audit Log

| Field | Details |
|-------|---------|
| **ID** | UC-SA003 |
| **Name** | View Audit Log |
| **Actor** | Admin, Auditor |
| **Goal** | Review system activity |
| **Preconditions** | User has permission |
| **Priority** | High |

**Main Flow:**
1. User navigates to Audit Log
2. User filters by:
   - Date range
   - User
   - Action type
   - Object type
3. System displays log entries:
   - Timestamp
   - User
   - Action
   - Object
   - IP Address
4. User can drill down to see details

---

### UC-SA004: Close Accounting Period

| Field | Details |
|-------|---------|
| **ID** | UC-SA004 |
| **Name** | Close Accounting Period |
| **Actor** | Accountant, Admin |
| **Goal** | Lock period to prevent changes |
| **Preconditions** | Period is open, all vouchers posted |
| **Priority** | High |

**Main Flow:**
1. User navigates to Settings > Periods
2. User selects period to close
3. System validates:
   - No pending vouchers
   - Trial balance balanced
4. User confirms
5. System:
   - Changes status to Closed
   - Prevents new vouchers in period
   - Logs action

---

## 7. Use Case Prioritization

### Phase 1 - MVP (Critical)

| Use Case | Actor | Priority |
|----------|-------|----------|
| UC-V001 | Clerk | P0 |
| UC-V004 | Manager | P0 |
| UC-MD001 | Accountant | P0 |
| UC-MD002 | Clerk | P0 |
| UC-R001 | Accountant | P0 |
| UC-SA001 | Admin | P1 |

### Phase 2 - Essential

| Use Case | Actor | Priority |
|----------|-------|----------|
| UC-V002 | Clerk | P1 |
| UC-V003 | Accountant | P1 |
| UC-V005 | System | P1 |
| UC-R002 | Accountant | P1 |
| UC-R003 | Accountant | P1 |
| UC-SA002 | Admin | P1 |

### Phase 3 - Enhanced

| Use Case | Actor | Priority |
|----------|-------|----------|
| UC-V006 | Manager | P2 |
| UC-MD003 | Admin | P2 |
| UC-SA003 | Auditor | P2 |
| UC-SA004 | Accountant | P2 |

---

## Appendix A: Use Case Template

```
### UC-XXX: [Use Case Name]

| Field | Details |
|-------|---------|
| **ID** | UC-XXX |
| **Name** | [Name] |
| **Actor** | [Who] |
| **Goal** | [What they want] |
| **Preconditions** | [What must be true] |
| **Priority** | [Critical/High/Medium] |

**Main Flow:**
1. [Step]
2. [Step]
3. [Step]

**Alternative Flow:**
- [Variation]

**Business Rules:**
- [Rule 1]
- [Rule 2]

**Postconditions:**
- [Result]
```

---

## Appendix B: Priority Definitions

| Priority | Description |
|----------|-------------|
| P0 | Must have for MVP - system unusable without |
| P1 | Should have for MVP - major impact |
| P2 | Nice to have - enhances experience |
| P3 | Future - not in current scope |

---

*Document Owner: Product Team*  
*Status: Draft - Requires Validation*