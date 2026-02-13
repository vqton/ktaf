-- ============================================================================
-- DATABASE SCHEMA FOR ACCOUNTING ERP SYSTEM
-- TT99/2025 Compliance - Core Business Logic
-- Version: 1.0
-- Created: 2026-02-13
-- ============================================================================

-- ============================================================================
-- 1. ENUMERATION TABLES (Lookup tables for statuses and types)
-- ============================================================================

-- Period Status Lookup
CREATE TABLE PeriodStatuses (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255)
);

INSERT INTO PeriodStatuses (Id, Name, Description) VALUES
(1, N'Open', N'Kỳ đang mở - cho phép ghi sổ'),
(2, N'Closing', N'Đang đóng kỳ - thực hiện các thao tác đóng'),
(3, N'Closed', N'Đã đóng - không cho phép ghi sổ'),
(4, N'Locked', N'Đã khóa vĩnh viễn - sau quyết toán thuế');

-- Journal Entry Status Lookup
CREATE TABLE JournalEntryStatuses (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255)
);

INSERT INTO JournalEntryStatuses (Id, Name, Description) VALUES
(1, N'Draft', N'Bản nháp - đang chỉnh sửa'),
(2, N'Posted', N'Đã ghi sổ'),
(3, N'Cancelled', N'Đã hủy - có bút toán đảo');

-- ============================================================================
-- 2. CORE TABLES
-- ============================================================================

-- Accounting Periods (Kỳ kế toán)
CREATE TABLE AccountingPeriods (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Year INT NOT NULL,
    Month INT NOT NULL,
    StatusId INT NOT NULL DEFAULT 1,
    ClosedAt DATETIME2 NULL,
    ClosedBy NVARCHAR(100) NULL,
    LockedAt DATETIME2 NULL,
    LockedBy NVARCHAR(100) NULL,
    ReopenReason NVARCHAR(500) NULL,
    ReopenCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    UpdatedBy NVARCHAR(100) NULL,
    
    -- Constraints
    CONSTRAINT CHK_AccountingPeriods_Year CHECK (Year BETWEEN 2000 AND 2100),
    CONSTRAINT CHK_AccountingPeriods_Month CHECK (Month BETWEEN 1 AND 12),
    CONSTRAINT FK_AccountingPeriods_Status FOREIGN KEY (StatusId) REFERENCES PeriodStatuses(Id),
    CONSTRAINT UQ_AccountingPeriods_YearMonth UNIQUE (Year, Month)
);

-- Chart of Accounts (Hệ thống tài khoản)
CREATE TABLE Accounts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(10) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    EnglishName NVARCHAR(200) NULL,
    Category INT NOT NULL, -- 1-9 per TT99
    ParentCode NVARCHAR(10) NULL,
    Level INT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDetail BIT NOT NULL DEFAULT 0,
    Description NVARCHAR(500) NULL,
    RequiresCostCenter BIT NOT NULL DEFAULT 0,
    RequiresProject BIT NOT NULL DEFAULT 0,
    RequiresCustomer BIT NOT NULL DEFAULT 0,
    RequiresSupplier BIT NOT NULL DEFAULT 0,
    OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT CHK_Accounts_Code CHECK (Code LIKE '[1-9][0-9][0-9]' OR Code LIKE '[1-9][0-9][0-9][0-9]' OR Code LIKE '[1-9][0-9][0-9][0-9][0-9]'),
    CONSTRAINT CHK_Accounts_Category CHECK (Category BETWEEN 1 AND 9),
    CONSTRAINT CHK_Accounts_Level CHECK (Level BETWEEN 1 AND 3),
    CONSTRAINT UQ_Accounts_Code UNIQUE (Code),
    CONSTRAINT CHK_Accounts_No911 CHECK (Code <> '911') -- TK 911 is special
);

-- Journal Entries (Bút toán kế toán)
CREATE TABLE JournalEntries (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EntryNumber NVARCHAR(50) NOT NULL,
    OriginalDocumentNumber NVARCHAR(100) NOT NULL,
    EntryDate DATE NOT NULL,
    OriginalDocumentDate DATE NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Reference NVARCHAR(100) NULL,
    PeriodId UNIQUEIDENTIFIER NOT NULL,
    StatusId INT NOT NULL DEFAULT 1,
    IsPosted BIT NOT NULL DEFAULT 0,
    PostedDate DATETIME2 NULL,
    PostedBy NVARCHAR(100) NULL,
    VoidReason NVARCHAR(500) NULL,
    AdjustedEntryId UNIQUEIDENTIFIER NULL,
    ReversalEntryId UNIQUEIDENTIFIER NULL,
    CustomerCode NVARCHAR(50) NULL,
    SupplierCode NVARCHAR(50) NULL,
    ProjectCode NVARCHAR(50) NULL,
    CostCenterCode NVARCHAR(50) NULL,
    TotalDebit DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalCredit DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    UpdatedBy NVARCHAR(100) NULL,
    
    -- Constraints
    CONSTRAINT CHK_JournalEntries_EntryDate CHECK (EntryDate <= DATEADD(day, 1, GETDATE())),
    CONSTRAINT CHK_JournalEntries_OriginalDocumentDate CHECK (OriginalDocumentDate <= EntryDate),
    CONSTRAINT CHK_JournalEntries_Dates CHECK (OriginalDocumentDate >= DATEADD(year, -1, EntryDate)),
    CONSTRAINT CHK_JournalEntries_Balance CHECK (TotalDebit = TotalCredit OR IsPosted = 0),
    CONSTRAINT FK_JournalEntries_Period FOREIGN KEY (PeriodId) REFERENCES AccountingPeriods(Id),
    CONSTRAINT FK_JournalEntries_Status FOREIGN KEY (StatusId) REFERENCES JournalEntryStatuses(Id),
    CONSTRAINT UQ_JournalEntries_EntryNumber UNIQUE (EntryNumber)
);

-- Journal Entry Lines (Chi tiết bút toán)
CREATE TABLE JournalEntryLines (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    JournalEntryId UNIQUEIDENTIFIER NOT NULL,
    LineNumber INT NOT NULL,
    AccountCode NVARCHAR(10) NOT NULL,
    DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Description NVARCHAR(500) NOT NULL,
    CostCenterCode NVARCHAR(50) NULL,
    ProjectCode NVARCHAR(50) NULL,
    CustomerCode NVARCHAR(50) NULL,
    SupplierCode NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Constraints
    CONSTRAINT CHK_JournalEntryLines_Amount CHECK (DebitAmount >= 0 AND CreditAmount >= 0),
    CONSTRAINT CHK_JournalEntryLines_DebitOrCredit CHECK (
        (DebitAmount > 0 AND CreditAmount = 0) OR 
        (DebitAmount = 0 AND CreditAmount > 0) OR
        (DebitAmount = 0 AND CreditAmount = 0) -- Allow zero for flexibility
    ),
    CONSTRAINT CHK_JournalEntryLines_No911 CHECK (AccountCode <> '911'),
    CONSTRAINT FK_JournalEntryLines_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES JournalEntries(Id) ON DELETE CASCADE,
    CONSTRAINT FK_JournalEntryLines_Account FOREIGN KEY (AccountCode) REFERENCES Accounts(Code),
    CONSTRAINT UQ_JournalEntryLines_EntryLine UNIQUE (JournalEntryId, LineNumber)
);

-- ============================================================================
-- 3. AUDIT TRAIL TABLES (Immutable logging)
-- ============================================================================

-- Audit Log (Nhật ký audit)
CREATE TABLE AuditLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UserId NVARCHAR(100) NOT NULL,
    UserRole NVARCHAR(50) NOT NULL,
    ActionType NVARCHAR(50) NOT NULL, -- Create, Update, Delete, View, Export
    EntityType NVARCHAR(100) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    OldValues NVARCHAR(MAX) NULL, -- JSON
    NewValues NVARCHAR(MAX) NULL, -- JSON
    SourceIpAddress NVARCHAR(50) NULL,
    SessionId NVARCHAR(100) NULL,
    DigitalSignatureHash NVARCHAR(256) NULL
);

-- ============================================================================
-- 4. INDEXES (Performance optimization)
-- ============================================================================

-- Indexes for Journal Entries
CREATE INDEX IX_JournalEntries_EntryDate ON JournalEntries(EntryDate);
CREATE INDEX IX_JournalEntries_PeriodId ON JournalEntries(PeriodId);
CREATE INDEX IX_JournalEntries_StatusId ON JournalEntries(StatusId);
CREATE INDEX IX_JournalEntries_IsPosted ON JournalEntries(IsPosted);
CREATE INDEX IX_JournalEntries_OriginalDoc ON JournalEntries(OriginalDocumentNumber);
CREATE INDEX IX_JournalEntries_PostedDate ON JournalEntries(PostedDate) WHERE PostedDate IS NOT NULL;

-- Indexes for Journal Entry Lines
CREATE INDEX IX_JournalEntryLines_JournalEntryId ON JournalEntryLines(JournalEntryId);
CREATE INDEX IX_JournalEntryLines_AccountCode ON JournalEntryLines(AccountCode);
CREATE INDEX IX_JournalEntryLines_DebitCredit ON JournalEntryLines(JournalEntryId, AccountCode, DebitAmount, CreditAmount);

-- Indexes for Accounting Periods
CREATE INDEX IX_AccountingPeriods_StatusId ON AccountingPeriods(StatusId);
CREATE INDEX IX_AccountingPeriods_YearMonth ON AccountingPeriods(Year, Month);

-- Indexes for Accounts
CREATE INDEX IX_Accounts_Category ON Accounts(Category);
CREATE INDEX IX_Accounts_IsActive ON Accounts(IsActive);
CREATE INDEX IX_Accounts_ParentCode ON Accounts(ParentCode);

-- Indexes for Audit Logs
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_EntityTypeId ON AuditLogs(EntityType, EntityId);
CREATE INDEX IX_AuditLogs_ActionType ON AuditLogs(ActionType);

-- ============================================================================
-- 5. COMPUTED COLUMNS AND TRIGGERS
-- ============================================================================

-- Trigger: Update Journal Entry totals when lines change
CREATE TRIGGER TR_JournalEntryLines_UpdateTotals
ON JournalEntryLines
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE je
    SET 
        TotalDebit = ISNULL((SELECT SUM(DebitAmount) FROM JournalEntryLines WHERE JournalEntryId = je.Id), 0),
        TotalCredit = ISNULL((SELECT SUM(CreditAmount) FROM JournalEntryLines WHERE JournalEntryId = je.Id), 0),
        UpdatedAt = GETUTCDATE()
    FROM JournalEntries je
    WHERE je.Id IN (
        SELECT JournalEntryId FROM inserted
        UNION
        SELECT JournalEntryId FROM deleted
    );
END;

-- Trigger: Prevent hard delete on Journal Entries (only allow soft delete)
CREATE TRIGGER TR_JournalEntries_PreventDelete
ON JournalEntries
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    RAISERROR (N'TT99: Không được phép xóa cứng bút toán. Sử dụng chức năng hủy bút toán để tạo bút toán đảo.', 16, 1);
    ROLLBACK TRANSACTION;
END;

-- Trigger: Log audit trail on Journal Entry changes
CREATE TRIGGER TR_JournalEntries_AuditLog
ON JournalEntries
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AuditLogs (UserId, UserRole, ActionType, EntityType, EntityId, OldValues, NewValues)
    SELECT 
        ISNULL(i.UpdatedBy, i.CreatedBy),
        'Accountant', -- This should be retrieved from user context
        CASE WHEN d.Id IS NULL THEN 'Create' ELSE 'Update' END,
        'JournalEntry',
        i.Id,
        CASE WHEN d.Id IS NULL THEN NULL ELSE (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) END,
        (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i
    LEFT JOIN deleted d ON i.Id = d.Id;
END;

-- ============================================================================
-- 6. VIEWS (For reporting and validation)
-- ============================================================================

-- View: Trial Balance (Bảng cân đối số phát sinh)
CREATE VIEW vw_TrialBalance AS
SELECT 
    a.Code AS AccountCode,
    a.Name AS AccountName,
    SUM(CASE WHEN jel.DebitAmount > 0 THEN jel.DebitAmount ELSE 0 END) AS TotalDebit,
    SUM(CASE WHEN jel.CreditAmount > 0 THEN jel.CreditAmount ELSE 0 END) AS TotalCredit,
    SUM(CASE WHEN jel.DebitAmount > 0 THEN jel.DebitAmount ELSE 0 END) - 
    SUM(CASE WHEN jel.CreditAmount > 0 THEN jel.CreditAmount ELSE 0 END) AS Balance
FROM Accounts a
LEFT JOIN JournalEntryLines jel ON a.Code = jel.AccountCode
LEFT JOIN JournalEntries je ON jel.JournalEntryId = je.Id AND je.IsPosted = 1
GROUP BY a.Code, a.Name;

-- View: Unposted Journal Entries
CREATE VIEW vw_UnpostedEntries AS
SELECT * FROM JournalEntries WHERE IsPosted = 0;

-- View: Period Closing Checklist Status
CREATE VIEW vw_PeriodClosingStatus AS
SELECT 
    ap.Year,
    ap.Month,
    ap.StatusId,
    ps.Name AS StatusName,
    COUNT(je.Id) AS TotalEntries,
    SUM(CASE WHEN je.IsPosted = 1 THEN 1 ELSE 0 END) AS PostedEntries,
    SUM(CASE WHEN je.IsPosted = 0 THEN 1 ELSE 0 END) AS UnpostedEntries
FROM AccountingPeriods ap
JOIN PeriodStatuses ps ON ap.StatusId = ps.Id
LEFT JOIN JournalEntries je ON ap.Id = je.PeriodId
GROUP BY ap.Year, ap.Month, ap.StatusId, ps.Name;

-- ============================================================================
-- 7. STORED PROCEDURES (Common operations)
-- ============================================================================

-- SP: Close Accounting Period
CREATE PROCEDURE sp_CloseAccountingPeriod
    @PeriodId UNIQUEIDENTIFIER,
    @ClosedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Check for unposted entries
        IF EXISTS (SELECT 1 FROM JournalEntries WHERE PeriodId = @PeriodId AND IsPosted = 0)
        BEGIN
            RAISERROR (N'Không thể đóng kỳ: Còn bút toán chưa ghi sổ.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Check trial balance
        DECLARE @TotalDebit DECIMAL(18,2), @TotalCredit DECIMAL(18,2);
        SELECT 
            @TotalDebit = SUM(TotalDebit),
            @TotalCredit = SUM(TotalCredit)
        FROM vw_TrialBalance;
        
        IF @TotalDebit <> @TotalCredit
        BEGIN
            RAISERROR (N'Không thể đóng kỳ: Bảng cân đối không cân bằng.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Close period
        UPDATE AccountingPeriods
        SET StatusId = 3, -- Closed
            ClosedAt = GETUTCDATE(),
            ClosedBy = @ClosedBy,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @ClosedBy
        WHERE Id = @PeriodId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

-- ============================================================================
-- 8. INITIAL DATA (Seed data)
-- ============================================================================

-- Seed Chart of Accounts (TT99 Standard Accounts)
INSERT INTO Accounts (Code, Name, Category, Level, IsDetail, Description) VALUES
('111', N'Tiền mặt', 1, 1, 0, N'Tiền mặt trong két và ngoài quỹ'),
('1111', N'Tiền mặt VND', 1, 2, 1, N'Tiền mặt đồng Việt Nam'),
('1112', N'Tiền mặt ngoại tệ', 1, 2, 1, N'Tiền mặt ngoại tệ'),
('112', N'Tiền gửi ngân hàng', 1, 1, 0, N'Tiền gửi tại các ngân hàng'),
('1121', N'Tiền gửi ngân hàng VND', 1, 2, 1, N'Tiền gửi ngân hàng đồng Việt Nam'),
('1122', N'Tiền gửi ngân hàng ngoại tệ', 1, 2, 1, N'Tiền gửi ngân hàng ngoại tệ'),
('131', N'Phải thu của khách hàng', 1, 1, 0, N'Công nợ phải thu từ khách hàng'),
('133', N'Thuế GTGT được khấu trừ', 1, 1, 0, N'Thuế giá trị gia tăng đầu vào'),
('1331', N'Thuế GTGT được khấu trừ của hàng hóa, dịch vụ', 1, 2, 1, N'Thuế GTGT đầu vào hàng hóa và dịch vụ'),
('156', N'Hàng hóa', 1, 1, 1, N'Hàng hóa tồn kho'),
('211', N'TSCĐ hữu hình', 2, 1, 0, N'Tài sản cố định hữu hình'),
('214', N'Khấu hao TSCĐ', 2, 1, 1, N'Giá trị hao mòn TSCĐ'),
('331', N'Phải trả cho ngườii bán', 3, 1, 1, N'Công nợ phải trả nhà cung cấp'),
('3331', N'Thuế GTGT phải nộp', 3, 1, 1, N'Thuế GTGT đầu ra phải nộp'),
('3334', N'Thuế TNDN phải nộp', 3, 1, 1, N'Thuế thu nhập doanh nghiệp'),
('411', N'Vốn góp của chủ sở hữu', 4, 1, 1, N'Vốn điều lệ và vốn góp'),
('421', N'Lợi nhuận sau thuế chưa phân phối', 4, 1, 1, N'Lợi nhuận giữ lại'),
('511', N'Doanh thu bán hàng', 5, 1, 1, N'Doanh thu từ bán hàng hóa'),
('515', N'Doanh thu hoạt động tài chính', 5, 1, 1, N'Doanh thu tài chính'),
('611', N'Mua hàng', 6, 1, 1, N'Chi phí mua hàng hóa'),
('632', N'Giá vốn hàng bán', 6, 1, 1, N'Chi phí giá vốn hàng bán'),
('641', N'Chi phí quản lý doanh nghiệp', 6, 1, 1, N'Chi phí hành chính'),
('642', N'Chi phí bán hàng', 6, 1, 1, N'Chi phí kinh doanh');

-- ============================================================================
-- END OF SCHEMA
-- ============================================================================
