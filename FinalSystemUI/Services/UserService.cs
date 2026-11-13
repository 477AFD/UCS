using System;
using System.Collections.Generic;
using System.Net.Mail;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;
using FinalSystemUI.Repositories;

namespace FinalSystemUI.Services
{
    /// <summary>Account management. Only OSSE can use these.</summary>
    public static class UserService
    {
        public static List<UserListItem> List(string kind)
        {
            Session.Require(UserRole.Osse);
            return UserRepository.List(kind);
        }

        public static UserCounts Counts()
        {
            Session.Require(UserRole.Osse);
            return UserRepository.Counts();
        }

        /// <summary>
        /// HSG and OSSE can register a new student. Returns the password that was set
        /// (a temporary one is made when password is empty).
        /// </summary>
        public static string AddStudent(string studentNumber, string fullName, string email, string password)
        {
            AppUser me = Session.Require(UserRole.Hsg, UserRole.Osse);
            studentNumber = (studentNumber ?? "").Trim();
            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();

            if (studentNumber.Length == 0 || studentNumber.Length > 20) throw new ArgumentException("Please enter a valid ID number.");
            if (fullName.Length == 0) throw new ArgumentException("Please enter the full name.");
            CheckEmail(email);

            if (string.IsNullOrEmpty(password)) password = GeneratePassword();
            else CheckPassword(password);

            if (StudentRepository.NumberOrEmailExists(studentNumber, email))
                throw new ArgumentException("A student with that ID number or email already exists.");

            StudentRepository.Insert(studentNumber, fullName, email, password);
            AuditRepository.Write(me, "ADD_STUDENT", studentNumber);
            return password;
        }

        /// <summary>A random 10-character password (no look-alike letters such as O, 0, l, 1).</summary>
        private static string GeneratePassword()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            byte[] bytes = new byte[10];
            using (System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            char[] chars = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; i++) chars[i] = alphabet[bytes[i] % alphabet.Length];
            return new string(chars);
        }

        /// <summary>
        /// Creates an HSG account. Returns the password that was set
        /// (a temporary one is made when password is empty).
        /// </summary>
        public static string AddHsg(string fullName, string email, bool isStudent, string password)
        {
            AppUser me = Session.Require(UserRole.Osse);
            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();

            if (fullName.Length == 0) throw new ArgumentException("Please enter the full name.");
            CheckEmail(email);
            if (HsgRepository.EmailExists(email)) throw new ArgumentException("An HSG account with that email already exists.");

            if (string.IsNullOrEmpty(password)) password = GeneratePassword();
            else CheckPassword(password);

            HsgRepository.Insert(fullName, email, password, isStudent);
            AuditRepository.Write(me, "ADD_HSG", email);
            return password;
        }

        public static void SetActive(string kind, int id, bool active)
        {
            AppUser me = Session.Require(UserRole.Osse);
            if (kind == "Student") StudentRepository.SetActive(id, active);
            else if (kind == "HSG") HsgRepository.SetActive(id, active);
            else throw new ArgumentException("Unknown account type.");
            AuditRepository.Write(me, active ? "ENABLE_USER" : "DISABLE_USER", kind + " id " + id);
        }

        /// <summary>
        /// OSSE can delete an account only if it has no records. An account with records must be disabled instead,
        /// so the violation history is never lost.
        /// </summary>
        public static void DeleteUser(string kind, int id)
        {
            AppUser me = Session.Require(UserRole.Osse);
            string label;
            if (kind == "Student")
            {
                StudentRow st = StudentRepository.FindById(id);
                if (st == null) throw new InvalidOperationException("This account no longer exists.");
                label = st.StudentNumber;
            }
            else if (kind == "HSG")
            {
                HsgRow h = HsgRepository.FindById(id);
                if (h == null) throw new InvalidOperationException("This account no longer exists.");
                label = h.Email;
            }
            else throw new ArgumentException("Unknown account type.");

            if (UserRepository.ViolationCount(kind, id) > 0)
                throw new InvalidOperationException("This account already has violation records, so it cannot be deleted. Disable it instead.");

            UserRepository.Delete(kind, id);
            AuditRepository.Write(me, "DELETE_USER", kind + " " + label);
        }

        /// <summary>OSSE sets a new password for a student or an HSG account.</summary>
        public static void ResetPassword(string kind, int id, string newPassword)
        {
            AppUser me = Session.Require(UserRole.Osse);
            CheckPassword(newPassword);
            if (kind == "Student")
            {
                if (StudentRepository.FindById(id) == null) throw new InvalidOperationException("This account no longer exists.");
                StudentRepository.SetPassword(id, newPassword);
            }
            else if (kind == "HSG")
            {
                if (HsgRepository.FindById(id) == null) throw new InvalidOperationException("This account no longer exists.");
                HsgRepository.SetPassword(id, newPassword);
            }
            else throw new ArgumentException("Unknown account type.");
            AuditRepository.Write(me, "RESET_PASSWORD", kind + " id " + id);
        }

        private static void CheckEmail(string email)
        {
            try
            {
                MailAddress m = new MailAddress(email);
                if (!string.Equals(m.Address, email, StringComparison.OrdinalIgnoreCase)) throw new FormatException();
            }
            catch (Exception)
            {
                throw new ArgumentException("Please enter a valid email address.");
            }
        }

        private static void CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                throw new ArgumentException("The password must have at least 8 characters.");
        }
    }
}
