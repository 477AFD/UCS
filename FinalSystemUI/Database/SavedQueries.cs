using System.Collections.Generic;

namespace FinalSystemUI.Database
{
    /// <summary>One ready-made SQL query that OSSE can read and run from the "SQL Queries" window.</summary>
    public class SavedQuery
    {
        public int Number { get; set; }
        public string Group { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Sql { get; set; }
    }

    /// <summary>
    /// The queries shown in the program (OSSE > SQL Queries). They only READ data (SELECT / EXEC of a read-only procedure).
    /// The same queries are in Database/SqlScripts/04_Queries.sql, so you can also run them in Visual Studio or SSMS.
    /// </summary>
    public static class SavedQueries
    {
        public static readonly List<SavedQuery> All =
        [
            new SavedQuery { Number = 1, Group = @"1. Connection and basic SELECT", Title = @"Connection information", Description = @"Shows which server and database file the program is connected to.",
                Sql = @"SELECT DB_NAME()                         AS DatabaseName,
       @@SERVERNAME                      AS ServerName,
       (SELECT TOP 1 physical_name FROM sys.database_files WHERE type_desc = N'ROWS') AS DataFile,
       SYSDATETIME()                     AS ServerTime,
       LEFT(@@VERSION, 60)               AS SqlServerVersion;" },
            new SavedQuery { Number = 2, Group = @"1. Connection and basic SELECT", Title = @"All students (SELECT, ORDER BY)", Description = @"Lists every student account, sorted by name.",
                Sql = @"SELECT StudentId, StudentNumber, FullName, Email, IsActive, CreatedAt
FROM dbo.Students
ORDER BY FullName;" },
            new SavedQuery { Number = 3, Group = @"1. Connection and basic SELECT", Title = @"Violation types and their fines", Description = @"The four violation types, and which ones are fined.",
                Sql = @"SELECT TypeId, Name, IsFinable, DefaultFine
FROM dbo.ViolationTypes
ORDER BY TypeId;" },
            new SavedQuery { Number = 4, Group = @"1. Connection and basic SELECT", Title = @"Active HSG accounts (WHERE)", Description = @"HSG accounts that are enabled.",
                Sql = @"SELECT HsgId, FullName, Email, IsStudent, CreatedAt
FROM dbo.HsgAdmin
WHERE IsActive = 1
ORDER BY FullName;" },
            new SavedQuery { Number = 5, Group = @"1. Connection and basic SELECT", Title = @"Search students by name (LIKE)", Description = @"Finds students whose name contains the text 'Cruz'.",
                Sql = @"SELECT StudentNumber, FullName, Email
FROM dbo.Students
WHERE FullName LIKE N'%Cruz%';" },
            new SavedQuery { Number = 6, Group = @"2. JOIN", Title = @"Violations with student and type (INNER JOIN)", Description = @"Joins three tables to show each violation with the student and the violation name.",
                Sql = @"SELECT v.ViolationId, s.StudentNumber, s.FullName AS Student,
       t.Name AS Violation, v.RecordedAt
FROM dbo.Violations v
INNER JOIN dbo.Students s ON s.StudentId = v.StudentId
INNER JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
ORDER BY v.RecordedAt DESC;" },
            new SavedQuery { Number = 7, Group = @"2. JOIN", Title = @"Who recorded each violation (LEFT JOIN)", Description = @"A record is made by an HSG or an OSSE account, so two LEFT JOINs are used.",
                Sql = @"SELECT v.ViolationId, s.FullName AS Student, t.Name AS Violation,
       v.RecordedByRole, COALESCE(h.FullName, o.FullName) AS RecordedBy
FROM dbo.Violations v
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
LEFT JOIN dbo.HsgAdmin  h ON v.RecordedByRole = N'HSG'  AND h.HsgId  = v.RecordedById
LEFT JOIN dbo.OsseAdmin o ON v.RecordedByRole = N'OSSE' AND o.OsseId = v.RecordedById
ORDER BY v.ViolationId;" },
            new SavedQuery { Number = 8, Group = @"2. JOIN", Title = @"Students with no violation (LEFT JOIN, IS NULL)", Description = @"Students who have never been recorded.",
                Sql = @"SELECT s.StudentNumber, s.FullName
FROM dbo.Students s
LEFT JOIN dbo.Violations v ON v.StudentId = s.StudentId
WHERE v.ViolationId IS NULL;" },
            new SavedQuery { Number = 9, Group = @"2. JOIN", Title = @"Fines with student and violation", Description = @"Each fine with the student, the violation, the amount and whether it is paid.",
                Sql = @"SELECT s.FullName AS Student, t.Name AS Violation, f.Amount, f.IsPaid, f.PaidAt
FROM dbo.Fines f
JOIN dbo.Violations v ON v.ViolationId = f.ViolationId
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
ORDER BY s.FullName;" },
            new SavedQuery { Number = 10, Group = @"3. Aggregate functions (COUNT, SUM, AVG, GROUP BY, HAVING)", Title = @"Violations per type (GROUP BY, COUNT)", Description = @"How many records each violation type has.",
                Sql = @"SELECT t.Name AS Violation, COUNT(v.ViolationId) AS Total
FROM dbo.ViolationTypes t
LEFT JOIN dbo.Violations v ON v.TypeId = t.TypeId
GROUP BY t.TypeId, t.Name
ORDER BY t.TypeId;" },
            new SavedQuery { Number = 11, Group = @"3. Aggregate functions (COUNT, SUM, AVG, GROUP BY, HAVING)", Title = @"Total fines per student (SUM, GROUP BY)", Description = @"The total and the unpaid amount of fines of each student.",
                Sql = @"SELECT s.StudentNumber, s.FullName,
       SUM(f.Amount) AS TotalFines,
       SUM(CASE WHEN f.IsPaid = 0 THEN f.Amount ELSE 0 END) AS Unpaid
FROM dbo.Students s
JOIN dbo.Violations v ON v.StudentId = s.StudentId
JOIN dbo.Fines f ON f.ViolationId = v.ViolationId
GROUP BY s.StudentNumber, s.FullName
ORDER BY TotalFines DESC;" },
            new SavedQuery { Number = 12, Group = @"3. Aggregate functions (COUNT, SUM, AVG, GROUP BY, HAVING)", Title = @"Students with 2 or more violations (HAVING)", Description = @"HAVING filters the groups after they are counted.",
                Sql = @"SELECT s.StudentNumber, s.FullName, COUNT(*) AS Violations
FROM dbo.Students s
JOIN dbo.Violations v ON v.StudentId = s.StudentId
GROUP BY s.StudentNumber, s.FullName
HAVING COUNT(*) >= 2
ORDER BY Violations DESC;" },
            new SavedQuery { Number = 13, Group = @"3. Aggregate functions (COUNT, SUM, AVG, GROUP BY, HAVING)", Title = @"Violations per day (date functions)", Description = @"Counts the records for each date.",
                Sql = @"SELECT CAST(RecordedAt AS DATE) AS [Day], COUNT(*) AS Violations
FROM dbo.Violations
GROUP BY CAST(RecordedAt AS DATE)
ORDER BY [Day] DESC;" },
            new SavedQuery { Number = 14, Group = @"3. Aggregate functions (COUNT, SUM, AVG, GROUP BY, HAVING)", Title = @"Records added by each account", Description = @"How many records each HSG or OSSE account has added.",
                Sql = @"SELECT v.RecordedByRole AS Role,
       COALESCE(h.FullName, o.FullName) AS Account,
       COUNT(*) AS RecordsAdded
FROM dbo.Violations v
LEFT JOIN dbo.HsgAdmin  h ON v.RecordedByRole = N'HSG'  AND h.HsgId  = v.RecordedById
LEFT JOIN dbo.OsseAdmin o ON v.RecordedByRole = N'OSSE' AND o.OsseId = v.RecordedById
GROUP BY v.RecordedByRole, COALESCE(h.FullName, o.FullName)
ORDER BY RecordsAdded DESC;" },
            new SavedQuery { Number = 15, Group = @"3. Aggregate functions (COUNT, SUM, AVG, GROUP BY, HAVING)", Title = @"Fine summary (COUNT, MIN, MAX, AVG, SUM)", Description = @"One row that summarizes all the fines.",
                Sql = @"SELECT COUNT(*)    AS Fines,
       MIN(Amount) AS Smallest,
       MAX(Amount) AS Largest,
       AVG(Amount) AS Average,
       SUM(Amount) AS Total,
       SUM(CASE WHEN IsPaid = 1 THEN Amount ELSE 0 END) AS Collected
FROM dbo.Fines;" },
            new SavedQuery { Number = 16, Group = @"4. Subqueries (IN, EXISTS, NOT EXISTS)", Title = @"Students at or above the clearance limit", Description = @"The limit is read from the Settings table with a subquery.",
                Sql = @"SELECT s.StudentNumber, s.FullName, COUNT(v.ViolationId) AS Violations
FROM dbo.Students s
JOIN dbo.Violations v ON v.StudentId = s.StudentId
GROUP BY s.StudentNumber, s.FullName
HAVING COUNT(v.ViolationId) >= (SELECT CAST(SettingValue AS INT)
                                FROM dbo.Settings
                                WHERE SettingKey = N'ClearanceThreshold');" },
            new SavedQuery { Number = 17, Group = @"4. Subqueries (IN, EXISTS, NOT EXISTS)", Title = @"Students with an unpaid fine (EXISTS)", Description = @"EXISTS checks if at least one matching row is found.",
                Sql = @"SELECT s.StudentNumber, s.FullName
FROM dbo.Students s
WHERE EXISTS (SELECT 1
              FROM dbo.Violations v
              JOIN dbo.Fines f ON f.ViolationId = v.ViolationId
              WHERE v.StudentId = s.StudentId AND f.IsPaid = 0);" },
            new SavedQuery { Number = 18, Group = @"4. Subqueries (IN, EXISTS, NOT EXISTS)", Title = @"Records waiting for clearance (NOT EXISTS)", Description = @"Lanyard and No-ID records that have no clearance yet.",
                Sql = @"SELECT v.ViolationId, s.FullName AS Student, t.Name AS Violation, v.RecordedAt
FROM dbo.Violations v
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
WHERE t.IsFinable = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.ClearanceRecords c WHERE c.ViolationId = v.ViolationId)
ORDER BY v.RecordedAt;" },
            new SavedQuery { Number = 19, Group = @"4. Subqueries (IN, EXISTS, NOT EXISTS)", Title = @"Students above the average number of violations", Description = @"A subquery inside a subquery calculates the average.",
                Sql = @"SELECT s.FullName, COUNT(*) AS Violations
FROM dbo.Students s
JOIN dbo.Violations v ON v.StudentId = s.StudentId
GROUP BY s.StudentId, s.FullName
HAVING COUNT(*) > (SELECT AVG(CAST(Cnt AS DECIMAL(10, 2)))
                   FROM (SELECT COUNT(*) AS Cnt FROM dbo.Violations GROUP BY StudentId) x);" },
            new SavedQuery { Number = 20, Group = @"4. Subqueries (IN, EXISTS, NOT EXISTS)", Title = @"Violation types that were used (IN)", Description = @"IN with a subquery.",
                Sql = @"SELECT Name
FROM dbo.ViolationTypes
WHERE TypeId IN (SELECT DISTINCT TypeId FROM dbo.Violations);" },
            new SavedQuery { Number = 21, Group = @"5. CASE, dates, UNION", Title = @"Standing of each student (CASE)", Description = @"Good, Warning or Clearance hold, based on the number of violations.",
                Sql = @"SELECT s.StudentNumber, s.FullName, COUNT(v.ViolationId) AS Violations,
       CASE WHEN COUNT(v.ViolationId) = 0 THEN N'Good'
            WHEN COUNT(v.ViolationId) < (SELECT CAST(SettingValue AS INT)
                                         FROM dbo.Settings
                                         WHERE SettingKey = N'ClearanceThreshold') THEN N'Warning'
            ELSE N'Clearance hold' END AS Standing
FROM dbo.Students s
LEFT JOIN dbo.Violations v ON v.StudentId = s.StudentId
GROUP BY s.StudentNumber, s.FullName
ORDER BY Violations DESC;" },
            new SavedQuery { Number = 22, Group = @"5. CASE, dates, UNION", Title = @"Records HSG can still edit (DATEDIFF)", Description = @"Records made less than 12 hours ago (the number comes from the Settings table).",
                Sql = @"SELECT v.ViolationId, s.FullName AS Student, t.Name AS Violation, v.RecordedAt,
       DATEDIFF(MINUTE, v.RecordedAt, SYSDATETIME()) AS MinutesOld
FROM dbo.Violations v
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
WHERE DATEDIFF(MINUTE, v.RecordedAt, SYSDATETIME()) <
      (SELECT CAST(SettingValue AS INT) FROM dbo.Settings WHERE SettingKey = N'EditWindowHours') * 60
ORDER BY v.RecordedAt DESC;" },
            new SavedQuery { Number = 23, Group = @"5. CASE, dates, UNION", Title = @"All accounts (UNION ALL)", Description = @"Combines the three account tables into one list.",
                Sql = @"SELECT N'Student' AS AccountType, StudentNumber AS Number, FullName, Email FROM dbo.Students
UNION ALL
SELECT N'HSG', N'HSG-' + CAST(HsgId AS NVARCHAR(10)), FullName, Email FROM dbo.HsgAdmin
UNION ALL
SELECT N'OSSE', N'OSSE-' + CAST(OsseId AS NVARCHAR(10)), FullName, Email FROM dbo.OsseAdmin
ORDER BY AccountType, FullName;" },
            new SavedQuery { Number = 24, Group = @"5. CASE, dates, UNION", Title = @"Clearance history (JOIN)", Description = @"Who cleared which record, and when.",
                Sql = @"SELECT c.ClearedAt, s.FullName AS Student, t.Name AS Violation,
       o.FullName AS ClearedBy, c.Remarks
FROM dbo.ClearanceRecords c
JOIN dbo.Violations v ON v.ViolationId = c.ViolationId
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
JOIN dbo.OsseAdmin o ON o.OsseId = c.ClearedByOsseId
ORDER BY c.ClearedAt DESC;" },
            new SavedQuery { Number = 25, Group = @"5. CASE, dates, UNION", Title = @"Latest audit log (TOP)", Description = @"The last 20 actions saved by the system.",
                Sql = @"SELECT TOP 20 At, ActorRole, ActorId, Action, Details
FROM dbo.AuditLog
ORDER BY AuditId DESC;" },
            new SavedQuery { Number = 26, Group = @"6. Views and stored procedure", Title = @"View: vw_ViolationDetails", Description = @"A view hides the long JOIN. It is used like a table.",
                Sql = @"SELECT ViolationId, StudentNumber, StudentName, ViolationType,
       RecordedAt, RecordedByName, FineAmount, Status
FROM dbo.vw_ViolationDetails
ORDER BY RecordedAt DESC;" },
            new SavedQuery { Number = 27, Group = @"6. Views and stored procedure", Title = @"View: vw_StudentStanding", Description = @"Violation count, unpaid fines and standing of every student (current semester).",
                Sql = @"SELECT StudentNumber, FullName, TotalViolations, UnpaidFines, Standing
FROM dbo.vw_StudentStanding
ORDER BY TotalViolations DESC;" },
            new SavedQuery { Number = 28, Group = @"6. Views and stored procedure", Title = @"Stored procedure: usp_StudentSummary", Description = @"Runs a stored procedure for the first student.",
                Sql = @"DECLARE @id INT = (SELECT TOP 1 StudentId FROM dbo.Students ORDER BY StudentId);
EXEC dbo.usp_StudentSummary @StudentId = @id;" },
        ];
    }
}
