using System.Collections.Generic;
using FinalSystemUI.Database;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;

namespace FinalSystemUI.Repositories
{
    public static class StudentRepository
    {
        private const string Columns = "StudentId, StudentNumber, FullName, Email, PasswordHash, IsActive";

        private static StudentRow Map(System.Data.SqlClient.SqlDataReader r)
        {
            return new StudentRow
            {
                StudentId = Db.Int(r, "StudentId"),
                StudentNumber = Db.Str(r, "StudentNumber"),
                FullName = Db.Str(r, "FullName"),
                Email = Db.Str(r, "Email"),
                PasswordHash = Db.Str(r, "PasswordHash"),
                IsActive = Db.Bool(r, "IsActive")
            };
        }

        public static StudentRow FindForLogin(string studentNumber, string email)
        {
            return Db.ReadOne(
                "SELECT " + Columns + " FROM dbo.Students WHERE StudentNumber = @n AND Email = @e",
                Map, Db.P("@n", studentNumber), Db.P("@e", email));
        }

        public static StudentRow FindByEmail(string email)
        {
            return Db.ReadOne(
                "SELECT " + Columns + " FROM dbo.Students WHERE Email = @e",
                Map, Db.P("@e", email));
        }

        public static StudentRow FindByNumber(string studentNumber)
        {
            return Db.ReadOne(
                "SELECT " + Columns + " FROM dbo.Students WHERE StudentNumber = @n",
                Map, Db.P("@n", studentNumber));
        }

        public static StudentRow FindById(int id)
        {
            return Db.ReadOne(
                "SELECT " + Columns + " FROM dbo.Students WHERE StudentId = @i",
                Map, Db.P("@i", id));
        }

        public static bool NumberOrEmailExists(string studentNumber, string email)
        {
            return Db.ScalarInt("SELECT COUNT(*) FROM dbo.Students WHERE StudentNumber = @n OR Email = @e",
                Db.P("@n", studentNumber), Db.P("@e", email)) > 0;
        }

        public static int Insert(string studentNumber, string fullName, string email, string password)
        {
            return Db.ScalarInt(
                "INSERT INTO dbo.Students (StudentNumber, FullName, Email, PasswordHash) " +
                "OUTPUT INSERTED.StudentId VALUES (@n, @f, @e, @p)",
                Db.P("@n", studentNumber), Db.P("@f", fullName), Db.P("@e", email),
                Db.P("@p", PasswordHasher.Hash(password)));
        }

        public static void SetActive(int studentId, bool active)
        {
            Db.Execute("UPDATE dbo.Students SET IsActive = @a WHERE StudentId = @i",
                Db.P("@a", active), Db.P("@i", studentId));
        }

        public static void SetPassword(int studentId, string newPassword)
        {
            Db.Execute("UPDATE dbo.Students SET PasswordHash = @p WHERE StudentId = @i",
                Db.P("@p", PasswordHasher.Hash(newPassword)), Db.P("@i", studentId));
        }
    }
}
