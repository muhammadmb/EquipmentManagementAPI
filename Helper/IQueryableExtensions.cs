using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace EquipmentAPI.Helper
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
    }
}
