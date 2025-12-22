using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Shared.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string propertyName, bool descending = false)
        {
            var entityType = typeof(T);
            var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null) throw new ArgumentException($"Property '{propertyName}' not found on type '{entityType.Name}'.");

            var parameter = Expression.Parameter(entityType, "e");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            var orderByMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == (descending ? "OrderByDescending" : "OrderBy") && m.GetParameters().Length == 2)
                .MakeGenericMethod(entityType, property.PropertyType);

            return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { source, orderByExpression });
        }

        public static IQueryable<T> ApplyInclude<T>(this IQueryable<T> source, string fields) where T : class
        {
            if (string.IsNullOrWhiteSpace(fields)) return source;

            var fieldsList = fields.Split(',').Select(f => f.Trim()).ToList();

            var entityType = typeof(T);
            var navProperties =
                entityType.GetProperties()
                .Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string) && p.PropertyType != typeof(byte[])).ToList();

            foreach (var property in navProperties)
            {
                if (fieldsList.Contains(property.Name, StringComparer.OrdinalIgnoreCase)) source = source.Include(property.Name);
            }

            return source;
        }
    }
}
