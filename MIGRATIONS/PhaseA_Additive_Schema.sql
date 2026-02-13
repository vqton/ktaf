-- Phase A Additive Schema Migration (TT99)
-- Create new tables for Phase A additive engines
CREATE TABLE AccountingPrinciples_Accrual (
  Id VARCHAR(36) PRIMARY KEY,
  Amount DECIMAL(18,2) NOT NULL,
  Date DATE NOT NULL,
  RevenueAccount VARCHAR(50),
  ExpenseAccount VARCHAR(50),
  Reversed BIT NOT NULL DEFAULT 0
);

CREATE TABLE AccountingPrinciples_Prepaid (
  Id VARCHAR(36) PRIMARY KEY,
  TotalAmount DECIMAL(18,2) NOT NULL,
  Months INT NOT NULL,
  AssetAccount VARCHAR(50),
  StartDate DATE NOT NULL
);

CREATE TABLE AccountingPrinciples_Provision (
  Id VARCHAR(36) PRIMARY KEY,
  Amount DECIMAL(18,2) NOT NULL,
  Category VARCHAR(50),
  AsOf DATE
);

CREATE TABLE AuditHash_Baseline (
  Id BIGINT IDENTITY PRIMARY KEY,
  BaselineHash VARCHAR(64) NOT NULL,
  CreatedAt DATETIME NOT NULL
);
