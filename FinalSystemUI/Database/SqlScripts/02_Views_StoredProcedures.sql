
-- 02 - Views and stored procedure

-- view vw_ViolationDetails
CREATE OR ALTER VIEW dbo.vw_ViolationDetails
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
LEFT JOIN dbo.ClearanceRecords c ON c.ViolationId = v.ViolationId;
GO

-- view vw_StudentStanding
CREATE OR ALTER VIEW dbo.vw_StudentStanding
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
GROUP BY s.StudentId, s.StudentNumber, s.FullName;
GO

-- procedure usp_StudentSummary
CREATE OR ALTER PROCEDURE dbo.usp_StudentSummary
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StudentNumber, FullName, TotalViolations, UnpaidFines, Standing
    FROM dbo.vw_StudentStanding
    WHERE StudentId = @StudentId;
END;
GO

