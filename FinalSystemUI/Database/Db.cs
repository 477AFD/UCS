using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace FinalSystemUI.Database
{
    /// <summary>
    /// Small helper for talking to SQL Server. Every query uses parameters (no string joining of user input).
    /// </summary>
    public static class Db
    {
        public static string ConnectionString
        {
            get
            {
                DataPath.Init();     // sets |DataDirectory| for the service-based database file
                ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["UniformDb"];
                if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                    throw new InvalidOperationException("The connection string 'UniformDb' is missing in App.config.");
                return cs.ConnectionString;
            }
        }

        public static SqlConnection Open()
        {
            SqlConnection c = new SqlConnection(ConnectionString);
            c.Open();
            return c;
        }

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static SqlParameter DateTime2(string name, DateTime? value)
        {
            SqlParameter p = new SqlParameter(name, SqlDbType.DateTime2);
            p.Value = value.HasValue ? (object)value.Value : DBNull.Value;
            return p;
        }

        public static SqlParameter Money(string name, decimal value)
        {
            SqlParameter p = new SqlParameter(name, SqlDbType.Decimal);
            p.Precision = 10;
            p.Scale = 2;
            p.Value = value;
            return p;
        }

        public static int Execute(string sql, params SqlParameter[] ps)
        {
            using (SqlConnection c = Open())
            {
                return Execute(c, null, sql, ps);
            }
        }

        public static int Execute(SqlConnection c, SqlTransaction t, string sql, params SqlParameter[] ps)
        {
            using (SqlCommand cmd = new SqlCommand(sql, c, t))
            {
                if (ps != null && ps.Length > 0) cmd.Parameters.AddRange(ps);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(string sql, params SqlParameter[] ps)
        {
            using (SqlConnection c = Open())
            {
                return Scalar(c, null, sql, ps);
            }
        }

        public static object Scalar(SqlConnection c, SqlTransaction t, string sql, params SqlParameter[] ps)
        {
            using (SqlCommand cmd = new SqlCommand(sql, c, t))
            {
                if (ps != null && ps.Length > 0) cmd.Parameters.AddRange(ps);
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? null : o;
            }
        }

        public static int ScalarInt(string sql, params SqlParameter[] ps)
        {
            object o = Scalar(sql, ps);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        public static decimal ScalarDecimal(string sql, params SqlParameter[] ps)
        {
            object o = Scalar(sql, ps);
            return o == null ? 0m : Convert.ToDecimal(o);
        }

        public static List<T> Read<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] ps)
        {
            List<T> list = new List<T>();
            using (SqlConnection c = Open())
            using (SqlCommand cmd = new SqlCommand(sql, c))
            {
                if (ps != null && ps.Length > 0) cmd.Parameters.AddRange(ps);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(map(r));
                }
            }
            return list;
        }

        public static T ReadOne<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] ps) where T : class
        {
            List<T> list = Read(sql, map, ps);
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>Runs several commands as one unit. If anything fails, nothing is saved.</summary>
        public static void InTransaction(Action<SqlConnection, SqlTransaction> work)
        {
            using (SqlConnection c = Open())
            using (SqlTransaction t = c.BeginTransaction())
            {
                try
                {
                    work(c, t);
                    t.Commit();
                }
                catch
                {
                    t.Rollback();
                    throw;
                }
            }
        }

        // ---- reading helpers (null safe) ----
        public static string Str(SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        public static int Int(SqlDataReader r, string col)
        {
            return r.GetInt32(r.GetOrdinal(col));
        }

        public static bool Bool(SqlDataReader r, string col)
        {
            return r.GetBoolean(r.GetOrdinal(col));
        }

        public static DateTime Date(SqlDataReader r, string col)
        {
            return r.GetDateTime(r.GetOrdinal(col));
        }

        public static DateTime? DateN(SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? (DateTime?)null : r.GetDateTime(i);
        }

        public static decimal? DecN(SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? (decimal?)null : r.GetDecimal(i);
        }
    }
}
