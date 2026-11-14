using System;
using Microsoft.Data.SqlClient;
using System.IO;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;
using FinalSystemUI.Repositories;

namespace FinalSystemUI.Database
{
    public class InitResult
    {
        public bool SeededSampleData { get; set; }
        public string DatabaseFile { get; set; }
        public bool CreatedDatabaseFile { get; set; }
    }

    /// <summary>
    /// Runs when the program starts. Creates the database and tables if they do not exist yet.
    /// The first time, it also adds sample accounts and sample records so you can try the system right away.
    /// </summary>
    public static class DatabaseInitializer
    {
        // Passwords of the sample accounts (and of old accounts that were made before passwords were used).
        public const string SampleStudentPassword = "Student@123";
        public const string SampleHsgPassword = "Hsg@12345";
        public const string SampleOssePassword = "Osse@12345";

        public static InitResult EnsureReady()
        {
            InitResult result = new()
            {
                DatabaseFile = DataPath.MdfPath,
                CreatedDatabaseFile = EnsureDatabaseFile()
            };

            // From here on, every connection auto-attaches the .mdf file (AttachDbFilename in App.config).
            foreach (string statement in DbSchema.Statements)
            {
                Db.Execute(statement);
            }

            UpgradeOldAccounts();

            bool empty = Db.ScalarInt("SELECT COUNT(*) FROM dbo.Students") == 0
                      && Db.ScalarInt("SELECT COUNT(*) FROM dbo.HsgAdmin") == 0
                      && Db.ScalarInt("SELECT COUNT(*) FROM dbo.OsseAdmin") == 0;
            if (empty)
            {
                SeedSampleData();
                result.SeededSampleData = true;
            }
            return result;
        }

        /// <summary>
        /// Creates the empty service-based database file (UniformComplianceDB.mdf) if it is not there yet.
        /// It is created on the server, then detached, so LocalDB can attach it by file name.
        /// Returns true if a new file was created.
        /// </summary>
        private static bool EnsureDatabaseFile()
        {
            string mdf = DataPath.MdfPath;
            string ldf = DataPath.LdfPath;
            if (File.Exists(mdf)) return false;
            if (File.Exists(ldf)) File.Delete(ldf);          // an old log file would block the new database

            SqlConnectionStringBuilder builder = new(Db.ConnectionString);
            builder.Remove("AttachDbFilename");
            builder.InitialCatalog = "master";

            string tempName = "tmp_" + Guid.NewGuid().ToString("N");
            using SqlConnection c = new(builder.ConnectionString);
            c.Open();
            // A name or a file path cannot be a parameter in CREATE DATABASE.
            // tempName is only letters and numbers, and the paths have their quotes doubled, so this is safe.
            Db.Execute(c, null,
                "CREATE DATABASE [" + tempName + "] ON PRIMARY (NAME = N'UniformComplianceDB', FILENAME = N'" + Quote(mdf) + "') " +
                "LOG ON (NAME = N'UniformComplianceDB_log', FILENAME = N'" + Quote(ldf) + "')");
            Db.Execute(c, null, "EXEC master.dbo.sp_detach_db @dbname = @n, @skipchecks = N'true'", Db.P("@n", tempName));
            return true;
        }

        private static string Quote(string text)
        {
            return text.Replace("'", "''");
        }

        /// <summary>
        /// An older database file has HSG/OSSE accounts without a password (they used key files).
        /// Give them the sample password so they can log in, then make the column required.
        /// OSSE can change an HSG password later from the Users screen.
        /// </summary>
        private static void UpgradeOldAccounts()
        {
            string hsgHash = null;
            foreach (int id in Db.Read("SELECT HsgId FROM dbo.HsgAdmin WHERE PasswordHash IS NULL", r => Db.Int(r, "HsgId")))
            {
                hsgHash ??= PasswordHasher.Hash(SampleHsgPassword);
                Db.Execute("UPDATE dbo.HsgAdmin SET PasswordHash = @p WHERE HsgId = @i", Db.P("@p", hsgHash), Db.P("@i", id));
            }

            string osseHash = null;
            foreach (int id in Db.Read("SELECT OsseId FROM dbo.OsseAdmin WHERE PasswordHash IS NULL", r => Db.Int(r, "OsseId")))
            {
                osseHash ??= PasswordHasher.Hash(SampleOssePassword);
                Db.Execute("UPDATE dbo.OsseAdmin SET PasswordHash = @p WHERE OsseId = @i", Db.P("@p", osseHash), Db.P("@i", id));
            }

            Db.Execute("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.HsgAdmin') AND name = N'PasswordHash' AND is_nullable = 1) " +
                       "ALTER TABLE dbo.HsgAdmin ALTER COLUMN PasswordHash NVARCHAR(200) NOT NULL");
            Db.Execute("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.OsseAdmin') AND name = N'PasswordHash' AND is_nullable = 1) " +
                       "ALTER TABLE dbo.OsseAdmin ALTER COLUMN PasswordHash NVARCHAR(200) NOT NULL");
        }

        private static void SeedSampleData()
        {
            // Students
            int juan = StudentRepository.Insert("23-00123", "Juan Dela Cruz", "juan@minsu.edu.ph", SampleStudentPassword);
            int ana = StudentRepository.Insert("24-00458", "Ana Reyes", "ana@minsu.edu.ph", SampleStudentPassword);
            int mark = StudentRepository.Insert("22-01987", "Mark Villanueva", "mark@minsu.edu.ph", SampleStudentPassword);

            // HSG and OSSE accounts (email + password)
            int hsgId = HsgRepository.Insert("Maria Santos", "hsg@minsu.edu.ph", SampleHsgPassword, true);
            int osseId = OsseRepository.Insert("OSSE Administrator", "osse@minsu.edu.ph", SampleOssePassword);

            AppUser hsg = new() { Role = UserRole.Hsg, Id = hsgId, FullName = "Maria Santos" };
            AppUser osseUser = new() { Role = UserRole.Osse, Id = osseId, FullName = "OSSE Administrator" };
            int semesterId = SettingsRepository.CurrentSemester().SemesterId;
            DateTime now = DateTime.Now;

            AddSample(juan, ViolationTypeIds.NoUniform, now.AddHours(-3), hsg, semesterId, false);
            AddSample(juan, ViolationTypeIds.InvalidLanyard, now.AddDays(-17), hsg, semesterId, false);
            AddSample(juan, ViolationTypeIds.InappropriateAttire, now.AddDays(-38), hsg, semesterId, true);
            AddSample(juan, ViolationTypeIds.NoId, now.AddDays(-9), hsg, semesterId, false);
            AddSample(ana, ViolationTypeIds.NoId, now.AddHours(-5), osseUser, semesterId, false);
            AddSample(mark, ViolationTypeIds.NoUniform, now.AddDays(-2), hsg, semesterId, true);
        }

        private static void AddSample(int studentId, int typeId, DateTime at, AppUser actor, int semesterId, bool paid)
        {
            NewViolation nv = new()
            {
                StudentId = studentId,
                TypeId = typeId,
                SemesterId = semesterId,
                RecordedAt = at,
                RecordedByRole = actor.DbRole,
                RecordedById = actor.Id,
                Remarks = null,
                FineAmount = ViolationTypeIds.IsFinable(typeId) ? SettingsRepository.DefaultFine(typeId) : 0m
            };
            int id = ViolationRepository.Add(nv, actor);
            if (paid) ViolationRepository.MarkPaid(id, actor);
        }
    }
}
