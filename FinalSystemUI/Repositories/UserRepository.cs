using System;
using System.Collections.Generic;
using FinalSystemUI.Database;
using FinalSystemUI.Models;

namespace FinalSystemUI.Repositories
{
    /// <summary>The list of student and HSG accounts shown on the OSSE Users screen.</summary>
    public static class UserRepository
    {
        private const string UnionSql =
            "SELECT N'Student' AS Kind, StudentId AS Id, StudentNumber AS Number, FullName, Email, IsActive FROM dbo.Students " +
            "UNION ALL " +
            "SELECT N'HSG', HsgId, N'HSG-' + RIGHT(N'000' + CAST(HsgId AS NVARCHAR(10)), 3), FullName, Email, IsActive FROM dbo.HsgAdmin";

        /// <summary>kind: "All", "Student" or "HSG".</summary>
        public static List<UserListItem> List(string kind)
        {
            string sql = "SELECT Kind, Id, Number, FullName, Email, IsActive FROM (" + UnionSql + ") u";
            if (kind == "Student" || kind == "HSG") sql += " WHERE Kind = @k";
            sql += " ORDER BY Kind, FullName";

            return Db.Read(sql,
                r => new UserListItem
                {
                    Kind = Db.Str(r, "Kind"),
                    Id = Db.Int(r, "Id"),
                    Number = Db.Str(r, "Number"),
                    FullName = Db.Str(r, "FullName"),
                    Email = Db.Str(r, "Email"),
                    IsActive = Db.Bool(r, "IsActive")
                },
                Db.P("@k", kind));
        }

        /// <summary>How many violation records are tied to this account (as the student, or as the HSG who recorded or edited).</summary>
        public static int ViolationCount(string kind, int id)
        {
            if (kind == "Student")
                return Db.ScalarInt("SELECT COUNT(*) FROM dbo.Violations WHERE StudentId = @i", Db.P("@i", id));
            return Db.ScalarInt(
                "SELECT COUNT(*) FROM dbo.Violations WHERE (RecordedByRole = N'HSG' AND RecordedById = @i) " +
                "OR (LastEditedByRole = N'HSG' AND LastEditedById = @i)", Db.P("@i", id));
        }

        public static void Delete(string kind, int id)
        {
            if (kind == "Student") Db.Execute("DELETE FROM dbo.Students WHERE StudentId = @i", Db.P("@i", id));
            else if (kind == "HSG") Db.Execute("DELETE FROM dbo.HsgAdmin WHERE HsgId = @i", Db.P("@i", id));
            else throw new ArgumentException("Unknown account type.");
        }

        public static UserCounts Counts()
        {
            UserCounts c = new UserCounts();
            c.Students = Db.ScalarInt("SELECT COUNT(*) FROM dbo.Students");
            c.Hsg = Db.ScalarInt("SELECT COUNT(*) FROM dbo.HsgAdmin");
            c.Active = Db.ScalarInt("SELECT (SELECT COUNT(*) FROM dbo.Students WHERE IsActive = 1) + (SELECT COUNT(*) FROM dbo.HsgAdmin WHERE IsActive = 1)");
            c.Disabled = Db.ScalarInt("SELECT (SELECT COUNT(*) FROM dbo.Students WHERE IsActive = 0) + (SELECT COUNT(*) FROM dbo.HsgAdmin WHERE IsActive = 0)");
            return c;
        }
    }
}
