using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    public static partial class CustomExtensions
    {
        public static IQueryable<Object> Set(this DbContext _context, Type t)
        {
            var method = typeof(DbContext).GetMethods().Single(p =>
                p.Name == nameof(DbContext.Set) && p.ContainsGenericParameters && !p.GetParameters().Any());

            // Build a method with the specific type argument you're interested in
            method = method.MakeGenericMethod(t);

            return (IQueryable<Object>)method.Invoke(_context, null);
        }

        public static IQueryable<Object> Set(this DbContext _context, String table)
        {
            Type TableType = _context.GetType().Assembly.GetExportedTypes().FirstOrDefault(t => t.Name == table);
            IQueryable<Object> ObjectContext = _context.Set(TableType);
            return ObjectContext;
        }
    }
}