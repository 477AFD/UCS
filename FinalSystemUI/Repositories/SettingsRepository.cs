using System;
using FinalSystemUI.Database;
using FinalSystemUI.Models;

namespace FinalSystemUI.Repositories
{
    /// <summary>Settings (like the clearance limit) and the current semester.</summary>
    public static class SettingsRepository
    {
        public static int GetInt(string key, int defaultValue)
        {
            object o = Db.Scalar("SELECT SettingValue FROM dbo.Settings WHERE SettingKey = @k", Db.P("@k", key));
            int value;
            if (o != null && int.TryParse(Convert.ToString(o), out value)) return value;
            return defaultValue;
        }

        public static int ClearanceThreshold
        {
            get { return GetInt("ClearanceThreshold", 5); }
        }

        public static int EditWindowHours
        {
            get { return GetInt("EditWindowHours", 12); }
        }

        public static SemesterInfo CurrentSemester()
        {
            SemesterInfo s = Db.ReadOne(
                "SELECT TOP 1 SemesterId, Name FROM dbo.Semesters WHERE IsCurrent = 1 ORDER BY SemesterId DESC",
                r => new SemesterInfo { SemesterId = Db.Int(r, "SemesterId"), Name = Db.Str(r, "Name") });
            if (s == null)
                throw new InvalidOperationException("No current semester is set in the database.");
            return s;
        }

        public static decimal DefaultFine(int typeId)
        {
            return Db.ScalarDecimal("SELECT DefaultFine FROM dbo.ViolationTypes WHERE TypeId = @t", Db.P("@t", typeId));
        }
    }
}
