
-- 03 - Sample data (INSERT)

-- Every account in this script (students, HSG and OSSE) has the password: Student@123
-- (the program itself creates its sample HSG/OSSE accounts with their own passwords, see README)


INSERT INTO dbo.Students (StudentNumber, FullName, Email, PasswordHash) VALUES
    (N'23-00123', N'Juan Dela Cruz',   N'juan@minsu.edu.ph', N'100000.BwgJCgsMDQ4PEBESExQVFg==.1xQ/oM+22VDq8xEq2yxSCYQ23u6J32P3SXYA0V8uJBI='),
    (N'24-00458', N'Ana Reyes',        N'ana@minsu.edu.ph',  N'100000.BwgJCgsMDQ4PEBESExQVFg==.1xQ/oM+22VDq8xEq2yxSCYQ23u6J32P3SXYA0V8uJBI='),
    (N'22-01987', N'Mark Villanueva',  N'mark@minsu.edu.ph', N'100000.BwgJCgsMDQ4PEBESExQVFg==.1xQ/oM+22VDq8xEq2yxSCYQ23u6J32P3SXYA0V8uJBI=');
GO

INSERT INTO dbo.HsgAdmin (FullName, Email, PasswordHash, IsStudent) VALUES
    (N'Maria Santos', N'hsg@minsu.edu.ph', N'100000.BwgJCgsMDQ4PEBESExQVFg==.1xQ/oM+22VDq8xEq2yxSCYQ23u6J32P3SXYA0V8uJBI=', 1);
GO

INSERT INTO dbo.OsseAdmin (FullName, Email, PasswordHash) VALUES
    (N'OSSE Administrator', N'osse@minsu.edu.ph', N'100000.BwgJCgsMDQ4PEBESExQVFg==.1xQ/oM+22VDq8xEq2yxSCYQ23u6J32P3SXYA0V8uJBI=');
GO

DECLARE @sem INT = (SELECT TOP 1 SemesterId FROM dbo.Semesters WHERE IsCurrent = 1);
DECLARE @juan INT = (SELECT StudentId FROM dbo.Students WHERE StudentNumber = N'23-00123');
DECLARE @ana  INT = (SELECT StudentId FROM dbo.Students WHERE StudentNumber = N'24-00458');
DECLARE @mark INT = (SELECT StudentId FROM dbo.Students WHERE StudentNumber = N'22-01987');

INSERT INTO dbo.Violations (StudentId, TypeId, SemesterId, RecordedAt, RecordedByRole, RecordedById) VALUES
    (@juan, 1, @sem, DATEADD(HOUR, -3,  SYSDATETIME()), N'HSG',  1),   -- No uniform
    (@juan, 3, @sem, DATEADD(DAY,  -17, SYSDATETIME()), N'HSG',  1),   -- Invalid ID lanyard
    (@juan, 2, @sem, DATEADD(DAY,  -38, SYSDATETIME()), N'HSG',  1),   -- Inappropriate attire
    (@ana,  4, @sem, DATEADD(HOUR, -5,  SYSDATETIME()), N'OSSE', 1),   -- No ID
    (@mark, 1, @sem, DATEADD(DAY,  -2,  SYSDATETIME()), N'HSG',  1);   -- No uniform

-- a fine for every violation type that is fined
INSERT INTO dbo.Fines (ViolationId, Amount, SetByRole, SetById)
SELECT v.ViolationId, t.DefaultFine, v.RecordedByRole, v.RecordedById
FROM dbo.Violations v
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
WHERE t.IsFinable = 1;

-- two of the fines are already paid
UPDATE f SET IsPaid = 1, PaidAt = SYSDATETIME()
FROM dbo.Fines f
JOIN dbo.Violations v ON v.ViolationId = f.ViolationId
WHERE v.TypeId = 2 OR (v.StudentId = @mark AND v.TypeId = 1);
GO
