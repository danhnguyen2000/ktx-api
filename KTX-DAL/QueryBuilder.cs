using KTX_DAL.Extension;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL
{
    public class QueryBuilder
    {
        private readonly IConfiguration _configuration;

        public QueryBuilder(
            IConfiguration configuration
        )
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get mapping select Column attribute as column name
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        /// <returns>string</returns>
        public static string GetColumnName<T>() where T : class
        {
            Type tType = typeof(T);
            string mapping = "";
            var properties = tType.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<ColumnAttribute>();
                if (attribute != null)
                    mapping += $"`{attribute.Name}` as `{property.Name}`,";
            }
            if (mapping.Length > 0)
                mapping = mapping.Substring(0, mapping.Length - 1);
            return mapping;
        }

        public static string GetTableName<T>() where T : class
        {
            Type tType = typeof(T);
            return "`" + tType.GetAttributeValue((TableAttribute ta) => ta.Name) + "`" ?? tType.Name;
        }
        public static string Upsert<T>(object obj, List<string> namesToUpdate = null, bool isUseColumnAttributeName = false, string primaryKey = "id", List<string> namesToSkip = null)
            where T : class
        {
            if (namesToSkip == null)
            {
                namesToSkip = new List<string>();
            }

            Type tType = typeof(T);
            var props = obj.GetType().GetProperties();

            List<string> cols = new List<string>();
            List<string> colUpdates = new List<string>();
            List<string> colValueParams = new List<string>();

            foreach (var item in props)
            {
                if (item.CanRead && !namesToSkip.Contains(item.Name))
                {
                    var tProp = tType.GetProperty(item.Name);

                    if (tProp != null)
                    {
                        ColumnAttribute cAttr = tProp.GetCustomAttribute<ColumnAttribute>();
                        if (cAttr != null)
                        {
                            string name = cAttr.Name;
                            cols.Add("`" + name + "`");
                            colUpdates.Add($"{"`" + name + "`"}=VALUES({"`" + name + "`"})");
                            colValueParams.Add("@" + (isUseColumnAttributeName ? name : item.Name));
                        }
                    }
                }
            }

            return @"INSERT INTO " + GetTableName<T>() + @"
                    (
                        " + string.Join(",", cols) + @"
                    )
	                VALUES (
                            " + string.Join(",", colValueParams) + @"
                    )
	                ON DUPLICATE KEY UPDATE
                            " + string.Join(",", colUpdates) + @"
                    ;";
        }

        public static string InsertQuery<T>(object obj, List<string> namesToSkip = null, bool isUseColumnAttributeName = false)
            where T : class
        {
            if (namesToSkip == null)
            {
                namesToSkip = new List<string>();
            }

            Type tType = typeof(T);
            var props = obj.GetType().GetProperties();

            List<string> cols = new List<string>();
            List<string> colValueParams = new List<string>();

            foreach (var item in props)
            {
                if (item.CanRead && !namesToSkip.Contains(item.Name))
                {
                    var tProp = tType.GetProperty(item.Name);

                    if (tProp != null)
                    {
                        string name = tProp.GetColumnAttributeName();
                        cols.Add("`" + name + "`");
                        colValueParams.Add("@" + (isUseColumnAttributeName ? name : item.Name));
                    }
                }
            }

            return @"INSERT INTO " + GetTableName<T>() + @"
                    (
                        " + string.Join(",", cols) + @"
                    )
	                VALUES (
                            " + string.Join(",", colValueParams) + @"
                    ); SELECT LAST_INSERT_ID();";
        }

        /// <summary>
        /// Get Update query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="whereCondition"></param>
        /// <param name="namesToSkip"></param>
        /// <param name="isUseColumnAttributeName"></param>
        /// <returns></returns>
        public static string UpdateQuery<T>(object obj, string whereCondition)
            where T : class
        {

            Type tType = typeof(T);
            var props = obj.GetType().GetProperties();

            List<string> cols = new List<string>();

            foreach (var item in props)
            {
                if (item.CanRead)
                {
                    var tProp = tType.GetProperty(item.Name);

                    if (tProp != null)
                    {
                        string name = tProp.GetColumnAttributeName();

                        cols.Add("`" + name + "`" + " = @" + item.Name);
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(whereCondition))
            {
                return $"Update {GetTableName<T>()} SET {string.Join(",", cols)};";
            }
            else
            {
                return $"Update {GetTableName<T>()} SET {string.Join(",", cols)} WHERE {whereCondition};";
            }
        }

        public static string DisableQuery<T>(int id, string defaultKeyName, string defaultDisableColumnName)
             where T : class
        {
            return $"UPDATE {GetTableName<T>()} SET {defaultDisableColumnName} = true WHERE {defaultKeyName} = {id}";
        }

        public static string DisableQuery<T>(List<int> id, string defaultKeyName, string defaultDisableColumnName)
             where T : class
        {
            return $"UPDATE {GetTableName<T>()} SET {defaultDisableColumnName} = true WHERE {defaultKeyName} in ({string.Join(",", id.Select(x => x.ToString()))})";
        }

        public static string DisableQuery<T>(int id, int disableBy, string defaultKeyName = "id", string defaultDisableColumnName = "is_disabled")
            where T : class
        {
            return $"UPDATE {GetTableName<T>()} SET { defaultDisableColumnName } = 1, modified_by = { disableBy }, modified_date = '{ DateTime.Now }' WHERE { defaultKeyName } = { id }";
        }
    }
}
