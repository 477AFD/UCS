using System;
using System.Collections.Generic;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;
using FinalSystemUI.Repositories;

namespace FinalSystemUI.Services
{
    /// <summary>
    /// The rules of the system. The screens never talk to the database directly for records.
    /// They call these methods, which check WHO is logged in and WHAT they are allowed to do.
    /// </summary>
    public static class RecordService
    {
        private const decimal MaxFine = 100000m;

        // ============================================================ Student (own data only)

        public static StudentSummary MySummary()
        {
            AppUser me = Session.Require(UserRole.Student);
            SemesterInfo sem = SettingsRepository.CurrentSemester();
            return ViolationRepository.SummaryForStudent(me.Id, sem.SemesterId, SettingsRepository.ClearanceThreshold);
        }

        /// <summary>A student always gets only their own records, whatever filter is given.</summary>
        public static List<ViolationRow> MyViolations(ViolationFilter filter)
        {
            AppUser me = Session.Require(UserRole.Student);
            filter ??= new ViolationFilter();
            filter.StudentId = me.Id;
            filter.SemesterId = SettingsRepository.CurrentSemester().SemesterId;
            return ViolationRepository.Query(filter);
        }

        // ============================================================ HSG and OSSE

        public static string SemesterName()
        {
            Session.Require(UserRole.Student, UserRole.Hsg, UserRole.Osse);
            return SettingsRepository.CurrentSemester().Name;
        }

        public static List<ViolationRow> AllRecords(ViolationFilter filter)
        {
            Session.Require(UserRole.Hsg, UserRole.Osse);
            filter ??= new ViolationFilter();
            filter.SemesterId = SettingsRepository.CurrentSemester().SemesterId;
            return ViolationRepository.Query(filter);
        }

        public static RecordCounts Counts()
        {
            Session.Require(UserRole.Hsg, UserRole.Osse);
            return ViolationRepository.CountsForSemester(SettingsRepository.CurrentSemester().SemesterId);
        }

        /// <summary>Violation count and standing of any student (HSG and OSSE only).</summary>
        public static StudentSummary SummaryFor(int studentId)
        {
            Session.Require(UserRole.Hsg, UserRole.Osse);
            SemesterInfo sem = SettingsRepository.CurrentSemester();
            return ViolationRepository.SummaryForStudent(studentId, sem.SemesterId, SettingsRepository.ClearanceThreshold);
        }

        public static int EditWindowHours
        {
            get { return SettingsRepository.EditWindowHours; }
        }

        public static decimal DefaultFine(int typeId)
        {
            return SettingsRepository.DefaultFine(typeId);
        }

        public static StudentRow FindStudent(string studentNumber)
        {
            Session.Require(UserRole.Hsg, UserRole.Osse);
            if (string.IsNullOrWhiteSpace(studentNumber)) return null;
            return StudentRepository.FindByNumber(studentNumber.Trim());
        }

        public static int AddRecord(string studentNumber, int typeId, string remarks, decimal? fineAmount)
        {
            AppUser me = Session.Require(UserRole.Hsg, UserRole.Osse);

            if (typeId < 1 || typeId > 4)
                throw new ArgumentException("Please choose a violation type.");
            if (remarks != null && remarks.Length > 300)
                throw new ArgumentException("The remarks are too long (300 characters at most).");

            StudentRow student = StudentRepository.FindByNumber((studentNumber ?? "").Trim()) ?? throw new ArgumentException("No student was found with that ID number.");
            if (!student.IsActive)
                throw new ArgumentException("This student account is disabled.");

            decimal fine = 0m;
            if (ViolationTypeIds.IsFinable(typeId))
            {
                fine = fineAmount ?? SettingsRepository.DefaultFine(typeId);
                if (fine < 0 || fine > MaxFine)
                    throw new ArgumentException("The fine amount is not valid.");
            }

            NewViolation nv = new()
            {
                StudentId = student.StudentId,
                TypeId = typeId,
                SemesterId = SettingsRepository.CurrentSemester().SemesterId,
                RecordedByRole = me.DbRole,
                RecordedById = me.Id,
                Remarks = remarks,
                FineAmount = fine
            };
            return ViolationRepository.Add(nv, me);
        }

        // ============================================================ HSG only

        /// <summary>HSG can edit a record only in the first 12 hours (checked here and again in SQL).</summary>
        public static void EditRecord(int violationId, int typeId, decimal? fineAmount, string remarks)
        {
            AppUser me = Session.Require(UserRole.Hsg);

            ViolationRow row = ViolationRepository.Get(violationId) ?? throw new InvalidOperationException("This record no longer exists.");
            if (!row.CanEdit(SettingsRepository.EditWindowHours))
                throw new InvalidOperationException("This record is locked. HSG can edit a record only within " +
                    SettingsRepository.EditWindowHours + " hours after it was made.");
            if (row.TypeId == ViolationTypeIds.NoId || typeId == ViolationTypeIds.NoId)
                throw new UnauthorizedAccessException("HSG accounts cannot change No-ID records.");
            if (typeId < 1 || typeId > 4)
                throw new ArgumentException("Please choose a violation type.");
            if (row.IsCleared)
                throw new InvalidOperationException("This record is already cleared and cannot be edited.");
            if (remarks != null && remarks.Length > 300)
                throw new ArgumentException("The remarks are too long (300 characters at most).");
            if (row.IsFinable && row.IsPaid && typeId != row.TypeId)
                throw new InvalidOperationException("The fine of this record is already paid, so its type cannot be changed.");

            decimal fine = 0m;
            if (ViolationTypeIds.IsFinable(typeId))
            {
                fine = fineAmount ?? SettingsRepository.DefaultFine(typeId);
                if (row.IsPaid && row.FineAmount.HasValue) fine = row.FineAmount.Value;   // a paid fine cannot change
                if (fine < 0 || fine > MaxFine)
                    throw new ArgumentException("The fine amount is not valid.");
            }

            ViolationRepository.UpdateRecord(violationId, typeId, fine, remarks, me, SettingsRepository.EditWindowHours);
        }

        // ============================================================ fines (HSG and OSSE)

        public static void SetFine(int violationId, decimal amount)
        {
            AppUser me = Session.Require(UserRole.Hsg, UserRole.Osse);
            if (amount < 0 || amount > MaxFine)
                throw new ArgumentException("The fine amount is not valid.");
            ViolationRepository.SetFine(violationId, amount, me);
        }

        public static void MarkPaid(int violationId)
        {
            AppUser me = Session.Require(UserRole.Hsg, UserRole.Osse);
            ViolationRepository.MarkPaid(violationId, me);
        }

        // ============================================================ OSSE only

        public static void DeleteRecord(int violationId)
        {
            AppUser me = Session.Require(UserRole.Osse);
            ViolationRepository.Delete(violationId, me);
        }

        public static void ClearRecord(int violationId, string remarks)
        {
            AppUser me = Session.Require(UserRole.Osse);
            ViolationRepository.MarkCleared(violationId, me, remarks);
        }
    }
}
