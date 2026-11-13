
-- 01 - Database and tables (SQL Server)
-- The program creates this database by itself (a service-based .mdf file in App_Data).
-- Use this script only if you want to create the database by hand in SQL Server Management Studio.
IF DB_ID(N'UniformComplianceDB') IS NULL CREATE DATABASE UniformComplianceDB;
GO
USE UniformComplianceDB;
GO

IF OBJECT_ID(N'dbo.Semesters', N'U') IS NULL
CREATE TABLE dbo.Semesters (
    SemesterId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name       NVARCHAR(60) NOT NULL,
    StartDate  DATE NOT NULL,
    EndDate    DATE NOT NULL,
    IsCurrent  BIT NOT NULL CONSTRAINT DF_Semesters_IsCurrent DEFAULT 0
);
GO

IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
CREATE TABLE dbo.Students (
    StudentId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    StudentNumber NVARCHAR(20)  NOT NULL CONSTRAINT UQ_Students_Number UNIQUE,
    FullName      NVARCHAR(120) NOT NULL,
    Email         NVARCHAR(150) NOT NULL CONSTRAINT UQ_Students_Email UNIQUE,
    PasswordHash  NVARCHAR(200) NOT NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_Students_IsActive DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL CONSTRAINT DF_Students_CreatedAt DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID(N'dbo.HsgAdmin', N'U') IS NULL
CREATE TABLE dbo.HsgAdmin (
    HsgId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName  NVARCHAR(120) NOT NULL,
    Email     NVARCHAR(150) NOT NULL CONSTRAINT UQ_HsgAdmin_Email UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    IsStudent BIT NOT NULL CONSTRAINT DF_HsgAdmin_IsStudent DEFAULT 0,
    IsActive  BIT NOT NULL CONSTRAINT DF_HsgAdmin_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_HsgAdmin_CreatedAt DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID(N'dbo.OsseAdmin', N'U') IS NULL
CREATE TABLE dbo.OsseAdmin (
    OsseId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName   NVARCHAR(120) NOT NULL,
    Email      NVARCHAR(150) NOT NULL CONSTRAINT UQ_OsseAdmin_Email UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    IsActive   BIT NOT NULL CONSTRAINT DF_OsseAdmin_IsActive DEFAULT 1,
    CreatedAt  DATETIME2 NOT NULL CONSTRAINT DF_OsseAdmin_CreatedAt DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID(N'dbo.ViolationTypes', N'U') IS NULL
CREATE TABLE dbo.ViolationTypes (
    TypeId      INT NOT NULL PRIMARY KEY,
    Name        NVARCHAR(40) NOT NULL CONSTRAINT UQ_ViolationTypes_Name UNIQUE,
    IsFinable   BIT NOT NULL,
    DefaultFine DECIMAL(10,2) NOT NULL CONSTRAINT DF_ViolationTypes_Fine DEFAULT 0
);
GO

IF OBJECT_ID(N'dbo.Violations', N'U') IS NULL
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
);
GO

IF OBJECT_ID(N'dbo.Fines', N'U') IS NULL
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
);
GO

IF OBJECT_ID(N'dbo.ClearanceRecords', N'U') IS NULL
CREATE TABLE dbo.ClearanceRecords (
    ClearanceId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ViolationId     INT NOT NULL CONSTRAINT UQ_Clearance_Violation UNIQUE
                    CONSTRAINT FK_Clearance_Violations REFERENCES dbo.Violations(ViolationId) ON DELETE CASCADE,
    ClearedAt       DATETIME2 NOT NULL CONSTRAINT DF_Clearance_At DEFAULT SYSDATETIME(),
    ClearedByOsseId INT NOT NULL CONSTRAINT FK_Clearance_Osse REFERENCES dbo.OsseAdmin(OsseId),
    Remarks         NVARCHAR(300) NULL
);
GO

IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
CREATE TABLE dbo.AuditLog (
    AuditId   BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    At        DATETIME2 NOT NULL CONSTRAINT DF_Audit_At DEFAULT SYSDATETIME(),
    ActorRole NVARCHAR(10) NOT NULL,
    ActorId   INT NOT NULL,
    Action    NVARCHAR(50) NOT NULL,
    Details   NVARCHAR(500) NULL
);
GO

IF OBJECT_ID(N'dbo.Settings', N'U') IS NULL
CREATE TABLE dbo.Settings (
    SettingKey   NVARCHAR(50)  NOT NULL PRIMARY KEY,
    SettingValue NVARCHAR(100) NOT NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Violations_Student' AND object_id = OBJECT_ID(N'dbo.Violations'))
CREATE INDEX IX_Violations_Student ON dbo.Violations (StudentId, RecordedAt DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Violations_Type' AND object_id = OBJECT_ID(N'dbo.Violations'))
CREATE INDEX IX_Violations_Type ON dbo.Violations (TypeId, RecordedAt DESC);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ViolationTypes)
INSERT INTO dbo.ViolationTypes (TypeId, Name, IsFinable, DefaultFine) VALUES
    (1, N'No uniform',           1, 100.00),
    (2, N'Inappropriate attire', 1, 100.00),
    (3, N'Invalid ID lanyard',   0, 0.00),
    (4, N'No ID',                0, 0.00);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Settings)
INSERT INTO dbo.Settings (SettingKey, SettingValue) VALUES
    (N'ClearanceThreshold', N'5'),
    (N'EditWindowHours',    N'12');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Semesters)
INSERT INTO dbo.Semesters (Name, StartDate, EndDate, IsCurrent) VALUES
    (N'1st Semester, 2026-2027', '2026-08-01', '2026-12-31', 1);
GO

