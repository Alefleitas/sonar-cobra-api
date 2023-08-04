using System;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Helpers
{
    public static class TypeExtensions
    {
        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return type is not null && Attribute.GetCustomAttributes(type, typeof(T), false).Length > 0;
        }

        public static IEnumerable<Type> GetInheritanceHierarchy(this Type type)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                yield return current;
            }
        }
    }
}
