# UI/UX Design Guidelines for AMS ERP Web Application

**Version:** 2.0  
**Status:** Foundation Document  
**Last Updated:** March 2026

---

## Table of Contents

1. [Business Context](#1-business-context)
2. [Design Principles](#2-design-principles)
3. [Design System](#3-design-system)
4. [User Interface Structure](#4-user-interface-structure)
5. [Core User Workflows](#5-core-user-workflows)
6. [Component Library](#6-component-library)
7. [Error Handling](#7-error-handling)
8. [Localization](#8-localization)
9. [Security UX](#9-security-ux)
10. [Accessibility Requirements](#10-accessibility-requirements)
11. [Responsive Strategy](#11-responsive-strategy)
12. [Performance Standards](#12-performance-standards)
13. [Testing Requirements](#13-testing-requirements)
14. [Implementation Priorities](#14-implementation-priorities)

---

## 1. Business Context

### 1.1 Product Overview

AMS (Accounting Management System) is an enterprise ERP web application for Vietnamese small-to-medium businesses. Primary functions include voucher management, general ledger, inventory tracking, and financial reporting.

### 1.2 Target Users

| User Type | Role | Primary Tasks | Tech Proficiency |
|-----------|------|---------------|------------------|
| **Data Entry Clerk** | Daily transaction entry | Create vouchers, input inventory | Medium |
| **Accountant** | Recording & reconciling | Ledger entries, bank reconciliation | High |
| **Finance Manager** | Review & approve | Approve vouchers, generate reports | Medium |
| **Auditor** | Periodic review | Query data, export reports | High |
| **System Admin** | Configuration | User management, settings | High |

### 1.3 Key User Needs

- **Speed**: Data entry users type all day - every click counts
- **Accuracy**: Financial data must be precise - prevent errors at input
- **Clarity**: Users must always know where they are and what they can do
- **Trust**: Interface must inspire confidence in data integrity
- **Efficiency**: Power users need keyboard shortcuts and bulk operations

### 1.4 Business Constraints

- Single-page application feel with Razor Pages (no full SPA)
- Target browsers: Chrome, Edge (latest 2 versions)
- Support Windows 10/11 - common in Vietnamese enterprises
- Must work on 1366x768 displays (still common in SMB)
- Vietnamese language primary - may add English later

---

## 2. Design Principles

### 2.1 Primary Principles (Ranked)

1. **Clarity Over Density**
   - More whitespace, clearer hierarchy
   - One primary action per screen
   - Secondary actions visible but not competing

2. **Progressive Disclosure**
   - Show essential data by default
   - Expand sections for details
   - Don't overwhelm with all fields at once

3. **Consistent Patterns**
   - Same patterns solve same problems
   - Position, colors, and behaviors are predictable
   - Learning one part helps with others

4. **Immediate Feedback**
   - Every action has visual confirmation
   - Save states are clear
   - Errors are shown immediately at point of entry

5. **Keyboard Efficiency**
   - Full keyboard navigation support
   - Tab order follows visual flow
   - Common actions have shortcuts

### 2.2 Anti-Patterns to Avoid

- ❌ Hidden actions (no guessing where to click)
- ❌ Multi-step forms where one step suffices
- ❌ Page reloads for simple actions (use AJAX)
- ❌ Modal windows for critical workflows
- ❌ Dense tables that require scrolling horizontally
- ❌ Generic error messages
- ❌ Without clear error recovery paths

---

## 3. Design System

### 3.1 Color Palette

#### Primary Colors
```
--color-primary:          #1E3A5F    /* Deep Navy - authority, trust */
--color-primary-light:   #2D5A8A    /* Lighter navy for hover */
--color-primary-dark:   #0F1E33    /* Darker for emphasis */
```

#### Secondary Colors
```
--color-secondary:       #4A90A4    /* Teal - approachable */
--color-accent:          #E8A838    /* Gold - Vietnamese aesthetic, attention */
```

#### Semantic Colors
```
--color-success:         #28A745    /* Green - confirmations */
--color-success-light:   #D4EDDA    /* Success background */

--color-warning:         #FFC107    /* Amber - attention needed */
--color-warning-light:   #FFF3CD    /* Warning background */

--color-danger:         #DC3545    /* Red - errors, deletions */
--color-danger-light:   #F8D7DA    /* Error background */

--color-info:           #17A2B8    /* Cyan - information */
--color-info-light:     #D1ECF1    /* Info background */
```

#### Neutrals
```
--color-gray-900:       #212529    /* Primary text */
--color-gray-700:       #495057    /* Secondary text */
--color-gray-500:       #6C757D    /* Placeholder, disabled */
--color-gray-300:       #DEE2E6    /* Borders */
--color-gray-100:       #F8F9FA    /* Backgrounds */
--color-white:          #FFFFFF    /* Cards, inputs */
```

#### Background
```
--color-body-bg:        #F5F7FA    /* Main background - slight blue tint */
--color-sidebar-bg:     #1E2836    /* Sidebar - dark */
--color-header-bg:      #FFFFFF    /* Header - white */
```

### 3.2 Typography

#### Font Stack
```css
--font-family-base: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
--font-family-mono: 'JetBrains Mono', 'Fira Code', monospace;
```

#### Type Scale
| Element | Size | Weight | Line Height |
|---------|------|--------|-------------|
| H1 - Page Title | 28px | 600 | 1.2 |
| H2 - Section | 22px | 600 | 1.3 |
| H3 - Card Title | 18px | 600 | 1.4 |
| Body | 14px | 400 | 1.5 |
| Body Small | 13px | 400 | 1.5 |
| Caption | 12px | 400 | 1.4 |
| Button | 14px | 500 | 1 |
| Input | 14px | 400 | 1.5 |

#### Usage Rules
- **Headings**: Use H1 for page title only, H2 for sections, H3 for cards
- **Body text**: 14px minimum for data-heavy screens
- **Numbers**: Use monospace font for amounts (alignment)
- **Vietnamese**: Ensure proper character rendering (Inter supports)

### 3.3 Spacing System

Base unit: 4px

```
--space-1: 4px    /* Tight spacing */
--space-2: 8px    /* Between related elements */
--space-3: 12px   /* Default internal padding */
--space-4: 16px   /* Standard gap */
--space-5: 24px   /* Section spacing */
--space-6: 32px   /* Major section gaps */
--space-8: 48px   /* Page margins */
```

**Rules:**
- Card padding: 20px
- Form field spacing: 16px vertical
- Button padding: 10px 20px
- Table cell padding: 12px 16px

### 3.4 Visual Effects

#### Shadows
```css
--shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
--shadow-md: 0 4px 6px rgba(0, 0, 0, 0.07);
--shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.1);
--shadow-input: 0 1px 3px rgba(0, 0, 0, 0.08);
```

#### Border Radius
```css
--radius-sm: 4px;    /* Buttons, inputs */
--radius-md: 6px;    /* Cards */
--radius-lg: 8px;    /* Modals */
--radius-full: 9999px; /* Pills, avatars */
```

#### Transitions
```css
--transition-fast: 150ms ease;
--transition-base: 250ms ease;
--transition-slow: 350ms ease;
```

### 3.5 Component States

| State | Visual Change |
|-------|---------------|
| Default | Base colors |
| Hover | 10% darker/lighter, slight shadow |
| Active | 15% darker, pressed effect |
| Focus | 2px outline with primary color |
| Disabled | 50% opacity, cursor not-allowed |
| Loading | Skeleton or spinner overlay |
| Error | Red border, error message below |

---

## 4. User Interface Structure

### 4.1 Layout Structure

```
┌──────────────────────────────────────────────────────────────┐
│  HEADER (64px) - Logo | Search | Notifications | User       │
├────────────┬─────────────────────────────────────────────────┤
│            │  BREADCRUMB (40px)                              │
│  SIDEBAR   ├─────────────────────────────────────────────────┤
│  (260px)   │                                                 │
│            │  CONTENT AREA                                   │
│  - Home    │  - Page Title                                   │
│  - Voucher │  - Action Bar (Create, Filter)                  │
│  - Ledger  │  - Main Content (Tables, Forms, Cards)         │
│  - Report  │  - Pagination/Stats                            │
│  - Settings│                                                 │
│            │                                                 │
├────────────┴─────────────────────────────────────────────────┤
│  FOOTER (40px) - Copyright | Version                        │
└──────────────────────────────────────────────────────────────┘
```

### 4.2 Responsive Breakpoints

| Breakpoint | Width | Layout Changes |
|------------|-------|----------------|
| Mobile | <768px | Sidebar hidden, hamburger menu, stacked cards |
| Tablet | 768-1024px | Collapsed sidebar (icons), 2-column grids |
| Desktop | 1024-1366px | Full sidebar, 3-column max |
| Large | >1366px | Max content width 1400px, centered |

### 4.3 Navigation Architecture

```
SIDEBAR STRUCTURE
=================

📊 Dashboard          → /Home/Index

📝 Accounting
   ├─ Chứng Từ       → /Vouchers/Index
   ├─ Sổ Cái          → /Ledger/Index
   ├─ Thử Nghiệm     → /TrialBalance/Index
   └─ Báo Cáo        → /Reports/Index

📦 Inventory
   ├─ Danh Mục       → /Products/Index
   ├─ Kho            → /Warehouses/Index
   └─ Chuyển Kho     → /StockMovements/Index

⚙️ System
   ├─ Người Dùng     → /Users/Index
   ├─ Phân Quyền    → /Roles/Index
   └─ Cấu Hình      → /Settings/Index
```

### 4.4 Page Templates

#### Template A: List View (Chứng Từ, Sản Phẩm)
```
┌─────────────────────────────────────────────────┐
│ Page Title                    [+ Create New]   │
├─────────────────────────────────────────────────┤
│ [Search........] [Filter ▼] [Export ▼]          │
├─────────────────────────────────────────────────┤
│ # │ Date     │ No.   │ Type  │ Amount  │ Status │
│───│──────────│───────│───────│─────────│────────│
│ 1 │ 26/03/26 │ CT001 │ PN   │ 5,000,000│ Done  │
│ 2 │ 26/03/26 │ CT002 │ PX   │ 3,200,000│ Pend  │
├─────────────────────────────────────────────────┤
│ ◀ 1 2 3 4 5 ▶    Showing 1-20 of 1,234         │
└─────────────────────────────────────────────────┘
```

#### Template B: Form View (Create/Edit)
```
┌─────────────────────────────────────────────────┐
│ ← Back    Create Voucher         [Save] [Cancel]│
├─────────────────────────────────────────────────┤
│ ┌─ Voucher Information ────────────────────────┐│
│ │ Voucher No: [AUTO]   Date: [📅]              ││
│ │ Type:       [▼ Select]                        ││
│ │ Description:[..............................]   ││
│ └───────────────────────────────────────────────┘│
│ ┌─ Line Items ─────────────────────────────────┐│
│ │ + Add Line                                    ││
│ │ Account │ Debit    │ Credit   │ Description  ││
│ │ ────────┼──────────┼──────────┼───────────── ││
│ │ [▼    ] │ [      ] │ [      ] │ [          ] ││
│ └───────────────────────────────────────────────┘│
│ Total: Debit 5,000,000 | Credit 5,000,000 ✅   │
└─────────────────────────────────────────────────┘
```

#### Template C: Dashboard
```
┌─────────────┬─────────────┬─────────────┬─────────┐
│ Total Revenue│ Vouchers    │ Products    │ Users   │
│ 125,000,000  │ 156         │ 89          │ 12      │
│ ↑12% vs last │ ↑5%         │ ↓2%         │ =       │
├─────────────┴─────────────┴─────────────┴─────────┤
│                                                     │
│  Revenue Chart (Line/Bar)          Recent Activity │
│  ┌────────────────────────┐       ┌──────────────┐ │
│  │                        │       │ CT003 - PN   │ │
│  │    📈                  │       │ CT002 - PX   │ │
│  │                        │       │ CT001 - PC   │ │
│  └────────────────────────┘       └──────────────┘ │
├────────────────────────────────────────────────────┤
│ Quick Actions: [New Voucher] [New Product] [Report]│
└────────────────────────────────────────────────────┘
```

---

## 5. Core User Workflows

### 5.1 Voucher Creation Flow

```
USER: Data Entry Clerk
FREQUENCY: 50-100 times/day
PAIN POINTS: Repetitive entry, typo risks

FLOW:
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ 1. Select   │ ──▶ │ 2. Enter    │ ──▶ │ 3. Add Line │
│ Type (PN/PX) │     │ Date, No.   │     │ Items       │
└─────────────┘     └─────────────┘     └─────────────┘
       │                                       │
       │                                       ▼
       │                               ┌─────────────┐
       │                               │ 4. Validate │
       │                               │ (Auto-check)│
       │                               └─────────────┘
       │                                       │
       ▼                                       ▼
┌─────────────┐                       ┌─────────────┐
│ 8. Done     │ ◀──── 7. Save & New  │ 6. Confirm │
│ Success Msg │     or 7b. Save & Close │ (Optional)  │
└─────────────┘                       └─────────────┘

UX REQUIREMENTS:
- Enter → Tab through fields (no mouse)
- Ctrl+S to save
- Duplicate previous voucher option
- Auto-suggest account codes
- Running total always visible
- Validation before save (debit = credit)
```

### 5.2 Approval Flow (Manager)

```
USER: Finance Manager
FREQUENCY: 3-5 times/day

FLOW:
Dashboard → Pending Vouchers → Review → Approve/Reject → Next

REQUIREMENTS:
- One-click approve from list
- Bulk select and approve
- Rejection requires reason
- Audit trail of all actions
- Email notification (optional)
```

### 5.3 Report Generation

```
USER: Manager, Auditor
FREQUENCY: Weekly/Monthly

REPORTS NEEDED:
- Trial Balance
- General Ledger
- Cash Flow
- Inventory Report

REQUIREMENTS:
- Date range selection
- Export to Excel/PDF
- Print-friendly view
- Save filter presets
```

---

## 6. Component Library

### 6.1 Recommended Stack

| Category | Library | Version | Notes |
|----------|---------|---------|-------|
| CSS Framework | Bootstrap | 5.3.x | Customized with design tokens |
| Admin Template | **CoreUI** or **Tabler** | Latest | See note below |
| Data Tables | DataTables | 2.x | Server-side processing |
| Charts | ApexCharts or Chart.js | Latest | |
| Date Picker | Flatpickr | Latest | |
| Select | Select2 | 4.x | For dropdowns with search |
| Icons | **Bootstrap Icons** | 1.11.x | MIT license, matches Bootstrap stack |
| Toast | Noty or SweetAlert2 | Latest | |
| Validation | jQuery Validate | Latest | |

**Template Note:** Reconsider AdminLTE (dated). Better options:
- **Tabler** - Modern, clean, actively maintained
- **CoreUI** - Good Bootstrap integration
- **Volt** - Modern dashboard look
All are Bootstrap-based and work with ASP.NET Core.

### 6.2 Core Components to Build

| Component | Purpose | States |
|-----------|---------|--------|
| `VoucherTable` | Main data grid | Loading, Empty, Error, Data |
| `VoucherForm` | Create/Edit voucher | Default, Validation, Saving |
| `AmountInput` | Currency input with formatting | Default, Focused, Error |
| `AccountSelect` | Searchable account dropdown | Loading, Default, Selected |
| `StatusBadge` | Visual status indicator | Draft, Pending, Approved, Rejected |
| `KpiCard` | Dashboard metrics | Loading, Default |
| `DateRangePicker` | Report date selection | Default, Selected |
| `ActionMenu` | Row actions dropdown | Default, Open |
| `ConfirmDialog` | Delete confirmation | Default |
| `Toast` | Success/Error notification | Success, Error, Warning |
| `Pagination` | List navigation | Default, Active, Disabled |
| `EmptyState` | No data display | Default, Custom message |
| `LoadingSpinner` | Async operation | Default, With message |
| `LoadingSkeleton` | Content placeholder | Animated |

### 6.3 Component API Standards

Every component must have:
- Clear prop/parameter documentation
- Loading state
- Error state
- Accessibility attributes (aria-*)
- Test IDs for automation

---

## 7. Error Handling

### 7.1 Error Types & Responses

| Error Type | User Message | Technical Details | Recovery Action |
|------------|--------------|-------------------|-----------------|
| **Validation** | "Số tiền phải lớn hơn 0" | Field + Rule violated | Highlight field, show hint |
| **Business Rule** | "Không thể xóa chứng từ đã duyệt" | Rule code | Show what prevents action |
| **Not Found** | "Không tìm thấy dữ liệu" | Resource type, ID | Link to list |
| **Unauthorized** | "Bạn không có quyền thực hiện" | Permission needed | Contact admin |
| **Server Error** | "Đã xảy ra lỗi hệ thống" | Error ID (for support) | Retry button |
| **Network Error** | "Mất kết nối mạng" | Connection status | Retry button |
| **Timeout** | "Yêu cầu quá thời gian chờ" | Operation type | Retry button |

### 7.2 Validation Rules

#### Voucher Validation
```
- Số chứng từ: Required, unique, format [A-Z]{2}[0-9]{4}
- Ngày chứng từ: Required, <= today, >= period start
- Số tiền: Required, > 0, max 15 digits
- Tổng Nợ = Tổng Có: Must match
- Ít nhất 1 dòng hạch toán: Required
- Mã tài khoản: Required, exists in system
```

#### Product Validation
```
- Mã sản phẩm: Required, unique, max 20 chars
- Tên sản phẩm: Required, max 200 chars
- Giá bán: >= 0, <= 999,999,999,999
- Số lượng tồn: >= 0
```

### 7.3 Form Error Display

```html
<!-- Field Error -->
<div class="form-group has-error">
    <label for="amount">Số tiền</label>
    <input type="text" id="amount" class="form-control is-invalid" 
           aria-describedby="amount-error" value="0">
    <span id="amount-error" class="error-message" role="alert">
        Số tiền phải lớn hơn 0
    </span>
</div>

<!-- Form-level Error -->
<div class="alert alert-danger" role="alert">
    <i class="bi bi-exclamation-triangle-fill"></i>
    <strong>Lỗi:</strong> Không thể lưu chứng từ. Vui lòng kiểm tra lại.
    <ul>
        <li>Tổng Nợ và Tổng Có không bằng nhau</li>
    </ul>
</div>
```

### 7.4 Error Message Standards

| Rule | Example |
|------|---------|
| Use Vietnamese | "Vui lòng nhập số tiền" (not "Please enter amount") |
| Be specific | "Mã chứng từ CT001 đã tồn tại" |
| Suggest fix | "Vui lòng nhập ngày theo định dd/MM/yyyy" |
| No technical jargon | "Database error" → "Đã xảy ra lỗi" |
| Show error code for support | "Mã lỗi: #AMS-2026-001" |

### 7.5 Retry & Recovery

```javascript
// Automatic retry for transient errors
async function saveVoucher(data) {
    const maxRetries = 3;
    const retryDelay = 1000;
    
    for (let i = 0; i < maxRetries; i++) {
        try {
            return await api.post('/api/vouchers', data);
        } catch (error) {
            if (!error.retryable || i === maxRetries - 1) {
                throw error;
            }
            await delay(retryDelay * Math.pow(2, i));
        }
    }
}

// Offline handling
window.addEventListener('online', () => {
    showToast('Kết nối đã khôi phục. Đang đồng bộ dữ liệu...');
    syncPendingOperations();
});
```

### 7.6 Confirmation Dialogs

| Severity | Icon | Use Case | Button Labels |
|----------|------|----------|----------------|
| **Danger** | `bi bi-exclamation-triangle-fill text-danger` | Delete, Cancel approved | [Hủy bỏ], [Xác nhận xóa] |
| **Warning** | `bi bi-exclamation-triangle-fill text-warning` | Reject, Rollback | [Hủy], [Xác nhận] |
| **Info** | `bi bi-question-circle-fill text-info` | Bulk operations | [Hủy], [Tiếp tục] |

```html
<!-- Delete Confirmation -->
<div class="modal" tabindex="-1" role="dialog">
    <div class="modal-dialog modal-sm">
        <div class="modal-content">
            <div class="modal-header bg-danger text-white">
                <h5 class="modal-title">Xác nhận xóa</h5>
            </div>
            <div class="modal-body">
                <p>Bạn có chắc chắn muốn xóa chứng từ <strong>CT001</strong>?</p>
                <p class="text-muted">Hành động này không thể hoàn tác.</p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">
                    Hủy bỏ
                </button>
                <button type="button" class="btn btn-danger">
                    <i class="bi bi-trash"></i> Xóa
                </button>
            </div>
        </div>
    </div>
</div>
```

---

## 8. Localization

### 8.1 Supported Locales

| Locale | Language | Number Format | Date Format |
|--------|----------|---------------|-------------|
| vi-VN | Vietnamese (Primary) | 1.234.567,89 ₫ | dd/MM/yyyy |
| en-US | English (Secondary) | $1,234,567.89 | MM/dd/yyyy |

### 8.2 Currency Formatting (VND)

```javascript
// Vietnamese Dong - no decimals, thousand separator
const formatVND = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount);
};

// Examples: 5,000,000 ₫
```

### 8.3 Date Formatting

| Display | Format | Example |
|---------|--------|---------|
| Short Date | dd/MM/yyyy | 26/03/2026 |
| Long Date | "dd" MMMM "năm" yyyy | 26 tháng 3 năm 2026 |
| DateTime | dd/MM/yyyy HH:mm | 26/03/2026 14:30 |
| Month-Year | MM/yyyy | 03/2026 |
| Quarter | "Q"Q yyyy | Q1 2026 |

```javascript
const formatDate = (date) => {
    return new Intl.DateTimeFormat('vi-VN').format(date);
    // "26/03/2026"
};

const formatDateLong = (date) => {
    return new Intl.DateTimeFormat('vi-VN', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(date);
    // "26 tháng 3 năm 2026"
};
```

### 8.4 Number Formatting

| Type | Format | Example |
|------|--------|---------|
| Amount (VND) | #.### | 5.000.000 |
| Quantity | #.## | 1,234 |
| Percentage | ##% | 15% |
| Decimal | #.#### | 3.1416 |

### 8.5 UI Text Standards

| Rule | Example |
|------|---------|
| Buttons: Verb + Object | [Lưu], [Hủy bỏ], [Xóa] |
| Labels: Noun | Số chứng từ, Ngày, Số tiền |
| Messages: Complete sentences | "Chứng từ đã được lưu thành công" |
| Errors: Specific | "Mã chứng từ đã tồn tại" |
| Confirmations: Question | "Bạn có muốn xóa chứng từ này?" |

### 8.6 Vietnamese Character Handling

```css
/* Ensure proper Vietnamese character rendering */
body {
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
}

/* Vietnamese diacritics */
.unicode-word {
    unicode-bidi: plaintext; /* Prevents word-break issues */
}
```

---

## 9. Security UX

### 9.1 Authentication

#### Login Form
```html
<form id="login-form">
    <div class="form-group">
        <label for="username">Tên đăng nhập</label>
        <input type="text" id="username" autocomplete="username" required>
    </div>
    <div class="form-group">
        <label for="password">Mật khẩu</label>
        <input type="password" id="password" autocomplete="current-password" required>
    </div>
    <div class="form-check">
        <input type="checkbox" id="remember-me">
        <label for="remember-me">Ghi nhớ đăng nhập</label>
    </div>
    <button type="submit">Đăng nhập</button>
</form>
```

#### Password Requirements Display
```
Mật khẩu cần:
☑ Ít nhất 8 ký tự
☑ Ít nhất 1 chữ hoa
☑ Ít nhất 1 chữ thường
☑ Ít nhất 1 số
☑ Ít nhất 1 ký tự đặc biệt
```

### 9.2 Session Management

| Scenario | Behavior |
|----------|----------|
| **Idle Warning** | Show modal after 25 min: "Phiên sẽ hết sau 5 phút" |
| **Session Timeout** | Redirect to login with message "Phiên đã hết. Vui lòng đăng nhập lại" |
| **Auto Logout** | Logout after 30 min inactivity |
| **Concurrent Login** | Warn user, option to force logout other sessions |

```javascript
const SESSION_TIMEOUT = 30 * 60 * 1000; // 30 minutes
const WARNING_BEFORE = 5 * 60 * 1000; // 5 minutes

let sessionTimer;

function resetSessionTimer() {
    clearTimeout(sessionTimer);
    sessionTimer = setTimeout(() => {
        showSessionWarning();
    }, SESSION_TIMEOUT - WARNING_BEFORE);
}

function showSessionWarning() {
    showModal({
        title: 'Cảnh báo phiên',
        message: 'Phiên làm việc sẽ hết sau 5 phút. Bạn có muốn tiếp tục?',
        buttons: [
            { label: 'Đăng xuất', action: logout },
            { label: 'Tiếp tục', action: resetSessionTimer }
        ]
    });
}
```

### 9.3 Authorization UI

| Scenario | Display |
|----------|---------|
| **No Permission** | Hide action button entirely |
| **View Only** | Show disabled with tooltip "Bạn không có quyền" |
| **Access Denied** | Show 403 page with contact admin option |

```html
<!-- Conditional rendering based on permission -->
<a href="/vouchers/edit/1" class="btn btn-sm btn-primary"
   data-permission="voucher:edit">
    <i class="bi bi-pencil"></i> Sửa
</a>

<!-- Without permission: element is hidden or shows disabled state -->
```

### 9.4 Audit Trail Display

```html
<div class="audit-trail card">
    <h4><i class="bi bi-clock-history"></i> Lịch sử thao tác</h4>
    <table class="table table-sm">
        <thead>
            <tr>
                <th>Thời gian</th>
                <th>Người thực hiện</th>
                <th>Hành động</th>
                <th>Chi tiết</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>26/03/2026 14:30</td>
                <td>Nguyễn Văn A</td>
                <td><span class="badge bg-success">Tạo mới</span></td>
                <td>Tạo chứng từ CT001</td>
            </tr>
            <tr>
                <td>26/03/2026 15:00</td>
                <td>Trần Thị B</td>
                <td><span class="badge bg-primary">Duyệt</span></td>
                <td>Duyệt chứng từ CT001</td>
            </tr>
        </tbody>
    </table>
</div>
```

### 9.5 Sensitive Data Masking

| Data Type | Display | Reveal Action |
|-----------|---------|----------------|
| Password | `••••••••` | Click eye icon |
| Salary | `•••.•••.•••` | Hover or click |
| Bank Account | `••••••••1234` | Click to copy |
| Tax ID | `••••••••••••` | Click to reveal |

---

## 10. Accessibility Requirements

### 10.1 Compliance Target

**WCAG 2.1 Level AA** - Minimum required

### 10.2 Specific Requirements

| Requirement | Implementation |
|-------------|----------------|
| Color contrast | Minimum 4.5:1 for text, 3:1 for large text |
| Focus indicators | Visible 2px outline on all interactive elements |
| Keyboard navigation | Full tab order, arrow keys in grids |
| Screen readers | Proper ARIA labels, live regions for updates |
| Error identification | Error messages linked to fields with aria-describedby |
| Resize support | Works up to 200% zoom without horizontal scroll |

### 10.3 Testing Requirements

- [ ] Keyboard-only navigation test
- [ ] Screen reader test (NVDA/VoiceOver)
- [ ] Color blindness simulation
- [ ] 200% zoom test
- [ ] High contrast mode test

---

## 11. Responsive Strategy

### 11.1 Device Personas

| Device | User | Use Case | Priority |
|--------|------|----------|----------|
| Desktop 1366px+ | All | Primary | Must have |
| Laptop 1024px | All | Common | Must have |
| Tablet 768px | Manager | Review, Approve | Should have |
| Mobile 375px | Field | Quick lookup | Nice to have |

### 11.2 Responsive Priorities

**Desktop (Must Work)**
- All features fully functional
- Optimal keyboard navigation
- Full data tables with horizontal scroll if needed

**Tablet (Should Work)**
- View data, approve/reject actions
- Touch-friendly targets (44px min)
- Swipe gestures for navigation

**Mobile (Nice to Have)**
- Dashboard KPIs only
- Simple list views
- No complex forms

### 11.3 Mobile Considerations

Given this is an accounting app, mobile is NOT primary. Focus desktop experience first. Tablet is useful for managers doing approval work.

---

## 12. Performance Standards

### 12.1 Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| First Contentful Paint | <1.5s | Lighthouse |
| Time to Interactive | <3s | Lighthouse |
| Page load (server) | <500ms | Server metrics |
| Table render (1000 rows) | <100ms | Client |
| Search response | <300ms | Network |
| Save operation | <500ms | Network |

### 12.2 Strategies

- Server-side rendering (Razor) - good for initial load
- Lazy load sidebar, modals
- Virtual scrolling for large tables
- Debounce search inputs (300ms)
- Optimistic UI updates
- Browser caching for static assets
- Compression (gzip/brotli)

### 12.3 Budget

- JavaScript: <200KB gzipped
- CSS: <50KB gzipped
- Images: WebP, lazy loaded
- Total page weight: <500KB initial

---

## 13. Testing Requirements

### 13.1 UX Testing Types

| Type | When | Who | Tools |
|------|------|-----|-------|
| **Usability Testing** | Before dev, After major changes | Real users | UserZoom, Maze |
| **A/B Testing** | Continuous | % of users | Google Optimize, Optimizely |
| **Accessibility Audit** | Before release | QA + Tools | AXE, WAVE |
| **Performance Testing** | Pre-release | DevOps | Lighthouse, k6 |
| **Visual Regression** | After UI changes | CI Pipeline | Percy, Chromatic |

### 13.2 Usability Test Scenarios

```
TASK 1: Tạo chứng từ mới
- Thời gian mục tiêu: < 60 giây
- Thành công: 90% users
- Đánh giá độ khó: Dễ / Trung bình / Khó

TASK 2: Tìm kiếm chứng từ theo ngày
- Thời gian mục tiêu: < 30 giây  
- Thành công: 95% users
- Đánh giá độ khó: Dễ / Trung bình / Khó

TASK 3: Duyệt chứng từ (Manager)
- Thời gian mục tiêu: < 20 giây
- Thành công: 95% users
- Đánh giá độ khó: Dễ / Trung bình / Khó
```

### 13.3 Feedback Collection

```html
<!-- In-app feedback widget -->
<div id="feedback-widget" class="feedback-button">
    <i class="bi bi-chat-dots"></i> Góp ý
</div>

<!-- Feedback Modal -->
<div class="modal" id="feedback-modal">
    <div class="modal-header">
        <h5>Góp ý về hệ thống</h5>
    </div>
    <div class="modal-body">
        <div class="mb-3">
            <label>Loại góp ý</label>
            <select class="form-select">
                <option>Báo lỗi</option>
                <option>Gợi ý cải thiện</option>
                <option>Khác</option>
            </select>
        </div>
        <div class="mb-3">
            <label>Mô tả</label>
            <textarea class="form-control" rows="4" 
                      placeholder="Mô tả chi tiết vấn đề hoặc đề xuất..."></textarea>
        </div>
        <div class="mb-3">
            <label>Ảnh chụp màn hình (nếu có)</label>
            <input type="file" accept="image/*" multiple>
        </div>
    </div>
    <div class="modal-footer">
        <button class="btn btn-secondary">Hủy</button>
        <button class="btn btn-primary">Gửi góp ý</button>
    </div>
</div>
```

### 13.4 Success Metrics (NPS-style)

| Metric | Target |
|--------|--------|
| Task Completion Rate | > 90% |
| Time on Task | < Target time |
| Error Rate | < 5% |
| User Satisfaction | > 4/5 |
| System Usability Scale (SUS) | > 70 |

---

## 14. Implementation Priorities

### Phase 1: Foundation (Week 1-2)

| Task | Effort | Priority |
|------|--------|----------|
| Set up design tokens (CSS variables) | Medium | Must |
| Create base layout (sidebar, header) | Medium | Must |
| Implement navigation structure | Low | Must |
| Build responsive breakpoints | Medium | Must |
| Add color scheme customization | Low | Should |

### Phase 2: Core Components (Week 3-4)

| Task | Effort | Priority |
|------|--------|----------|
| Data table component | High | Must |
| Form base components | High | Must |
| Validation UI | Medium | Must |
| Toast notifications | Low | Should |
| Loading states | Low | Should |
| Error display patterns | Medium | Must |
| Empty state designs | Low | Should |

### Phase 3: Business Components (Week 5-6)

| Task | Effort | Priority |
|------|--------|----------|
| Voucher list & form | High | Must |
| Account selector | Medium | Must |
| Amount input with formatting | Medium | Must |
| Status badges | Low | Must |
| Dashboard KPIs | Medium | Should |
| Pagination | Medium | Must |
| Confirmation dialogs | Medium | Must |

### Phase 4: Advanced Features (Week 7-8)

| Task | Effort | Priority |
|------|--------|----------|
| Session management | Medium | Must |
| Security UI patterns | High | Must |
| Keyboard shortcuts | Medium | Should |
| Localization | Medium | Must |
| Accessibility audit | High | Must |
| Performance optimization | Medium | Should |
| Cross-browser testing | Medium | Must |
| Usability testing | High | Must |
| User feedback system | Medium | Should |

---

## Appendix A: Design Token Reference

```css
/* Complete Design Tokens */
:root {
  /* Colors */
  --color-primary: #1E3A5F;
  --color-primary-light: #2D5A8A;
  --color-primary-dark: #0F1E33;
  --color-secondary: #4A90A4;
  --color-accent: #E8A838;
  
  --color-success: #28A745;
  --color-success-light: #D4EDDA;
  --color-warning: #FFC107;
  --color-warning-light: #FFF3CD;
  --color-danger: #DC3545;
  --color-danger-light: #F8D7DA;
  --color-info: #17A2B8;
  --color-info-light: #D1ECF1;
  
  --color-gray-900: #212529;
  --color-gray-700: #495057;
  --color-gray-500: #6C757D;
  --color-gray-300: #DEE2E6;
  --color-gray-100: #F8F9FA;
  --color-white: #FFFFFF;
  
  --color-body-bg: #F5F7FA;
  --color-sidebar-bg: #1E2836;
  --color-header-bg: #FFFFFF;
  
  /* Typography */
  --font-family-base: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --font-family-mono: 'JetBrains Mono', 'Fira Code', monospace;
  
  --font-size-h1: 28px;
  --font-size-h2: 22px;
  --font-size-h3: 18px;
  --font-size-body: 14px;
  --font-size-small: 13px;
  --font-size-caption: 12px;
  
  /* Spacing */
  --space-1: 4px;
  --space-2: 8px;
  --space-3: 12px;
  --space-4: 16px;
  --space-5: 24px;
  --space-6: 32px;
  --space-8: 48px;
  
  /* Effects */
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px rgba(0, 0, 0, 0.07);
  --shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.1);
  
  --radius-sm: 4px;
  --radius-md: 6px;
  --radius-lg: 8px;
  --radius-full: 9999px;
  
  --transition-fast: 150ms ease;
  --transition-base: 250ms ease;
  
  /* Layout */
  --header-height: 64px;
  --sidebar-width: 260px;
  --sidebar-collapsed: 72px;
  --footer-height: 40px;
  --content-max-width: 1400px;
}
```

---

## Appendix B: Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+S | Save |
| Ctrl+N | New (context-aware) |
| Ctrl+F | Focus search |
| Ctrl+E | Export |
| Escape | Close modal/cancel |
| Enter | Confirm (in forms) |
| Tab | Next field |
| Shift+Tab | Previous field |
| Ctrl+D | Duplicate (voucher) |
| F5 | Refresh |

---

## Appendix C: Acceptance Criteria

- [ ] All pages pass WCAG 2.1 Level AA
- [ ] Page load <3s on 3G
- [ ] All interactive elements keyboard accessible
- [ ] Color contrast meets 4.5:1 minimum
- [ ] Focus indicators visible
- [ ] No horizontal scroll at 1366px
- [ ] Forms prevent submission with errors
- [ ] Loading states shown for async operations
- [ ] Responsive down to 768px functional
- [ ] Session timeout warning at 25 min
- [ ] Currency formatted as VND (1.234.567)
- [ ] Date formatted as DD/MM/yyyy
- [ ] Error messages in Vietnamese
- [ ] Confirmation dialogs for destructive actions
- [ ] Empty states for all list views

---

## Appendix D: Icon Reference (Bootstrap Icons)

### Integration

```html
<!-- Add to <head> in _Layout.cshtml -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
```

### Common Icons by Category

#### Navigation
| Use Case | Icon Class |
|----------|------------|
| Dashboard | `bi bi-grid-1x2` |
| Home | `bi bi-house-door` |
| Menu | `bi bi-list` |
| Back | `bi bi-arrow-left` |
| Forward | `bi bi-arrow-right` |

#### Accounting / Vouchers
| Use Case | Icon Class |
|----------|------------|
| Voucher | `bi bi-receipt` |
| Invoice | `bi bi-file-earmark-text` |
| Money | `bi bi-cash` |
| Bank | `bi bi-bank` |
| Credit Card | `bi bi-credit-card` |
| Graph | `bi bi-graph-up-arrow` |
| Calculator | `bi bi-calculator` |

#### Actions
| Use Case | Icon Class |
|----------|------------|
| Create | `bi bi-plus-lg` |
| Edit | `bi bi-pencil` |
| Delete | `bi bi-trash` |
| Save | `bi bi-save` |
| Search | `bi bi-search` |
| Filter | `bi bi-funnel` |
| Export | `bi bi-download` |
| Print | `bi bi-printer` |
| Refresh | `bi bi-arrow-clockwise` |
| Settings | `bi bi-gear` |

#### Status
| Use Case | Icon Class |
|----------|------------|
| Success | `bi bi-check-circle-fill` |
| Warning | `bi bi-exclamation-triangle-fill` |
| Error | `bi bi-x-circle-fill` |
| Info | `bi bi-info-circle-fill` |
| Pending | `bi bi-clock` |
| Approved | `bi bi-check2-all` |
| Rejected | `bi bi-x-lg` |

### Sidebar Navigation Icon Mapping

```
📊 Dashboard          → bi-grid-1x2
📝 Accounting
   ├─ Chứng Từ       → bi-receipt
   ├─ Sổ Cái          → bi-book
   ├─ Thử Nghiệm     → bi-calculator
   └─ Báo Cáo        → bi-bar-chart-line
📦 Inventory
   ├─ Danh Mục       → bi-box-seam
   ├─ Kho            → bi-building
   └─ Chuyển Kho     → bi-arrow-left-right
⚙️ System
   ├─ Người Dùng     → bi-people
   ├─ Phân Quyền    → bi-shield-lock
   └─ Cấu Hình      → bi-gear
```

### Usage Guidelines

1. **Always pair icon with text** for primary navigation
2. **Icon-only acceptable** for toolbar actions with tooltips
3. **Consistent sizing**: 
   - Nav: 1.25rem (20px)
   - Buttons: inherit
   - Headers: 1.5-2rem
4. **Color**: Use `text-primary`, `text-muted`, or semantic colors
5. **Spacing**: Use `me-1` (margin-end) between icon and text

---

**Document Status:** v2.0 - Enhanced  
**Next Review:** After Phase 2 implementation  
**Owner:** Development Team
