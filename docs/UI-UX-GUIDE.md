# UI/UX Design Guidelines for AMS ERP Web Application

**Version:** 1.0  
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
7. [Accessibility Requirements](#7-accessibility-requirements)
8. [Responsive Strategy](#8-responsive-strategy)
9. [Performance Standards](#9-performance-standards)
10. [Implementation Priorities](#10-implementation-priorities)

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
│  - Ledger  │  - Main Content (Tables, Forms, Cards)          │
│  - Report  │  - Pagination/Stats                             │
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
│ 8. Done     │ ◀──── 7. Save & New    │ 6. Confirm │
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
| Icons | Tabler Icons | 2.x | Better than Font Awesome |
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

### 6.3 Component API Standards

Every component must have:
- Clear prop/parameter documentation
- Loading state
- Error state
- Accessibility attributes (aria-*)
- Test IDs for automation

---

## 7. Accessibility Requirements

### 7.1 Compliance Target

**WCAG 2.1 Level AA** - Minimum required

### 7.2 Specific Requirements

| Requirement | Implementation |
|-------------|----------------|
| Color contrast | Minimum 4.5:1 for text, 3:1 for large text |
| Focus indicators | Visible 2px outline on all interactive elements |
| Keyboard navigation | Full tab order, arrow keys in grids |
| Screen readers | Proper ARIA labels, live regions for updates |
| Error identification | Error messages linked to fields with aria-describedby |
| Resize support | Works up to 200% zoom without horizontal scroll |

### 7.3 Testing Requirements

- [ ] Keyboard-only navigation test
- [ ] Screen reader test (NVDA/VoiceOver)
- [ ] Color blindness simulation
- [ ] 200% zoom test
- [ ] High contrast mode test

---

## 8. Responsive Strategy

### 8.1 Device Personas

| Device | User | Use Case | Priority |
|--------|------|----------|----------|
| Desktop 1366px+ | All | Primary | Must have |
| Laptop 1024px | All | Common | Must have |
| Tablet 768px | Manager | Review, Approve | Should have |
| Mobile 375px | Field | Quick lookup | Nice to have |

### 8.2 Responsive Priorities

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

### 8.3 Mobile Considerations

Given this is an accounting app, mobile is NOT primary. Focus desktop experience first. Tablet is useful for managers doing approval work.

---

## 9. Performance Standards

### 9.1 Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| First Contentful Paint | <1.5s | Lighthouse |
| Time to Interactive | <3s | Lighthouse |
| Page load (server) | <500ms | Server metrics |
| Table render (1000 rows) | <100ms | Client |
| Search response | <300ms | Network |
| Save operation | <500ms | Network |

### 9.2 Strategies

- Server-side rendering (Razor) - good for initial load
- Lazy load sidebar, modals
- Virtual scrolling for large tables
- Debounce search inputs (300ms)
- Optimistic UI updates
- Browser caching for static assets
- Compression (gzip/brotli)

### 9.3 Budget

- JavaScript: <200KB gzipped
- CSS: <50KB gzipped
- Images: WebP, lazy loaded
- Total page weight: <500KB initial

---

## 10. Implementation Priorities

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

### Phase 3: Business Components (Week 5-6)

| Task | Effort | Priority |
|------|--------|----------|
| Voucher list & form | High | Must |
| Account selector | Medium | Must |
| Amount input with formatting | Medium | Must |
| Status badges | Low | Must |
| Dashboard KPIs | Medium | Should |

### Phase 4: Refinement (Week 7-8)

| Task | Effort | Priority |
|------|--------|----------|
| Keyboard shortcuts | Medium | Should |
| Accessibility audit | High | Must |
| Performance optimization | Medium | Should |
| Cross-browser testing | Medium | Must |
| Mobile adjustments | Medium | Should |

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

---

**Document Status:** Foundation - Requires Validation  
**Next Review:** After Phase 1 implementation  
**Owner:** Development Team
