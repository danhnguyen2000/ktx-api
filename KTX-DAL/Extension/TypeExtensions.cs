using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL.Extension
{
    public static class TypeExtensions
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(
        this Type type,
        Func<TAttribute, TValue> valueSelector)
        where TAttribute : Attribute
        {
            return type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() is TAttribute att ? valueSelector(att) : default;
        }
    }
}
