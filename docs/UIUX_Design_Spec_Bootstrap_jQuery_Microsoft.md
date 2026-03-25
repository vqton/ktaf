# UI/UX & Frontend Design Specification
## Accounting Web Application — Bootstrap 5 + jQuery + Microsoft Ecosystem
**Document Type:** AI Agent Implementation Guide  
**Stack:** Bootstrap 5 · jQuery 3.x · ASP.NET Core MVC / Razor Pages / Blazor  
**Author Role:** Senior UI/UX Designer (20 years experience)  
**Version:** 2.0  
**Audience:** AI Coding Agent / .NET Frontend Developer  

---

## 0. TECHNOLOGY STACK OVERVIEW

### 0.1 Recommended Full Stack (Microsoft Ecosystem)

| Layer | Technology | Version | Ghi chú |
|-------|-----------|---------|---------|
| **Backend** | ASP.NET Core | 8.0 LTS | Web API + MVC |
| **Frontend Framework** | Bootstrap | 5.3.x | Grid, components |
| **JS Runtime** | jQuery | 3.7.x | DOM, AJAX, events |
| **Language** | TypeScript | 5.x | Qua bundler, optional |
| **Bundler** | .NET Bundler (LibMan / Vite) | latest | Quản lý static assets |
| **Charts** | Chart.js | 4.x | Tích hợp tốt với jQuery |
| **Date Picker** | Flatpickr | 4.x | Nhẹ, đẹp, không phụ thuộc jQuery UI |
| **Select / Autocomplete** | Select2 | 4.1.x | Account selector, customer lookup |
| **Data Table** | DataTables.js | 2.x | Server-side paging, sorting |
| **Money Mask** | Cleave.js | 1.x | Tự động format số tiền khi nhập |
| **Toast / Alert** | Toastr.js | 2.x | Thay thế Bootstrap toast cho dễ dùng |
| **Validation** | jQuery Validate + Unobtrusive | 3.x | Tích hợp sẵn ASP.NET MVC |
| **Icons** | Bootstrap Icons | 1.11.x | SVG icons, không cần font riêng |
| **HTTP Client** | jQuery AJAX ($.ajax / $.get / $.post) | built-in | Gọi ASP.NET Web API |
| **Auth** | ASP.NET Core Identity | 8.x | Cookie auth hoặc JWT |
| **ORM** | Entity Framework Core | 8.x | Code-first migrations |
| **Report Export** | EPPlus (Excel) + iTextSharp (PDF) | latest | Server-side export |
| **Hosting** | Azure App Service | — | Windows hoặc Linux plan |
| **DB** | Azure SQL / SQL Server | 2022 | Full MSSQL |
| **Storage** | Azure Blob Storage | — | File đính kèm, PDF invoice |
| **CI/CD** | Azure DevOps Pipelines | — | Build + Deploy |

### 0.2 CDN Links (Dùng cho _Layout.cshtml)

```html
<!-- === CSS === -->
<!-- Bootstrap 5.3 -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css">
<!-- Bootstrap Icons -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css">
<!-- DataTables + Bootstrap 5 theme -->
<link rel="stylesheet" href="https://cdn.datatables.net/2.0.8/css/dataTables.bootstrap5.min.css">
<!-- Select2 + Bootstrap 5 theme -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css">
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2-bootstrap-5-theme@1.3.0/dist/select2-bootstrap-5-theme.min.css">
<!-- Flatpickr -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css">
<!-- Toastr -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/toastr@2.1.4/build/toastr.min.css">
<!-- Custom theme (luôn để cuối cùng) -->
<link rel="stylesheet" href="~/css/accounting-theme.css">

<!-- === JS (trước </body>) === -->
<!-- jQuery — phải load trước Bootstrap -->
<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
<!-- Bootstrap 5 Bundle (gồm Popper.js) -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<!-- DataTables -->
<script src="https://cdn.datatables.net/2.0.8/js/dataTables.min.js"></script>
<script src="https://cdn.datatables.net/2.0.8/js/dataTables.bootstrap5.min.js"></script>
<!-- Select2 -->
<script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
<!-- Flatpickr -->
<script src="https://cdn.jsdelivr.net/npm/flatpickr"></script>
<!-- Chart.js -->
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.3/dist/chart.umd.min.js"></script>
<!-- Cleave.js (money input mask) -->
<script src="https://cdn.jsdelivr.net/npm/cleave.js@1.6.0/dist/cleave.min.js"></script>
<!-- Toastr -->
<script src="https://cdn.jsdelivr.net/npm/toastr@2.1.4/build/toastr.min.js"></script>
<!-- jQuery Validate -->
<script src="https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/jquery-validation-unobtrusive@4.0.0/dist/jquery.validate.unobtrusive.min.js"></script>
<!-- App scripts -->
<script src="~/js/accounting-app.js"></script>
```

---

## 1. THIẾT LẬP CUSTOM THEME (accounting-theme.css)

File này ghi đè Bootstrap và định nghĩa Design Tokens. **Luôn import SAU Bootstrap.**

```css
/* ============================================================
   accounting-theme.css
   Custom theme trên Bootstrap 5 cho Accounting Webapp
   ============================================================ */

/* --- 1. DESIGN TOKENS (CSS Variables) --- */
:root {
  /* Brand Colors */
  --acc-primary-900: #0F2D4A;
  --acc-primary-800: #1A3F63;
  --acc-primary-700: #1E5080;
  --acc-primary-600: #1D6FA4;   /* màu chủ đạo interactive */
  --acc-primary-500: #2589C8;   /* hover */
  --acc-primary-400: #4AAEE0;   /* focus ring */
  --acc-primary-100: #D6EDFB;   /* selected row */
  --acc-primary-050: #EDF6FD;   /* subtle background */

  /* Semantic — Ghi đè Bootstrap vars */
  --bs-primary:        #1D6FA4;
  --bs-primary-rgb:    29, 111, 164;
  --bs-success:        #22C55E;
  --bs-danger:         #EF4444;
  --bs-warning:        #F59E0B;
  --bs-body-bg:        #F9FAFB;
  --bs-body-color:     #030712;
  --bs-border-color:   #D1D5DB;
  --bs-border-radius:  6px;
  --bs-box-shadow-sm:  0 1px 2px rgba(0,0,0,.06), 0 1px 3px rgba(0,0,0,.10);

  /* Accounting-specific */
  --acc-positive:      #166534;   /* Thu nhập, lãi */
  --acc-negative:      #9B1C1C;   /* Chi phí, lỗ */
  --acc-neutral:       #374151;

  /* Font */
  --acc-font-ui:       'Segoe UI', system-ui, sans-serif;     /* Microsoft standard */
  --acc-font-data:     'Cascadia Mono', 'Consolas', monospace; /* Microsoft monospace */

  /* Spacing scale (bội số 4px) */
  --acc-sp-1: 4px;   --acc-sp-2: 8px;   --acc-sp-3: 12px;
  --acc-sp-4: 16px;  --acc-sp-5: 20px;  --acc-sp-6: 24px;
  --acc-sp-8: 32px;
}

/* --- 2. BASE / RESET --- */
body {
  font-family: var(--acc-font-ui);
  font-size: 14px;
  background-color: var(--bs-body-bg);
  color: var(--bs-body-color);
}

/* Tất cả số tiền, mã tài khoản dùng monospace */
.font-mono,
.amount-cell,
.account-code,
.invoice-number {
  font-family: var(--acc-font-data);
  letter-spacing: -0.01em;
}

/* --- 3. OVERRIDE BOOTSTRAP PRIMARY COLOR --- */
.btn-primary {
  --bs-btn-bg: var(--acc-primary-600);
  --bs-btn-border-color: var(--acc-primary-600);
  --bs-btn-hover-bg: var(--acc-primary-500);
  --bs-btn-hover-border-color: var(--acc-primary-500);
  --bs-btn-active-bg: var(--acc-primary-700);
}

.btn-outline-primary {
  --bs-btn-color: var(--acc-primary-600);
  --bs-btn-border-color: var(--acc-primary-600);
  --bs-btn-hover-bg: var(--acc-primary-600);
}

/* Focus ring — dùng màu brand */
.btn:focus-visible,
.form-control:focus,
.form-select:focus {
  border-color: var(--acc-primary-400);
  box-shadow: 0 0 0 3px rgba(74, 174, 224, 0.2);
  outline: none;
}

/* --- 4. LAYOUT: APP SHELL --- */
.app-shell {
  display: grid;
  grid-template-columns: 240px 1fr;
  grid-template-rows: 56px 1fr;
  height: 100vh;
  overflow: hidden;
}

/* Top Bar */
#topbar {
  grid-column: 1 / -1;
  height: 56px;
  background-color: var(--acc-primary-900);
  display: flex;
  align-items: center;
  padding: 0 var(--acc-sp-5);
  gap: var(--acc-sp-4);
  z-index: 1030;
}

#topbar .brand-logo {
  color: white;
  font-size: 16px;
  font-weight: 600;
  text-decoration: none;
  display: flex;
  align-items: center;
  gap: var(--acc-sp-2);
  white-space: nowrap;
  min-width: 200px;
}

#topbar .global-search {
  flex: 1;
  max-width: 480px;
  margin: 0 auto;
}

#topbar .global-search .form-control {
  background: rgba(255,255,255,0.12);
  border-color: rgba(255,255,255,0.2);
  color: white;
  border-radius: 20px;
  font-size: 13px;
}

#topbar .global-search .form-control::placeholder { color: rgba(255,255,255,0.55); }
#topbar .global-search .form-control:focus {
  background: rgba(255,255,255,0.18);
  border-color: var(--acc-primary-400);
  color: white;
  box-shadow: none;
}

#topbar .topbar-actions {
  display: flex;
  align-items: center;
  gap: var(--acc-sp-2);
  margin-left: auto;
}

#topbar .topbar-icon-btn {
  color: rgba(255,255,255,0.8);
  background: transparent;
  border: none;
  padding: var(--acc-sp-1) var(--acc-sp-2);
  border-radius: 6px;
  font-size: 18px;
  cursor: pointer;
  transition: background 150ms;
}

#topbar .topbar-icon-btn:hover { background: rgba(255,255,255,0.12); color: white; }

/* Side Nav */
#sidenav {
  background: #fff;
  border-right: 1px solid var(--bs-border-color);
  overflow-y: auto;
  overflow-x: hidden;
  padding: var(--acc-sp-4) 0 var(--acc-sp-8);
}

.nav-section-label {
  padding: var(--acc-sp-4) var(--acc-sp-5) var(--acc-sp-1);
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: #9CA3AF;
}

.sidenav-link {
  display: flex;
  align-items: center;
  gap: var(--acc-sp-3);
  padding: var(--acc-sp-2) var(--acc-sp-4);
  margin: 1px var(--acc-sp-2);
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  color: #374151;
  text-decoration: none;
  transition: background 120ms, color 120ms;
}

.sidenav-link:hover {
  background: #F3F4F6;
  color: #111827;
}

.sidenav-link.active {
  background: var(--acc-primary-050);
  color: var(--acc-primary-700);
  font-weight: 600;
}

.sidenav-link .bi {
  font-size: 15px;
  flex-shrink: 0;
  width: 18px;
  text-align: center;
}

/* Submenu */
.sidenav-sub {
  padding-left: calc(var(--acc-sp-4) + 18px + var(--acc-sp-3));
  list-style: none;
  margin: 0;
}

.sidenav-sub .sidenav-link {
  font-size: 12.5px;
  padding: var(--acc-sp-1) var(--acc-sp-3);
}

/* Main content area */
#main-content {
  overflow-y: auto;
  background: var(--bs-body-bg);
}

/* --- 5. PAGE HEADER --- */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--acc-sp-5) var(--acc-sp-8);
  background: #fff;
  border-bottom: 1px solid var(--bs-border-color);
  min-height: 64px;
}

.page-header .page-title {
  font-size: 20px;
  font-weight: 600;
  color: var(--acc-primary-900);
  margin: 0;
}

.page-header .page-actions {
  display: flex;
  align-items: center;
  gap: var(--acc-sp-3);
}

.page-body {
  padding: var(--acc-sp-8);
  max-width: 1400px;
}

/* Breadcrumb */
.breadcrumb-bar {
  padding: var(--acc-sp-2) var(--acc-sp-8);
  background: #F9FAFB;
  border-bottom: 1px solid #E5E7EB;
  font-size: 12px;
}

.breadcrumb-bar .breadcrumb { margin: 0; }
.breadcrumb-bar .breadcrumb-item a { color: var(--acc-primary-600); text-decoration: none; }
.breadcrumb-bar .breadcrumb-item.active { color: #6B7280; }

/* --- 6. CARDS --- */
.acc-card {
  background: #fff;
  border: 1px solid var(--bs-border-color);
  border-radius: 8px;
  box-shadow: var(--bs-box-shadow-sm);
}

.acc-card .acc-card-header {
  padding: var(--acc-sp-4) var(--acc-sp-6);
  border-bottom: 1px solid var(--bs-border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #F9FAFB;
  border-radius: 8px 8px 0 0;
}

.acc-card .acc-card-header h5 {
  font-size: 14px;
  font-weight: 600;
  margin: 0;
  color: #111827;
}

.acc-card .acc-card-body { padding: var(--acc-sp-6); }

/* --- 7. KPI CARDS --- */
.kpi-card {
  background: #fff;
  border: 1px solid var(--bs-border-color);
  border-radius: 8px;
  padding: var(--acc-sp-6);
  box-shadow: var(--bs-box-shadow-sm);
  position: relative;
  overflow: hidden;
}

.kpi-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 3px;
  background: var(--acc-primary-600);
}

.kpi-card.kpi-success::before { background: #22C55E; }
.kpi-card.kpi-danger::before  { background: #EF4444; }
.kpi-card.kpi-warning::before { background: #F59E0B; }

.kpi-label {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #6B7280;
  margin-bottom: var(--acc-sp-2);
}

.kpi-value {
  font-family: var(--acc-font-data);
  font-size: 28px;
  font-weight: 700;
  color: #030712;
  line-height: 1.2;
  letter-spacing: -0.03em;
}

.kpi-change {
  font-size: 12px;
  margin-top: var(--acc-sp-2);
  display: flex;
  align-items: center;
  gap: 4px;
}

.kpi-change.up   { color: var(--acc-positive); }
.kpi-change.down { color: var(--acc-negative); }
.kpi-icon {
  position: absolute;
  right: var(--acc-sp-5);
  top: 50%;
  transform: translateY(-50%);
  font-size: 36px;
  color: var(--acc-primary-100);
}

/* --- 8. DATA TABLES (DataTables.js) --- */
.table-wrapper {
  background: #fff;
  border: 1px solid var(--bs-border-color);
  border-radius: 8px;
  overflow: hidden;
  box-shadow: var(--bs-box-shadow-sm);
}

.table-toolbar {
  padding: var(--acc-sp-4) var(--acc-sp-5);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--acc-sp-3);
  border-bottom: 1px solid var(--bs-border-color);
  flex-wrap: wrap;
}

.table-toolbar .search-input {
  width: 280px;
}

/* Override DataTables default styling */
table.dataTable thead th {
  font-size: 11px !important;
  font-weight: 600 !important;
  text-transform: uppercase !important;
  letter-spacing: 0.05em !important;
  color: #6B7280 !important;
  background: #F9FAFB !important;
  border-bottom: 1px solid var(--bs-border-color) !important;
  padding: 10px 14px !important;
  white-space: nowrap;
}

table.dataTable tbody td {
  padding: 10px 14px !important;
  font-size: 13px;
  vertical-align: middle !important;
  border-color: #F3F4F6 !important;
}

table.dataTable tbody tr:hover > td {
  background-color: var(--acc-primary-050) !important;
}

table.dataTable tbody tr.selected > td {
  background-color: var(--acc-primary-100) !important;
  box-shadow: inset 0 0 0 1px var(--acc-primary-400);
}

/* Amount columns */
.col-amount {
  font-family: var(--acc-font-data) !important;
  text-align: right !important;
  white-space: nowrap;
}

.amount-positive { color: var(--acc-positive) !important; }
.amount-negative { color: var(--acc-negative) !important; }

/* Total row */
.table-total-row td {
  font-family: var(--acc-font-data);
  font-weight: 600 !important;
  font-size: 14px !important;
  background: #F3F4F6 !important;
  border-top: 2px solid var(--bs-border-color) !important;
}

/* --- 9. BADGES --- */
.badge-draft    { background-color: #F3F4F6; color: #374151; }
.badge-pending  { background-color: #FEF3C7; color: #92400E; }
.badge-approved { background-color: var(--acc-primary-100); color: var(--acc-primary-700); }
.badge-paid     { background-color: #DCFCE7; color: #166534; }
.badge-overdue  { background-color: #FEE2E2; color: #9B1C1C; }
.badge-partial  { background-color: #FEF3C7; color: #92400E; }
.badge-voided   { background-color: #F3F4F6; color: #9CA3AF; text-decoration: line-through; }

.status-badge {
  padding: 2px 10px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 20px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  white-space: nowrap;
}

.status-badge::before {
  content: '';
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
  flex-shrink: 0;
}

/* --- 10. FORMS (Override Bootstrap) --- */
.form-label {
  font-size: 13px;
  font-weight: 500;
  color: #111827;
  margin-bottom: 5px;
}

.form-label .required { color: #EF4444; margin-left: 2px; }

.form-control, .form-select {
  font-size: 14px;
  border-color: var(--bs-border-color);
  border-radius: 6px;
  height: 38px;
  color: #030712;
}

.form-control:hover, .form-select:hover {
  border-color: #9CA3AF;
}

.form-text { font-size: 11px; color: #6B7280; }

/* Amount input - always right-align + monospace */
.form-control-amount {
  font-family: var(--acc-font-data);
  text-align: right;
  letter-spacing: -0.01em;
}

/* --- 11. MODALS (Bootstrap Modal override) --- */
.modal-header {
  padding: 16px 24px;
  border-bottom-color: var(--bs-border-color);
  background: #F9FAFB;
}

.modal-header .modal-title {
  font-size: 16px;
  font-weight: 600;
  color: #111827;
}

.modal-body { padding: 24px; }

.modal-footer {
  padding: 12px 24px;
  border-top-color: var(--bs-border-color);
  background: #F9FAFB;
}

/* Sizes — thêm vào .modal-dialog */
.modal-xs  { max-width: 380px; }  /* Confirmation dialogs */
.modal-xl  { max-width: 1140px; } /* Invoice form */

/* --- 12. SKELETON LOADING --- */
.skeleton {
  background: linear-gradient(
    90deg,
    #F3F4F6 25%,
    #E5E7EB 50%,
    #F3F4F6 75%
  );
  background-size: 400% 100%;
  animation: skeleton-shimmer 1.5s infinite;
  border-radius: 4px;
  display: inline-block;
}

@keyframes skeleton-shimmer {
  0%   { background-position: 100% 0; }
  100% { background-position: -100% 0; }
}

.skeleton-text { height: 14px; width: 100%; margin-bottom: 8px; }
.skeleton-text.w-50 { width: 50%; }
.skeleton-text.w-75 { width: 75%; }
.skeleton-amount { height: 28px; width: 120px; }

/* --- 13. EMPTY STATES --- */
.empty-state {
  text-align: center;
  padding: 64px var(--acc-sp-8);
  color: #6B7280;
}

.empty-state .empty-icon {
  font-size: 52px;
  color: #D1D5DB;
  margin-bottom: var(--acc-sp-4);
  display: block;
}

.empty-state h5 {
  font-size: 16px;
  font-weight: 600;
  color: #374151;
  margin-bottom: var(--acc-sp-2);
}

.empty-state p {
  font-size: 13px;
  max-width: 320px;
  margin: 0 auto var(--acc-sp-5);
}

/* --- 14. RESPONSIVE --- */
@media (max-width: 1024px) {
  .app-shell {
    grid-template-columns: 56px 1fr;
  }
  #sidenav .nav-section-label,
  #sidenav .sidenav-link span {
    display: none;
  }
  .sidenav-link {
    justify-content: center;
    padding: var(--acc-sp-3);
    margin: 2px var(--acc-sp-1);
  }
}

@media (max-width: 768px) {
  .app-shell {
    grid-template-columns: 1fr;
    grid-template-rows: 56px 1fr;
  }
  #sidenav {
    display: none; /* Replaced by offcanvas on mobile */
  }
  .page-body { padding: var(--acc-sp-4); }
  .kpi-value { font-size: 22px; }
}
```

---

## 2. LAYOUT HTML TEMPLATE (_Layout.cshtml)

```html
<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>@ViewData["Title"] - Accounting</title>
  <!-- CSS CDN links như phần 0.2 -->
  @RenderSection("Styles", required: false)
</head>
<body>

<div class="app-shell">

  <!-- TOP BAR -->
  <header id="topbar">
    <a href="/" class="brand-logo">
      <i class="bi bi-bar-chart-line-fill"></i>
      <span>AccSoft</span>
    </a>

    <!-- Company selector -->
    <div class="dropdown ms-3">
      <button class="btn btn-sm text-white border-0 dropdown-toggle" 
              style="background:rgba(255,255,255,.1)" 
              data-bs-toggle="dropdown">
        <i class="bi bi-building me-1"></i> 
        @ViewBag.CompanyName
      </button>
      <ul class="dropdown-menu dropdown-menu-dark">
        <!-- rendered by Razor -->
      </ul>
    </div>

    <!-- Global Search -->
    <div class="global-search d-none d-md-block">
      <input type="search" 
             class="form-control form-control-sm" 
             id="globalSearch"
             placeholder="Tìm kiếm... (Ctrl+K)">
    </div>

    <!-- Actions -->
    <div class="topbar-actions">
      <button class="topbar-icon-btn position-relative" title="Thông báo">
        <i class="bi bi-bell"></i>
        <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
              style="font-size:9px">3</span>
      </button>
      <button class="topbar-icon-btn" title="Trợ giúp">
        <i class="bi bi-question-circle"></i>
      </button>

      <!-- Avatar dropdown -->
      <div class="dropdown">
        <button class="topbar-icon-btn d-flex align-items-center gap-2 dropdown-toggle" 
                data-bs-toggle="dropdown"
                style="border: none">
          <span style="width:32px;height:32px;border-radius:50%;background:var(--acc-primary-600);
                       display:flex;align-items:center;justify-content:center;
                       font-size:13px;font-weight:600;color:white">
            @ViewBag.UserInitials
          </span>
        </button>
        <ul class="dropdown-menu dropdown-menu-end">
          <li><a class="dropdown-item" href="/profile"><i class="bi bi-person me-2"></i>Hồ sơ</a></li>
          <li><a class="dropdown-item" href="/settings"><i class="bi bi-gear me-2"></i>Cài đặt</a></li>
          <li><hr class="dropdown-divider"></li>
          <li>
            <form asp-action="Logout" asp-controller="Account" method="post">
              <button type="submit" class="dropdown-item text-danger">
                <i class="bi bi-box-arrow-right me-2"></i>Đăng xuất
              </button>
            </form>
          </li>
        </ul>
      </div>
    </div>
  </header>

  <!-- SIDE NAV -->
  <nav id="sidenav">
    <div class="nav-section-label">Tổng quan</div>
    <a href="/dashboard" class="sidenav-link @(currentPage == "dashboard" ? "active" : "")">
      <i class="bi bi-speedometer2"></i>
      <span>Dashboard</span>
    </a>

    <!-- Reports with collapse -->
    <a class="sidenav-link collapsed" data-bs-toggle="collapse" href="#reportsMenu">
      <i class="bi bi-file-earmark-bar-graph"></i>
      <span>Báo cáo</span>
      <i class="bi bi-chevron-down ms-auto" style="font-size:11px"></i>
    </a>
    <div class="collapse" id="reportsMenu">
      <ul class="sidenav-sub">
        <li><a href="/reports/pl" class="sidenav-link"><span>Lãi & Lỗ</span></a></li>
        <li><a href="/reports/balance-sheet" class="sidenav-link"><span>Bảng CĐKT</span></a></li>
        <li><a href="/reports/cashflow" class="sidenav-link"><span>Dòng tiền</span></a></li>
        <li><a href="/reports/trial-balance" class="sidenav-link"><span>Bảng cân đối TK</span></a></li>
      </ul>
    </div>

    <div class="nav-section-label">Giao dịch</div>
    <a href="/journal-entries" class="sidenav-link">
      <i class="bi bi-journal-text"></i><span>Bút toán</span>
    </a>
    <a href="/bank-transactions" class="sidenav-link">
      <i class="bi bi-bank"></i><span>Giao dịch ngân hàng</span>
    </a>
    <a href="/reconciliation" class="sidenav-link">
      <i class="bi bi-check2-all"></i><span>Đối chiếu</span>
    </a>

    <div class="nav-section-label">Phải thu</div>
    <a href="/invoices" class="sidenav-link">
      <i class="bi bi-receipt"></i><span>Hóa đơn bán</span>
    </a>
    <a href="/customers" class="sidenav-link">
      <i class="bi bi-people"></i><span>Khách hàng</span>
    </a>
    <a href="/receipts" class="sidenav-link">
      <i class="bi bi-cash-stack"></i><span>Thu tiền</span>
    </a>

    <div class="nav-section-label">Phải trả</div>
    <a href="/bills" class="sidenav-link">
      <i class="bi bi-file-earmark-text"></i><span>Hóa đơn mua</span>
    </a>
    <a href="/vendors" class="sidenav-link">
      <i class="bi bi-shop"></i><span>Nhà cung cấp</span>
    </a>
    <a href="/payments" class="sidenav-link">
      <i class="bi bi-credit-card"></i><span>Chi tiền</span>
    </a>

    <div class="nav-section-label">Kế toán</div>
    <a href="/chart-of-accounts" class="sidenav-link">
      <i class="bi bi-diagram-3"></i><span>Hệ thống tài khoản</span>
    </a>
    <a href="/tax-rates" class="sidenav-link">
      <i class="bi bi-percent"></i><span>Thuế suất</span>
    </a>
    <a href="/fiscal-years" class="sidenav-link">
      <i class="bi bi-calendar3"></i><span>Năm tài chính</span>
    </a>

    <!-- Settings pinned at bottom -->
    <div class="mt-auto pt-4">
      <hr class="mx-3">
      <a href="/settings" class="sidenav-link">
        <i class="bi bi-gear"></i><span>Cài đặt</span>
      </a>
      <a href="/users" class="sidenav-link">
        <i class="bi bi-person-lock"></i><span>Người dùng & Phân quyền</span>
      </a>
    </div>
  </nav>

  <!-- MAIN CONTENT -->
  <main id="main-content">
    <!-- Breadcrumb -->
    @await Html.PartialAsync("_Breadcrumb")

    <!-- Page content injected here -->
    @RenderBody()
  </main>

</div><!-- /.app-shell -->

<!-- JS CDN links như phần 0.2 -->
@RenderSection("Scripts", required: false)
</body>
</html>
```

---

## 3. JQUERY PLUGIN INITIALIZATION (accounting-app.js)

```javascript
// accounting-app.js
// Khởi tạo tất cả plugins và global behaviors

$(function () {

  // ============================================================
  // 1. TOASTR CONFIG
  // ============================================================
  toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: 'toast-bottom-right',
    timeOut: 4000,         // 4 giây cho success/info
    extendedTimeOut: 2000,
    preventDuplicates: true
  };

  // Hàm helper — dùng xuyên suốt app
  window.ACC = window.ACC || {};

  ACC.toast = {
    success: (msg) => toastr.success(msg),
    error:   (msg) => toastr.error(msg, null, { timeOut: 0 }),  // Error không tự đóng
    warning: (msg) => toastr.warning(msg),
    info:    (msg) => toastr.info(msg)
  };

  // ============================================================
  // 2. FLATPICKR — Date picker cho tất cả input date
  // ============================================================
  flatpickr('.date-picker', {
    dateFormat: 'd/m/Y',        // Hiển thị DD/MM/YYYY
    allowInput: true,
    locale: 'vn'
  });

  flatpickr('.date-range-picker', {
    mode: 'range',
    dateFormat: 'd/m/Y'
  });

  // ============================================================
  // 3. SELECT2 — Account selector, Customer lookup
  // ============================================================
  // Basic select
  $('.select2-basic').select2({
    theme: 'bootstrap-5',
    width: '100%'
  });

  // Account selector với search API (AJAX)
  $('.select2-accounts').select2({
    theme: 'bootstrap-5',
    width: '100%',
    minimumInputLength: 1,
    placeholder: 'Tìm tài khoản...',
    ajax: {
      url: '/api/accounts/search',
      dataType: 'json',
      delay: 250,
      data: (params) => ({ q: params.term }),
      processResults: (data) => ({
        results: data.map(acc => ({
          id: acc.id,
          text: `${acc.code} - ${acc.name}`
        }))
      })
    }
  });

  // Customer search
  $('.select2-customers').select2({
    theme: 'bootstrap-5',
    width: '100%',
    minimumInputLength: 1,
    placeholder: 'Tìm khách hàng...',
    ajax: {
      url: '/api/customers/search',
      dataType: 'json',
      delay: 250,
      data: (params) => ({ q: params.term }),
      processResults: (data) => ({
        results: data.map(c => ({ id: c.id, text: c.name }))
      })
    }
  });

  // ============================================================
  // 4. CLEAVE.JS — Money input formatting
  // ============================================================
  document.querySelectorAll('.input-money').forEach(el => {
    new Cleave(el, {
      numeral: true,
      numeralThousandsGroupStyle: 'thousand',
      numeralDecimalMark: '.',
      delimiter: ',',
      numeralDecimalScale: 2
    });
  });

  // ============================================================
  // 5. KEYBOARD SHORTCUTS
  // ============================================================
  $(document).on('keydown', function (e) {
    const ctrl = e.ctrlKey || e.metaKey;

    // Ctrl+K → Global search
    if (ctrl && e.key === 'k') {
      e.preventDefault();
      $('#globalSearch').focus().select();
    }

    // Ctrl+S → Save form (nếu có form đang focus)
    if (ctrl && e.key === 's') {
      const $activeForm = $('form.acc-form:visible').first();
      if ($activeForm.length) {
        e.preventDefault();
        $activeForm.find('[type=submit]').trigger('click');
      }
    }

    // Escape → Đóng modal mở nhất
    if (e.key === 'Escape') {
      // Bootstrap Modal đã tự xử lý, không cần override
    }
  });

  // ============================================================
  // 6. AJAX GLOBAL SETUP
  // ============================================================
  $.ajaxSetup({
    headers: {
      'X-Requested-With': 'XMLHttpRequest',
      'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
    }
  });

  // Global AJAX error handler
  $(document).ajaxError(function (event, xhr) {
    if (xhr.status === 401) {
      window.location.href = '/Account/Login';
    } else if (xhr.status === 403) {
      ACC.toast.error('Bạn không có quyền thực hiện thao tác này.');
    } else if (xhr.status === 0) {
      ACC.toast.error('Mất kết nối. Vui lòng kiểm tra internet.');
    } else if (xhr.status >= 500) {
      ACC.toast.error('Lỗi hệ thống. Vui lòng thử lại sau.');
    }
  });

  // ============================================================
  // 7. CONFIRMATION DIALOGS (dùng Bootstrap Modal)
  // ============================================================
  // Dùng: <button data-confirm="Bạn có muốn xóa?" data-url="/invoices/1" data-method="DELETE">
  $(document).on('click', '[data-confirm]', function (e) {
    e.preventDefault();
    const $btn = $(this);
    const message = $btn.data('confirm');
    const url     = $btn.data('url');
    const method  = $btn.data('method') || 'POST';
    const label   = $btn.data('confirm-label') || 'Xác nhận';

    $('#confirmModal .modal-body p').text(message);
    $('#confirmModal .btn-confirm-ok')
      .text(label)
      .off('click')
      .on('click', function () {
        $.ajax({ url, method, success: () => {
          bootstrap.Modal.getInstance('#confirmModal').hide();
          const redirect = $btn.data('redirect');
          if (redirect) window.location.href = redirect;
          else location.reload();
        }});
      });

    new bootstrap.Modal('#confirmModal').show();
  });

  // ============================================================
  // 8. FORM SUBMIT WITH AJAX
  // ============================================================
  // Dùng: <form class="acc-ajax-form" data-success-msg="Lưu thành công">
  $(document).on('submit', '.acc-ajax-form', function (e) {
    e.preventDefault();
    const $form = $(this);
    const $btn  = $form.find('[type=submit]');

    if (!$form.valid()) return;

    $btn.prop('disabled', true)
        .html('<span class="spinner-border spinner-border-sm me-2"></span>Đang lưu...');

    $.ajax({
      url: $form.attr('action'),
      method: $form.attr('method') || 'POST',
      data: $form.serialize(),
      success: function (res) {
        ACC.toast.success($form.data('success-msg') || 'Lưu thành công');
        const redirect = $form.data('redirect') || res.redirect;
        if (redirect) setTimeout(() => window.location.href = redirect, 800);
      },
      error: function (xhr) {
        const err = xhr.responseJSON;
        if (err && err.errors) {
          // Hiển thị lỗi validation từ server lên từng field
          $.each(err.errors, function (field, messages) {
            const $field = $form.find(`[name="${field}"]`);
            $field.addClass('is-invalid');
            $field.siblings('.invalid-feedback').text(messages[0]);
          });
        }
      },
      complete: function () {
        $btn.prop('disabled', false).html($btn.data('original-text') || 'Lưu');
      }
    });
  });

});

// ============================================================
// 9. DATATABLES HELPER
// ============================================================
ACC.initDataTable = function (selector, options) {
  const defaults = {
    processing: true,
    serverSide: true,
    language: {
      processing: '<div class="spinner-border spinner-border-sm text-primary"></div>',
      search: '',
      searchPlaceholder: 'Tìm kiếm...',
      lengthMenu: 'Hiển thị _MENU_ dòng',
      info: 'Hiển thị _START_–_END_ của _TOTAL_ bản ghi',
      paginate: {
        previous: '<i class="bi bi-chevron-left"></i>',
        next:     '<i class="bi bi-chevron-right"></i>'
      }
    },
    pageLength: 50,
    dom: '<"d-none"f>rt<"d-flex align-items-center justify-content-between p-3"ip>',
    columnDefs: [
      { className: 'col-amount', targets: '.col-amount' }
    ]
  };

  return $(selector).DataTable($.extend(true, defaults, options));
};

// ============================================================
// 10. CURRENCY FORMATTING HELPERS
// ============================================================
ACC.format = {
  currency: function (amount, currency = 'VND') {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency', currency,
      minimumFractionDigits: 0
    }).format(amount);
  },
  currencyUSD: function (amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency', currency: 'USD',
      minimumFractionDigits: 2
    }).format(amount);
  },
  negative: function (amount, currency = 'VND') {
    // Kế toán dùng dấu ngoặc cho âm: (1.250.000)
    if (amount < 0) {
      return `(${ACC.format.currency(Math.abs(amount), currency)})`;
    }
    return ACC.format.currency(amount, currency);
  },
  date: function (dateStr) {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('vi-VN', {
      day: '2-digit', month: '2-digit', year: 'numeric'
    });
  },
  percent: function (val, decimals = 1) {
    return `${val >= 0 ? '+' : ''}${val.toFixed(decimals)}%`;
  }
};
```

---

## 4. CÁC PAGE CHÍNH

### 4.1 Dashboard Page

```html
<!-- Views/Dashboard/Index.cshtml -->
@{
  ViewData["Title"] = "Dashboard";
}

<div class="page-header">
  <h1 class="page-title">Dashboard</h1>
  <div class="page-actions">
    <input type="text" class="form-control form-control-sm date-range-picker" 
           style="width:220px" placeholder="Chọn kỳ...">
    <button class="btn btn-sm btn-outline-secondary">
      <i class="bi bi-arrow-clockwise me-1"></i>Làm mới
    </button>
  </div>
</div>

<div class="page-body">

  <!-- KPI Row -->
  <div class="row g-4 mb-4">
    <div class="col-md-3">
      <div class="kpi-card">
        <i class="bi bi-arrow-up-circle kpi-icon text-success" style="color:#DCFCE7!important"></i>
        <div class="kpi-label">Doanh thu tháng này</div>
        <div class="kpi-value" id="kpiRevenue">—</div>
        <div class="kpi-change up" id="kpiRevenueChange">
          <i class="bi bi-arrow-up"></i> <span></span>
        </div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="kpi-card kpi-danger">
        <i class="bi bi-arrow-down-circle kpi-icon" style="color:#FEE2E2!important"></i>
        <div class="kpi-label">Chi phí tháng này</div>
        <div class="kpi-value" id="kpiExpenses">—</div>
        <div class="kpi-change down" id="kpiExpensesChange">
          <i class="bi bi-arrow-down"></i> <span></span>
        </div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="kpi-card kpi-success">
        <i class="bi bi-graph-up-arrow kpi-icon" style="color:#DCFCE7!important"></i>
        <div class="kpi-label">Lợi nhuận ròng</div>
        <div class="kpi-value" id="kpiProfit">—</div>
        <div class="kpi-change" id="kpiProfitChange"></div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="kpi-card">
        <i class="bi bi-wallet2 kpi-icon"></i>
        <div class="kpi-label">Số dư tiền mặt</div>
        <div class="kpi-value" id="kpiCash">—</div>
        <div class="kpi-change" id="kpiCashChange"></div>
      </div>
    </div>
  </div>

  <!-- Charts Row -->
  <div class="row g-4 mb-4">
    <div class="col-lg-8">
      <div class="acc-card">
        <div class="acc-card-header">
          <h5>Doanh thu vs Chi phí</h5>
          <div class="btn-group btn-group-sm">
            <button class="btn btn-outline-secondary active" data-period="month">Tháng</button>
            <button class="btn btn-outline-secondary" data-period="quarter">Quý</button>
            <button class="btn btn-outline-secondary" data-period="year">Năm</button>
          </div>
        </div>
        <div class="acc-card-body">
          <canvas id="revenueChart" height="280"></canvas>
        </div>
      </div>
    </div>
    <div class="col-lg-4">
      <div class="acc-card">
        <div class="acc-card-header"><h5>Phân loại chi phí</h5></div>
        <div class="acc-card-body">
          <canvas id="expenseChart" height="280"></canvas>
        </div>
      </div>
    </div>
  </div>

  <!-- Bottom Row -->
  <div class="row g-4">
    <div class="col-lg-6">
      <div class="acc-card">
        <div class="acc-card-header">
          <h5>Hóa đơn quá hạn</h5>
          <a href="/invoices?status=overdue" class="btn btn-sm btn-ghost">Xem tất cả</a>
        </div>
        <div class="acc-card-body p-0">
          <table class="table table-hover mb-0" id="overdueTable">
            <thead>
              <tr>
                <th>Số HĐ</th>
                <th>Khách hàng</th>
                <th>Quá hạn</th>
                <th class="text-end">Số tiền</th>
              </tr>
            </thead>
            <tbody id="overdueBody">
              <!-- Loaded via AJAX -->
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <div class="col-lg-6">
      <div class="acc-card">
        <div class="acc-card-header">
          <h5>Giao dịch gần đây</h5>
          <a href="/journal-entries" class="btn btn-sm btn-ghost">Xem tất cả</a>
        </div>
        <div class="acc-card-body p-0">
          <table class="table table-hover mb-0" id="recentTxTable">
            <thead>
              <tr>
                <th>Ngày</th>
                <th>Mô tả</th>
                <th class="text-end">Nợ</th>
                <th class="text-end">Có</th>
              </tr>
            </thead>
            <tbody id="recentTxBody">
              <!-- Loaded via AJAX -->
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>

</div>

@section Scripts {
<script>
$(function() {
  // Load KPI data
  $.get('/api/dashboard/kpis', function(data) {
    $('#kpiRevenue').text(ACC.format.currency(data.revenue));
    $('#kpiExpenses').text(ACC.format.currency(data.expenses));
    $('#kpiProfit').text(ACC.format.negative(data.profit));
    $('#kpiCash').text(ACC.format.currency(data.cashBalance));
    $('#kpiRevenueChange span').text(ACC.format.percent(data.revenueChange));
  });

  // Revenue vs Expense Chart (Chart.js)
  const ctx = document.getElementById('revenueChart').getContext('2d');
  const revenueChart = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: [], datasets: [
        { label: 'Doanh thu', data: [], backgroundColor: 'rgba(29,111,164,0.7)', borderRadius: 4 },
        { label: 'Chi phí',   data: [], backgroundColor: 'rgba(239,68,68,0.5)',  borderRadius: 4 }
      ]
    },
    options: {
      responsive: true, maintainAspectRatio: false,
      plugins: { legend: { position: 'top' } },
      scales: {
        y: { ticks: { callback: v => ACC.format.currency(v) } }
      }
    }
  });

  $.get('/api/dashboard/chart?period=month', function(data) {
    revenueChart.data.labels = data.labels;
    revenueChart.data.datasets[0].data = data.revenue;
    revenueChart.data.datasets[1].data = data.expenses;
    revenueChart.update();
  });
});
</script>
}
```

### 4.2 Invoice List Page

```html
<!-- Views/Invoices/Index.cshtml -->
<div class="page-header">
  <h1 class="page-title">Hóa đơn bán hàng</h1>
  <div class="page-actions">
    <button class="btn btn-sm btn-outline-secondary" id="btnExport">
      <i class="bi bi-download me-1"></i>Xuất Excel
    </button>
    <a href="/invoices/create" class="btn btn-sm btn-primary">
      <i class="bi bi-plus-lg me-1"></i>Tạo hóa đơn
    </a>
  </div>
</div>

<div class="page-body">
  <div class="table-wrapper">
    <!-- Toolbar -->
    <div class="table-toolbar">
      <div class="d-flex align-items-center gap-2 flex-wrap">
        <input type="search" class="form-control form-control-sm search-input" 
               id="invoiceSearch" placeholder="Tìm số HĐ, khách hàng...">
        <select class="form-select form-select-sm select2-basic" id="filterStatus" style="width:150px">
          <option value="">Tất cả trạng thái</option>
          <option value="draft">Nháp</option>
          <option value="pending">Chờ duyệt</option>
          <option value="approved">Đã duyệt</option>
          <option value="paid">Đã thu</option>
          <option value="overdue">Quá hạn</option>
          <option value="voided">Đã hủy</option>
        </select>
        <input type="text" class="form-control form-control-sm date-range-picker" 
               style="width:200px" placeholder="Ngày lọc..." id="filterDate">
        <button class="btn btn-sm btn-link text-muted" id="btnClearFilters">Xóa lọc</button>
      </div>
      <!-- Bulk actions (ẩn khi chưa chọn) -->
      <div class="d-flex align-items-center gap-2" id="bulkActions" style="display:none!important">
        <span class="text-muted small" id="selectionCount"></span>
        <button class="btn btn-sm btn-outline-success">
          <i class="bi bi-check-circle me-1"></i>Đánh dấu đã thu
        </button>
        <button class="btn btn-sm btn-outline-danger">
          <i class="bi bi-trash me-1"></i>Xóa
        </button>
      </div>
    </div>

    <!-- Table -->
    <table id="invoicesTable" class="table table-hover w-100">
      <thead>
        <tr>
          <th><input type="checkbox" id="selectAll" class="form-check-input"></th>
          <th>Số hóa đơn</th>
          <th>Khách hàng</th>
          <th>Ngày lập</th>
          <th>Ngày đến hạn</th>
          <th>Trạng thái</th>
          <th class="col-amount">Tổng tiền</th>
          <th class="col-amount">Còn phải thu</th>
          <th></th>
        </tr>
      </thead>
    </table>
  </div>
</div>

@section Scripts {
<script>
$(function() {
  const dt = ACC.initDataTable('#invoicesTable', {
    ajax: {
      url: '/api/invoices/datatable',
      type: 'POST',
      data: function(d) {
        d.status   = $('#filterStatus').val();
        d.dateFrom = $('#filterDate').data('flatpickr') 
                     ? flatpickr.parseDate($('#filterDate').val(), 'd/m/Y - d/m/Y') 
                     : null;
      }
    },
    columns: [
      { data: null, orderable: false, className: 'text-center',
        render: () => '<input type="checkbox" class="form-check-input row-check">' },
      { data: 'invoiceNumber', 
        render: (d, t, row) => `<a href="/invoices/${row.id}" class="invoice-number fw-semibold">${d}</a>` },
      { data: 'customerName',
        render: (d) => `<span class="text-truncate d-block" style="max-width:200px" title="${d}">${d}</span>` },
      { data: 'issueDate',   render: (d) => ACC.format.date(d) },
      { data: 'dueDate',
        render: (d, t, row) => {
          const formatted = ACC.format.date(d);
          return row.isOverdue 
            ? `<span class="text-danger fw-medium">${formatted}</span>` 
            : formatted;
        }
      },
      { data: 'status',
        render: (d) => {
          const map = {
            draft:'Nháp', pending:'Chờ duyệt', approved:'Đã duyệt',
            paid:'Đã thu', overdue:'Quá hạn', voided:'Đã hủy', partial:'Thu một phần'
          };
          return `<span class="status-badge badge-${d}">${map[d] || d}</span>`;
        }
      },
      { data: 'totalAmount',
        className: 'col-amount',
        render: (d) => ACC.format.currency(d) },
      { data: 'amountDue',
        className: 'col-amount',
        render: (d) => d > 0 
          ? `<span class="amount-negative">${ACC.format.currency(d)}</span>` 
          : `<span class="text-muted">—</span>` },
      { data: null, orderable: false,
        render: (d, t, row) => `
          <div class="dropdown">
            <button class="btn btn-sm btn-ghost dropdown-toggle" data-bs-toggle="dropdown">
              <i class="bi bi-three-dots"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-end">
              <li><a class="dropdown-item" href="/invoices/${row.id}">
                <i class="bi bi-eye me-2"></i>Xem / Sửa</a></li>
              <li><a class="dropdown-item" href="/invoices/${row.id}/payment">
                <i class="bi bi-cash me-2"></i>Ghi nhận thu tiền</a></li>
              <li><a class="dropdown-item" href="/invoices/${row.id}/pdf" target="_blank">
                <i class="bi bi-file-pdf me-2"></i>Tải PDF</a></li>
              <li><hr class="dropdown-divider"></li>
              <li><a class="dropdown-item text-danger" 
                     data-confirm="Bạn có muốn hủy hóa đơn ${row.invoiceNumber}?"
                     data-url="/api/invoices/${row.id}/void"
                     data-confirm-label="Hủy hóa đơn">
                <i class="bi bi-x-circle me-2"></i>Hủy hóa đơn</a></li>
            </ul>
          </div>`
      }
    ]
  });

  // Live search
  $('#invoiceSearch').on('input', _.debounce(function() {
    dt.search($(this).val()).draw();
  }, 300));

  // Filter change
  $('#filterStatus, #filterDate').on('change', () => dt.ajax.reload());
  $('#btnClearFilters').on('click', () => {
    $('#filterStatus').val('').trigger('change');
    $('#filterDate').val('');
    dt.ajax.reload();
  });

  // Bulk select
  $('#selectAll').on('change', function() {
    $('.row-check').prop('checked', this.checked);
    updateBulkActions();
  });
  $(document).on('change', '.row-check', updateBulkActions);

  function updateBulkActions() {
    const count = $('.row-check:checked').length;
    if (count > 0) {
      $('#bulkActions').show();
      $('#selectionCount').text(`Đã chọn ${count} hóa đơn`);
    } else {
      $('#bulkActions').hide();
    }
  }

  // Export
  $('#btnExport').on('click', function() {
    window.location.href = '/invoices/export?' + $.param({
      status: $('#filterStatus').val(),
      search: $('#invoiceSearch').val()
    });
  });
});
</script>
}
```

### 4.3 Journal Entry Form

```html
<!-- Views/JournalEntries/Create.cshtml -->
<div class="page-header">
  <h1 class="page-title">Tạo bút toán mới</h1>
  <div class="page-actions">
    <a href="/journal-entries" class="btn btn-sm btn-outline-secondary">
      <i class="bi bi-arrow-left me-1"></i>Quay lại
    </a>
  </div>
</div>

<div class="page-body">
  <form id="journalForm" class="acc-ajax-form" 
        asp-action="Create" asp-controller="JournalEntries"
        data-success-msg="Bút toán đã được lưu thành công"
        data-redirect="/journal-entries">

    @Html.AntiForgeryToken()

    <div class="row g-4 mb-4">
      <div class="col-md-3">
        <label class="form-label">Ngày <span class="required">*</span></label>
        <input type="text" class="form-control date-picker" name="EntryDate" 
               asp-for="EntryDate" required>
        <span asp-validation-for="EntryDate" class="invalid-feedback d-block"></span>
      </div>
      <div class="col-md-3">
        <label class="form-label">Số chứng từ</label>
        <input type="text" class="form-control font-mono" name="Reference" 
               asp-for="Reference" placeholder="Tự động">
      </div>
      <div class="col-md-6">
        <label class="form-label">Mô tả <span class="required">*</span></label>
        <input type="text" class="form-control" name="Description" 
               asp-for="Description" required>
        <span asp-validation-for="Description" class="invalid-feedback d-block"></span>
      </div>
    </div>

    <!-- Line Items -->
    <div class="acc-card mb-4">
      <div class="acc-card-header">
        <h5>Chi tiết bút toán</h5>
      </div>
      <div class="acc-card-body p-0">
        <div class="table-responsive">
          <table class="table mb-0" id="journalLines">
            <thead>
              <tr>
                <th style="width:35%">Tài khoản</th>
                <th>Mô tả dòng</th>
                <th class="text-end" style="width:150px">Nợ (Debit)</th>
                <th class="text-end" style="width:150px">Có (Credit)</th>
                <th style="width:40px"></th>
              </tr>
            </thead>
            <tbody id="linesBody">
              <!-- Dynamic rows added by JS -->
            </tbody>
            <tfoot>
              <tr class="table-total-row">
                <td colspan="2" class="text-end pe-3">
                  <div id="balanceIndicator" class="d-flex align-items-center justify-content-end gap-2">
                    <span class="text-muted small">Chênh lệch:</span>
                    <span id="differenceAmount" class="font-mono fw-semibold text-danger">—</span>
                  </div>
                </td>
                <td class="text-end font-mono fw-semibold" id="totalDebit">0</td>
                <td class="text-end font-mono fw-semibold" id="totalCredit">0</td>
                <td></td>
              </tr>
            </tfoot>
          </table>
        </div>
        <div class="p-3 border-top">
          <button type="button" class="btn btn-sm btn-outline-primary" id="addLine">
            <i class="bi bi-plus-lg me-1"></i>Thêm dòng
          </button>
        </div>
      </div>
    </div>

    <!-- Balance indicator -->
    <div class="alert" id="balanceAlert" role="alert" style="display:none"></div>

    <!-- Footer -->
    <div class="d-flex justify-content-end gap-3">
      <button type="button" class="btn btn-outline-secondary" 
              onclick="window.history.back()">Hủy</button>
      <button type="submit" name="action" value="draft" 
              class="btn btn-outline-primary">
        <i class="bi bi-save me-1"></i>Lưu nháp
      </button>
      <button type="submit" name="action" value="post" 
              class="btn btn-primary" id="btnPost" disabled>
        <i class="bi bi-check2-circle me-1"></i>Hạch toán
      </button>
    </div>
  </form>
</div>

@section Scripts {
<script>
$(function() {
  let lineIndex = 0;

  // Template cho 1 dòng bút toán
  function createLineRow(index) {
    return `
    <tr class="journal-line" data-index="${index}">
      <td>
        <select class="form-select form-select-sm select2-accounts line-account"
                name="Lines[${index}].AccountId" required>
        </select>
      </td>
      <td>
        <input type="text" class="form-control form-control-sm" 
               name="Lines[${index}].Description" placeholder="Mô tả (tùy chọn)">
      </td>
      <td>
        <input type="text" class="form-control form-control-sm form-control-amount input-money line-debit"
               name="Lines[${index}].Debit" placeholder="0" 
               data-line="${index}">
      </td>
      <td>
        <input type="text" class="form-control form-control-sm form-control-amount input-money line-credit"
               name="Lines[${index}].Credit" placeholder="0"
               data-line="${index}">
      </td>
      <td>
        <button type="button" class="btn btn-sm btn-ghost text-danger remove-line" title="Xóa dòng">
          <i class="bi bi-x-lg"></i>
        </button>
      </td>
    </tr>`;
  }

  function addLine() {
    const row = $(createLineRow(lineIndex++));
    $('#linesBody').append(row);
    // Khởi tạo Select2 cho dòng vừa thêm
    row.find('.select2-accounts').select2({
      theme: 'bootstrap-5', width: '100%',
      minimumInputLength: 1, placeholder: 'Chọn tài khoản...',
      ajax: {
        url: '/api/accounts/search', dataType: 'json', delay: 250,
        data: p => ({ q: p.term }),
        processResults: data => ({
          results: data.map(a => ({ id: a.id, text: `${a.code} — ${a.name}` }))
        })
      }
    });
    // Cleave cho inputs
    new Cleave(row.find('.line-debit')[0],  { numeral: true, numeralThousandsGroupStyle: 'thousand' });
    new Cleave(row.find('.line-credit')[0], { numeral: true, numeralThousandsGroupStyle: 'thousand' });
  }

  // Add 2 dòng mặc định
  addLine(); addLine();

  $('#addLine').on('click', addLine);

  $(document).on('click', '.remove-line', function() {
    if ($('.journal-line').length <= 2) {
      ACC.toast.warning('Bút toán phải có ít nhất 2 dòng.');
      return;
    }
    $(this).closest('tr').remove();
    recalculate();
  });

  // Tính lại tổng khi thay đổi
  $(document).on('input', '.line-debit, .line-credit', function() {
    // Khi nhập Nợ, xóa Có trên cùng dòng và ngược lại
    const $row = $(this).closest('tr');
    if ($(this).hasClass('line-debit') && $(this).val()) {
      $row.find('.line-credit').val('');
    } else if ($(this).hasClass('line-credit') && $(this).val()) {
      $row.find('.line-debit').val('');
    }
    recalculate();
  });

  function getNumericValue($input) {
    return parseFloat($input.val().replace(/,/g, '')) || 0;
  }

  function recalculate() {
    let totalDebit = 0, totalCredit = 0;
    $('.line-debit').each(function()  { totalDebit  += getNumericValue($(this)); });
    $('.line-credit').each(function() { totalCredit += getNumericValue($(this)); });

    const diff = Math.abs(totalDebit - totalCredit);
    const balanced = diff < 0.01; // float tolerance

    $('#totalDebit').text(ACC.format.currency(totalDebit));
    $('#totalCredit').text(ACC.format.currency(totalCredit));
    $('#differenceAmount').text(ACC.format.currency(diff))
      .toggleClass('text-danger', !balanced)
      .toggleClass('text-success', balanced);

    const $alert = $('#balanceAlert');
    if (balanced) {
      $alert.removeClass('alert-danger').addClass('alert-success')
        .html('<i class="bi bi-check-circle me-2"></i>Bút toán đã cân bằng. Có thể hạch toán.')
        .show();
    } else {
      $alert.removeClass('alert-success').addClass('alert-danger')
        .html(`<i class="bi bi-exclamation-triangle me-2"></i>Chênh lệch: ${ACC.format.currency(diff)}. Bút toán chưa cân bằng.`)
        .show();
    }

    $('#btnPost').prop('disabled', !balanced);
  }
});
</script>
}
```

---

## 5. CONFIRMATION MODAL (Global, trong _Layout.cshtml)

```html
<!-- Thêm vào cuối body trong _Layout.cshtml -->
<div class="modal fade" id="confirmModal" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-xs modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">
          <i class="bi bi-exclamation-triangle-fill text-danger me-2"></i>Xác nhận
        </h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <p class="mb-0"></p>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Hủy</button>
        <button type="button" class="btn btn-danger btn-confirm-ok">Xác nhận</button>
      </div>
    </div>
  </div>
</div>
```

---

## 6. FORMATTING RULES (Quan trọng)

### 6.1 Số tiền (Currency)

```javascript
// Dùng VND — không hiển thị số thập phân
ACC.format.currency(1250000)         // → "1.250.000 ₫"

// Âm dùng ngoặc (chuẩn kế toán)
ACC.format.negative(-1250000)        // → "(1.250.000 ₫)"

// Trong bảng HTML
`<td class="col-amount amount-negative">(1.250.000 ₫)</td>`

// KPI lớn
ACC.format.kpi = (v) => v >= 1e9 ? `${(v/1e9).toFixed(1)} tỷ`
                       : v >= 1e6 ? `${(v/1e6).toFixed(1)} triệu`
                       : ACC.format.currency(v);
```

### 6.2 Ngày tháng (Date)

```javascript
// Việt Nam format: DD/MM/YYYY
ACC.format.date('2024-01-15')   // → "15/01/2024"

// Flatpickr locale config
flatpickr.localize(flatpickr.l10ns.vn);
flatpickr('.date-picker', { dateFormat: 'd/m/Y' });
```

### 6.3 Phần trăm

```javascript
ACC.format.percent(12.5)    // → "+12.5%"
ACC.format.percent(-3.2)    // → "-3.2%"
```

---

## 7. ACCESSIBILITY CHECKLIST

Tất cả components phải đáp ứng:

```html
<!-- Input với label đúng cách -->
<label class="form-label" for="invoiceDate">
  Ngày hóa đơn <span class="required" aria-label="bắt buộc">*</span>
</label>
<input class="form-control" id="invoiceDate" aria-required="true"
       aria-describedby="invoiceDateHelp invoiceDateError">
<div class="form-text" id="invoiceDateHelp">Ngày lập hóa đơn theo hợp đồng</div>
<div class="invalid-feedback" id="invoiceDateError" role="alert"></div>

<!-- Badge với screen reader label -->
<span class="status-badge badge-overdue" 
      aria-label="Trạng thái: Quá hạn">Quá hạn</span>

<!-- Sortable table header -->
<th scope="col" aria-sort="ascending">Ngày <i class="bi bi-arrow-up"></i></th>

<!-- Loading state -->
<div role="status" aria-live="polite" id="tableLoading">
  <span class="visually-hidden">Đang tải dữ liệu...</span>
</div>
```

---

## 8. ANTI-PATTERNS — TUYỆT ĐỐI KHÔNG LÀM

| ❌ Không làm | ✅ Thay bằng |
|-------------|------------|
| `$.getJSON` không có error handler | Luôn có `.fail()` hoặc dùng `ACC.ajax` wrapper |
| `parseInt()` cho số tiền | Dùng `parseFloat().toFixed(2)` hoặc Cleave |
| Hardcode màu trong `style=""` | Dùng class CSS đã định nghĩa |
| Dùng `alert()` hoặc `confirm()` | Dùng Toastr + Bootstrap Modal |
| `$("#table").html(...)` để render toàn bộ | Dùng DataTables `ajax.reload()` |
| Số âm hiển thị bằng màu đỏ đơn thuần | Màu đỏ + dấu ngoặc `(amount)` |
| Xóa không có confirm | Luôn có `data-confirm` attribute |
| `type="number"` cho ô tiền | `type="text"` + Cleave.js mask |
| Bootstrap `alert` cho toast | Toastr.js |
| Reload toàn bộ trang sau save | AJAX + toastr + `dt.ajax.reload()` |
| Dùng `disabled` mà không giải thích | Thêm tooltip giải thích tại sao disabled |

---

## 9. ASP.NET CORE INTEGRATION NOTES

### 9.1 Anti-Forgery Token (CSRF)

```javascript
// Thêm vào mọi AJAX POST/PUT/DELETE
$.ajaxSetup({
  headers: {
    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
  }
});
```

### 9.2 Server-side DataTables Response Format

```csharp
// Controller trả về đúng format DataTables
[HttpPost]
public IActionResult GetInvoices([FromBody] DataTablesRequest request)
{
  var query = _context.Invoices.AsQueryable();
  // ... filter, sort, paginate
  return Ok(new {
    draw = request.Draw,
    recordsTotal = totalCount,
    recordsFiltered = filteredCount,
    data = pagedData
  });
}
```

### 9.3 Validation (jQuery Unobtrusive)

```html
<!-- Form tự động validate nhờ asp-for + data-val attributes -->
<input asp-for="Amount" class="form-control form-control-amount"
       data-val="true"
       data-val-required="Số tiền là bắt buộc"
       data-val-range="Số tiền phải lớn hơn 0"
       data-val-range-min="0.01"
       data-val-range-max="999999999">
<span asp-validation-for="Amount" class="invalid-feedback d-block"></span>
```

### 9.4 Export (Server-side với EPPlus)

```csharp
// NuGet: EPPlus
[HttpGet("export")]
public IActionResult ExportToExcel(InvoiceFilterDto filter)
{
  var data = _invoiceService.GetFiltered(filter);
  using var package = new ExcelPackage();
  var ws = package.Workbook.Worksheets.Add("Invoices");
  // ... fill data
  return File(package.GetAsByteArray(), 
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    "invoices.xlsx");
}
```

---

## 10. IMPLEMENTATION CHECKLIST (AI Agent)

Khi implement bất kỳ trang/component nào, kiểm tra:

**Bootstrap + jQuery**
- [ ] Dùng Bootstrap class, không tự viết grid từ đầu
- [ ] Override Bootstrap qua CSS variables, không dùng `!important` lung tung
- [ ] jQuery AJAX có cả `.done()` và `.fail()` handler
- [ ] Tất cả DOM manipulation dùng jQuery, không mix vanilla JS

**Plugins**
- [ ] Date inputs dùng Flatpickr (không dùng `type="date"` native)
- [ ] Select cho tài khoản/khách hàng dùng Select2 với AJAX
- [ ] Ô nhập tiền dùng Cleave.js + `type="text"`
- [ ] Thông báo dùng Toastr (không dùng Bootstrap toast trực tiếp)
- [ ] Bảng dữ liệu lớn dùng DataTables với server-side

**Số liệu kế toán**
- [ ] Số tiền dùng `ACC.format.currency()` hoặc `ACC.format.negative()`
- [ ] Cột số tiền: class `col-amount`, monospace, right-align
- [ ] Số âm: màu đỏ + ngoặc đơn `(amount)`
- [ ] Ngày: Flatpickr DD/MM/YYYY

**ASP.NET Core**
- [ ] Form có `@Html.AntiForgeryToken()`
- [ ] AJAX setup có `RequestVerificationToken` header
- [ ] Validation dùng jQuery Validate Unobtrusive
- [ ] Export server-side qua EPPlus / iTextSharp

**UX**
- [ ] Mọi thao tác xóa/hủy có confirm modal
- [ ] Button submit có loading state khi đang xử lý
- [ ] Table có empty state
- [ ] Lỗi API hiển thị thông báo rõ ràng bằng tiếng Việt

---

*Hết tài liệu. Version 2.0 — Bootstrap 5 + jQuery + Microsoft Ecosystem*
