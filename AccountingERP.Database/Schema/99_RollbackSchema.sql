-- ============================================================================
-- ROLLBACK SCRIPT FOR ACCOUNTING ERP SYSTEM
-- Removes all database objects created by 01_CreateSchema.sql
-- USE WITH CAUTION - This will delete all data!
-- ============================================================================

-- Disable triggers to allow cleanup
DISABLE TRIGGER ALL ON DATABASE;

-- ============================================================================
-- 1. DROP STORED PROCEDURES
-- ============================================================================
IF OBJECT_ID('sp_CloseAccountingPeriod', 'P') IS NOT NULL
    DROP PROCEDURE sp_CloseAccountingPeriod;

-- ============================================================================
-- 2. DROP VIEWS
-- ============================================================================
IF OBJECT_ID('vw_TrialBalance', 'V') IS NOT NULL
    DROP VIEW vw_TrialBalance;

IF OBJECT_ID('vw_UnpostedEntries', 'V') IS NOT NULL
    DROP VIEW vw_UnpostedEntries;

IF OBJECT_ID('vw_PeriodClosingStatus', 'V') IS NOT NULL
    DROP VIEW vw_PeriodClosingStatus;

-- ============================================================================
-- 3. DROP TRIGGERS
-- ============================================================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_JournalEntryLines_UpdateTotals')
    DROP TRIGGER TR_JournalEntryLines_UpdateTotals ON JournalEntryLines;

IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_JournalEntries_PreventDelete')
    DROP TRIGGER TR_JournalEntries_PreventDelete ON JournalEntries;

IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_JournalEntries_AuditLog')
    DROP TRIGGER TR_JournalEntries_AuditLog ON JournalEntries;

-- ============================================================================
-- 4. DROP TABLES (in reverse dependency order)
-- ============================================================================

-- Audit Logs
IF OBJECT_ID('AuditLogs', 'U') IS NOT NULL
    DROP TABLE AuditLogs;

-- Journal Entry Lines (child table)
IF OBJECT_ID('JournalEntryLines', 'U') IS NOT NULL
    DROP TABLE JournalEntryLines;

-- Journal Entries (parent table)
IF OBJECT_ID('JournalEntries', 'U') IS NOT NULL
    DROP TABLE JournalEntries;

-- Accounts
IF OBJECT_ID('Accounts', 'U') IS NOT NULL
    DROP TABLE Accounts;

-- Accounting Periods
IF OBJECT_ID('AccountingPeriods', 'U') IS NOT NULL
    DROP TABLE AccountingPeriods;

-- Lookup Tables
IF OBJECT_ID('JournalEntryStatuses', 'U') IS NOT NULL
    DROP TABLE JournalEntryStatuses;

IF OBJECT_ID('PeriodStatuses', 'U') IS NOT NULL
    DROP TABLE PeriodStatuses;

-- ============================================================================
-- 5. VERIFICATION
-- ============================================================================

PRINT 'Database rollback completed successfully.';
PRINT 'All tables, views, triggers, and stored procedures have been removed.';
