# User Personas for AMS (Accounting Management System)

**Version:** 1.0  
**Last Updated:** March 2026

---

## Table of Contents

1. [Overview](#1-overview)
2. [Persona Definitions](#2-persona-definitions)
3. [Persona Comparison Matrix](#3-persona-comparison-matrix)
4. [User Goals & Pain Points](#4-user-goals--pain-points)
5. [Design Implications](#5-design-implications)

---

## 1. Overview

This document defines the target users of the AMS ERP system. Each persona represents a distinct user group with specific needs, behaviors, and technical capabilities.

**Target Users:**
- Data Entry Clerk (Kế toán viên nhập liệu)
- Accountant (Kế toán tổng hợp)
- Finance Manager (Quản lý tài chính)
- Auditor (Kiểm toán viên)
- System Administrator (Quản trị viên)

---

## 2. Persona Definitions

### 2.1 Data Entry Clerk (Kế toán viên nhập liệu)

| Attribute | Details |
|-----------|---------|
| **Name** | Nguyễn Thị Mai |
| **Age** | 24-30 |
| **Education** | Intermediate accounting certificate |
| **Experience** | 1-3 years |
| **Tech Proficiency** | Medium |
| **Daily Tasks** | Input vouchers,录入 inventory transactions |
| **Work Style** | High volume, repetitive tasks, speed-focused |
| **Computer Skills** | Comfortable with Excel, familiar with accounting software |
| **Work Hours** | 8 hours/day, peak periods month-end |

**Key Characteristics:**
- Processes 50-100 vouchers per day
- Prefers keyboard over mouse
- Needs clear field guidance
- Wants quick save & continue

**Quote:** "Tôi cần nhập nhanh mà không sợ sai. Mỗi lần click chuột là tốn thời gian."

---

### 2.2 Accountant (Kế toán tổng hợp)

| Attribute | Details |
|-----------|---------|
| **Name** | Trần Văn Hùng |
| **Age** | 30-45 |
| **Education** | University degree in Accounting |
| **Experience** | 5-10 years |
| **Tech Proficiency** | High |
| **Daily Tasks** | Ledger entries, reconciliation, month-end close |
| **Work Style** | Detail-oriented, accuracy-focused |
| **Computer Skills** | Proficient in Excel, understands accounting concepts deeply |
| **Work Hours** | 8-10 hours/day, heavy during close periods |

**Key Characteristics:**
- Reviews and corrects voucher entries
- Performs bank reconciliation
- Generates trial balance
- Understands chart of accounts structure

**Quote:** "Tôi cần thấy rõ sổ sách để đối chiếu. Mỗi con số phải khớp từng dòng."

---

### 2.3 Finance Manager (Quản lý tài chính)

| Attribute | Details |
|-----------|---------|
| **Name** | Phạm Thị Hương |
| **Age** | 35-50 |
| **Education** | CPA or equivalent |
| **Experience** | 10+ years |
| **Tech Proficiency** | Medium-High |
| **Daily Tasks** | Approve vouchers, review reports, strategic planning |
| **Work Style** | Overview-focused, delegation-oriented |
| **Computer Skills** | Understands reports, uses dashboards, mobile-friendly |
| **Work Hours** | Variable, quick reviews throughout day |

**Key Characteristics:**
- Approves 20-50 vouchers daily (quick review)
- Needs dashboard overview
- Reviews financial statements
- Sets budget targets

**Quote:** "Tôi cần nhìn thấy tổng quan. Phê duyệt nhanh nhưng vẫn kiểm soát được."

---

### 2.4 Auditor (Kiểm toán viên)

| Attribute | Details |
|-----------|---------|
| **Name** | Lê Minh Quang |
| **Age** | 28-40 |
| **Education** | CPA, Auditor certification |
| **Experience** | 3-7 years |
| **Tech Proficiency** | High |
| **Daily Tasks** | Query data, trace transactions, export reports |
| **Work Style** | Investigation-focused, detail-oriented |
| **Computer Skills** | Expert in data analysis, SQL, Excel |
| **Work Hours** | Project-based, intensive during audit season |

**Key Characteristics:**
- Traces transactions from source to financial statements
- Exports large datasets for analysis
- Verifies audit trail
- Creates custom queries

**Quote:** "Tôi cần truy xuất được mọi thứ. Từ chứng từ gốc đến báo cáo cuối cùng."

---

### 2.5 System Administrator (Quản trị viên)

| Attribute | Details |
|-----------|---------|
| **Name** | Ngô Đình Nam |
| **Age** | 25-40 |
| **Education** | IT or equivalent |
| **Experience** | 2-8 years |
| **Tech Proficiency** | High |
| **Daily Tasks** | User management, security, system configuration |
| **Work Style** | Technical, security-focused |
| **Computer Skills** | System administration, network, database basics |
| **Work Hours** | On-call, incident-driven |

**Key Characteristics:**
- Creates and manages user accounts
- Configures roles and permissions
- Monitors system health
- Handles technical issues

**Quote:** "Tôi cần quản lý người dùng dễ dàng. Bảo mật nhưng không phức tạp."

---

## 3. Persona Comparison Matrix

| Criteria | Clerk | Accountant | Manager | Auditor | Admin |
|----------|-------|------------|---------|---------|-------|
| **Primary Goal** | Speed | Accuracy | Overview | Traceability | Security |
| **Daily Vouchers** | 50-100 | 20-50 | 20-50 | 0-10 | 0-5 |
| **Keyboard Usage** | High | Medium | Low | Medium | Low |
| **Mouse Usage** | Low | Medium | High | Medium | High |
| **Report Usage** | Low | High | High | High | Low |
| **Mobile Usage** | Low | Low | Medium | Low | Low |
| **Training Needed** | High | Medium | Low | Medium | High |
| **Permission Level** | Low | Medium | High | High | Full |

---

## 4. User Goals & Pain Points

### 4.1 Goals

| Persona | Primary Goal | Secondary Goal |
|---------|--------------|----------------|
| Clerk | Enter data quickly | Avoid errors, easy corrections |
| Accountant | Ensure accurate records | Complete close on time |
| Manager | Monitor business health | Quick approvals |
| Auditor | Verify compliance | Efficient data extraction |
| Admin | Maintain system security | Easy user management |

### 4.2 Pain Points

| Persona | Pain Point | Current Workaround |
|---------|------------|-------------------|
| Clerk | Too many clicks | Keyboard shortcuts in Excel |
| Accountant | Hard to find errors | Print and manually check |
| Manager | Can't see real-time data | Request reports from staff |
| Auditor | Difficult to trace | Export to Excel, manual trace |
| Admin | No audit of user actions | Keep separate logs |

### 4.2 System Opportunities

| Pain Point | System Solution |
|------------|-----------------|
| Too many clicks | Auto-save, keyboard navigation, Tab flow |
| Hard to find errors | Inline validation, running totals |
| Can't see real-time | Live dashboard, auto-refresh |
| Difficult to trace | Full audit trail, click-through |
| No audit of user actions | Activity logging, session tracking |

---

## 5. Design Implications

### 5.1 UI Requirements by Persona

| Persona | Layout Priority | Key Features |
|---------|-----------------|--------------|
| Clerk | Compact, form-focused | Tab navigation, quick save, duplicate prev |
| Accountant | Data-dense, detailed | Drill-down, filters, reconciliation tools |
| Manager | Dashboard, overview | KPIs, charts, mobile-responsive |
| Auditor | Search, export | Advanced filters, data export, audit trail |
| Admin | Settings, security | User management, role config, logs |

### 5.2 Accessibility Considerations

- Clerk: High contrast, large click targets
- Accountant: Detailed view, collapsible sections
- Manager: Responsive, works on tablet
- Auditor: Search optimization, export formats
- Admin: Audit logs, confirmation dialogs

### 5.3 Performance Requirements

| Persona | Max Response Time | Priority Features |
|---------|-------------------|-------------------|
| Clerk | <500ms | Save, load, search |
| Accountant | <1s | Report generation, filtering |
| Manager | <2s | Dashboard load, mobile |
| Auditor | <3s | Large exports, complex queries |
| Admin | <1s | User operations, config saves |

---

## Appendix A: Persona Quick Reference Card

```
┌─────────────────────────────────────────────────────────────┐
│                    PERSONA QUICK REFERENCE                   │
├──────────────┬─────────────┬────────────┬──────────────────┤
│ Clerk        │ Speed       │ 50-100/day │ Keyboard-first   │
│ Accountant   │ Accuracy    │ 20-50/day  │ Detail-focused   │
│ Manager     │ Overview    │ 20-50/day  │ Dashboard-first  │
│ Auditor     │ Traceability│ 0-10/day   │ Export-first     │
│ Admin       │ Security    │ 0-5/day    │ Settings-first   │
└──────────────┴─────────────┴────────────┴──────────────────┘
```

---

*Document Owner: Product Team*  
*Status: Draft - Requires Validation*