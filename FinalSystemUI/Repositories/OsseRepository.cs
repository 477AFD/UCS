using System.Data.SqlClient;
using FinalSystemUI.Database;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;

namespace FinalSystemUI.Repositories
{
    /// <summary>OSSE (admin) accounts. They are stored in the OsseAdmin table and log in with email + password.</summary>
    public static class OsseRepository
    {
        private const string Columns = "OsseId, FullName, Email, PasswordHash, IsActive";

        private static OsseRow Map(SqlDataReader r)
        {
            return new OsseRow
            {
                OsseId = Db.Int(r, "OsseId"),
                FullName = Db.Str(r, "FullName"),
                Email = Db.Str(r, "Email"),
                PasswordHash = Db.Str(r, "PasswordHash"),
                IsActive = Db.Bool(r, "IsActive")
            };
        }

        public static OsseRow FindByEmail(string email)
        {
            return Db.ReadOne("SELECT " + Columns + " FROM dbo.OsseAdmin WHERE Email = @e", Map, Db.P("@e", email));
        }

        public static int Insert(string fullName, string email, string password)
        {
            return Db.ScalarInt(
                "INSERT INTO dbo.OsseAdmin (FullName, Email, PasswordHash) OUTPUT INSERTED.OsseId VALUES (@f, @e, @p)",
                Db.P("@f", fullName), Db.P("@e", email), Db.P("@p", PasswordHasher.Hash(password)));
        }

        public static void SetPassword(int osseId, string newPassword)
        {
            Db.Execute("UPDATE dbo.OsseAdmin SET PasswordHash = @p WHERE OsseId = @i",
                Db.P("@p", PasswordHasher.Hash(newPassword)), Db.P("@i", osseId));
        }
    }
}
