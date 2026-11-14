using DatabaseManager;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Services.Description;
using System.Windows.Forms;

namespace FinalSystemUI.Database
{
    internal static class SystemLoader
    {
        // Database credentials
        public static CredentialHolder<Semesters> _1 { get; private set; } = null!;
        public static CredentialHolder<Students> _2 { get; private set; } = null!;
        public static CredentialHolder<HsgAdmin> _3 { get; private set; } = null!;
        public static CredentialHolder<OsseAdmin> _4 { get; private set; } = null!;
        public static CredentialHolder<ViolationTypes> _5 { get; private set; } = null!;
        public static CredentialHolder<Violations> _6 { get; private set; } = null!;
        public static CredentialHolder<Fines> _7 { get; private set; } = null!;
        public static CredentialHolder<ClearanceRecords> _8 { get; private set; } = null!;
        public static CredentialHolder<AuditLog> _9 { get; private set; } = null!;
        public static CredentialHolder<Settings> _10 { get; private set; } = null!;
        // Database operators
        public static DatabaseOperation<Semesters> SemOperation { get; private set; } = null!;
        public static DatabaseOperation<Students> StudOperation { get; private set; } = null!;
        public static DatabaseOperation<HsgAdmin> HsgOperation { get; private set; } = null!;
        public static DatabaseOperation<OsseAdmin> OsseOperation { get; private set; } = null!;
        public static DatabaseOperation<ViolationTypes> TypeOperation { get; private set; } = null!;
        public static DatabaseOperation<Violations> ViolOperation { get; private set; } = null!;
        public static DatabaseOperation<Fines> FineOperation { get; private set; } = null!;
        public static DatabaseOperation<ClearanceRecords> ClearOperation { get; private set; } = null!;
        public static DatabaseOperation<AuditLog> AuditOperation { get; private set; } = null!;
        public static DatabaseOperation<Settings> SettingOperation { get; private set; } = null!;
        // SQL batch runner
        /// <summary>
        /// Runs SQL code in a batch.
        /// </summary>
        /// <typeparam name="T">The class type where the operator will use.</typeparam>
        /// <param name="op">The database operator class to use.</param>
        /// <param name="statements">The SQL code to run in batches.</param>
        private static void Run<T>(DatabaseOperation<T> op, params FormattableString[] statements) where T : class, IDatabaseTemplate
        {
            foreach (FormattableString s in statements) op.ExecuteSql(s);
        }
        // Database loader
        /// <summary>
        /// Loads all the databases, tables, and its constraints.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="conn"></param>
        public static bool Load(DatabaseType type, Connection conn)
        {
            try
            {
                Verbose.Prt("TABLE", "Loading database and types");
            // Initialize database credentials (yes, it is different for each class type)
            _1 = new(type, conn); _2 = new(type, conn); _3 = new(type, conn); _4 = new(type, conn); _5 = new(type, conn);
                _6 = new(type, conn); _7 = new(type, conn); _8 = new(type, conn); _9 = new(type, conn); _10 = new(type, conn);
                // Initialize database operators and table names
                SemOperation = new(_1.Fetch("Semesters"));
                StudOperation = new(_2.Fetch("Students"));
                HsgOperation = new(_3.Fetch("HsgAdmin"));
                OsseOperation = new(_4.Fetch("OsseAdmin"));
                TypeOperation = new(_5.Fetch("ViolationTypes"));
                ViolOperation = new(_6.Fetch("Violations"));
                FineOperation = new(_7.Fetch("Fines"));
                ClearOperation = new(_8.Fetch("ClearanceRecords"));
                AuditOperation = new(_9.Fetch("AuditLog"));
                SettingOperation = new(_10.Fetch("Settings"));
                // Set up table and constraints
                if (!SemOperation.EnsureTable())
                    Run(SemOperation,
                        $"ALTER TABLE Semesters ADD CONSTRAINT CK_Semesters_StartDate CHECK (StartDate < EndDate)",
                        $"ALTER TABLE Semesters ADD CONSTRAINT DF_Semesters_IsCurrent DEFAULT 0 FOR IsCurrent");

                if (!StudOperation.EnsureTable())
                    Run(StudOperation,
                        $"ALTER TABLE Students ADD CONSTRAINT DF_Students_IsActive DEFAULT 1 FOR IsActive",
                        $"ALTER TABLE Students ADD CONSTRAINT DF_Students_CreatedAt DEFAULT SYSDATETIME() FOR CreatedAt");

                if (!HsgOperation.EnsureTable())
                    Run(HsgOperation,
                        $"ALTER TABLE HsgAdmin ADD CONSTRAINT DF_HsgAdmin_IsStudent DEFAULT 0 FOR IsStudent",
                        $"ALTER TABLE HsgAdmin ADD CONSTRAINT DF_HsgAdmin_IsActive DEFAULT 1 FOR IsActive",
                        $"ALTER TABLE HsgAdmin ADD CONSTRAINT DF_HsgAdmin_CreatedAt DEFAULT SYSDATETIME() FOR CreatedAt");

                if (!OsseOperation.EnsureTable())
                    Run(OsseOperation,
                        $"ALTER TABLE OsseAdmin ADD CONSTRAINT DF_OsseAdmin_IsActive DEFAULT 1 FOR IsActive",
                        $"ALTER TABLE OsseAdmin ADD CONSTRAINT DF_OsseAdmin_CreatedAt DEFAULT SYSDATETIME() FOR CreatedAt");

                if (!TypeOperation.EnsureTable())
                    Run(TypeOperation,
                        $"ALTER TABLE ViolationTypes ADD CONSTRAINT DF_ViolationTypes_Fine DEFAULT 0 FOR DefaultFine");

                if (!ViolOperation.EnsureTable())
                    Run(ViolOperation,
                        $"ALTER TABLE Violations ADD CONSTRAINT FK_Violations_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId)",
                        $"ALTER TABLE Violations ADD CONSTRAINT FK_Violations_Types FOREIGN KEY (TypeId) REFERENCES ViolationTypes(TypeId)",
                        $"ALTER TABLE Violations ADD CONSTRAINT FK_Violations_Semesters FOREIGN KEY (SemesterId) REFERENCES Semesters(SemesterId)",
                        $"ALTER TABLE Violations ADD CONSTRAINT CK_Violations_Role CHECK (RecordedByRole IN (N'HSG', N'OSSE'))",
                        $"ALTER TABLE Violations ADD CONSTRAINT DF_Violations_RecordedAt DEFAULT SYSDATETIME() FOR RecordedAt");

                if (!FineOperation.EnsureTable())
                    Run(FineOperation,
                        $"ALTER TABLE Fines ADD CONSTRAINT FK_Fines_Violations FOREIGN KEY (ViolationId) REFERENCES Violations(ViolationId) ON DELETE CASCADE",
                        $"ALTER TABLE Fines ADD CONSTRAINT CK_Fines_Amount CHECK (Amount >= 0)",
                        $"ALTER TABLE Fines ADD CONSTRAINT DF_Fines_IsPaid DEFAULT 0 FOR IsPaid",
                        $"ALTER TABLE Fines ADD CONSTRAINT DF_Fines_SetAt DEFAULT SYSDATETIME() FOR SetAt");

                if (!ClearOperation.EnsureTable())
                    Run(ClearOperation,
                        $"ALTER TABLE ClearanceRecords ADD CONSTRAINT FK_Clearance_Violations FOREIGN KEY (ViolationId) REFERENCES Violations(ViolationId) ON DELETE CASCADE",
                        $"ALTER TABLE ClearanceRecords ADD CONSTRAINT FK_Clearance_Osse FOREIGN KEY (ClearedByOsseId) REFERENCES OsseAdmin(OsseId)",
                        $"ALTER TABLE ClearanceRecords ADD CONSTRAINT DF_Clearance_At DEFAULT SYSDATETIME() FOR ClearedAt");

                if (!AuditOperation.EnsureTable())
                    Run(AuditOperation,
                        $"ALTER TABLE AuditLog ADD CONSTRAINT DF_Audit_At DEFAULT SYSDATETIME() FOR At");

                SettingOperation.EnsureTable();   // no extra constraints needed: the unique index comes from the class
                return true;
            } catch (Exception ex)
            {
                MessageBox.Show($"Failed to load database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        // Classes
        public class Semesters : IDatabaseTemplate
        {
            [Key, Column("SemesterId")] public long ID { get; set; }
            [MaxLength(60)] public string Name { get; set; } = "";
            [Column(TypeName = "date")] public DateTime StartDate { get; set; }
            [Column(TypeName = "date")] public DateTime EndDate { get; set; }
            public bool IsCurrent { get; set; }
        }

        [Index(nameof(StudentNumber), IsUnique = true, Name = "UQ_Students_Number")]
        [Index(nameof(Email), IsUnique = true, Name = "UQ_Students_Email")]
        public class Students : IDatabaseTemplate
        {
            [Key, Column("StudentId")] public long ID { get; set; }
            [MaxLength(20)] public string StudentNumber { get; set; } = "";
            [MaxLength(120)] public string FullName { get; set; } = "";
            [MaxLength(150)] public string Email { get; set; } = "";
            [MaxLength(200)] public string PasswordHash { get; set; } = "";
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        }

        [Index(nameof(Email), IsUnique = true, Name = "UQ_HsgAdmin_Email")]
        public class HsgAdmin : IDatabaseTemplate
        {
            [Key, Column("HsgId")] public long ID { get; set; }
            [MaxLength(120)] public string FullName { get; set; } = "";
            [MaxLength(150)] public string Email { get; set; } = "";
            [MaxLength(200)] public string PasswordHash { get; set; } = "";
            public bool IsStudent { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        }

        [Index(nameof(Email), IsUnique = true, Name = "UQ_OsseAdmin_Email")]
        public class OsseAdmin : IDatabaseTemplate
        {
            [Key, Column("OsseId")] public long ID { get; set; }
            [MaxLength(120)] public string FullName { get; set; } = "";
            [MaxLength(150)] public string Email { get; set; } = "";
            [MaxLength(200)] public string PasswordHash { get; set; } = "";
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        }

        [Index(nameof(Name), IsUnique = true, Name = "UQ_ViolationTypes_Name")]
        public class ViolationTypes : IDatabaseTemplate
        {
            [Key, Column("TypeId"), DatabaseGenerated(DatabaseGeneratedOption.None)] public long ID { get; set; }
            [MaxLength(40)] public string Name { get; set; } = "";
            public bool IsFinable { get; set; }
            [Precision(10, 2)] public decimal DefaultFine { get; set; }
        }

        public class Violations : IDatabaseTemplate
        {
            [Key, Column("ViolationId")] public long ID { get; set; }
            public long StudentId { get; set; }                    // FK -> Students
            public long TypeId { get; set; }                       // FK -> ViolationTypes
            public long SemesterId { get; set; }                   // FK -> Semesters
            public DateTime RecordedAt { get; set; } = DateTime.Now;
            [MaxLength(10)] public string RecordedByRole { get; set; } = "";   // 'HSG' or 'OSSE'
            public long RecordedById { get; set; }
            [MaxLength(300)] public string? Remarks { get; set; }
            public DateTime? LastEditedAt { get; set; }
            [MaxLength(10)] public string? LastEditedByRole { get; set; }
            public long? LastEditedById { get; set; }
        }

        [Index(nameof(ViolationId), IsUnique = true, Name = "UQ_Fines_Violation")]
        public class Fines : IDatabaseTemplate
        {
            [Key, Column("FineId")] public long ID { get; set; }
            public long ViolationId { get; set; }                  // FK -> Violations
            [Precision(10, 2)] public decimal Amount { get; set; }
            public bool IsPaid { get; set; }
            public DateTime? PaidAt { get; set; }
            [MaxLength(10)] public string SetByRole { get; set; } = "";
            public long SetById { get; set; }
            public DateTime SetAt { get; set; } = DateTime.Now;
        }

        [Index(nameof(ViolationId), IsUnique = true, Name = "UQ_Clearance_Violation")]
        public class ClearanceRecords : IDatabaseTemplate
        {
            [Key, Column("ClearanceId")] public long ID { get; set; }
            public long ViolationId { get; set; }                  // FK -> Violations
            public DateTime ClearedAt { get; set; } = DateTime.Now;
            public long ClearedByOsseId { get; set; }              // FK -> OsseAdmin
            [MaxLength(300)] public string? Remarks { get; set; }
        }

        public class AuditLog : IDatabaseTemplate
        {
            [Key, Column("AuditId")] public long ID { get; set; }
            public DateTime At { get; set; } = DateTime.Now;
            [MaxLength(10)] public string ActorRole { get; set; } = "";
            public long ActorId { get; set; }
            [MaxLength(50)] public string Action { get; set; } = "";
            [MaxLength(500)] public string? Details { get; set; }
        }

        [Index(nameof(SettingKey), IsUnique = true, Name = "UQ_Settings_Key")]
        public class Settings : IDatabaseTemplate
        {
            [Key, Column("SettingId")] public long ID { get; set; }
            [MaxLength(50)] public string SettingKey { get; set; } = "";
            [MaxLength(100)] public string SettingValue { get; set; } = "";
        }
    }
}