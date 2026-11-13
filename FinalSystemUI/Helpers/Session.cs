using System;
using FinalSystemUI.Models;

namespace FinalSystemUI.Helpers
{
    /// <summary>Remembers who is logged in. Services use this to check permissions.</summary>
    public static class Session
    {
        public static AppUser Current { get; private set; }

        public static void Start(AppUser user)
        {
            Current = user;
        }

        public static void End()
        {
            Current = null;
        }

        public static bool IsLoggedIn
        {
            get { return Current != null; }
        }

        /// <summary>Throws if nobody is logged in or the user has a different role.</summary>
        public static AppUser Require(params UserRole[] allowed)
        {
            if (Current == null)
                throw new UnauthorizedAccessException("Please log in first.");
            foreach (UserRole r in allowed)
            {
                if (Current.Role == r) return Current;
            }
            throw new UnauthorizedAccessException("Your account is not allowed to do this.");
        }
    }
}
