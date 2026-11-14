using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;
using FinalSystemUI.Database;
using FinalSystemUI.Models;

namespace FinalSystemUI.Repositories
{
    /// <summary>
    /// Everything about violation records, their fines and their clearance.
    /// The business rules (who may do what) are checked in RecordService.
    /// A few rules are ALSO enforced here in SQL (the 12-hour edit window, paid fines), so they cannot be skipped.
    /// </summary>
    public static class ViolationRepository
    {
        private const string SelectSql =
            "SELECT v.ViolationId, v.StudentId, s.StudentNumber, s.FullName AS StudentName, " +
            "       v.TypeId, t.Name AS TypeName, t.IsFinable, v.RecordedAt, v.RecordedByRole, v.RecordedById, " +
            "       CASE v.RecordedByRole WHEN N'HSG' THEN h.FullName WHEN N'OSSE' THEN o.FullName END AS RecordedByName, " +
            "       v.Remarks, f.Amount AS FineAmount, ISNULL(f.IsPaid, 0) AS IsPaid, f.PaidAt, c.ClearedAt, " +
            "       SYSDATETIME() AS ServerNow " +
            "FROM dbo.Violations v " +
            "JOIN dbo.Students s ON s.StudentId = v.StudentId " +
            "JOIN dbo.ViolationTypes t ON t.TypeId = v.TypeId " +
            "LEFT JOIN dbo.HsgAdmin h ON v.RecordedByRole = N'HSG' AND h.HsgId = v.RecordedById " +
            "LEFT JOIN dbo.OsseAdmin o ON v.RecordedByRole = N'OSSE' AND o.OsseId = v.RecordedById " +
            "LEFT JOIN dbo.Fines f ON f.ViolationId = v.ViolationId " +
            "LEFT JOIN dbo.ClearanceRecords c ON c.ViolationId = v.ViolationId ";

        private static ViolationRow Map(SqlDataReader r)
        {
            return new ViolationRow
            {
                ViolationId = Db.Int(r, "ViolationId"),
                StudentId = Db.Int(r, "StudentId"),
                StudentNumber = Db.Str(r, "StudentNumber"),
                StudentName = Db.Str(r, "StudentName"),
                TypeId = Db.Int(r, "TypeId"),
                TypeName = Db.Str(r, "TypeName"),
                IsFinable = Db.Bool(r, "IsFinable"),
                RecordedAt = Db.Date(r, "RecordedAt"),
                RecordedByRole = Db.Str(r, "RecordedByRole"),
                RecordedById = Db.Int(r, "RecordedById"),
                RecordedByName = Db.Str(r, "RecordedByName"),
                Remarks = Db.Str(r, "Remarks"),
                FineAmount = Db.DecN(r, "FineAmount"),
                IsPaid = Db.Bool(r, "IsPaid"),
                PaidAt = Db.DateN(r, "PaidAt"),
                ClearedAt = Db.DateN(r, "ClearedAt"),
                ServerNow = Db.Date(r, "ServerNow")
            };
        }

        // ------------------------------------------------------------ reading

        public static List<ViolationRow> Query(ViolationFilter f)
        {
            List<string> where = [];
            List<SqlParameter> ps = [];

            if (f != null)
            {
                if (f.StudentId.HasValue) { where.Add("v.StudentId = @sid"); ps.Add(Db.P("@sid", f.StudentId.Value)); }
                if (f.SemesterId.HasValue) { where.Add("v.SemesterId = @sem"); ps.Add(Db.P("@sem", f.SemesterId.Value)); }

                if (f.TypeIds != null && f.TypeIds.Length > 0)
                {
                    // Only whole numbers are joined here, so this is safe.
                    StringBuilder sb = new();
                    foreach (int id in f.TypeIds)
                    {
                        if (sb.Length > 0) sb.Append(',');
                        sb.Append(id.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                    where.Add("v.TypeId IN (" + sb + ")");
                }

                if (!string.IsNullOrWhiteSpace(f.Search))
                {
                    where.Add("(s.FullName LIKE @q ESCAPE '\\' OR s.StudentNumber LIKE @q ESCAPE '\\')");
                    ps.Add(Db.P("@q", "%" + EscapeLike(f.Search.Trim()) + "%"));
                }

                if (f.Clearance == 1) where.Add("t.IsFinable = 0 AND c.ViolationId IS NULL");
                if (f.Clearance == 2) where.Add("c.ViolationId IS NOT NULL");
                if (f.FineState == 1) where.Add("t.IsFinable = 1 AND ISNULL(f.IsPaid, 0) = 0");
                if (f.FineState == 2) where.Add("t.IsFinable = 1 AND f.IsPaid = 1");
                if (f.OnlyFined) where.Add("t.IsFinable = 1");
                if (f.OnlyNeedsClearance) where.Add("t.IsFinable = 0");
            }

            string sql = SelectSql;
            if (where.Count > 0) sql += "WHERE " + string.Join(" AND ", where) + " ";
            sql += "ORDER BY v.RecordedAt DESC, v.ViolationId DESC";
            return Db.Read(sql, Map, [.. ps]);
        }

        public static ViolationRow Get(int violationId)
        {
            return Db.ReadOne(SelectSql + "WHERE v.ViolationId = @id", Map, Db.P("@id", violationId));
        }

        private static string EscapeLike(string s)
        {
            return s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
        }

        // ------------------------------------------------------------ numbers for the cards

        public static StudentSummary SummaryForStudent(int studentId, int semesterId, int threshold)
        {
            StudentSummary s = new()
            {
                Threshold = threshold
            };

            List<int[]> counts = Db.Read(
                "SELECT COUNT(*) AS Total, " +
                "  ISNULL(SUM(CASE WHEN v.TypeId IN (1, 2) THEN 1 ELSE 0 END), 0) AS Uniform, " +
                "  ISNULL(SUM(CASE WHEN v.TypeId = 3 THEN 1 ELSE 0 END), 0) AS Lanyard, " +
                "  ISNULL(SUM(CASE WHEN v.TypeId = 4 THEN 1 ELSE 0 END), 0) AS NoId, " +
                "  ISNULL(SUM(CASE WHEN v.TypeId IN (3, 4) AND c.ViolationId IS NULL THEN 1 ELSE 0 END), 0) AS Pending " +
                "FROM dbo.Violations v LEFT JOIN dbo.ClearanceRecords c ON c.ViolationId = v.ViolationId " +
                "WHERE v.StudentId = @s AND v.SemesterId = @m",
                r => new int[] { Db.Int(r, "Total"), Db.Int(r, "Uniform"), Db.Int(r, "Lanyard"), Db.Int(r, "NoId"), Db.Int(r, "Pending") },
                Db.P("@s", studentId), Db.P("@m", semesterId));
            if (counts.Count > 0)
            {
                s.TotalViolations = counts[0][0];
                s.UniformViolations = counts[0][1];
                s.LanyardViolations = counts[0][2];
                s.NoIdViolations = counts[0][3];
                s.PendingClearance = counts[0][4];
            }

            List<decimal[]> fines = Db.Read(
                "SELECT ISNULL(SUM(f.Amount), 0) AS Total, " +
                "  ISNULL(SUM(CASE WHEN f.IsPaid = 0 THEN f.Amount ELSE 0 END), 0) AS Unpaid, " +
                "  ISNULL(SUM(CASE WHEN f.IsPaid = 1 THEN f.Amount ELSE 0 END), 0) AS Paid, " +
                "  COUNT(*) AS Cnt " +
                "FROM dbo.Fines f JOIN dbo.Violations v ON v.ViolationId = f.ViolationId " +
                "WHERE v.StudentId = @s AND v.SemesterId = @m",
                r => new decimal[] { r.GetDecimal(0), r.GetDecimal(1), r.GetDecimal(2), Convert.ToDecimal(r.GetInt32(3)) },
                Db.P("@s", studentId), Db.P("@m", semesterId));
            if (fines.Count > 0)
            {
                s.TotalFines = fines[0][0];
                s.UnpaidFines = fines[0][1];
                s.PaidFines = fines[0][2];
                s.FinedCount = (int)fines[0][3];
            }
            return s;
        }

        public static RecordCounts CountsForSemester(int semesterId)
        {
            RecordCounts c = new()
            {
                NoUniform = TypeCount(semesterId, 1),
                InappropriateAttire = TypeCount(semesterId, 2),
                InvalidLanyard = TypeCount(semesterId, 3),
                NoId = TypeCount(semesterId, 4),
                TotalRecords = Db.ScalarInt("SELECT COUNT(*) FROM dbo.Violations WHERE SemesterId = @m", Db.P("@m", semesterId)),
                ForClearance = Db.ScalarInt(
                    "SELECT COUNT(*) FROM dbo.Violations v WHERE v.SemesterId = @m AND v.TypeId IN (3, 4) " +
                    "AND NOT EXISTS (SELECT 1 FROM dbo.ClearanceRecords c WHERE c.ViolationId = v.ViolationId)",
                    Db.P("@m", semesterId)),
                ClearedToday = Db.ScalarInt(
                    "SELECT COUNT(*) FROM dbo.ClearanceRecords c JOIN dbo.Violations v ON v.ViolationId = c.ViolationId " +
                    "WHERE v.SemesterId = @m AND CAST(c.ClearedAt AS DATE) = CAST(SYSDATETIME() AS DATE)",
                    Db.P("@m", semesterId)),
                UnpaidFines = Db.ScalarDecimal(
                    "SELECT ISNULL(SUM(f.Amount), 0) FROM dbo.Fines f JOIN dbo.Violations v ON v.ViolationId = f.ViolationId " +
                    "WHERE v.SemesterId = @m AND f.IsPaid = 0", Db.P("@m", semesterId)),
                CollectedFines = Db.ScalarDecimal(
                    "SELECT ISNULL(SUM(f.Amount), 0) FROM dbo.Fines f JOIN dbo.Violations v ON v.ViolationId = f.ViolationId " +
                    "WHERE v.SemesterId = @m AND f.IsPaid = 1", Db.P("@m", semesterId))
            };
            return c;
        }

        private static int TypeCount(int semesterId, int typeId)
        {
            return Db.ScalarInt("SELECT COUNT(*) FROM dbo.Violations WHERE SemesterId = @m AND TypeId = @t",
                Db.P("@m", semesterId), Db.P("@t", typeId));
        }

        // ------------------------------------------------------------ writing

        /// <summary>Saves a violation (and its fine, if the type has one) in one transaction.</summary>
        public static int Add(NewViolation nv, AppUser actor)
        {
            int newId = 0;
            Db.InTransaction((c, t) =>
            {
                object o = Db.Scalar(c, t,
                    "INSERT INTO dbo.Violations (StudentId, TypeId, SemesterId, RecordedAt, RecordedByRole, RecordedById, Remarks) " +
                    "OUTPUT INSERTED.ViolationId " +
                    "VALUES (@s, @t, @m, ISNULL(@at, SYSDATETIME()), @r, @b, @rem)",
                    Db.P("@s", nv.StudentId), Db.P("@t", nv.TypeId), Db.P("@m", nv.SemesterId),
                    Db.DateTime2("@at", nv.RecordedAt),
                    Db.P("@r", nv.RecordedByRole), Db.P("@b", nv.RecordedById),
                    Db.P("@rem", string.IsNullOrWhiteSpace(nv.Remarks) ? null : nv.Remarks.Trim()));
                newId = Convert.ToInt32(o);

                if (ViolationTypeIds.IsFinable(nv.TypeId))
                {
                    Db.Execute(c, t,
                        "INSERT INTO dbo.Fines (ViolationId, Amount, SetByRole, SetById) VALUES (@v, @a, @r, @b)",
                        Db.P("@v", newId), Db.Money("@a", nv.FineAmount),
                        Db.P("@r", nv.RecordedByRole), Db.P("@b", nv.RecordedById));
                }

                AuditRepository.Write(c, t, actor, "ADD_RECORD",
                    "Violation " + newId + ": " + ViolationTypeIds.NameOf(nv.TypeId) + " for student id " + nv.StudentId);
            });
            return newId;
        }

        /// <summary>
        /// Changes the type, fine and remarks of a record.
        /// windowHours &gt; 0 means: only allowed if the record was made less than that many hours ago.
        /// </summary>
        public static void UpdateRecord(int violationId, int typeId, decimal fineAmount, string remarks, AppUser actor, int windowHours)
        {
            Db.InTransaction((c, t) =>
            {
                int rows = Db.Execute(c, t,
                    "UPDATE dbo.Violations SET TypeId = @t, Remarks = @rem, LastEditedAt = SYSDATETIME(), " +
                    "       LastEditedByRole = @r, LastEditedById = @b " +
                    "WHERE ViolationId = @id AND (@mins = 0 OR DATEDIFF(MINUTE, RecordedAt, SYSDATETIME()) < @mins)",
                    Db.P("@t", typeId), Db.P("@rem", string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim()),
                    Db.P("@r", actor.DbRole), Db.P("@b", actor.Id),
                    Db.P("@id", violationId), Db.P("@mins", windowHours * 60));
                if (rows == 0)
                    throw new InvalidOperationException("This record is locked (older than " + windowHours + " hours) or no longer exists.");

                if (ViolationTypeIds.IsFinable(typeId))
                {
                    int n = Db.Execute(c, t,
                        "UPDATE dbo.Fines SET Amount = @a, SetByRole = @r, SetById = @b, SetAt = SYSDATETIME() WHERE ViolationId = @id",
                        Db.Money("@a", fineAmount), Db.P("@r", actor.DbRole), Db.P("@b", actor.Id), Db.P("@id", violationId));
                    if (n == 0)
                    {
                        Db.Execute(c, t,
                            "INSERT INTO dbo.Fines (ViolationId, Amount, SetByRole, SetById) VALUES (@id, @a, @r, @b)",
                            Db.P("@id", violationId), Db.Money("@a", fineAmount), Db.P("@r", actor.DbRole), Db.P("@b", actor.Id));
                    }
                    Db.Execute(c, t, "DELETE FROM dbo.ClearanceRecords WHERE ViolationId = @id", Db.P("@id", violationId));
                }
                else
                {
                    Db.Execute(c, t, "DELETE FROM dbo.Fines WHERE ViolationId = @id", Db.P("@id", violationId));
                }

                AuditRepository.Write(c, t, actor, "EDIT_RECORD",
                    "Violation " + violationId + " is now " + ViolationTypeIds.NameOf(typeId));
            });
        }

        public static void SetFine(int violationId, decimal amount, AppUser actor)
        {
            int rows = Db.Execute(
                "UPDATE dbo.Fines SET Amount = @a, SetByRole = @r, SetById = @b, SetAt = SYSDATETIME() " +
                "WHERE ViolationId = @id AND IsPaid = 0",
                Db.Money("@a", amount), Db.P("@r", actor.DbRole), Db.P("@b", actor.Id), Db.P("@id", violationId));
            if (rows == 0)
                throw new InvalidOperationException("This record has no fine, or its fine is already paid.");
            AuditRepository.Write(actor, "SET_FINE", "Violation " + violationId + " fine set to " + amount.ToString("0.00"));
        }

        public static void MarkPaid(int violationId, AppUser actor)
        {
            int rows = Db.Execute(
                "UPDATE dbo.Fines SET IsPaid = 1, PaidAt = SYSDATETIME() WHERE ViolationId = @id AND IsPaid = 0",
                Db.P("@id", violationId));
            if (rows == 0)
                throw new InvalidOperationException("This record has no fine, or its fine is already paid.");
            AuditRepository.Write(actor, "MARK_PAID", "Violation " + violationId);
        }

        public static void Delete(int violationId, AppUser actor)
        {
            Db.InTransaction((c, t) =>
            {
                int rows = Db.Execute(c, t, "DELETE FROM dbo.Violations WHERE ViolationId = @id", Db.P("@id", violationId));
                if (rows == 0) throw new InvalidOperationException("This record no longer exists.");
                AuditRepository.Write(c, t, actor, "DELETE_RECORD", "Violation " + violationId);
            });
        }

        // ------------------------------------------------------------ clearance

        public static void MarkCleared(int violationId, AppUser actor, string remarks)
        {
            Db.InTransaction((c, t) =>
            {
                int rows = Db.Execute(c, t,
                    "INSERT INTO dbo.ClearanceRecords (ViolationId, ClearedByOsseId, Remarks) " +
                    "SELECT v.ViolationId, @o, @rem FROM dbo.Violations v " +
                    "WHERE v.ViolationId = @id AND v.TypeId IN (3, 4) " +
                    "AND NOT EXISTS (SELECT 1 FROM dbo.ClearanceRecords x WHERE x.ViolationId = v.ViolationId)",
                    Db.P("@o", actor.Id), Db.P("@rem", string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim()),
                    Db.P("@id", violationId));
                if (rows == 0)
                    throw new InvalidOperationException("This record cannot be cleared. It is not a lanyard or no-ID record, or it is already cleared.");
                AuditRepository.Write(c, t, actor, "CLEAR_RECORD", "Violation " + violationId);
            });
        }
    }
}
