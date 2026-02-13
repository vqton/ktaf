# Phase 4 – Upgrade Test Strategy

1) Regression Test Suite
- Validate Trial Balance before upgrade vs after upgrade; must be 100% identical
- Validate all GL balances for all accounts across all periods

2) Snapshot Comparison Test
- Compare Total Assets, Total Liabilities, and Accumulated Profit; must match pre-upgrade values

3) Migration Test
- Run migration on a database copy; verify no data loss, FK integrity, no NULLs unexpectedly
- Validate re-calc for trial balance after migration remains consistent

4) Test data strategy
- Use anonymized or synthetic data mirroring production scale
- Include test cases for accruals, deferrals, and year-end close
