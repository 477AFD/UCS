using System;
using FinalSystemUI.Helpers;

namespace FinalSystemUI.Models
{
    public enum UserRole { None, Student, Hsg, Osse }

    /// <summary>The person who is logged in.</summary>
    public class AppUser
    {
        public UserRole Role { get; set; }
        public int Id { get; set; }                  // StudentId, HsgId or OsseId
        public string StudentNumber { get; set; }    // students only
        public string FullName { get; set; }
        public string Email { get; set; }

        public string RoleText
        {
            get
            {
                return Role switch
                {
                    UserRole.Student => "Student",
                    UserRole.Hsg => "HSG",
                    UserRole.Osse => "OSSE",
                    _ => "",
                };
            }
        }

        /// <summary>The name used in the database (RecordedByRole column).</summary>
        public string DbRole
        {
            get { return Role == UserRole.Hsg ? "HSG" : (Role == UserRole.Osse ? "OSSE" : "STUDENT"); }
        }
    }

    public static class ViolationTypeIds
    {
        public const int NoUniform = 1;
        public const int InappropriateAttire = 2;
        public const int InvalidLanyard = 3;
        public const int NoId = 4;

        public static readonly string[] Names =
            ["No uniform", "Inappropriate attire", "Invalid ID lanyard", "No ID"];

        public static string NameOf(int typeId)
        {
            return (typeId >= 1 && typeId <= 4) ? Names[typeId - 1] : "Unknown";
        }

        public static bool IsFinable(int typeId)
        {
            return typeId == NoUniform || typeId == InappropriateAttire;
        }

        public static bool NeedsClearance(int typeId)
        {
            return typeId == InvalidLanyard || typeId == NoId;
        }
    }

    public class SemesterInfo
    {
        public int SemesterId { get; set; }
        public string Name { get; set; }
    }

    public class StudentRow
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
    }

    public class HsgRow
    {
        public int HsgId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsStudent { get; set; }
        public bool IsActive { get; set; }
    }

    public class OsseRow
    {
        public int OsseId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>One line in the Users screen (a student or an HSG account).</summary>
    public class UserListItem
    {
        public string Kind { get; set; }        // "Student" or "HSG"
        public int Id { get; set; }
        public string Number { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserCounts
    {
        public int Students { get; set; }
        public int Hsg { get; set; }
        public int Active { get; set; }
        public int Disabled { get; set; }
    }

    /// <summary>One violation record with everything the screens need.</summary>
    public class ViolationRow
    {
        public int ViolationId { get; set; }
        public int StudentId { get; set; }
        public string StudentNumber { get; set; }
        public string StudentName { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public bool IsFinable { get; set; }
        public DateTime RecordedAt { get; set; }
        public string RecordedByRole { get; set; }
        public int RecordedById { get; set; }
        public string RecordedByName { get; set; }
        public string Remarks { get; set; }
        public decimal? FineAmount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public DateTime ServerNow { get; set; }

        public bool NeedsClearance { get { return ViolationTypeIds.NeedsClearance(TypeId); } }
        public bool IsCleared { get { return ClearedAt.HasValue; } }

        /// <summary>Text for the Status column.</summary>
        public string StatusText
        {
            get
            {
                if (IsFinable) return IsPaid ? "Paid" : "Unpaid";
                return IsCleared ? "Cleared" : "For clearance";
            }
        }

        public string FineText
        {
            get { return (IsFinable && FineAmount.HasValue) ? Ui.Money(FineAmount.Value) : "None"; }
        }

        public TimeSpan EditTimeLeft(int windowHours)
        {
            return RecordedAt.AddHours(windowHours) - ServerNow;
        }

        public bool CanEdit(int windowHours)
        {
            return EditTimeLeft(windowHours) > TimeSpan.Zero;
        }

        public string EditWindowText(int windowHours)
        {
            TimeSpan left = EditTimeLeft(windowHours);
            if (left <= TimeSpan.Zero) return "Locked";
            return string.Format("Open ({0:0.0} h left)", left.TotalHours);
        }
    }

    /// <summary>Numbers shown on a student's dashboard and fines page.</summary>
    public class StudentSummary
    {
        public int TotalViolations { get; set; }
        public int UniformViolations { get; set; }      // no uniform + inappropriate attire
        public int LanyardViolations { get; set; }      // invalid lanyard
        public int NoIdViolations { get; set; }
        public decimal TotalFines { get; set; }
        public decimal UnpaidFines { get; set; }
        public decimal PaidFines { get; set; }
        public int FinedCount { get; set; }
        public int Threshold { get; set; }
        public int PendingClearance { get; set; }

        public string Standing
        {
            get
            {
                if (TotalViolations == 0) return "Good";
                if (TotalViolations < Threshold) return "Warning";
                return "Clearance hold";
            }
        }
    }

    public class RecordCounts
    {
        public int NoUniform { get; set; }
        public int InappropriateAttire { get; set; }
        public int InvalidLanyard { get; set; }
        public int NoId { get; set; }
        public int ForClearance { get; set; }
        public int ClearedToday { get; set; }
        public int TotalRecords { get; set; }
        public decimal UnpaidFines { get; set; }
        public decimal CollectedFines { get; set; }
    }

    /// <summary>Filter used when loading violation records.</summary>
    public class ViolationFilter
    {
        public int? StudentId { get; set; }
        public int? SemesterId { get; set; }
        public int[] TypeIds { get; set; }          // null = all types
        public string Search { get; set; }          // name or student number
        public int Clearance { get; set; }          // 0 = all, 1 = not cleared yet, 2 = cleared
        public int FineState { get; set; }          // 0 = all, 1 = unpaid, 2 = paid
        public bool OnlyFined { get; set; }
        public bool OnlyNeedsClearance { get; set; }
    }

    public class NewViolation
    {
        public int StudentId { get; set; }
        public int TypeId { get; set; }
        public int SemesterId { get; set; }
        public DateTime? RecordedAt { get; set; }   // null = now (server time)
        public string RecordedByRole { get; set; }
        public int RecordedById { get; set; }
        public string Remarks { get; set; }
        public decimal FineAmount { get; set; }
    }
}
