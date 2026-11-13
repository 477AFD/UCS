
-- 05 - INSERT, UPDATE and DELETE examples
-- ROLL BACK
BEGIN TRANSACTION;

-- INSERT: add a new student
INSERT INTO dbo.Students (StudentNumber, FullName, Email, PasswordHash)
VALUES (N'25-00001', N'Test Student', N'test.student@minsu.edu.ph', N'not-a-real-hash');
SELECT StudentId, StudentNumber, FullName FROM dbo.Students WHERE StudentNumber = N'25-00001';

-- INSERT: add a violation (No uniform) for that student, then its fine
DECLARE @s   INT = (SELECT StudentId FROM dbo.Students WHERE StudentNumber = N'25-00001');
DECLARE @sem INT = (SELECT TOP 1 SemesterId FROM dbo.Semesters WHERE IsCurrent = 1);
INSERT INTO dbo.Violations (StudentId, TypeId, SemesterId, RecordedByRole, RecordedById)
VALUES (@s, 1, @sem, N'HSG', 1);
DECLARE @v INT = SCOPE_IDENTITY();
INSERT INTO dbo.Fines (ViolationId, Amount, SetByRole, SetById)
VALUES (@v, (SELECT DefaultFine FROM dbo.ViolationTypes WHERE TypeId = 1), N'HSG', 1);

-- UPDATE: mark the fine as paid
UPDATE dbo.Fines SET IsPaid = 1, PaidAt = SYSDATETIME() WHERE ViolationId = @v;
SELECT * FROM dbo.vw_ViolationDetails WHERE ViolationId = @v;

-- UPDATE: change the violation type (only allowed in the first 12 hours in the program)
UPDATE dbo.Violations SET TypeId = 2 WHERE ViolationId = @v;

-- DELETE: remove the violation (its fine is deleted too: ON DELETE CASCADE)
DELETE FROM dbo.Violations WHERE ViolationId = @v;
SELECT COUNT(*) AS FinesLeftForThatViolation FROM dbo.Fines WHERE ViolationId = @v;

-- DELETE: remove the test student
DELETE FROM dbo.Students WHERE StudentId = @s;

ROLLBACK TRANSACTION;   -- change to COMMIT TRANSACTION to keep the changes
GO
