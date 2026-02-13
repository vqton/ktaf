# Phase 5 – TT99 Missing Modules Implementation (Upgrade Mode)

A. Accrual / Deferral Engine
- Additive: creates journals starting from activation; no retroactive impacts to historical balances
- Toggle: enable for FiscalYear >= 2026

B. Provision Engine
- Compute only for new periods; old balances remain unchanged
- If adjustments required, create separate Adjustment Journal

C. Year-End Closing Engine
- If past closings exist, do not reopen automatically; new closing flow for next year

D. Reconciliation Module
- Add checks and reports; no changes to existing GL logic

E. Audit Hardening
- Introduce hash chain baseline at upgrade; do not modify old journals; maintain audit trail integrity

F. Tax Audit Simulation Mode
- Extend tests to validate TT99 checks during upgrade

G. Documentation & Notes
- Document policy changes and test results for audit traceability

H. Data Integrity & Security
- Ensure tamper detection and immutable storage for new data
