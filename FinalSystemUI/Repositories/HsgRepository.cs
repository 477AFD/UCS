using Microsoft.Data.SqlClient;
using FinalSystemUI.Database;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;

namespace FinalSystemUI.Repositories
{
    /// <summary>HSG accounts (guards and student officers). They are stored in the HsgAdmin table.</summary>
    public static class HsgRepository
    {
        private const string Columns = "HsgId, FullName, Email, PasswordHash, IsStudent, IsActive";

        private static HsgRow Map(SqlDataReader r)
        {
            return new HsgRow
            {
                HsgId = Db.Int(r, "HsgId"),
                FullName = Db.Str(r, "FullName"),
                Email = Db.Str(r, "Email"),
                PasswordHash = Db.Str(r, "PasswordHash"),
                IsStudent = Db.Bool(r, "IsStudent"),
                IsActive = Db.Bool(r, "IsActive")
            };
        }

        public static HsgRow FindByEmail(string email)
        {
            return Db.ReadOne("SELECT " + Columns + " FROM dbo.HsgAdmin WHERE Email = @e", Map, Db.P("@e", email));
        }

        public static HsgRow FindById(int id)
        {
            return Db.ReadOne("SELECT " + Columns + " FROM dbo.HsgAdmin WHERE HsgId = @i", Map, Db.P("@i", id));
        }

        public static bool EmailExists(string email)
        {
            return Db.ScalarInt("SELECT COUNT(*) FROM dbo.HsgAdmin WHERE Email = @e", Db.P("@e", email)) > 0;
        }

        public static int Insert(string fullName, string email, string password, bool isStudent)
        {
            return Db.ScalarInt(
                "INSERT INTO dbo.HsgAdmin (FullName, Email, PasswordHash, IsStudent) OUTPUT INSERTED.HsgId VALUES (@f, @e, @p, @s)",
                Db.P("@f", fullName), Db.P("@e", email), Db.P("@p", PasswordHasher.Hash(password)), Db.P("@s", isStudent));
        }

        public static void SetActive(int hsgId, bool active)
        {
            Db.Execute("UPDATE dbo.HsgAdmin SET IsActive = @a WHERE HsgId = @i", Db.P("@a", active), Db.P("@i", hsgId));
        }

        public static void SetPassword(int hsgId, string newPassword)
        {
            Db.Execute("UPDATE dbo.HsgAdmin SET PasswordHash = @p WHERE HsgId = @i",
                Db.P("@p", PasswordHasher.Hash(newPassword)), Db.P("@i", hsgId));
        }
    }
}
