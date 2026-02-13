# Phase 2 – Data Impact Analysis

Pre-upgrade data considerations and actions.

1) JournalEntry structure and mapping
- Current: verify fields, data types, nullable flags, foreign keys
- Required: ensure journal entries support accruals, pret paid, provisions without affecting closed periods

2) Account mapping
- Map legacy accounts to new ledger structures; ensure GL integrity

3) VAT logic
- Validate current VAT calculation rules; ensure alignment with TT99 VAT checks

4) Inventory cost history
- Validate historical cost history integrity; ensure no negative values unless consistent with accounting policy

5) Backfill necessity
- Determine if backfill is needed for pre-2026 data; plan safe backfills if required without affecting closed periods

6) Backward recomputation
- Identify any cases where recomputation is required; plan to avoid modifying closed years

7) Pre-migration snapshot
- Capture TB, Balance Sheet, P&L for baseline

8) Data checksum report
- Build checksum table for data integrity verification

9) Trial balance before upgrade
- Prepare TB snapshot that users can compare post-upgrade

10) Migration scope
- Define data dictionaries, required schema changes, and how to verify post-migration data integrity

Deliverables
- Pre-migration snapshot, data checksum report, and trial balance before upgrade
- Migration plan with backfill rules and rollback strategy
- Mapping between old and new data structures
