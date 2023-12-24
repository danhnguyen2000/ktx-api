using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL.Extension
{
    public static class PropertyInfoExtensions
    {
        public static string GetColumnAttributeName(this PropertyInfo pInfo)
        {
            ColumnAttribute cAttr = pInfo.GetCustomAttribute<ColumnAttribute>();

            if (cAttr != null)
            {
                return cAttr.Name;
            }
            else
            {
                return pInfo.Name;
            }
        }

        /// <summary>
        /// Get mapping select Column attribute as column name of 1 field
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        /// <param name="columnName">Name of column</param>
        /// <returns></returns>
        public static string GetColumnAttributeName<T>(string columnName) where T : class
        {
            Type tType = typeof(T);
            var properties = tType.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<ColumnAttribute>();
                if (attribute != null && property.Name == columnName)
                    return attribute.Name;
            }
            return columnName;
        }
    }
}
