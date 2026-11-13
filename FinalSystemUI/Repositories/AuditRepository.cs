using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FinalSystemUI.Database;
using FinalSystemUI.Models;

namespace FinalSystemUI.Repositories
{
    /// <summary>The audit trail: who did what and when.</summary>
    public static class AuditRepository
    {
        public static void Write(AppUser actor, string action, string details)
        {
            if (actor == null) return;
            Db.Execute(
                "INSERT INTO dbo.AuditLog (ActorRole, ActorId, Action, Details) VALUES (@r, @i, @a, @d)",
                Db.P("@r", actor.DbRole), Db.P("@i", actor.Id), Db.P("@a", action), Db.P("@d", Trim(details)));
        }

        public static void Write(SqlConnection c, SqlTransaction t, AppUser actor, string action, string details)
        {
            if (actor == null) return;
            Db.Execute(c, t,
                "INSERT INTO dbo.AuditLog (ActorRole, ActorId, Action, Details) VALUES (@r, @i, @a, @d)",
                Db.P("@r", actor.DbRole), Db.P("@i", actor.Id), Db.P("@a", action), Db.P("@d", Trim(details)));
        }

        /// <summary>The newest audit entries (OSSE reads them on the Audit Log window).</summary>
        public static List<AuditEntry> Recent(int top)
        {
            return Db.Read(
                "SELECT TOP (@n) a.At, a.ActorRole, COALESCE(h.FullName, o.FullName, N'-') AS ActorName, a.Action, a.Details " +
                "FROM dbo.AuditLog a " +
                "LEFT JOIN dbo.HsgAdmin h ON a.ActorRole = N'HSG' AND h.HsgId = a.ActorId " +
                "LEFT JOIN dbo.OsseAdmin o ON a.ActorRole = N'OSSE' AND o.OsseId = a.ActorId " +
                "ORDER BY a.AuditId DESC",
                r => new AuditEntry
                {
                    At = Db.Date(r, "At"),
                    Role = Db.Str(r, "ActorRole"),
                    ActorName = Db.Str(r, "ActorName"),
                    Action = Db.Str(r, "Action"),
                    Details = Db.Str(r, "Details")
                },
                Db.P("@n", top));
        }

        private static string Trim(string s)
        {
            if (s == null) return null;
            return s.Length > 500 ? s.Substring(0, 500) : s;
        }
    }
    public class AuditEntry
    {
        public DateTime At;
        public string Role;
        public string ActorName;
        public string Action;
        public string Details;
    }
}
