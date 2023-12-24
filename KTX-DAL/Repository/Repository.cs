using Dapper;
using KTX_DAL.Extension;
using KTX_DAL.IRepository;
using KTX_DAL.Models.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL.Repository
{
    public abstract class Repository<T> : IRepository<T>
        where T : BaseEntity
    {
        protected string DefaultKeyName { get; set; } = "id";
        protected string NamesToSkipCommaDelimited { get; set; } = "Id";
        protected string DefaultDisableColumnName { get; set; } = "is_disabled";

        protected readonly IConfiguration _configuration;
        protected readonly ILogger<T> _logger;
        private readonly string ConnectStringServer;
        private readonly string ConnectStringUID;
        private readonly string ConnectStringPWD;
        private readonly string ConnectStringDB;

        protected IDbConnection DbConnection { get; set; }

        public Repository(IConfiguration configuration, ILogger<T> logger)
        {
            _configuration = configuration;
            ConnectStringServer = _configuration["DBConnect:ConnectString:Server"];
            ConnectStringUID = _configuration["DBConnect:ConnectString:Uid"];
            ConnectStringPWD = _configuration["DBConnect:ConnectString:Pwd"];
            ConnectStringDB = _configuration["DBConnect:ConnectString:Database"];
            _logger = logger;
            SimpleCRUD.SetDialect(SimpleCRUD.Dialect.MySQL);
        }

        protected IDbConnection Connection
        {
            get
            {
                string connectionString = $"server={ConnectStringServer};uid={ConnectStringUID};pwd={ConnectStringPWD};" +
                    $"database={ConnectStringDB};CHARSET=utf8;Allow User Variables=True;convert zero datetime=True";
                return new MySqlConnection(connectionString);
            }
        }
        protected virtual string GetDefaultKeyName()
        {
            if (DefaultKeyName == null)
            {
                var pi = typeof(T).GetProperties();

                foreach (var item in pi)
                {
                    var keyAttribute = item.GetCustomAttribute<KeyAttribute>();

                    if (keyAttribute != null)
                    {
                        DefaultKeyName = item.GetColumnAttributeName();
                        break;
                    }
                }

                if (DefaultKeyName == null)
                    throw new System.Exception("KeyAttribute not found for entity of type: " + typeof(T).Name);
            }

            return DefaultKeyName;
        }

        public List<T> GetAll(QueryFlags flags, int page, int row)
        {
            using (DbConnection = Connection)
            {
                List<T> result;
                try
                {
                    DbConnection.Open();

                    SqlBuilder builder = new SqlBuilder();
                    if (flags == QueryFlags.Enabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 0");
                    }
                    else if (flags == QueryFlags.Disabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 1");
                    }
                    var sql = builder.AddTemplate($"select {QueryBuilder.GetColumnName<T>()} from {QueryBuilder.GetTableName<T>()} /**where**/ /**orderby**/ { (page > 0 ? $"limit {row} offset {(page - 1) * row}" : "")}");
                    builder.OrderBy($"{GetDefaultKeyName()} desc");

                    result = DbConnection.Query<T>(sql.RawSql, sql.Parameters).ToList();
                }
                catch (Exception ex)
                {
                    Log(ex);
                    result = new List<T>();
                }
                finally
                {
                    CloseConnection();

                }
                return result;
            }
        }
        public T GetById(int id, QueryFlags flags)
        {
            using (DbConnection = Connection)
            {
                T result;
                try
                {
                    DbConnection.Open();

                    SqlBuilder builder = new SqlBuilder();
                    if (flags == QueryFlags.Enabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 0");
                    }
                    else if (flags == QueryFlags.Disabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 1");
                    }
                    builder.Where($"{DefaultKeyName} = @id", new { id });

                    var sql = builder.AddTemplate($"select {QueryBuilder.GetColumnName<T>()} from {QueryBuilder.GetTableName<T>()} /**where**/");

                    result = DbConnection.QueryFirstOrDefault<T>(sql.RawSql, sql.Parameters);
                }
                catch (Exception ex)
                {
                    Log(ex, message: id.ToString());
                    result = null;
                }
                finally
                {
                    CloseConnection();
                }
                return result;
            }
        }

        public List<T> GetByListId(List<int> id, QueryFlags flags, int page = 0, int row = 0)
        {
            if (id == null || id.Count == 0)
                return new List<T>();
            List<T> result;
            using (DbConnection = Connection)
            {
                try
                {
                    DbConnection.Open();

                    SqlBuilder builder = new SqlBuilder();
                    if (flags == QueryFlags.Enabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 0");
                    }
                    else if (flags == QueryFlags.Disabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 1");
                    }
                    builder.Where($"{DefaultKeyName} in ({string.Join(",", id.Select(x => x.ToString()))})");
                    var sql = builder.AddTemplate($"select {QueryBuilder.GetColumnName<T>()} from {QueryBuilder.GetTableName<T>()} /**where**/ { (page > 0 ? $"limit {row} offset {(page - 1) * row}" : "")}");

                    result = DbConnection.Query<T>(sql.RawSql, sql.Parameters).ToList();
                }
                catch (Exception ex)
                {
                    Log(ex, message: JsonConvert.SerializeObject(id));
                    result = new List<T>();
                }
                finally
                {
                    CloseConnection();

                }
                return result;
            }
        }

        public T GetByKey(string key, object value, QueryFlags flags)
        {
            using (DbConnection = Connection)
            {
                T result;
                try
                {
                    DbConnection.Open();
                    key = Extension.PropertyInfoExtensions.GetColumnAttributeName<T>(key);
                    SqlBuilder builder = new SqlBuilder();
                    if (flags == QueryFlags.Enabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 0");
                    }
                    else if (flags == QueryFlags.Disabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 1");
                    }
                    if (value.GetType() == typeof(string))
                        value = $"'{value}'";
                    builder.Where($"{key} = {value}");
                    var sql = builder.AddTemplate($"select {QueryBuilder.GetColumnName<T>()} from {QueryBuilder.GetTableName<T>()} /**where**/ /**orderby**/");
                    builder.OrderBy($"{GetDefaultKeyName()} DESC");
                    result = DbConnection.QueryFirstOrDefault<T>(sql.RawSql, sql.Parameters);
                }
                catch (Exception ex)
                {
                    Log(ex, message: $"key: {key}, value: {value}");
                    result = null;
                }
                finally
                {
                    CloseConnection();
                }
                return result;
            }
        }

        public List<T> GetListByKey(string key, object value, QueryFlags flags)
        {
            using (DbConnection = Connection)
            {
                List<T> result;
                try
                {
                    DbConnection.Open();
                    key = Extension.PropertyInfoExtensions.GetColumnAttributeName<T>(key);
                    SqlBuilder builder = new SqlBuilder();
                    if (flags == QueryFlags.Enabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 0");
                    }
                    else if (flags == QueryFlags.Disabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 1");
                    }
                    if (value.GetType() == typeof(string))
                        value = $"'{value}'";
                    builder.Where($"{key} = {value}");

                    var sql = builder.AddTemplate($"select {QueryBuilder.GetColumnName<T>()} from {QueryBuilder.GetTableName<T>()} /**where**/");
                    result = DbConnection.Query<T>(sql.RawSql, sql.Parameters).ToList();
                }
                catch (Exception ex)
                {
                    Log(ex, message: JsonConvert.SerializeObject(new { key, value }));
                    result = new List<T>();
                }
                finally
                {
                    CloseConnection();
                }
                return result;
            }
        }

        public List<T> GetListByListKey(string key, List<object> values, QueryFlags flags)
        {
            if (values == null || values.Count == 0)
                return new List<T>();
            using (DbConnection = Connection)
            {
                List<T> result;
                try
                {
                    DbConnection.Open();
                    key = Extension.PropertyInfoExtensions.GetColumnAttributeName<T>(key);
                    SqlBuilder builder = new SqlBuilder();
                    if (flags == QueryFlags.Enabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 0");
                    }
                    else if (flags == QueryFlags.Disabled)
                    {
                        builder.Where($"{DefaultDisableColumnName} = 1");
                    }
                    if ((values?.Count ?? 0) > 0)
                    {
                        if (values[0].GetType() == typeof(string) || values[0].GetType() == typeof(DateTime))
                            builder.Where($"{key} in ({string.Join(",", values.Select(x => $"'{x}'"))})");
                        else
                            builder.Where($"{key} in ({string.Join(",", values.Select(x => $"{x}"))})");
                    }
                    else return new List<T>();

                    var sql = builder.AddTemplate($"select {QueryBuilder.GetColumnName<T>()} from {QueryBuilder.GetTableName<T>()} /**where**/");
                    result = DbConnection.Query<T>(sql.RawSql, sql.Parameters).ToList();
                }
                catch (Exception ex)
                {
                    Log(ex, message: JsonConvert.SerializeObject(new { key, values }));
                    result = new List<T>();
                }
                finally
                {
                    CloseConnection();
                }
                return result;
            }
        }   

        public Task<int> Upsert<T2>(List<T2> entity) where T2 : class
        {
            return Task.Run<int>(async () =>
            {
                using (IDbConnection dbConnection = Connection)
                {
                    int result;
                    dbConnection.Open();
                    try
                    {

                        Type tType = typeof(T);
                        var props = entity[0].GetType().GetProperties();
                        List<string> cols = new List<string>();
                        foreach (var item in props)
                        {
                            if (item.CanRead)
                            {
                                var tProp = tType.GetProperty(item.Name);

                                if (tProp != null)
                                {
                                    string name = tProp.GetColumnAttributeName();
                                    if (!string.IsNullOrEmpty(name))
                                        cols.Add(name);
                                }
                            }
                        }
                        List<string> listNamesToSkip = string.IsNullOrWhiteSpace(NamesToSkipCommaDelimited) ? new List<string>() : NamesToSkipCommaDelimited.Split(',').AsList();
                        var sql = QueryBuilder.Upsert<T>(entity[0], cols, false, DefaultKeyName);
                        await dbConnection.ExecuteAsync(sql, entity);
                        result = 1;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        result = 0;
                        Log(ex, message: JsonConvert.SerializeObject(entity));
                    }
                    finally
                    {
                        dbConnection.Close();
                    }
                    return result;
                }
            });
        }

        public Task<int> Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("The entity parameter cannot be null.");
            }
            return Task.Run<int>(async () =>
            {
                using (DbConnection = Connection)
                {
                    int result;
                    DbConnection.Open();
                    try
                    {
                        List<string> listNamesToSkip = string.IsNullOrWhiteSpace(NamesToSkipCommaDelimited) ? new List<string>() : NamesToSkipCommaDelimited.Split(',').AsList();
                        result = await DbConnection.ExecuteScalarAsync<int>(QueryBuilder.InsertQuery<T>(entity, listNamesToSkip, false), entity);
                    }
                    catch (Exception ex)
                    {
                        result = 0;
                        Log(ex, message: JsonConvert.SerializeObject(entity));
                    }
                    finally
                    {
                        CloseConnection();
                    }
                    return result;
                }
            });
        }

        public Task<int> Insert<T2>(T2 entity, IDbTransaction transaction = null) where T2 : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("The entity parameter cannot be null.");
            }
            return Task.Run<int>(async () =>
            {
                using (DbConnection = Connection)
                {
                    int result;
                    DbConnection.Open();
                    try
                    {
                        List<string> listNamesToSkip = string.IsNullOrWhiteSpace(NamesToSkipCommaDelimited) ? new List<string>() : NamesToSkipCommaDelimited.Split(',').AsList();
                        result = await DbConnection.ExecuteScalarAsync<int>(QueryBuilder.InsertQuery<T2>(entity, listNamesToSkip, false), entity, transaction);
                    }
                    catch (Exception ex)
                    {
                        result = 0;
                        Log(ex, message: JsonConvert.SerializeObject(entity));
                    }
                    finally
                    {
                        CloseConnection();
                    }
                    return result;
                }
            });
        }

        public Task<int> Insert(List<T> entity)
        {
            return Task.Run<int>(async () =>
            {
                if (entity == null || entity.Count == 0)
                {
                    return 1;
                }
                using (DbConnection = Connection)
                {
                    int result;
                    DbConnection.Open();
                    try
                    {
                        List<string> listNamesToSkip = string.IsNullOrWhiteSpace(NamesToSkipCommaDelimited) ? new List<string>() : NamesToSkipCommaDelimited.Split(',').AsList();
                        await DbConnection.ExecuteAsync(QueryBuilder.InsertQuery<T>(entity[0], listNamesToSkip, false), entity);
                        result = 1;
                    }
                    catch (Exception ex)
                    {
                        result = 0;
                        Log(ex, message: JsonConvert.SerializeObject(entity));
                    }
                    finally
                    {
                        CloseConnection();
                    }
                    return result;
                }
            });

        }

        public Task<int> Insert<T2>(List<T2> entity, IDbTransaction transaction = null) where T2 : class
        {
            return Task.Run<int>(async () =>
            {
                if (entity == null || entity.Count == 0)
                {
                    return 1;
                }
                using (DbConnection = Connection)
                {
                    int result;
                    DbConnection.Open();
                    try
                    {
                        List<string> listNamesToSkip = string.IsNullOrWhiteSpace(NamesToSkipCommaDelimited) ? new List<string>() : NamesToSkipCommaDelimited.Split(',').AsList();
                        await DbConnection.ExecuteAsync(QueryBuilder.InsertQuery<T2>(entity[0], listNamesToSkip, false), entity, transaction);
                        result = 1;
                    }
                    catch (Exception ex)
                    {
                        result = 0;
                        Log(ex, message: JsonConvert.SerializeObject(entity));
                    }
                    finally
                    {
                        CloseConnection();
                    }
                    return result;
                }
            });
        }
        public Task<int> Update<T2>(int id, T2 data) where T2 : class
        {
            return Task.Run<int>(() =>
            {
                int numberOfRowsAffected = 0;

                using (DbConnection = Connection)
                {
                    DbConnection.Open();
                    using (IDbTransaction transaction = DbConnection.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        string sql = "";
                        try
                        {
                            sql = QueryBuilder.UpdateQuery<T>(data, $"{DefaultKeyName} = " + id);
                            numberOfRowsAffected = DbConnection.Execute(sql, data);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log(ex, message: JsonConvert.SerializeObject(new { sql, data }));
                            transaction.Rollback();
                            numberOfRowsAffected = 0;
                        }
                        finally
                        {
                            CloseConnection();
                        }
                    }
                }
                return numberOfRowsAffected;
            });
        }

        public Task<int> Update<T2>(List<int> listIds, T2 data) where T2 : class
        {
            return Task.Run<int>(() =>
            {
                int numberOfRowsAffected = 0;

                using (DbConnection = Connection)
                {
                    DbConnection.Open();
                    using (IDbTransaction transaction = DbConnection.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            numberOfRowsAffected = DbConnection.Execute(QueryBuilder.UpdateQuery<T>(data, $"{DefaultKeyName} in ({string.Join(", ", listIds)})"), data);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log(ex, message: JsonConvert.SerializeObject(data));
                            transaction.Rollback();
                            numberOfRowsAffected = 0;
                        }
                        finally
                        {
                            CloseConnection();
                        }
                    }
                }
                return numberOfRowsAffected;
            });
        }

        public Task<int> Disable(int id)
        {
            return Task.Run<int>(async () =>
            {
                using (DbConnection = Connection)
                {
                    int result;

                    try
                    {
                        DbConnection.Open();
                        result = await Connection.ExecuteAsync(QueryBuilder.DisableQuery<T>(id, GetDefaultKeyName(), DefaultDisableColumnName));
                    }
                    catch (Exception ex)
                    {
                        Log(ex, message: id.ToString());
                        result = 0;
                    }
                    finally
                    {
                        CloseConnection();
                    }
                    return result;
                }
            });
        }

        public Task<int> Disable(List<int> id)
        {
            return Task.Run<int>(async () =>
            {
                using (DbConnection = Connection)
                {
                    int result;

                    try
                    {
                        DbConnection.Open();
                        result = await Connection.ExecuteAsync(QueryBuilder.DisableQuery<T>(id, GetDefaultKeyName(), DefaultDisableColumnName));
                    }
                    catch (Exception ex)
                    {
                        Log(ex, message: id.ToString());
                        result = 0;
                    }
                    finally
                    {
                        CloseConnection();
                    }
                    return result;
                }
            });
        }

        public Task<int> Disable(int id, int disabledBy)
        {
            return Task.Run<int>(async () =>
            {
                using (DbConnection = Connection)
                {
                    int result;

                    try
                    {
                        DbConnection.Open();
                        result = await Connection.ExecuteAsync(QueryBuilder.DisableQuery<T>(id, disabledBy, GetDefaultKeyName(), DefaultDisableColumnName));
                    }
                    catch (Exception ex)
                    {
                        Log(ex, message: id.ToString());
                        result = 0;
                    }
                    finally
                    {
                        CloseConnection();
                    }
                    return result;
                }
            });
        }

        protected void Log(Exception ex, LoggerFlags loggerFlags = LoggerFlags.Error, string message = "")
        {
            if (loggerFlags == LoggerFlags.Error)
                _logger.LogError(ex, message);
            else
                _logger.LogWarning(ex, message);

        }
        protected void CloseConnection()
        {
            if (DbConnection != null)
                DbConnection.Close();
        }
    }
}
