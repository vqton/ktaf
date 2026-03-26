# User Journey Maps for AMS ERP

**Version:** 1.0  
**Last Updated:** March 2026

---

## Table of Contents

1. [Overview](#1-overview)
2. [Voucher Creation Journey](#2-voucher-creation-journey)
3. [Voucher Approval Journey](#3-voucher-approval-journey)
4. [Month-End Close Journey](#4-month-end-close-journey)
5. [Audit Trail Journey](#5-audit-trail-journey)
6. [User Management Journey](#6-user-management-journey)

---

## 1. Overview

This document maps the user journeys for key workflows in AMS ERP. Each journey shows the user's perspective, actions, touchpoints, and system responses.

**Journeys Covered:**
1. Voucher Creation (Clerk)
2. Voucher Approval (Manager)
3. Month-End Close (Accountant)
4. Audit Trail Review (Auditor)
5. User Management (Admin)

---

## 2. Voucher Creation Journey

**User:** Data Entry Clerk (Kế toán viên)  
**Frequency:** 50-100 times/day  
**Goal:** Create voucher quickly without errors

### Journey Map

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                         VOUCHER CREATION JOURNEY                                    │
│                         User: Nguyễn Thị Mai (Clerk)                              │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  START: Morning - Opens AMS                                                        │
│                                                                                     │
│  ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     │
│  │ 1. Login │────▶│ 2. Click │────▶│ 3. Enter│────▶│ 4. Add  │────▶│ 5. Check│     │
│  │  Windows│     │ Create   │     │ Header  │     │ Lines   │     │ Balance │     │
│  │  Auth   │     │ Button   │     │ Info    │     │         │     │         │     │
│  └─────────┘     └─────────┘     └─────────┘     └─────────┘     └─────────┘     │
│      │               │               │               │               │          │
│      ▼               ▼               ▼               ▼               ▼          │
│  Auto-login      Opens form      Auto-fill       Running         Debit=Credit│
│  with AD        blank form      today's date     total shows     ✓ or ✗      │
│                                                            │                    │
│  ┌───────────────────────────────────────────────────────────┘                    │
│  │                                                                                 │
│  ▼                                                                            ▼   │
│ ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐   │
│ │ 6. Save │◀────│ 7. Show │◀────│ 8. View │◀────│ 9. Print│◀────│ 10. Next│   │
│ │ & New   │     │ Success │     │ Details │     │ Voucher │     │ Voucher │   │
│ └─────────┘     └─────────┘     └─────────┘     └─────────┘     └─────────┘   │
│      │               │               │               │               │          │
│      ▼               ▼               ▼               ▼               ▼          │
│  Ctrl+S         Toast "Saved"    Opens in        PDF opens      Clear form   │
│  shortcut       + vibration       read-only       for customer   + ready for │
│  works                           mode             to sign       next entry   │
│                                                                                     │
│  END: Continues until all vouchers entered                                         │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### Touchpoints Detail

| Step | User Action | System Response | Emotion | Pain Point |
|------|-------------|----------------|---------|------------|
| 1 | Login | Auto-login via Windows Auth | Neutral | None |
| 2 | Click Create | Opens blank form | Focused | None |
| 3 | Enter header | Auto-complete suggestions | Flow | Account code lookup |
| 4 | Add lines | Running total updates | Flow | Too many clicks |
| 5 | Check balance | Validation message | Anxiety | Manual check needed |
| 6 | Save | Debit=Credit validation | Relief | None |
| 7 | Success | Toast notification | Happy | None |
| 8 | View | Read-only page | Confirmation | None |
| 9 | Print | PDF generation | Satisfaction | None |
| 10 | Next | Clear form | Ready | None |

### Opportunities

- **Step 3**: Add account code search with fuzzy matching
- **Step 4**: Add "Duplicate from previous" button
- **Step 5**: Show real-time balance indicator (green/red)
- **Step 10**: Add Ctrl+Shift+N for save & new

---

## 3. Voucher Approval Journey

**User:** Finance Manager (Quản lý tài chính)  
**Frequency:** 3-5 times/day  
**Goal:** Review and approve vouchers quickly

### Journey Map

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                       VOUCHER APPROVAL JOURNEY                                      │
│                       User: Phạm Thị Hương (Manager)                               │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  START: Morning - Opens Dashboard                                                  │
│                                                                                     │
│  ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     │
│  │ 1. View  │────▶│ 2. Check │────▶│ 3. Click│────▶│ 4. See  │────▶│ 5. Confirm│    │
│  │ Dashboard│     │ Pending  │     │ Approve │     │ Success │     │  + Next   │    │
│  └─────────┘     └─────────┘     └─────────┘     └─────────┘     └─────────┘     │
│      │               │               │               │               │          │
│      ▼               ▼               ▼               ▼               ▼          │
│  KPI cards      Filter: Today   Single click     Toast +        Auto-advance│
│  show:          + Unapproved    from list or     auto-refresh   to next    │
│  Revenue,       view            detail page      list            pending    │
│  Pending CT                                                           │         │
│                                              ┌──────────────────────┘         │
│                                              │                                 │
│                                              ▼                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │ ALTERNATIVE PATH: REJECT                                                │   │
│  │ 3b. Click Reject → Enter reason → Confirm → Auto-return to creator    │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                                                                     │
│  END: All pending vouchers reviewed                                                │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### Touchpoints Detail

| Step | User Action | System Response | Emotion | Pain Point |
|------|-------------|----------------|---------|------------|
| 1 | View Dashboard | KPIs load, charts render | Informed | None |
| 2 | Check Pending | Filter + count badge | Focused | Too many to review |
| 3 | Approve | Confirmation + audit log | Empowered | None |
| 4 | See Success | Toast, badge updates | Satisfied | None |
| 5 | Confirm | Auto-advance to next | Efficient | None |

### Alternative Flow: Rejection

```
User clicks "Từ chối" → Modal opens → User selects reason (dropdown) 
→ User adds note (optional) → Clicks "Xác nhận" → 
System: Marks as Rejected + Notifies creator + Logs action
```

### Opportunities

- **Step 2**: Add "Approve All" for bulk action
- **Step 2**: Add quick preview tooltip on hover
- **Step 3**: Add keyboard shortcut (A=Approve, R=Reject)
- **Mobile**: Add mobile approval for on-the-go

---

## 4. Month-End Close Journey

**User:** Accountant (Kế toán tổng hợp)  
**Frequency:** Monthly (1-3 days)  
**Goal:** Close month accurately and on time

### Journey Map

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                        MONTH-END CLOSE JOURNEY                                      │
│                        User: Trần Văn Hùng (Accountant)                             │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  START: Day 1 of close period                                                      │
│                                                                                     │
│  ┌──────────────────────────────────────────────────────────────────────────────┐  │
│  │ PHASE 1: PREPARATION (Day 1)                                                │  │
│  ├──────────────────────────────────────────────────────────────────────────────┤  │
│  │ 1.1. Review unreconciled transactions                                       │  │
│  │     → Bank Reconciliation screen                                            │  │
│  │     → Match or flag differences                                             │  │
│  │                                                                              │  │
│  │ 1.2. Check pending vouchers                                                 │  │
│  │     → Filter: Status = Pending + Date < Close Date                         │  │
│  │     → Follow up with creators                                               │  │
│  └──────────────────────────────────────────────────────────────────────────────┘  │
│                                       │                                            │
│                                       ▼                                            │
│  ┌──────────────────────────────────────────────────────────────────────────────┐  │
│  │ PHASE 2: RECONCILIATION (Day 1-2)                                          │  │
│  ├──────────────────────────────────────────────────────────────────────────────┤  │
│  │ 2.1. Run Trial Balance                                                      │  │
│  │     → Report > Trial Balance                                               │  │
│  │     → Check: Debit = Credit                                                 │  │
│  │                                                                              │  │
│  │ 2.2. Drill into不平衡 (if any)                                            │  │
│  │     → Click account → See transactions                                      │  │
│  │     → Identify errors                                                      │  │
│  └──────────────────────────────────────────────────────────────────────────────┘  │
│                                       │                                            │
│                                       ▼                                            │
│  ┌──────────────────────────────────────────────────────────────────────────────┐  │
│  │ PHASE 3: CLOSE (Day 2-3)                                                   │  │
│  ├──────────────────────────────────────────────────────────────────────────────┤  │
│  │ 3.1. Close accounting period                                                │  │
│  │     → Settings > Periods > Close Month                                     │  │
│  │     → Confirmation dialog                                                   │  │
│  │     → System: Locks period + Creates closing entries                       │  │
│  │                                                                              │  │
│  │ 3.2. Generate final reports                                                │  │
│  │     → Balance Sheet, P&L, Cash Flow                                        │  │
│  │     → Export to Excel/PDF                                                  │  │
│  └──────────────────────────────────────────────────────────────────────────────┘  │
│                                       │                                            │
│                                       ▼                                            │
│  END: Month successfully closed                                                    │
│  - Reports exported and saved                                                     │
│  - Period locked for editing                                                      │
│  - Ready for next month                                                           │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### Checkpoints

| Phase | Checkpoint | Success Criteria |
|-------|------------|------------------|
| 1 | All vouchers posted | Pending count = 0 |
| 2 | Trial Balance | Debit balance = Credit balance |
| 3 | Period Closed | Status = Closed |

### Pain Points & Solutions

| Pain Point | Solution |
|------------|----------|
| Can't find unreconciled items | Highlight differences in red |
| Don't know which vouchers pending | Badge + filter by date |
| Manual P&L calculation | Auto-generate closing entries |
| Export reports take time | Background processing + notify |

---

## 5. Audit Trail Journey

**User:** Auditor (Kiểm toán viên)  
**Frequency:** Quarterly or on-demand  
**Goal:** Trace transactions from source to statements

### Journey Map

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                         AUDIT TRAIL JOURNEY                                        │
│                         User: Lê Minh Quang (Auditor)                             │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  START: Audit period begins                                                        │
│                                                                                     │
│  ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     │
│  │ 1. Start│────▶│ 2. Find │────▶│ 3. View │────▶│ 4. Trace│────▶│ 5. Export│    │
│  │ Period  │     │ Voucher │     │ Details │     │ to GL   │     │ Evidence │    │
│  └─────────┘     └─────────┘     └─────────┘     └─────────┘     └─────────┘     │
│      │               │               │               │               │          │
│      ▼               ▼               ▼               ▼               ▼          │
│  Select date   Search by      Full voucher     Link to        Download all│
│  range + type  number, date,   with:             General        supporting   │
│                or keyword       - Header        Ledger         documents    │
│                               - Line items      entry                         │
│                               - Attachments                                   │
│                               - Approval                                        │
│                               - User who                                       │
│                                 created/                                        │
│                                 modified                                        │
│                                                                                     │
│  ┌──────────────────────────────────────────────────────────────────────────────┐  │
│  │ ALTERNATIVE: Backward Trace                                                │  │
│  │ Start from GL → Drill down to voucher → View source document              │  │
│  └──────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                     │
│  ┌──────────────────────────────────────────────────────────────────────────────┐  │
│  │ ALTERNATIVE: User Activity                                                  │  │
│  │ View all actions by user → Filter by date → See what they did             │  │
│  └──────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                     │
│  END: Audit complete - all items verified                                        │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### Audit Trail Requirements

| Requirement | Description |
|------------|------------|
| **Who** | User ID, name, role |
| **What** | Action (Create, Update, Delete, Approve, etc.) |
| **When** | Timestamp (精确到秒) |
| **From Where** | IP address |
| **Before/After** | Old value vs new value |

### Data to Capture

- Login/Logout events
- Voucher create/modify/delete
- Approval actions
- Report generation
- Export actions
- Settings changes
- Period open/close

---

## 6. User Management Journey

**User:** System Administrator (Quản trị viên)  
**Frequency:** On-demand (rare)  
**Goal:** Manage users securely and efficiently

### Journey Map

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                       USER MANAGEMENT JOURNEY                                       │
│                       User: Ngô Đình Nam (Admin)                                   │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  START: New employee joins or access request                                       │
│                                                                                     │
│  ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     │
│  │ 1. Create│────▶│ 2. Assign│────▶│ 3. Set   │────▶│ 4. Notify│────▶│ 5. Log   │     │
│  │ User     │     │ Role     │     │ Permissions│    │ User     │     │ Action   │     │
│  └─────────┘     └─────────┘     └─────────┘     └─────────┘     └─────────┘     │
│      │               │               │               │               │          │
│      ▼               ▼               ▼               ▼               ▼          │
│  Enter name,    Select from    Fine-tune       System sends     Audit trail│
│  email, dept   predefined     if needed       welcome email    records all│
│  + AD sync     roles:         (rare)          + temp password  actions    │
│                Clerk, Acct,                                       │
│                Manager, etc.                                        │
│                                                                                     │
│  ┌──────────────────────────────────────────────────────────────────────────────┐  │
│  │ OTHER SCENARIOS                                                              │  │
│  │ - Reset password (admin or self-service)                                   │  │
│  │ - Disable user (leaver) → Keep history, revoke access                      │  │
│  │ - Modify permissions → Add/remove features                                  │  │
│  │ - View activity log → See user's actions for security review              │  │
│  └──────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                     │
│  END: User has appropriate access                                                   │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### Role Definitions

| Role | Permissions |
|------|-------------|
| Clerk | Create voucher, View own vouchers |
| Accountant | Full voucher, Ledger, Reports |
| Manager | Approve, Reports, Dashboard |
| Auditor | Read-only all, Export |
| Admin | Full system access |

---

## Appendix: Journey Summary Table

| Journey | User | Frequency | Duration | Key Metric |
|---------|------|-----------|----------|------------|
| Voucher Creation | Clerk | 50-100/day | 1-2 min each | Vouchers/hour |
| Voucher Approval | Manager | 20-50/day | 30 sec each | Approval time |
| Month-End Close | Accountant | 1/month | 2-3 days | Close time |
| Audit Trail | Auditor | On-demand | Hours | Trace time |
| User Management | Admin | Rare | 10-30 min | Setup time |

---

*Document Owner: Product Team*  
*Status: Draft - Requires Validation*