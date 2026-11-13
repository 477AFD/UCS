
-- The same queries are in the program: log in as OSSE and click "SQL Queries" in the sidebar.
-- Open it on the UniformComplianceDB connection: Server Explorer > right-click the database > New Query.

-- Q01. Connection information
-- Shows which server and database file the program is connected to.
SELECT DB_NAME()                         AS DatabaseName,
       @@SERVERNAME                      AS ServerName,
       (SELECT TOP 1 physical_name FROM sys.database_files WHERE type_desc = N'ROWS') AS DataFile,
       SYSDATETIME()                     AS ServerTime,
       LEFT(@@VERSION, 60)               AS SqlServerVersion;

-- Q02. All students (SELECT, ORDER BY)
-- Ishoshow Lahat ng student, nakaayos ayon sa pangalan (ORDER BY)
SELECT StudentId, StudentNumber, FullName, Email, IsActive, CreatedAt
FROM dbo.Students
ORDER BY FullName;

-- Q03. Violation types and their fines
-- Yung apat na types ng violation at ang fine nila
SELECT TypeId, Name, IsFinable, DefaultFine
FROM dbo.ViolationTypes
ORDER BY TypeId;

-- Q04. Active HSG accounts (WHERE)
-- HSG accounts na naka-enable (WHERE)
SELECT HsgId, FullName, Email, IsStudent, CreatedAt
FROM dbo.HsgAdmin
WHERE IsActive = 1
ORDER BY FullName;

-- Q05. Search students by name (LIKE)
-- Hahanapin ang student na may “Cruz” sa name (LIKE) .
SELECT StudentNumber, FullName, Email
FROM dbo.Students
WHERE FullName LIKE N'%Cruz%';




-- 2. JOIN 

-- Q06. Violations with student and type (INNER JOIN)
-- Violation with names ng student at ang types (INNER JOIN)
SELECT v.ViolationId, s.StudentNumber, s.FullName AS Student,
       t.Name AS Violation, v.RecordedAt
FROM dbo.Violations v
INNER JOIN dbo.Students s ON s.StudentId = v.StudentId
INNER JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
ORDER BY v.RecordedAt DESC;

-- Q07. Kung sino ang nag-record (HSG o OSSE) (LEFT JOIN)
SELECT v.ViolationId, s.FullName AS Student, t.Name AS Violation,
       v.RecordedByRole, COALESCE(h.FullName, o.FullName) AS RecordedBy
FROM dbo.Violations v
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
LEFT JOIN dbo.HsgAdmin  h ON v.RecordedByRole = N'HSG'  AND h.HsgId  = v.RecordedById
LEFT JOIN dbo.OsseAdmin o ON v.RecordedByRole = N'OSSE' AND o.OsseId = v.RecordedById
ORDER BY v.ViolationId;

-- Q8 Mga student na walang violation (LEFT JOIN ... IS NULL)
SELECT s.StudentNumber, s.FullName
FROM dbo.Students s
LEFT JOIN dbo.Violations v ON v.StudentId = s.StudentId
WHERE v.ViolationId IS NULL;

-- Q09. Fines with student and violation
-- Mga fine with student at ang kanilang violation, kung mag kano o kung bayad na.
SELECT s.FullName AS Student, t.Name AS Violation, f.Amount, f.IsPaid, f.PaidAt
FROM dbo.Fines f
JOIN dbo.Violations v ON v.ViolationId = f.ViolationId
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
ORDER BY s.FullName;


-- 3. Aggregate functions (COUNT, SUM, AVG, GROUP BY, HAVING)

-- Q10. Violations per type (GROUP BY, COUNT)
-- ilang records kada violation meron 
SELECT t.Name AS Violation, COUNT(v.ViolationId) AS Total
FROM dbo.ViolationTypes t
LEFT JOIN dbo.Violations v ON v.TypeId = t.TypeId
GROUP BY t.TypeId, t.Name
ORDER BY t.TypeId;

-- Q11. Total fines per student (SUM, GROUP BY)
-- The total and the unpaid amount of fines of each student.
SELECT s.StudentNumber, s.FullName,
       SUM(f.Amount) AS TotalFines,
       SUM(CASE WHEN f.IsPaid = 0 THEN f.Amount ELSE 0 END) AS Unpaid
FROM dbo.Students s
JOIN dbo.Violations v ON v.StudentId = s.StudentId
JOIN dbo.Fines f ON f.ViolationId = v.ViolationId
GROUP BY s.StudentNumber, s.FullName
ORDER BY TotalFines DESC;

-- Q12 Mga student na may 2 o higit pang violation (HAVING)
-- HAVING filters the groups after they are counted.
SELECT s.StudentNumber, s.FullName, COUNT(*) AS Violations
FROM dbo.Students s
JOIN dbo.Violations v ON v.StudentId = s.StudentId
GROUP BY s.StudentNumber, s.FullName
HAVING COUNT(*) >= 2
ORDER BY Violations DESC;

-- Q13 Ilan ang violation bawat araw (date)
-- Counts the records for each date.
SELECT CAST(RecordedAt AS DATE) AS [Day], COUNT(*) AS Violations
FROM dbo.Violations
GROUP BY CAST(RecordedAt AS DATE)
ORDER BY [Day] DESC;

-- Q14. Records added by each account
-- How many records each HSG or OSSE account has added.
SELECT v.RecordedByRole AS Role,
       COALESCE(h.FullName, o.FullName) AS Account,
       COUNT(*) AS RecordsAdded
FROM dbo.Violations v
LEFT JOIN dbo.HsgAdmin  h ON v.RecordedByRole = N'HSG'  AND h.HsgId  = v.RecordedById
LEFT JOIN dbo.OsseAdmin o ON v.RecordedByRole = N'OSSE' AND o.OsseId = v.RecordedById
GROUP BY v.RecordedByRole, COALESCE(h.FullName, o.FullName)
ORDER BY RecordsAdded DESC;

-- Q15. Summary ng fines: pinakamaliit, pinakamalaki, average, kabuuan (MIN, MAX, AVG)
SELECT COUNT(*)    AS Fines,
       MIN(Amount) AS Smallest,
       MAX(Amount) AS Largest,
       AVG(Amount) AS Average,
       SUM(Amount) AS Total,
       SUM(CASE WHEN IsPaid = 1 THEN Amount ELSE 0 END) AS Collected
FROM dbo.Fines;


-- 4. Subqueries (IN, EXISTS, NOT EXISTS)


-- The limit is read from the Settings table with a subquery.
-- Q16. Mga student na umabot sa limitasyon ng clearance
SELECT s.StudentNumber, s.FullName, COUNT(v.ViolationId) AS Violations
FROM dbo.Students s
JOIN dbo.Violations v ON v.StudentId = s.StudentId
GROUP BY s.StudentNumber, s.FullName
HAVING COUNT(v.ViolationId) >= (SELECT CAST(SettingValue AS INT)
                                FROM dbo.Settings
                                WHERE SettingKey = N'ClearanceThreshold');


-- EXISTS checks if at least one matching row is found.
-- Q17. Students with an unpaid fine (EXISTS)
SELECT s.StudentNumber, s.FullName
FROM dbo.Students s
WHERE EXISTS (SELECT 1
              FROM dbo.Violations v
              JOIN dbo.Fines f ON f.ViolationId = v.ViolationId
              WHERE v.StudentId = s.StudentId AND f.IsPaid = 0);

-- Q18. Records waiting for clearance (NOT EXISTS)
-- Lanyard and No-ID records that have no clearance yet.
SELECT v.ViolationId, s.FullName AS Student, t.Name AS Violation, v.RecordedAt
FROM dbo.Violations v
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
WHERE t.IsFinable = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.ClearanceRecords c WHERE c.ViolationId = v.ViolationId)
ORDER BY v.RecordedAt;

-- Q19. Students above the average number of violations
-- A subquery inside a subquery calculates the average.
SELECT s.FullName, COUNT(*) AS Violations
FROM dbo.Students s
JOIN dbo.Violations v ON v.StudentId = s.StudentId
GROUP BY s.StudentId, s.FullName
HAVING COUNT(*) > (SELECT AVG(CAST(Cnt AS DECIMAL(10, 2)))
                   FROM (SELECT COUNT(*) AS Cnt FROM dbo.Violations GROUP BY StudentId) x);


-- IN with a subquery.
-- Q20. Violation types that were used (IN)
SELECT Name
FROM dbo.ViolationTypes
WHERE TypeId IN (SELECT DISTINCT TypeId FROM dbo.Violations);


-- 5. CASE, dates, UNION

-- Q21. Standing of each student (CASE)
-- Standing ng kada students. Good, Warning or Clearance hold, based on the number of violations.
SELECT s.StudentNumber, s.FullName, COUNT(v.ViolationId) AS Violations,
       CASE WHEN COUNT(v.ViolationId) = 0 THEN N'Good'
            WHEN COUNT(v.ViolationId) < (SELECT CAST(SettingValue AS INT)
                                         FROM dbo.Settings
                                         WHERE SettingKey = N'ClearanceThreshold') THEN N'Warning'
            ELSE N'Clearance hold' END AS Standing
FROM dbo.Students s
LEFT JOIN dbo.Violations v ON v.StudentId = s.StudentId
GROUP BY s.StudentNumber, s.FullName
ORDER BY Violations DESC;


-- Q22. Records HSG can still edit (Records made less than 12 hours ago) (DATEDIFF numbers -from settings table)

SELECT v.ViolationId, s.FullName AS Student, t.Name AS Violation, v.RecordedAt,
       DATEDIFF(MINUTE, v.RecordedAt, SYSDATETIME()) AS MinutesOld
FROM dbo.Violations v
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
WHERE DATEDIFF(MINUTE, v.RecordedAt, SYSDATETIME()) <
      (SELECT CAST(SettingValue AS INT) FROM dbo.Settings WHERE SettingKey = N'EditWindowHours') * 60
ORDER BY v.RecordedAt DESC;

-- Q23. All accounts (UNION ALL)
-- Combines the three account tables into one list.
SELECT N'Student' AS AccountType, StudentNumber AS Number, FullName, Email FROM dbo.Students
UNION ALL
SELECT N'HSG', N'HSG-' + CAST(HsgId AS NVARCHAR(10)), FullName, Email FROM dbo.HsgAdmin
UNION ALL
SELECT N'OSSE', N'OSSE-' + CAST(OsseId AS NVARCHAR(10)), FullName, Email FROM dbo.OsseAdmin
ORDER BY AccountType, FullName;

-- Q24. 
-- Clearance history ,sino ang nag-clear at kailan (JOIN)
SELECT c.ClearedAt, s.FullName AS Student, t.Name AS Violation,
       o.FullName AS ClearedBy, c.Remarks
FROM dbo.ClearanceRecords c
JOIN dbo.Violations v ON v.ViolationId = c.ViolationId
JOIN dbo.Students s ON s.StudentId = v.StudentId
JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId
JOIN dbo.OsseAdmin o ON o.OsseId = c.ClearedByOsseId
ORDER BY c.ClearedAt DESC;

-- Q25. Latest audit log (TOP)
-- The last 20 actions saved by the system.
SELECT TOP 20 At, ActorRole, ActorId, Action, Details
FROM dbo.AuditLog
ORDER BY AuditId DESC;


-- 6. Views and stored procedure


-- Gumagamit ng view na vw_ViolationDetails (naka-save na JOIN) 
-- A view hides the long JOIN. It is used like a table.

-- Q26. View: vw_ViolationDetails
SELECT ViolationId, StudentNumber, StudentName, ViolationType,
       RecordedAt, RecordedByName, FineAmount, Status
FROM dbo.vw_ViolationDetails
ORDER BY RecordedAt DESC;

-- Q27. View: vw_StudentStanding
-- Violation count, unpaid fines and standing of every student (current semester).
SELECT StudentNumber, FullName, TotalViolations, UnpaidFines, Standing
FROM dbo.vw_StudentStanding
ORDER BY TotalViolations DESC;

-- Q28. Stored procedure: usp_StudentSummary
-- Runs a stored procedure for the first student.
DECLARE @id INT = (SELECT TOP 1 StudentId FROM dbo.Students ORDER BY StudentId);
EXEC dbo.usp_StudentSummary @StudentId = @id;

