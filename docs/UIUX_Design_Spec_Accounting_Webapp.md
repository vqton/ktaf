# UI/UX & Frontend Design Specification
## Accounting Web Application
**Document Type:** AI Agent Implementation Guide  
**Author Role:** Senior UI/UX Designer (20 years experience)  
**Version:** 1.0  
**Audience:** AI Coding Agent / Frontend Developer  

---

## 0. HOW TO READ THIS DOCUMENT

This document is structured for an AI agent to implement a complete accounting webapp UI/UX. Each section is self-contained and actionable. Follow sections in order. Every design decision includes the **reason why** — do not deviate without understanding the rationale.

---

## 1. DESIGN PHILOSOPHY & PRINCIPLES

### 1.1 Core Philosophy: "Calm Finance"
Accounting software must feel **trustworthy, precise, and effortless**. Users are often stressed (deadlines, audits, tax season). The UI must reduce cognitive load, not add to it.

**Three pillars:**
- **Clarity over cleverness** — every element has one job; no decorative noise
- **Data density with breathing room** — accountants need to see a lot of data, but never feel overwhelmed
- **Immediate feedback** — every action confirms itself; numbers never feel uncertain

### 1.2 Design Principles (In Priority Order)

| Priority | Principle | Meaning |
|----------|-----------|---------|
| 1 | Scannable | Users find what they need in < 3 seconds |
| 2 | Predictable | Same action always produces the same result |
| 3 | Forgiving | Mistakes are easy to undo; nothing is permanent without confirmation |
| 4 | Efficient | Power users can operate without mouse (keyboard shortcuts) |
| 5 | Trustworthy | Data looks authoritative; no playful fonts on financial figures |

---

## 2. VISUAL IDENTITY SYSTEM

### 2.1 Color Palette

**Primary Brand Colors:**
```css
:root {
  /* Primary — communicates stability and trust */
  --color-primary-900: #0F2D4A;   /* Darkest navy — main brand anchor */
  --color-primary-800: #1A3F63;
  --color-primary-700: #1E5080;
  --color-primary-600: #1D6FA4;   /* Default interactive state */
  --color-primary-500: #2589C8;   /* Hover state */
  --color-primary-400: #4AAEE0;   /* Focus rings */
  --color-primary-100: #D6EDFB;   /* Selected row backgrounds */
  --color-primary-050: #EDF6FD;   /* Subtle highlights */

  /* Semantic — always use semantic tokens, never raw values in components */
  --color-success-700: #166534;
  --color-success-500: #22C55E;
  --color-success-100: #DCFCE7;

  --color-danger-700: #9B1C1C;
  --color-danger-500: #EF4444;
  --color-danger-100: #FEE2E2;

  --color-warning-700: #92400E;
  --color-warning-500: #F59E0B;
  --color-warning-100: #FEF3C7;

  /* Neutrals — the backbone of the interface */
  --color-neutral-950: #030712;   /* Near-black text */
  --color-neutral-900: #111827;
  --color-neutral-700: #374151;   /* Secondary text */
  --color-neutral-500: #6B7280;   /* Placeholder, helper text */
  --color-neutral-300: #D1D5DB;   /* Borders */
  --color-neutral-200: #E5E7EB;   /* Dividers */
  --color-neutral-100: #F3F4F6;   /* Table zebra-stripe */
  --color-neutral-050: #F9FAFB;   /* Page background */
  --color-white:       #FFFFFF;

  /* Semantic Tokens (use THESE in components) */
  --bg-page:          var(--color-neutral-050);
  --bg-surface:       var(--color-white);
  --bg-surface-raised: var(--color-white);   /* cards, modals */
  --bg-muted:         var(--color-neutral-100);

  --text-primary:     var(--color-neutral-950);
  --text-secondary:   var(--color-neutral-700);
  --text-muted:       var(--color-neutral-500);
  --text-inverse:     var(--color-white);
  --text-link:        var(--color-primary-600);

  --border-default:   var(--color-neutral-300);
  --border-focus:     var(--color-primary-400);
  --border-error:     var(--color-danger-500);

  --interactive-default: var(--color-primary-600);
  --interactive-hover:   var(--color-primary-500);
  --interactive-active:  var(--color-primary-700);

  /* Financial-specific semantic colors */
  --financial-positive: var(--color-success-700);   /* Income, gains */
  --financial-negative: var(--color-danger-700);    /* Expenses, losses */
  --financial-neutral:  var(--color-neutral-700);   /* Zero, N/A */
}
```

**RULE:** Never hardcode color values in component CSS. Always use semantic tokens.

### 2.2 Typography

```css
:root {
  /* Font Families */
  --font-ui:       'Inter', 'Segoe UI', system-ui, sans-serif;    /* All UI labels, nav, buttons */
  --font-data:     'IBM Plex Mono', 'Fira Code', monospace;       /* Numbers, amounts, codes */
  --font-heading:  'Inter', sans-serif;                            /* Page headings */

  /* Scale (use only these sizes — no arbitrary values) */
  --text-xs:   11px;   /* Fine print, table footnotes */
  --text-sm:   12px;   /* Table cell data, helper text */
  --text-base: 14px;   /* Default body, form labels */
  --text-md:   15px;   /* Primary reading text */
  --text-lg:   16px;   /* Section headers, card titles */
  --text-xl:   20px;   /* Page titles */
  --text-2xl:  24px;   /* Dashboard metric headers */
  --text-3xl:  30px;   /* Hero numbers (total balance, etc) */
  --text-4xl:  36px;   /* Main KPI displays */

  /* Weight */
  --weight-regular: 400;
  --weight-medium:  500;
  --weight-semibold: 600;
  --weight-bold:    700;

  /* Line Heights */
  --leading-tight:  1.25;   /* Headings */
  --leading-snug:   1.375;  /* Labels */
  --leading-normal: 1.5;    /* Body */
  --leading-relaxed: 1.625; /* Help text */
}
```

**Critical Typography Rules:**
1. **All monetary amounts** → `font-family: var(--font-data)` — monospace prevents column misalignment
2. **All account codes / IDs** → `font-family: var(--font-data)` — never UI font for codes
3. **Negative amounts** → `color: var(--financial-negative)` — color alone is not enough; also use `(amount)` parentheses notation for accessibility
4. **Positive amounts** — default text color; **do NOT** always show green — only show green for net gain/profit context
5. **Never bold an entire table column** — bold is a signal word; overuse kills its meaning

### 2.3 Spacing System

```css
:root {
  /* Base unit: 4px */
  --space-1:  4px;
  --space-2:  8px;
  --space-3:  12px;
  --space-4:  16px;
  --space-5:  20px;
  --space-6:  24px;
  --space-8:  32px;
  --space-10: 40px;
  --space-12: 48px;
  --space-16: 64px;
  --space-20: 80px;
}
```

**Usage Guide:**
- **Between related items (label → input):** `--space-2` (8px)
- **Between form fields:** `--space-5` (20px)
- **Between sections on a page:** `--space-10` (40px)
- **Card internal padding:** `--space-6` (24px)
- **Table cell padding:** `--space-3` vertical, `--space-4` horizontal
- **Page content padding:** `--space-8` (32px) sides

### 2.4 Elevation & Shadows

```css
:root {
  --shadow-none: none;
  --shadow-sm:   0 1px 2px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.10);
  --shadow-md:   0 4px 6px rgba(0,0,0,0.05), 0 2px 4px rgba(0,0,0,0.08);
  --shadow-lg:   0 10px 15px rgba(0,0,0,0.07), 0 4px 6px rgba(0,0,0,0.05);
  --shadow-xl:   0 20px 25px rgba(0,0,0,0.10), 0 8px 10px rgba(0,0,0,0.06);

  /* Elevation map */
  /* Level 0 — Page background: no shadow */
  /* Level 1 — Cards, panels: shadow-sm */
  /* Level 2 — Dropdowns, tooltips: shadow-md */
  /* Level 3 — Modals, drawers: shadow-xl */
  /* Level 4 — Toast notifications: shadow-lg + fixed position */
}
```

### 2.5 Border Radius

```css
:root {
  --radius-sm:   4px;   /* Tags, badges, small chips */
  --radius-md:   6px;   /* Inputs, buttons, small cards */
  --radius-lg:   8px;   /* Panels, cards */
  --radius-xl:   12px;  /* Modals, large containers */
  --radius-full: 9999px; /* Pills, avatar circles */
}
```

---

## 3. LAYOUT ARCHITECTURE

### 3.1 App Shell Structure

```
┌─────────────────────────────────────────────────────────────────┐
│  TOP BAR (height: 56px, fixed)                                  │
│  [Logo]  [Company Selector ▼]        [Search]  [Bell] [Avatar] │
├──────────────┬──────────────────────────────────────────────────┤
│              │                                                   │
│  SIDE NAV    │   MAIN CONTENT AREA                              │
│  (240px)     │   (flex-1, scrollable)                           │
│  fixed       │                                                   │
│              │   ┌─ BREADCRUMB BAR (40px) ─────────────────┐   │
│              │   │  Home > Invoices > #INV-2024-0042        │   │
│              │   └──────────────────────────────────────────┘   │
│              │                                                   │
│              │   ┌─ PAGE CONTENT ──────────────────────────┐   │
│              │   │  padding: 32px                           │   │
│              │   │                                          │   │
│              │   └──────────────────────────────────────────┘   │
│              │                                                   │
└──────────────┴──────────────────────────────────────────────────┘
```

**Implementation:**
```css
.app-shell {
  display: grid;
  grid-template-columns: 240px 1fr;
  grid-template-rows: 56px 1fr;
  height: 100vh;
  overflow: hidden;
}

.top-bar {
  grid-column: 1 / -1;
  position: sticky;
  top: 0;
  z-index: 100;
  background: var(--color-primary-900);
  border-bottom: 1px solid var(--color-primary-800);
}

.side-nav {
  background: var(--bg-surface);
  border-right: 1px solid var(--border-default);
  overflow-y: auto;
  overflow-x: hidden;
}

.main-content {
  overflow-y: auto;
  background: var(--bg-page);
}
```

### 3.2 Responsive Breakpoints

```css
/* Accounting apps are primarily desktop — optimize for desktop first */
--bp-sm:  640px;    /* Small tablets — collapse to mobile nav */
--bp-md:  768px;    /* Tablets portrait */
--bp-lg:  1024px;   /* Laptops — minimum supported for full layout */
--bp-xl:  1280px;   /* Standard office monitors */
--bp-2xl: 1536px;   /* Wide monitors — expand content width */
```

**Behavior at breakpoints:**
- `< 1024px`: Side nav collapses to icon-only (48px wide) with tooltip labels
- `< 768px`: Side nav becomes bottom drawer, triggered by hamburger in top bar
- `>= 1536px`: Main content max-width: 1400px, centered

### 3.3 Content Grid

```css
.page-content {
  padding: var(--space-8);
  max-width: 1400px;
}

/* 12-column grid for page layouts */
.content-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: var(--space-6);
}

/* Common layouts */
.col-full        { grid-column: span 12; }
.col-8           { grid-column: span 8; }
.col-4           { grid-column: span 4; }
.col-6           { grid-column: span 6; }
.col-3           { grid-column: span 3; }

/* Dashboard KPI cards — always 4 per row */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-5);
}
```

---

## 4. NAVIGATION SYSTEM

### 4.1 Top Bar Specification

```
Height: 56px
Background: --color-primary-900 (dark navy)
Text: --color-white

Left section:
  - Logo: 32x32px icon + company name text (font-weight: 600, text-lg)
  - Company selector dropdown: shows current company name, chevron icon
    Width: 200px max, truncate overflow

Center section:
  - Global search bar: 480px wide, rounded-full
  - Placeholder: "Search transactions, contacts, accounts..."
  - Keyboard shortcut hint: Cmd/Ctrl+K shown inside input

Right section (icon group, gap: 8px):
  - Notifications bell: badge with count (red dot if unread > 0)
  - Help (?) icon
  - Avatar: 32px circle, initials or photo
    - On click: dropdown with Profile, Settings, Switch Company, Logout
```

### 4.2 Side Navigation Structure

```
Side Nav Width: 240px
Background: --bg-surface (white)

Top section: logo area (56px height — aligns with top bar)

Navigation groups:
  Each group has a category label (text-xs, uppercase, letter-spacing: 0.08em, color: --text-muted)
  Followed by nav items (height: 40px each)

Group 1: OVERVIEW
  - Dashboard
  - Reports (expandable)
    - Profit & Loss
    - Balance Sheet
    - Cash Flow
    - Trial Balance
    - Aged Receivables
    - Aged Payables

Group 2: TRANSACTIONS
  - Journal Entries
  - Bank Transactions
  - Reconciliation

Group 3: RECEIVABLES
  - Invoices
  - Customers
  - Receipts

Group 4: PAYABLES
  - Bills
  - Vendors
  - Payments

Group 5: ACCOUNTING
  - Chart of Accounts
  - Tax Rates
  - Fiscal Years

Group 6: SETTINGS (bottom, pinned)
  - Company Settings
  - Users & Roles
  - Integrations
```

**Nav Item Styling:**
```css
.nav-item {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-2) var(--space-4);
  border-radius: var(--radius-md);
  margin: 2px var(--space-2);
  cursor: pointer;
  color: var(--text-secondary);
  font-size: var(--text-sm);
  font-weight: var(--weight-medium);
  transition: background 150ms ease, color 150ms ease;
}

.nav-item:hover {
  background: var(--bg-muted);
  color: var(--text-primary);
}

.nav-item.active {
  background: var(--color-primary-050);
  color: var(--color-primary-700);
  font-weight: var(--weight-semibold);
}

.nav-item.active .nav-icon {
  color: var(--color-primary-600);
}
```

---

## 5. COMPONENT LIBRARY

### 5.1 Buttons

**Variants:**
```css
/* PRIMARY — main actions (Save, Submit, Create) */
.btn-primary {
  background: var(--interactive-default);
  color: var(--text-inverse);
  border: 1px solid transparent;
}
.btn-primary:hover { background: var(--interactive-hover); }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

/* SECONDARY — secondary actions (Cancel, Back, Export) */
.btn-secondary {
  background: var(--bg-surface);
  color: var(--text-primary);
  border: 1px solid var(--border-default);
}
.btn-secondary:hover { background: var(--bg-muted); }

/* DANGER — destructive actions (Delete, Void, Reverse) */
.btn-danger {
  background: var(--color-danger-500);
  color: white;
  border: 1px solid transparent;
}
.btn-danger:hover { background: var(--color-danger-700); }

/* GHOST — low-emphasis actions (Edit inline, minor actions) */
.btn-ghost {
  background: transparent;
  color: var(--text-link);
  border: none;
  padding-left: 0; padding-right: 0;
}
.btn-ghost:hover { text-decoration: underline; }
```

**Sizes:**
```css
/* All buttons share */
.btn {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  font-family: var(--font-ui);
  font-weight: var(--weight-medium);
  border-radius: var(--radius-md);
  cursor: pointer;
  white-space: nowrap;
  transition: all 150ms ease;
}

.btn-sm   { padding: 6px var(--space-3); font-size: var(--text-sm); height: 32px; }
.btn-md   { padding: 9px var(--space-4); font-size: var(--text-base); height: 38px; } /* default */
.btn-lg   { padding: 11px var(--space-5); font-size: var(--text-md); height: 44px; }
```

**RULES:**
1. Never place two primary buttons side by side — one action is always primary
2. Destructive actions (Delete, Void) always require a confirmation dialog before execution
3. Loading state: replace button content with spinner + "Saving..." text; disable button
4. Icon-only buttons always have `aria-label` and tooltip on hover

### 5.2 Form Inputs

```css
.input-field {
  width: 100%;
  height: 38px;
  padding: var(--space-2) var(--space-3);
  font-family: var(--font-ui);
  font-size: var(--text-base);
  color: var(--text-primary);
  background: var(--bg-surface);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-md);
  transition: border-color 150ms, box-shadow 150ms;
}

.input-field:hover { border-color: var(--color-neutral-400); }

.input-field:focus {
  outline: none;
  border-color: var(--border-focus);
  box-shadow: 0 0 0 3px rgba(74, 174, 224, 0.15);
}

.input-field.error {
  border-color: var(--border-error);
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.12);
}

/* AMOUNT INPUTS — always monospace, right-aligned */
.input-amount {
  font-family: var(--font-data);
  text-align: right;
  letter-spacing: -0.01em;
}
```

**Form Field Structure (always use this structure):**
```html
<div class="form-field">
  <label class="form-label" for="field-id">
    Invoice Date
    <span class="required-indicator" aria-hidden="true">*</span>
  </label>
  <input 
    class="input-field" 
    id="field-id"
    type="text"
    placeholder="DD/MM/YYYY"
    aria-required="true"
    aria-describedby="field-id-hint field-id-error"
  />
  <p class="form-hint" id="field-id-hint">The date the invoice was issued.</p>
  <p class="form-error" id="field-id-error" role="alert" aria-live="polite">
    <!-- Error message appears here only on validation failure -->
  </p>
</div>
```

```css
.form-label {
  display: block;
  font-size: var(--text-sm);
  font-weight: var(--weight-medium);
  color: var(--text-primary);
  margin-bottom: var(--space-2);
}

.required-indicator {
  color: var(--color-danger-500);
  margin-left: 2px;
}

.form-hint {
  font-size: var(--text-xs);
  color: var(--text-muted);
  margin-top: var(--space-1);
}

.form-error {
  font-size: var(--text-xs);
  color: var(--color-danger-700);
  margin-top: var(--space-1);
  display: none; /* shown via JS when error state */
}
```

### 5.3 Data Tables

**This is the most critical component in accounting apps. Get it right.**

```css
.data-table-wrapper {
  background: var(--bg-surface);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-lg);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: var(--text-sm);
}

/* HEADER ROW */
.data-table thead th {
  background: var(--color-neutral-050);
  padding: var(--space-3) var(--space-4);
  text-align: left;
  font-size: var(--text-xs);
  font-weight: var(--weight-semibold);
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  border-bottom: 1px solid var(--border-default);
  white-space: nowrap;
  user-select: none;
}

/* Numeric column headers: right-align */
.data-table thead th.col-numeric {
  text-align: right;
}

/* Sortable column headers */
.data-table thead th.sortable {
  cursor: pointer;
}
.data-table thead th.sortable:hover {
  background: var(--color-neutral-100);
  color: var(--text-primary);
}
.data-table thead th.sort-active {
  color: var(--color-primary-600);
}

/* DATA ROWS */
.data-table tbody tr {
  border-bottom: 1px solid var(--color-neutral-100);
  transition: background 100ms;
}

.data-table tbody tr:last-child { border-bottom: none; }

/* Zebra striping — subtle */
.data-table tbody tr:nth-child(even) {
  background: var(--color-neutral-050);
}

.data-table tbody tr:hover {
  background: var(--color-primary-050);
  cursor: pointer;
}

/* Selected row */
.data-table tbody tr.selected {
  background: var(--color-primary-100);
  outline: 1px solid var(--color-primary-400);
  outline-offset: -1px;
}

/* CELLS */
.data-table tbody td {
  padding: var(--space-3) var(--space-4);
  color: var(--text-primary);
  vertical-align: middle;
}

/* Amount cells */
.data-table tbody td.col-amount {
  font-family: var(--font-data);
  font-size: var(--text-sm);
  text-align: right;
  white-space: nowrap;
}

.col-amount.positive { color: var(--financial-positive); }
.col-amount.negative { color: var(--financial-negative); }

/* TOTAL / SUMMARY ROWS */
.data-table tfoot tr {
  background: var(--color-neutral-100);
  font-weight: var(--weight-semibold);
  border-top: 2px solid var(--border-default);
}

.data-table tfoot td {
  padding: var(--space-3) var(--space-4);
  font-family: var(--font-data);
  font-size: var(--text-base);
}
```

**Table Toolbar (above table):**
```html
<div class="table-toolbar">
  <!-- Left: bulk actions / selection info -->
  <div class="table-toolbar-left">
    <span class="selection-count">3 items selected</span>  <!-- shown when selected > 0 -->
    <button class="btn btn-sm btn-secondary">Mark as Paid</button>
    <button class="btn btn-sm btn-danger">Delete</button>
    <!-- OR when nothing selected: -->
    <span class="result-count">Showing 1–50 of 1,247 invoices</span>
  </div>
  <!-- Right: search + filters + export -->
  <div class="table-toolbar-right">
    <input class="input-field input-search" placeholder="Search..." />
    <button class="btn btn-sm btn-secondary">Filters</button>
    <button class="btn btn-sm btn-secondary">Export ↓</button>
    <button class="btn btn-sm btn-primary">+ New Invoice</button>
  </div>
</div>
```

### 5.4 Status Badges

```css
.badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 2px 8px;
  font-size: var(--text-xs);
  font-weight: var(--weight-medium);
  border-radius: var(--radius-full);
  white-space: nowrap;
}

/* Status dot (before text) */
.badge::before {
  content: '';
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
}

/* Accounting-specific statuses */
.badge-draft    { background: var(--color-neutral-100);   color: var(--color-neutral-700); }
.badge-pending  { background: var(--color-warning-100);   color: var(--color-warning-700); }
.badge-approved { background: var(--color-primary-100);   color: var(--color-primary-700); }
.badge-paid     { background: var(--color-success-100);   color: var(--color-success-700); }
.badge-overdue  { background: var(--color-danger-100);    color: var(--color-danger-700);  }
.badge-voided   { background: var(--color-neutral-100);   color: var(--color-neutral-500);
                  text-decoration: line-through; }
.badge-partial  { background: #FEF3C7;                    color: #92400E; }
```

### 5.5 Cards / KPI Widgets

```css
.card {
  background: var(--bg-surface);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  padding: var(--space-6);
}

/* KPI Card for Dashboard */
.kpi-card {
  position: relative;
  overflow: hidden;
}

.kpi-card .kpi-label {
  font-size: var(--text-sm);
  font-weight: var(--weight-medium);
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: var(--space-2);
}

.kpi-card .kpi-value {
  font-family: var(--font-data);
  font-size: var(--text-4xl);
  font-weight: var(--weight-bold);
  color: var(--text-primary);
  line-height: var(--leading-tight);
  letter-spacing: -0.03em;
}

.kpi-card .kpi-change {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: var(--text-sm);
  margin-top: var(--space-2);
}

.kpi-change.up   { color: var(--financial-positive); }
.kpi-change.down { color: var(--financial-negative); }
```

### 5.6 Modals & Dialogs

```css
/* Overlay */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(3, 7, 18, 0.5);
  backdrop-filter: blur(2px);
  z-index: 500;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-8);
}

/* Modal sizes */
.modal-sm  { width: 400px; }
.modal-md  { width: 600px; }   /* default */
.modal-lg  { width: 800px; }
.modal-xl  { width: 1100px; }  /* Full invoice form */
.modal-full { width: 95vw; max-height: 95vh; }

.modal {
  background: var(--bg-surface);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-xl);
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-5) var(--space-6);
  border-bottom: 1px solid var(--border-default);
}

.modal-body {
  overflow-y: auto;
  padding: var(--space-6);
  flex: 1;
}

.modal-footer {
  padding: var(--space-4) var(--space-6);
  border-top: 1px solid var(--border-default);
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3);
  background: var(--color-neutral-050);
}
```

**RULES for Modals:**
1. `Escape` key always closes modal (unless unsaved changes — then show "Discard changes?" confirm)
2. Clicking overlay closes modal (same rule as Escape)
3. Focus is trapped inside modal (Tab cycles within modal only)
4. On open: focus the first interactive element
5. On close: return focus to the element that triggered the modal
6. Confirmation dialogs (delete, void): max-width 400px, centered text, red danger button

### 5.7 Toast Notifications

```css
.toast-container {
  position: fixed;
  bottom: var(--space-6);
  right: var(--space-6);
  z-index: 1000;
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-3) var(--space-4);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  background: var(--color-neutral-900);
  color: white;
  font-size: var(--text-sm);
  max-width: 380px;
  pointer-events: all;
  animation: slide-in 250ms ease;
}

.toast-success { border-left: 4px solid var(--color-success-500); }
.toast-error   { border-left: 4px solid var(--color-danger-500); }
.toast-warning { border-left: 4px solid var(--color-warning-500); }
.toast-info    { border-left: 4px solid var(--color-primary-400); }
```

**Toast behavior rules:**
- Success toasts: auto-dismiss after 4 seconds
- Error toasts: do NOT auto-dismiss — user must close
- Always show max 4 toasts at once (stack from bottom)
- Include "Undo" action in toast where applicable (e.g., after deleting a record)

---

## 6. KEY PAGE DESIGNS

### 6.1 Dashboard Page

**Layout:**
```
┌─ Page Header ──────────────────────────────────────────────┐
│  Dashboard                        [Date Range ▼] [Refresh] │
└────────────────────────────────────────────────────────────┘

┌─ KPI Row (4 cards) ────────────────────────────────────────┐
│  [Revenue]    [Expenses]    [Net Profit]    [Cash Balance]  │
└────────────────────────────────────────────────────────────┘

┌─ Charts Row (8+4 split) ───────────────────────────────────┐
│  Revenue vs Expenses Chart (line/bar)  │  Cash Flow Chart  │
└────────────────────────────────────────────────────────────┘

┌─ Bottom Row (6+6) ─────────────────────────────────────────┐
│  Outstanding Invoices               │  Recent Transactions  │
│  (top 5 overdue, with quick Pay)    │  (last 10 entries)    │
└────────────────────────────────────────────────────────────┘
```

**KPI Card Content:**
| Card | Label | Format |
|------|-------|--------|
| Revenue | "Revenue (This Month)" | $1,234,567.89 |
| Expenses | "Expenses (This Month)" | $456,789.00 |
| Net Profit | "Net Profit" | $777,778.89 + % margin |
| Cash Balance | "Cash Balance" | $2,100,000.00 |

### 6.2 Invoice List Page

**Toolbar:**
```
[Search invoices...]   [Status ▼]  [Customer ▼]  [Date Range ▼]  [Clear Filters]    [Export ↓] [+ New Invoice]
```

**Table columns (in order):**
| Column | Width | Alignment | Notes |
|--------|-------|-----------|-------|
| Checkbox | 40px | center | Bulk select |
| Invoice # | 120px | left | Monospace, link |
| Customer | 200px | left | Truncate |
| Issue Date | 110px | left | DD/MM/YYYY |
| Due Date | 110px | left | Red if overdue |
| Status | 110px | center | Badge |
| Amount | 130px | right | Monospace |
| Amount Due | 130px | right | Monospace, red if >0 |
| Actions | 80px | center | ••• menu |

**Row actions (••• dropdown):**
- View / Edit
- Record Payment
- Send Reminder
- Duplicate
- Void Invoice
- Download PDF

### 6.3 Invoice Detail / Form Page

**This is the most complex form. Use a two-column layout for wider screens:**

```
┌─ Page Header ─────────────────────────────────────────────────────┐
│  ← Back to Invoices     Invoice #INV-2024-0042     [DRAFT badge]  │
│                                               [Save Draft] [Send] │
└───────────────────────────────────────────────────────────────────┘

┌─ Main Form (col-8) ──────────────────┐ ┌─ Summary (col-4) ───────┐
│                                       │ │                          │
│  [Customer *]                         │ │  Subtotal   $10,000.00   │
│  [Invoice Date *]  [Due Date *]       │ │  Tax (10%)   $1,000.00   │
│  [Invoice Number]  [Reference]        │ │  ─────────────────────   │
│                                       │ │  Total      $11,000.00   │
│  ── Line Items ───────────────────    │ │                          │
│  [Item][Description][Qty][Rate][Tax]  │ │  Amount Due $11,000.00   │
│  Row 1  Product A       1  $5000      │ │                          │
│  Row 2  Consulting      2  $2500      │ │  [Send Invoice]          │
│  [+ Add Item]           ──────────    │ │  [Save as Draft]         │
│                    Subtotal $10,000   │ │  [Preview PDF]           │
│                                       │ └──────────────────────────┘
│  [Notes]  [Terms]  [Attachments]      │
└───────────────────────────────────────┘
```

**Line Item Table Rules:**
- Last row is always an empty input row (pre-focused for entry)
- Tab key moves to next cell within row; at last cell, creates new row
- Delete row: show × on hover (never on focus, to avoid accidental deletion)
- Amounts auto-calculate on quantity or rate change
- Tax dropdown per line item

### 6.4 Chart of Accounts Page

```
Toolbar: [Search accounts...]  [Account Type ▼]  [+ Add Account]

Table:
  Code | Account Name         | Type        | Sub-type     | Balance      | Active
  1000 | Cash and Equivalents | Asset       | Current      | $250,000.00  | ●
  1100 | Accounts Receivable  | Asset       | Current      | $88,000.00   | ●
  2000 | Accounts Payable     | Liability   | Current      | ($42,000.00) | ●
```

**Hierarchical display (parent/child accounts):**
- Parent accounts: bold, no indent
- Child accounts: 20px left indent, normal weight
- Collapse/expand toggle (▶ ▼) on parent rows

### 6.5 Journal Entry Form

```
┌─────────────────────────────────────────────────────────────────┐
│  New Journal Entry                                               │
│                                                                  │
│  Date: [____]  Reference: [____]  Description: [____________]   │
│                                                                  │
│  ── Debit / Credit Lines ──────────────────────────────────────  │
│  Account               | Description      | Debit   | Credit    │
│  [Account selector  ▼] | [____________]   | [____]  |           │
│  [Account selector  ▼] | [____________]   |         | [____]    │
│  [+ Add line]                                                    │
│                                          ─────────  ─────────   │
│                              Totals:    $5,000.00  $5,000.00    │
│                                                                  │
│  ⚠  Difference: $0.00   ✓ Balanced                              │
│                                                                  │
│                              [Cancel]  [Save as Draft]  [Post]  │
└─────────────────────────────────────────────────────────────────┘
```

**Critical behavior:**
- Running total shown for Debit and Credit columns
- "Difference" indicator: green + checkmark if $0.00 (balanced); red + warning if not
- "Post" button disabled until entry is balanced
- Account selector: searchable dropdown with account code + name

---

## 7. INTERACTION PATTERNS

### 7.1 Inline Editing

For fields that appear frequently in lists (e.g., due date on invoice list):
```
Default state: Display value as text
Hover state: Subtle underline indicating editability
Click: Transform field to input in-place
Blur / Enter: Save immediately, show brief success flash
Escape: Cancel, revert to original value
```

### 7.2 Keyboard Shortcuts

```
Global:
  Cmd/Ctrl + K        → Open global search
  ?                   → Show keyboard shortcuts help

Navigation (when not in input):
  G then D            → Go to Dashboard
  G then I            → Go to Invoices
  G then B            → Go to Bills
  G then A            → Go to Accounts

List pages:
  N                   → New record
  /                   → Focus search bar
  J / K               → Navigate rows down / up
  Enter               → Open selected row
  Space               → Toggle row selection
  Shift+A             → Select all rows
  Escape              → Deselect all

Forms / Modals:
  Cmd/Ctrl + Enter    → Submit / Save
  Escape              → Close / Cancel
  Tab                 → Next field
  Shift + Tab         → Previous field
```

### 7.3 Empty States

Every list/table must have a thoughtful empty state (not just "No records found"):

```
┌─────────────────────────────────────┐
│                                     │
│        [Invoice icon, 64px]         │
│                                     │
│    No invoices yet                  │
│                                     │
│  Create your first invoice to       │
│  start tracking what you're owed.   │
│                                     │
│     [+ Create Invoice]              │
│                                     │
└─────────────────────────────────────┘
```

When a search/filter returns nothing:
```
[Search icon]
No results for "acme corp"
Try adjusting your search or filters.
[Clear Filters]
```

### 7.4 Loading States

| Context | Pattern |
|---------|---------|
| Page initial load | Skeleton screens (not spinner) |
| Table loading | Skeleton rows (5 rows, columns match table) |
| Button action | Button: spinner + "Saving..." text |
| Form submission | Disable all inputs + submit button |
| Data refresh | Subtle overlay on table with spinner |

**Skeleton pattern:**
```css
.skeleton {
  background: linear-gradient(
    90deg,
    var(--color-neutral-100) 25%,
    var(--color-neutral-200) 50%,
    var(--color-neutral-100) 75%
  );
  background-size: 400% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: var(--radius-sm);
}
@keyframes shimmer {
  0%   { background-position: 100% 0; }
  100% { background-position: -100% 0; }
}
```

### 7.5 Confirmation Dialogs

**Rule:** Never delete or void without a modal confirmation.

```
┌────────────────────────────────────────┐
│  Delete Invoice?                        │
│                                         │
│  This will permanently delete           │
│  Invoice #INV-2024-0042. This action    │
│  cannot be undone.                      │
│                                         │
│              [Cancel]  [Delete Invoice] │
└─────────────────────────────────────────┘
```

- Cancel: left-aligned or secondary button
- Destructive action: right-aligned, danger button, specific label ("Delete Invoice" not just "Delete")
- Danger confirmations: type the record name to confirm (optional, for bulk delete)

---

## 8. ACCESSIBILITY (A11Y)

### 8.1 Minimum Requirements (WCAG 2.1 AA)

| Requirement | Implementation |
|-------------|---------------|
| Color contrast (text) | 4.5:1 minimum for normal text |
| Color contrast (large text) | 3:1 minimum (≥18px regular or ≥14px bold) |
| Focus indicators | Visible focus ring on ALL interactive elements |
| Keyboard navigation | 100% keyboard operable |
| Form labels | Every input has a programmatic label |
| Error identification | Errors in text, not just color |
| Skip navigation | "Skip to main content" link as first element |

### 8.2 ARIA Implementation

```html
<!-- Tables with sortable columns -->
<th scope="col" aria-sort="ascending">Date ↑</th>
<th scope="col" aria-sort="none">Amount</th>

<!-- Status badges -->
<span class="badge-overdue" role="status" aria-label="Status: Overdue">Overdue</span>

<!-- Amount with currency -->
<td class="col-amount negative" aria-label="Amount: negative $1,250.00">
  ($1,250.00)
</td>

<!-- Loading states -->
<div role="status" aria-live="polite" aria-busy="true">
  <span class="sr-only">Loading invoices...</span>
</div>

<!-- Pagination -->
<nav aria-label="Pagination">
  <button aria-label="Previous page">←</button>
  <span aria-current="page" aria-label="Page 3 of 47">3</span>
  <button aria-label="Next page">→</button>
</nav>
```

### 8.3 Focus Management

```css
/* Visible focus ring — override browser default with custom design */
:focus-visible {
  outline: 2px solid var(--border-focus);
  outline-offset: 2px;
  border-radius: 2px;
}

/* Never use: outline: none without replacement */
:focus:not(:focus-visible) {
  outline: none; /* Safe: only removes for mouse users */
}
```

---

## 9. DATA FORMATTING

### 9.1 Currency Formatting Rules

```javascript
// Always use Intl.NumberFormat — never manual string concatenation
const formatCurrency = (amount, currency = 'USD', locale = 'en-US') => {
  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
};

// Negative amounts: two patterns based on context
// In financial statements: parentheses notation  → (1,250.00)
// In data tables: minus sign                     → -$1,250.00

const formatNegative = (amount) => {
  if (amount < 0) {
    return `(${formatCurrency(Math.abs(amount))})`;
  }
  return formatCurrency(amount);
};

// Zero values: display as $0.00, never blank
// Null/undefined values: display as "—" (em dash), never blank or zero
```

### 9.2 Date Formatting Rules

```javascript
// Always specify format explicitly — never rely on browser defaults
const formatDate = (date, format = 'display') => {
  const d = new Date(date);
  
  switch(format) {
    case 'display':   // For tables, labels
      return d.toLocaleDateString('en-US', { 
        year: 'numeric', month: 'short', day: '2-digit' 
      }); // → "Jan 15, 2024"
      
    case 'input':     // For HTML date inputs (always ISO)
      return d.toISOString().split('T')[0]; // → "2024-01-15"
      
    case 'relative':  // For "due in 3 days", "overdue by 5 days"
      const diff = Math.ceil((d - new Date()) / (1000 * 60 * 60 * 24));
      if (diff < 0) return `Overdue by ${Math.abs(diff)} day${Math.abs(diff)>1?'s':''}`;
      if (diff === 0) return 'Due today';
      return `Due in ${diff} day${diff>1?'s':''}`;
  }
};
```

### 9.3 Number Formatting

```javascript
// Large numbers in KPI cards
const formatKPI = (amount) => {
  if (Math.abs(amount) >= 1_000_000) {
    return `$${(amount / 1_000_000).toFixed(1)}M`;
  }
  if (Math.abs(amount) >= 1_000) {
    return `$${(amount / 1_000).toFixed(1)}K`;
  }
  return formatCurrency(amount);
};

// Percentages
const formatPercent = (value, decimals = 1) => {
  return `${value >= 0 ? '+' : ''}${value.toFixed(decimals)}%`;
};
```

---

## 10. ERROR HANDLING PATTERNS

### 10.1 Form Validation

**Rules:**
1. Validate on blur (not on keystroke — too aggressive)
2. Show all errors on submit attempt, not just first error
3. Error messages answer: "What went wrong and how do I fix it?"
4. Never say: "Invalid input" — always be specific

**Error message examples:**
| Bad | Good |
|-----|------|
| "Invalid date" | "Please enter a valid date (DD/MM/YYYY)" |
| "Required" | "Customer name is required" |
| "Error" | "Amount must be greater than 0" |
| "Invalid email" | "Enter a valid email address, e.g. name@company.com" |

### 10.2 API Error Handling

```
Network error toast:    "Connection lost. Check your internet and try again."
Server error toast:     "Something went wrong on our end. Try again in a moment."
Validation error:       Return to form, highlight fields with server-provided messages
Auth error (401):       Redirect to login with "Your session expired. Please sign in."
Not found (404):        Show 404 page with link back to previous section
Permission (403):       Show message: "You don't have permission to view this."
```

### 10.3 Optimistic Updates

For actions likely to succeed (status toggle, minor edits):
1. Update UI immediately
2. Send API request in background
3. If success: keep update (show brief success toast)
4. If failure: revert UI to previous state, show error toast with "Try again"

---

## 11. PERFORMANCE GUIDELINES

### 11.1 Table Performance

- **Virtual scrolling** for tables > 100 rows (react-virtual or similar)
- **Server-side pagination** for datasets > 500 records; default page size: 50
- **Debounce search inputs** by 300ms before firing API request
- **Memoize row components** — row re-renders only when its own data changes

### 11.2 Form Performance

- Lazy-load account selector options (search API on type, min 2 chars)
- Cache dropdown options in memory during session (accounts, tax rates, customers)
- Defer non-critical panels (attachments, notes) with lazy rendering

### 11.3 Loading Priority

```
1st priority (immediate): Top bar, side nav, page title, skeleton placeholders
2nd priority (< 500ms):   Critical data (KPIs, first page of table)
3rd priority (< 1500ms):  Charts, secondary data, counts
4th priority (deferred):  Export functions, help content, non-visible tabs
```

---

## 12. DESIGN TOKENS QUICK REFERENCE

Paste this into your design system's root:

```css
:root {
  /* Colors */
  --color-primary-900: #0F2D4A;
  --color-primary-700: #1E5080;
  --color-primary-600: #1D6FA4;
  --color-primary-500: #2589C8;
  --color-primary-400: #4AAEE0;
  --color-primary-100: #D6EDFB;
  --color-primary-050: #EDF6FD;
  --color-success-700: #166534;
  --color-success-500: #22C55E;
  --color-success-100: #DCFCE7;
  --color-danger-700:  #9B1C1C;
  --color-danger-500:  #EF4444;
  --color-danger-100:  #FEE2E2;
  --color-warning-700: #92400E;
  --color-warning-500: #F59E0B;
  --color-warning-100: #FEF3C7;
  --color-neutral-950: #030712;
  --color-neutral-900: #111827;
  --color-neutral-700: #374151;
  --color-neutral-500: #6B7280;
  --color-neutral-400: #9CA3AF;
  --color-neutral-300: #D1D5DB;
  --color-neutral-200: #E5E7EB;
  --color-neutral-100: #F3F4F6;
  --color-neutral-050: #F9FAFB;

  /* Semantic */
  --bg-page:            #F9FAFB;
  --bg-surface:         #FFFFFF;
  --bg-muted:           #F3F4F6;
  --text-primary:       #030712;
  --text-secondary:     #374151;
  --text-muted:         #6B7280;
  --text-inverse:       #FFFFFF;
  --text-link:          #1D6FA4;
  --border-default:     #D1D5DB;
  --border-focus:       #4AAEE0;
  --border-error:       #EF4444;
  --interactive-default: #1D6FA4;
  --interactive-hover:   #2589C8;
  --financial-positive:  #166534;
  --financial-negative:  #9B1C1C;

  /* Typography */
  --font-ui:   'Inter', system-ui, sans-serif;
  --font-data: 'IBM Plex Mono', monospace;
  --text-xs:   11px;  --text-sm:  12px;
  --text-base: 14px;  --text-md:  15px;
  --text-lg:   16px;  --text-xl:  20px;
  --text-2xl:  24px;  --text-3xl: 30px;
  --text-4xl:  36px;
  --weight-regular: 400; --weight-medium: 500;
  --weight-semibold: 600; --weight-bold: 700;

  /* Spacing */
  --space-1: 4px;  --space-2: 8px;   --space-3: 12px;
  --space-4: 16px; --space-5: 20px;  --space-6: 24px;
  --space-8: 32px; --space-10: 40px; --space-12: 48px;

  /* Borders */
  --radius-sm: 4px; --radius-md: 6px; --radius-lg: 8px;
  --radius-xl: 12px; --radius-full: 9999px;

  /* Shadows */
  --shadow-sm: 0 1px 2px rgba(0,0,0,.06), 0 1px 3px rgba(0,0,0,.10);
  --shadow-md: 0 4px 6px rgba(0,0,0,.05), 0 2px 4px rgba(0,0,0,.08);
  --shadow-lg: 0 10px 15px rgba(0,0,0,.07), 0 4px 6px rgba(0,0,0,.05);
  --shadow-xl: 0 20px 25px rgba(0,0,0,.10), 0 8px 10px rgba(0,0,0,.06);
}
```

---

## 13. THINGS TO NEVER DO

**Anti-patterns — if you see these in generated code, fix them:**

| ❌ Never | ✅ Instead |
|---------|----------|
| Hardcode hex colors in components | Use CSS variables |
| Use `outline: none` on focusable elements | Use `:focus-visible` with custom ring |
| Show dollar amounts in serif or default font | Always monospace for numbers |
| Use red for negative numbers only | Red color + parentheses notation |
| Have a table with no empty state | Design empty state for every table |
| Use "Submit" as button text | Use action-specific text: "Save Invoice", "Post Entry" |
| Allow delete without confirmation | Always show confirmation dialog |
| Auto-dismiss error toasts | Errors must be manually dismissed |
| Show raw API errors to users | Map errors to human-friendly messages |
| Use loading spinners for page loads | Use skeleton screens |
| Truncate currency amounts | Show full amount, let column scroll if needed |
| Use color alone to convey meaning | Color + icon + text always |
| Make modals uncloseable | Always allow Escape + overlay click to close |
| Nest modals inside modals | Use a different pattern (side drawer, accordion) |
| Build without keyboard support | Keyboard nav is required, not optional |

---

## 14. AI AGENT IMPLEMENTATION CHECKLIST

When implementing any page or component, verify:

**Design Tokens**
- [ ] Colors use CSS variables, no hardcoded hex
- [ ] Font families use defined variable names
- [ ] Spacing uses the 4px-base scale

**Typography**
- [ ] Numbers / amounts in monospace font
- [ ] Negative numbers in red + parentheses
- [ ] Account codes in monospace

**Tables**
- [ ] Monospace for amount columns
- [ ] Right-aligned numeric columns
- [ ] Zebra striping applied
- [ ] Hover state on rows
- [ ] Empty state designed
- [ ] Sortable columns have aria-sort
- [ ] Pagination or virtual scroll for large sets

**Forms**
- [ ] Every input has a label (not placeholder as label)
- [ ] Error messages are specific, not generic
- [ ] Required fields marked with asterisk + aria-required
- [ ] Amount inputs are right-aligned + monospace
- [ ] Form validates on blur, not on keypress

**Interactivity**
- [ ] All actions have loading states
- [ ] Destructive actions require confirmation
- [ ] Errors show toast (persistent, not auto-dismissed)
- [ ] Success shows toast (auto-dismissed, 4s)
- [ ] Keyboard shortcuts documented

**Accessibility**
- [ ] Focus ring visible on all interactive elements
- [ ] Aria labels on icon-only buttons
- [ ] Status messages use role="status" or aria-live
- [ ] Modal traps focus when open

---

*End of UI/UX Design Specification*  
*This document should be treated as the source of truth for all UI implementation decisions.*
