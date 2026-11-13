using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;
using FinalSystemUI.Repositories;

namespace FinalSystemUI.Services
{
    public class AuthResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public AppUser User { get; private set; }

        public static AuthResult Fail(string message)
        {
            return new AuthResult { Success = false, Message = message };
        }

        public static AuthResult Ok(AppUser user)
        {
            return new AuthResult { Success = true, User = user };
        }
    }

    /// <summary>Stops guessing: 5 wrong tries lock that account name for 5 minutes (until the app is closed).</summary>
    internal static class LoginThrottle
    {
        private const int MaxFailures = 5;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);
        private static readonly Dictionary<string, List<DateTime>> Failures = new Dictionary<string, List<DateTime>>();
        private static readonly object Gate = new object();

        public static bool IsLocked(string key, out int minutesLeft)
        {
            lock (Gate)
            {
                minutesLeft = 0;
                List<DateTime> list;
                if (!Failures.TryGetValue(key, out list)) return false;
                list.RemoveAll(d => DateTime.UtcNow - d > Window);
                if (list.Count < MaxFailures) return false;
                minutesLeft = Math.Max(1, (int)Math.Ceiling((Window - (DateTime.UtcNow - list[0])).TotalMinutes));
                return true;
            }
        }

        public static void Fail(string key)
        {
            lock (Gate)
            {
                List<DateTime> list;
                if (!Failures.TryGetValue(key, out list))
                {
                    list = new List<DateTime>();
                    Failures[key] = list;
                }
                list.Add(DateTime.UtcNow);
            }
        }

        public static void Success(string key)
        {
            lock (Gate)
            {
                Failures.Remove(key);
            }
        }
    }

    /// <summary>
    /// Checks the three kinds of login. All of them use an email and a password.
    /// The Async methods run on a background thread, so the window does not freeze while the server is being checked.
    /// </summary>
    public static class AuthService
    {
        // ------------------------------------------------------------ Student

        public static Task<AuthResult> LoginStudentAsync(string email, string password)
        {
            return Task.Run(() => LoginStudent(email, password));
        }

        private static AuthResult LoginStudent(string email, string password)
        {
            email = (email ?? "").Trim();
            if (email.Length == 0 || string.IsNullOrEmpty(password))
                return AuthResult.Fail("Please enter your email and password.");

            string key = "S:" + email.ToLowerInvariant();
            int minutes;
            if (LoginThrottle.IsLocked(key, out minutes))
                return AuthResult.Fail("Too many wrong tries. Please try again in " + minutes + " minute(s).");

            StudentRow s = StudentRepository.FindByEmail(email);
            if (s == null || !PasswordHasher.Verify(password, s.PasswordHash))
            {
                LoginThrottle.Fail(key);
                return AuthResult.Fail("The email or password is not correct.");
            }
            if (!s.IsActive)
                return AuthResult.Fail("This account is disabled. Please contact OSSE.");

            LoginThrottle.Success(key);
            AppUser user = new AppUser
            {
                Role = UserRole.Student,
                Id = s.StudentId,
                StudentNumber = s.StudentNumber,
                FullName = s.FullName,
                Email = s.Email
            };
            return AuthResult.Ok(user);
        }

        // ------------------------------------------------------------ HSG

        public static Task<AuthResult> LoginHsgAsync(string email, string password)
        {
            return Task.Run(() => LoginHsg(email, password));
        }

        private static AuthResult LoginHsg(string email, string password)
        {
            email = (email ?? "").Trim();
            if (email.Length == 0 || string.IsNullOrEmpty(password))
                return AuthResult.Fail("Please enter your email and password.");

            string key = "H:" + email.ToLowerInvariant();
            int minutes;
            if (LoginThrottle.IsLocked(key, out minutes))
                return AuthResult.Fail("Too many wrong tries. Please try again in " + minutes + " minute(s).");

            HsgRow h = HsgRepository.FindByEmail(email);
            if (h == null || !PasswordHasher.Verify(password, h.PasswordHash))
            {
                LoginThrottle.Fail(key);
                return AuthResult.Fail("The email or password is not correct.");
            }
            if (!h.IsActive)
                return AuthResult.Fail("This account is disabled. Please contact OSSE.");

            LoginThrottle.Success(key);
            AppUser user = new AppUser { Role = UserRole.Hsg, Id = h.HsgId, FullName = h.FullName, Email = h.Email };
            AuditRepository.Write(user, "LOGIN", null);
            return AuthResult.Ok(user);
        }

        // ------------------------------------------------------------ OSSE

        public static Task<AuthResult> LoginOsseAsync(string email, string password)
        {
            return Task.Run(() => LoginOsse(email, password));
        }

        private static AuthResult LoginOsse(string email, string password)
        {
            email = (email ?? "").Trim();
            if (email.Length == 0 || string.IsNullOrEmpty(password))
                return AuthResult.Fail("Please enter your email and password.");

            string key = "O:" + email.ToLowerInvariant();
            int minutes;
            if (LoginThrottle.IsLocked(key, out minutes))
                return AuthResult.Fail("Too many wrong tries. Please try again in " + minutes + " minute(s).");

            OsseRow o = OsseRepository.FindByEmail(email);
            if (o == null || !PasswordHasher.Verify(password, o.PasswordHash))
            {
                LoginThrottle.Fail(key);
                return AuthResult.Fail("The email or password is not correct.");
            }
            if (!o.IsActive)
                return AuthResult.Fail("This account is disabled.");

            LoginThrottle.Success(key);
            AppUser user = new AppUser { Role = UserRole.Osse, Id = o.OsseId, FullName = o.FullName, Email = o.Email };
            AuditRepository.Write(user, "LOGIN", null);
            return AuthResult.Ok(user);
        }
    }
}
