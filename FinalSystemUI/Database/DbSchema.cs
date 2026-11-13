namespace FinalSystemUI.Database
{
    /// <summary>
    /// Every CREATE statement of the database (tables, indexes, lookup data, views, stored procedure).
    /// Safe to run many times. The same statements are in the Database/SqlScripts folder.
    /// </summary>
    public static class DbSchema
    {
        public static readonly string[] Statements = new string[]
        {
            // IF OBJECT_ID(N'dbo.Semesters', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.Semesters', N'U') IS NULL
CREATE TABLE dbo.Semesters (
    SemesterId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name       NVARCHAR(60) NOT NULL,
    StartDate  DATE NOT NULL,
    EndDate    DATE NOT NULL,
    IsCurrent  BIT NOT NULL CONSTRAINT DF_Semesters_IsCurrent DEFAULT 0
)",
            // IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
CREATE TABLE dbo.Students (
    StudentId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    StudentNumber NVARCHAR(20)  NOT NULL CONSTRAINT UQ_Students_Number UNIQUE,
    FullName      NVARCHAR(120) NOT NULL,
    Email         NVARCHAR(150) NOT NULL CONSTRAINT UQ_Students_Email UNIQUE,
    PasswordHash  NVARCHAR(200) NOT NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_Students_IsActive DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL CONSTRAINT DF_Students_CreatedAt DEFAULT SYSDATETIME()
)",
            // IF OBJECT_ID(N'dbo.HsgAdmin', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.HsgAdmin', N'U') IS NULL
CREATE TABLE dbo.HsgAdmin (
    HsgId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName  NVARCHAR(120) NOT NULL,
    Email     NVARCHAR(150) NOT NULL CONSTRAINT UQ_HsgAdmin_Email UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    IsStudent BIT NOT NULL CONSTRAINT DF_HsgAdmin_IsStudent DEFAULT 0,
    IsActive  BIT NOT NULL CONSTRAINT DF_HsgAdmin_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HsgAdmin_CreatedAt DEFAULT SYSDATETIME()
)",
            // IF OBJECT_ID(N'dbo.OsseAdmin', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.OsseAdmin', N'U') IS NULL
CREATE TABLE dbo.OsseAdmin (
    OsseId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName   NVARCHAR(120) NOT NULL,
    Email      NVARCHAR(150) NOT NULL CONSTRAINT UQ_OsseAdmin_Email UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    IsActive   BIT NOT NULL CONSTRAINT DF_OsseAdmin_IsActive DEFAULT 1,
    CreatedAt  DATETIME2 NOT NULL CONSTRAINT DF_OsseAdmin_CreatedAt DEFAULT SYSDATETIME()
)",
            // ---- Upgrade of an older database file: HSG and OSSE used key files, now they use a password.
            // (Each statement is its own batch, so a column added by one can be used by the next.)
            @"IF COL_LENGTH(N'dbo.HsgAdmin', N'PasswordHash') IS NULL
ALTER TABLE dbo.HsgAdmin ADD PasswordHash NVARCHAR(200) NULL",
            @"IF COL_LENGTH(N'dbo.HsgAdmin', N'KeyHash') IS NOT NULL
ALTER TABLE dbo.HsgAdmin DROP COLUMN KeyHash",
            @"IF COL_LENGTH(N'dbo.OsseAdmin', N'PasswordHash') IS NULL
ALTER TABLE dbo.OsseAdmin ADD PasswordHash NVARCHAR(200) NULL",
            @"IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_OsseAdmin_Profile')
ALTER TABLE dbo.OsseAdmin DROP CONSTRAINT UQ_OsseAdmin_Profile",
            @"IF COL_LENGTH(N'dbo.OsseAdmin', N'ProfileId') IS NOT NULL
ALTER TABLE dbo.OsseAdmin DROP COLUMN ProfileId",
            @"IF COL_LENGTH(N'dbo.OsseAdmin', N'SecretHash') IS NOT NULL
ALTER TABLE dbo.OsseAdmin DROP COLUMN SecretHash",
            // IF OBJECT_ID(N'dbo.ViolationTypes', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.ViolationTypes', N'U') IS NULL
CREATE TABLE dbo.ViolationTypes (
    TypeId      INT NOT NULL PRIMARY KEY,
    Name        NVARCHAR(40) NOT NULL CONSTRAINT UQ_ViolationTypes_Name UNIQUE,
    IsFinable   BIT NOT NULL,
    DefaultFine DECIMAL(10,2) NOT NULL CONSTRAINT DF_ViolationTypes_Fine DEFAULT 0
)",
            // IF OBJECT_ID(N'dbo.Violations', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.Violations', N'U') IS NULL
CREATE TABLE dbo.Violations (
    ViolationId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    StudentId        INT NOT NULL CONSTRAINT FK_Violations_Students REFERENCES dbo.Students(StudentId),
    TypeId           INT NOT NULL CONSTRAINT FK_Violations_Types REFERENCES dbo.ViolationTypes(TypeId),
    SemesterId       INT NOT NULL CONSTRAINT FK_Violations_Semesters REFERENCES dbo.Semesters(SemesterId),
    RecordedAt       DATETIME2 NOT NULL CONSTRAINT DF_Violations_RecordedAt DEFAULT SYSDATETIME(),
    RecordedByRole   NVARCHAR(10) NOT NULL CONSTRAINT CK_Violations_Role CHECK (RecordedByRole IN (N'HSG', N'OSSE')),
    RecordedById     INT NOT NULL,
    Remarks          NVARCHAR(300) NULL,
    LastEditedAt     DATETIME2 NULL,
    LastEditedByRole NVARCHAR(10) NULL,
    LastEditedById   INT NULL
)",
            // IF OBJECT_ID(N'dbo.Fines', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.Fines', N'U') IS NULL
CREATE TABLE dbo.Fines (
    FineId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ViolationId INT NOT NULL CONSTRAINT UQ_Fines_Violation UNIQUE
                CONSTRAINT FK_Fines_Violations REFERENCES dbo.Violations(ViolationId) ON DELETE CASCADE,
    Amount      DECIMAL(10,2) NOT NULL CONSTRAINT CK_Fines_Amount CHECK (Amount >= 0),
    IsPaid      BIT NOT NULL CONSTRAINT DF_Fines_IsPaid DEFAULT 0,
    PaidAt      DATETIME2 NULL,
    SetByRole   NVARCHAR(10) NOT NULL,
    SetById     INT NOT NULL,
    SetAt       DATETIME2 NOT NULL CONSTRAINT DF_Fines_SetAt DEFAULT SYSDATETIME()
)",
            // IF OBJECT_ID(N'dbo.ClearanceRecords', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.ClearanceRecords', N'U') IS NULL
CREATE TABLE dbo.ClearanceRecords (
    ClearanceId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ViolationId     INT NOT NULL CONSTRAINT UQ_Clearance_Violation UNIQUE
                    CONSTRAINT FK_Clearance_Violations REFERENCES dbo.Violations(ViolationId) ON DELETE CASCADE,
    ClearedAt       DATETIME2 NOT NULL CONSTRAINT DF_Clearance_At DEFAULT SYSDATETIME(),
    ClearedByOsseId INT NOT NULL CONSTRAINT FK_Clearance_Osse REFERENCES dbo.OsseAdmin(OsseId),
    Remarks         NVARCHAR(300) NULL
)",
            // IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
CREATE TABLE dbo.AuditLog (
    AuditId   BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    At        DATETIME2 NOT NULL CONSTRAINT DF_Audit_At DEFAULT SYSDATETIME(),
    ActorRole NVARCHAR(10) NOT NULL,
    ActorId   INT NOT NULL,
    Action    NVARCHAR(50) NOT NULL,
    Details   NVARCHAR(500) NULL
)",
            // IF OBJECT_ID(N'dbo.Settings', N'U') IS NULL
            @"IF OBJECT_ID(N'dbo.Settings', N'U') IS NULL
CREATE TABLE dbo.Settings (
    SettingKey   NVARCHAR(50)  NOT NULL PRIMARY KEY,
    SettingValue NVARCHAR(100) NOT NULL
)",
            // IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Violations_
            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Violations_Student' AND object_id = OBJECT_ID(N'dbo.Violations'))
CREATE INDEX IX_Violations_Student ON dbo.Violations (StudentId, RecordedAt DESC)",
            // IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Violations_
            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Violations_Type' AND object_id = OBJECT_ID(N'dbo.Violations'))
CREATE INDEX IX_Violations_Type ON dbo.Violations (TypeId, RecordedAt DESC)",
            // IF NOT EXISTS (SELECT 1 FROM dbo.ViolationTypes)
            @"IF NOT EXISTS (SELECT 1 FROM dbo.ViolationTypes)
INSERT INTO dbo.ViolationTypes (TypeId, Name, IsFinable, DefaultFine) VALUES
    (1, N'No uniform',           1, 100.00),
    (2, N'Inappropriate attire', 1, 100.00),
    (3, N'Invalid ID lanyard',   0, 0.00),
    (4, N'No ID',                0, 0.00)",
            // IF NOT EXISTS (SELECT 1 FROM dbo.Settings)
            @"IF NOT EXISTS (SELECT 1 FROM dbo.Settings)
INSERT INTO dbo.Settings (SettingKey, SettingValue) VALUES
    (N'ClearanceThreshold', N'5'),
    (N'EditWindowHours',    N'12')",
            // IF NOT EXISTS (SELECT 1 FROM dbo.Semesters)
            @"IF NOT EXISTS (SELECT 1 FROM dbo.Semesters)
INSERT INTO dbo.Semesters (Name, StartDate, EndDate, IsCurrent) VALUES
    (N'1st Semester, 2026-2027', '2026-08-01', '2026-12-31', 1)",
            // view vw_ViolationDetails
            @"CREATE OR ALTER VIEW dbo.vw_ViolationDetails
AS
SELECT v.ViolationId,
       v.StudentId,
       s.StudentNumber,
       s.FullName AS StudentName,
       v.SemesterId,
       v.TypeId,
       t.Name AS ViolationType,
       t.IsFinable,
       v.RecordedAt,
       v.RecordedByRole,
       COALESCE(h.FullName, o.FullName) AS RecordedByName,
       v.Remarks,
       f.Amount AS FineAmount,
       ISNULL(f.IsPaid, 0) AS IsPaid,
       f.PaidAt,
       c.ClearedAt,
       CASE WHEN t.IsFinable = 1 THEN CASE WHEN f.IsPaid = 1 THEN N'Paid' ELSE N'Unpaid' END
            WHEN c.ViolationId IS NOT NULL THEN N'Cleared'
            ELSE N'For clearance' END AS Status
FROM dbo.Violations v
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
LEFT JOIN dbo.HsgAdmin h ON v.RecordedByRole = N'HSG' AND h.HsgId = v.RecordedById
LEFT JOIN dbo.OsseAdmin o ON v.RecordedByRole = N'OSSE' AND o.OsseId = v.RecordedById
LEFT JOIN dbo.Fines f ON f.ViolationId = v.ViolationId
LEFT JOIN dbo.ClearanceRecords c ON c.ViolationId = v.ViolationId",
            // view vw_StudentStanding
            @"CREATE OR ALTER VIEW dbo.vw_StudentStanding
AS
SELECT s.StudentId,
       s.StudentNumber,
       s.FullName,
       COUNT(v.ViolationId) AS TotalViolations,
       ISNULL(SUM(CASE WHEN f.IsPaid = 0 THEN f.Amount END), 0) AS UnpaidFines,
       CASE WHEN COUNT(v.ViolationId) = 0 THEN N'Good'
            WHEN COUNT(v.ViolationId) < (SELECT CAST(SettingValue AS INT) FROM dbo.Settings WHERE SettingKey = N'ClearanceThreshold') THEN N'Warning'
            ELSE N'Clearance hold' END AS Standing
FROM dbo.Students s
LEFT JOIN dbo.Violations v ON v.StudentId = s.StudentId
     AND v.SemesterId = (SELECT TOP 1 SemesterId FROM dbo.Semesters WHERE IsCurrent = 1)
LEFT JOIN dbo.Fines f ON f.ViolationId = v.ViolationId
GROUP BY s.StudentId, s.StudentNumber, s.FullName",
            // procedure usp_StudentSummary
            @"CREATE OR ALTER PROCEDURE dbo.usp_StudentSummary
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StudentNumber, FullName, TotalViolations, UnpaidFines, Standing
    FROM dbo.vw_StudentStanding
    WHERE StudentId = @StudentId;
END",
        };
    }
}
