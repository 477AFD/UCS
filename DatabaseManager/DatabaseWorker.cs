using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace DatabaseManager
{
    /// <summary>
    /// This represents the base class for the database workers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connectionString"></param>
    /// <param name="tableName"></param>
    public abstract class DatabaseWorker<T>(string connectionString, string tableName, string dbName) : DbContext, ITableMeta where T : class, IDatabaseTemplate
    {
        private readonly string _connString = connectionString;
        public string TableName { get; } = tableName;
        public string DatabaseName { get; } = dbName;
        public DbSet<T> Table => Set<T>();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(_connString).ReplaceService<IModelCacheKeyFactory, TableNameCacheKeyFactory>();
        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<T>().ToTable(TableName);
    }
    /// <summary>
    /// This represents the table name.
    /// </summary>
    internal interface ITableMeta { string TableName { get; } }
    /// <summary>
    /// This converts a table name as a string to an actual table.
    /// </summary>
    internal class TableNameCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => (context.GetType(), (context as ITableMeta)?.TableName, designTime);
    }
    /// <summary>
    /// This initializes a local database worker. It is made specifically for service-based databases.
    /// </summary>
    /// <typeparam name="T">This denotes the entity to use.</typeparam>
    /// <param name="MDFFilePath">This specifies the Master Database File. Requires an absolute file path.</param>
    /// <param name="tableName">This specifies the table name.</param>
    internal class LocalDbWorker<T>(string mdfFilePath, string tableName) : DatabaseWorker<T>(Build(mdfFilePath), tableName, "LocalFile") where T : class, IDatabaseTemplate
    {
        private static string Build(string path) => new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            AttachDBFilename = Path.GetFullPath(path),
            InitialCatalog = Path.GetFileNameWithoutExtension(path),
            IntegratedSecurity = true
        }.ConnectionString;
    }
    /// <summary>
    /// This initializes a remote database worker. It is made specifically for Microsoft SQL server connections.
    /// </summary>
    /// <typeparam name="T">This denotes the entity to use.</typeparam>
    /// <param name="host">Selects a hostname.</param>
    /// <param name="port">Selects a port. Valid values are from 0 to 65535.</param>
    /// <param name="database">Selects a database name.</param>
    /// <param name="user">This specifies the username of the database user.</param>
    /// <param name="password">This specifies the password for the selected user.</param>
    /// <param name="tableName">This specifies the table name.</param>
    internal class RemoteDbWorker<T>(string host, int port, string database, string user, string password, string tableName) : DatabaseWorker<T>(Build(host, port, database, user, password), tableName, database) where T : class, IDatabaseTemplate
    {
        private static string Build(string host, int port, string db, string user, string pwd) => new SqlConnectionStringBuilder
        {
            DataSource = port > 0 ? $"{host},{port}" : host,
            InitialCatalog = db,
            UserID = user,
            Password = pwd,
            TrustServerCertificate = true
        }.ConnectionString;
    }
    /// <summary>
    /// This holds the credentials for SQL operations.
    /// </summary>
    /// <typeparam name="T">This denotes the entity to use.</typeparam>
    /// <param name="type">Selects the database type. It can be "Local" or "Remote".</param>
    /// <param name="connection">Specifies the necessary credentials.</param>
    public class CredentialHolder<T>(DatabaseType type, Connection connection) where T : class, IDatabaseTemplate
    {
        private readonly Connection _conn = connection;
        private readonly DatabaseType _type = type;
        /// <summary>
        /// Generates a class constructor for use in database operations.
        /// </summary>
        /// <param name="tableName">This specifies the table name.</param>
        /// <returns>A class constructor to use in the DatabaseOperation class.</returns>
        /// <exception cref="ArgumentException"></exception>
        public Func<DatabaseWorker<T>> Fetch(string tableName)
        {
            Verbose.Prt("CREDENTIALS", " Fetching... ", true);
            DatabaseWorker<T> output() => _type switch
            {
                DatabaseType.Local => new LocalDbWorker<T>(_conn.MdfFilePath, tableName),
                DatabaseType.Remote => new RemoteDbWorker<T>(_conn.Hostname, _conn.Port, _conn.Database, _conn.Username, _conn.Password, tableName),
                _ => throw new ArgumentException($"Database Type can only be local or remote, specified: {_type}")
            };
            Verbose.PrtNoTime("done");
            return output;
        }
    }

    public class DatabaseOperation<T>(Func<DatabaseWorker<T>> createDb) where T : class, IDatabaseTemplate
    {
        private readonly Func<DatabaseWorker<T>> _createDb = createDb;
        /// <summary>
        /// Ensures that the table or database exists. If the table or database does not exist, it creates one automatically.
        /// </summary>
        public bool EnsureTable()
        {
            using DatabaseWorker<T> db = _createDb();
            var creator = db.GetService<IRelationalDatabaseCreator>();

            if (!creator.Exists())
                creator.Create();                 // database itself

            try
            {
                Verbose.Prt("SQL", "Searching for table... ", true);
                _ = db.Table.AsNoTracking().Any();   // generates SELECT ... TOP 1 on the mapped table
                Verbose.PrtNoTime("Present");
                return true;
            }
            catch (SqlException ex) when (ex.Number == 208)   // Invalid object name: table missing
            {
                Verbose.PrtNoTime("Absent");
                Verbose.Prt("SQL", "Attempting to create table... ");
                try
                {
                    Verbose.Prt("SQL", "Creating table... ", true);
                    creator.CreateTables();       // creates only this context's table
                    Verbose.PrtNoTime("done");
                    return false;
                }
                catch (SqlException ex2) when (ex2.Number == 2714)
                {
                    Verbose.PrtNoTime("already present");
                    return true;
                }
            }
        }
        public string TableName
        {
            get
            {
                using DatabaseWorker<T> db = _createDb();
                return db.TableName;
            }
        }
        // CREATE
        /// <summary>
        /// Insert a single record to the specified table.<br />SQL Equivalent: <![CDATA[INSERT INTO tableName VALUES (key, property1, property2, ... propertyN);]]> where properties and keys come from a specified class.
        /// </summary>
        /// <param name="entity">Select a matching entity class.</param>
        public void Insert(T entity)
        {
            Verbose.Prt("SQL", "Performing insert operation...");
            using DatabaseWorker<T> db = _createDb();
            db.Table.Add(entity);
            db.SaveChanges();
        }
        /// <summary>
        /// Insert any amount of records from a list.<br/> In SQL, it would require a long SQL code: <![CDATA[INSERT INTO tableName VALUES (key, p1, p2, ..., pN), (key, p1, p2, ..., pN), ..., (key, p1, p2, ..., pN);]]>
        /// </summary>
        /// <param name="entities">Select a list of classes to be inserted in a record.<br/>Supported classes: List, Queue, Stack, LinkedList, standard array, SortedSet, ConcurrentQueue, ConcurrentStack, ImmutableList, ImmutableArray, ImmutableHashSet, and ReadOnlyCollection.</param>
        public void Insert(IEnumerable<T> entities)
        {
            Verbose.Prt("SQL", "Performing insert operation...");
            using DatabaseWorker<T> db = _createDb();
            db.Table.AddRange(entities);
            db.SaveChanges();
        }

        // READ
        /// <summary>
        /// Select all records in a table. It is equivalent to: SELECT * FROM tableName;<br/> NOTE: do not use this function for big data as it can crash this system.
        /// </summary>
        /// <returns>A list of records in the form of classes.</returns>
        public List<T> Select()
        {
            Verbose.Prt("SQL", "Fetching data from server...");
            using DatabaseWorker<T> db = _createDb();
            return [.. db.Table.AsNoTracking()];
        }
        /// <summary>
        /// Selects a single record in a table by ID. Equivalent to: SELECT * FROM tableName WHERE ID=keyValues;
        /// </summary>
        /// <param name="keyValues">The ID number to search for. It must end in an L like <![CDATA[[3L]]]> and enclosed in square brackets.</param>
        /// <returns>A specified item based on the ID. If the item is not present in the database, it returns a NULL.</returns>
        public T? Select(params object[] keyValues)
        {
            Verbose.Prt("SQL", "Fetching data from server...");
            using DatabaseWorker<T> db = _createDb();
            return db.Table.Find(keyValues);
        }
        /// <summary>
        /// Selects a single record in a table by ID. Equivalent to: SELECT * FROM tableName WHERE ID=keyValue;
        /// </summary>
        /// <param name="keyValue">The ID number to search for. It must end in an L like <![CDATA[3L]]>.</param>
        /// <returns>A specified item based on the ID. If the item is not present in the database, it returns a NULL.</returns>
        public T? Select(long keyValue) => Select([keyValue]);
        /// <summary>
        /// Selects records based on a condition. Equivalent to: SELECT * FROM tableName WHERE predicate;
        /// </summary>
        /// <param name="predicate">This states a condition, e.g. <![CDATA[x => x.Age >= 18]]> or <![CDATA[x => x.Sex == 1 && x.Age > 15]]></param>
        /// <returns>A list of records in the form of classes.</returns>
        public List<T> Select(Expression<Func<T, bool>> predicate)
        {
            Verbose.Prt("SQL", "Fetching data from server...");
            using DatabaseWorker<T> db = _createDb();
            return [.. db.Table.AsNoTracking().Where(predicate)];
        }
        /// <summary>
        /// Converts the list of classes generated by the database operations into a DataTable. Required for use in DataGridView.
        /// </summary>
        /// <param name="Selector">Selects a function to use. To use it, here is the example: Func&lt;List&lt;T&gt;&gt; method = Select([4]);</param>
        /// <returns>A standard DataTable for use in DataGridView.</returns>
        public DataTable AsTable(Func<List<T>> Selector)
        {
            List<T> io = Selector();

            PropertyInfo[] props = [.. typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && IsSimpleType(p.PropertyType))];

            var table = new DataTable(typeof(T).Name);
            foreach (PropertyInfo p in props)
                table.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);

            foreach (T item in io)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo p in props)
                    row[p.Name] = p.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
        private static bool IsSimpleType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive || type.IsEnum
                || type == typeof(string) || type == typeof(decimal)
                || type == typeof(DateTime) || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan) || type == typeof(Guid) || type == typeof(byte[]);
        }

        // UPDATE
        /// <summary>
        /// Updates a single entity. Equivalent to: UPDATE tableName SET prop1=c1, prop2=c2, ... propN=cN WHERE ID=ClassID;<br/>Note that the ID is watched! If the ID is not present or set to a value not present in the database, it returns an exception.
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {
            Verbose.Prt("SQL", "Writing data to server... ", true);
            using DatabaseWorker<T> db = _createDb();
            db.Table.Update(entity);      // marks all columns as modified
            db.SaveChanges();
            Verbose.PrtNoTime("done");
        }
        public void Update(IEnumerable<T> entities)
        {
            Verbose.Prt("SQL", "Writing data to server... ", true);
            using DatabaseWorker<T> db = _createDb();
            db.Table.UpdateRange(entities);
            db.SaveChanges();            // one SaveChanges, runs in a single transaction
            Verbose.PrtNoTime("done");
        }
        public void Update(long id, Action<T> change)
        {
            Verbose.Prt("SQL", "Writing data to server... ", true);
            using DatabaseWorker<T> db = _createDb();
            T e = db.Table.Find(id) ?? throw new KeyNotFoundException($"Id {id} not found");
            change(e);
            db.SaveChanges();
            Verbose.PrtNoTime("done");
        }
        public void Update(IEnumerable<(long ID, Action<T> Change)> changes)
        {
            Verbose.Prt("SQL", "Writing data to server... ", true);
            using DatabaseWorker<T> db = _createDb();
            foreach (var (id, change) in changes)
            {
                T e = db.Table.Find(id) ?? throw new KeyNotFoundException($"Id {id} not found");
                change(e);
            }
            db.SaveChanges();            // all or nothing
            Verbose.PrtNoTime("done");
        }
        // DELETE
        public void Delete(T entity)
        {
            Verbose.Prt("SQL", "Deleting entity... ", true);
            using DatabaseWorker<T> db = _createDb();
            db.Table.Remove(entity);
            db.SaveChanges();
            Verbose.PrtNoTime("done");
        }

        public int Delete(Expression<Func<T, bool>> predicate)
        {
            Verbose.Prt("SQL", "Deleting entity");
            using DatabaseWorker<T> db = _createDb();
            return db.Table.Where(predicate).ExecuteDelete();   // EF Core 7+
        }
        public void Delete(IEnumerable<T> entities)
        {
            Verbose.Prt("SQL", "Deleting entity... ", true);
            using DatabaseWorker<T> db = _createDb();
            db.Table.RemoveRange(entities);
            db.SaveChanges();
            Verbose.PrtNoTime("done");
        }
        /// <summary>
        /// Executes raw SQL using the EF Core. Use this function to prevent system lock-up except if the data is mandatory.
        /// </summary>
        /// <param name="sql">A SQL command. </param>
        /// <returns></returns>
        public async Task<int> ExecuteSqlAsync(FormattableString sql)
        {
            Verbose.Prt("SQL", "Executing raw SQL");
            using DatabaseWorker<T> db = _createDb();
            return await db.Database.ExecuteSqlAsync(sql);
        }
        /// <summary>
        /// Executes raw SQL using the EF Core. Using it incorrectly can cause the system to lock up and crash.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteSql(FormattableString sql)    // synchronous version, for callers that don't use async
        {
            Verbose.Prt("SQL", "Executing raw SQL");
            using DatabaseWorker<T> db = _createDb();
            return db.Database.ExecuteSql(sql);
        }
        /// <summary>
        /// Queries data using raw SQL. It is done via the EF Core, and beware that an incorrect or malformed SQL command can cause an exception or return a List&lt;T&gt; without any content.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<T> QuerySql(FormattableString sql)
        {
            Verbose.Prt("SQL", "Fetching data from raw SQL... ", true);
            using DatabaseWorker<T> db = _createDb();
            List<T> boko = [.. db.Table.FromSql(sql).AsNoTracking()];
            Verbose.PrtNoTime("done");
            return boko;
        }
        /// <summary>
        /// DEVELOPMENT PURPOSE ONLY. Delete the whole database
        /// </summary>
        public void DropDatabase()
        {
#if DEBUG
            using DatabaseWorker<T> db = _createDb();
            Verbose.Prt("SQL", $"Attempting to drop the database {db.DatabaseName}. Please ensure that EnsureTable() is executed if you plan to reuse the object. Also note that this command affects all classes using the database.");
            do
            {
                string? de = Verbose.Input("SQL", "Drop it now? Say \"Yes\" in exact casing to confirm or any other word to cancel?", true);
                if (de == "Yes") break;
                else if (!string.IsNullOrWhiteSpace(de))
                {
                    Verbose.Prt("SQL", "DROP TABLE aborted by the user.");
                    return;
                }
            } while (true);
            db.Database.EnsureDeleted();     // DROP DATABASE; does nothing if it doesn't exist
#else
            Verbose.Prt("ERROR", "SQL cannot delete a whole database in release versions of Uniform Compliance System. DROP TABLE aborted.")
#endif
        }
        public DataTable QueryTable(FormattableString sql)
        {
            Verbose.Prt("SQL", "Fetching table from raw SQL...");
            using DatabaseWorker<T> db = _createDb();
            DbConnection conn = db.Database.GetDbConnection();
            using DbCommand cmd = BuildCommand(conn, sql);

            if (conn.State != ConnectionState.Open) conn.Open();   // disposing the context closes it again
            using DbDataReader reader = cmd.ExecuteReader();

            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        public async Task<DataTable> QueryTableAsync(FormattableString sql, CancellationToken ct = default)
        {
            Verbose.Prt("SQL", "Fetching table from raw SQL...");
            await using DatabaseWorker<T> db = _createDb();
            DbConnection conn = db.Database.GetDbConnection();
            await using DbCommand cmd = BuildCommand(conn, sql);

            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);
            await using DbDataReader reader = await cmd.ExecuteReaderAsync(ct);

            var table = new DataTable();
            table.Load(reader);          // DataTable.Load has no async version, but the I/O above is already async
            return table;
        }

        private static DbCommand BuildCommand(DbConnection conn, FormattableString sql)
        {
            DbCommand cmd = conn.CreateCommand();
            object?[] args = sql.GetArguments();
            string[] names = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                names[i] = $"@p{i}";
                DbParameter p = cmd.CreateParameter();
                p.ParameterName = names[i];
                p.Value = args[i] ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }
            cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql.Format, names);   // {0} becomes @p0, and so on
            return cmd;
        }
    }
    public enum DatabaseType : int
    {
        Local = 0,
        Remote = 1
    }
    public struct Connection()
    {
        public string Database { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public int Port { get; set; } = 0;
        public string Hostname { get; set; } = "";
        public string MdfFilePath { get; set; } = "";

    }
    /// <summary>
    /// This inteface provides a template with a key as a long number (that is a quintillion possible records).<br />
    /// The key must be implemented by the class using it as a database, and do not assign a value to the ID.
    /// </summary>
    public interface IDatabaseTemplate
    {
        [Key]
        public long ID { get; set; }
    }
}
