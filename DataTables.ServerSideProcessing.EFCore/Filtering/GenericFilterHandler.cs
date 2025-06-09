using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;
internal static class GenericFilterHandler
{
    internal static IQueryable<T> HandleGenericFilter<T>(this IQueryable<T> query, IEnumerable<string> properties, string search) where T : class
    {
        if (!properties.Any())
            return query;

        Expression? combinedExpression = null;
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e");

        foreach (var property in properties)
        {
            PropertyInfo? propertyInfo = typeof(T).GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{property}' not found on type '{typeof(T).Name}'.");

            Type propertyType = propertyInfo.PropertyType;

            Expression propertyAccess = Expression.Property(parameter, propertyInfo);
            Expression propertyAsString;
            // Convert to string for comparison
            if (propertyType == typeof(string))
            {
                propertyAsString = Expression.Coalesce(propertyAccess, Expression.Constant(string.Empty));
            }
            else
            {
                // Convert non-strings to string using ToString, adding null check
                Expression nullCheck = Expression.Equal(propertyAccess, Expression.Constant(default, propertyType));
                Expression defaultValue = Expression.Constant(string.Empty);
                Expression toStringCall = Expression.Call(propertyAccess, propertyType.GetMethod("ToString", Type.EmptyTypes)!);
                propertyAsString = Expression.Condition(nullCheck, defaultValue, toStringCall);
            }

            var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
            var searchExpression = Expression.Constant(search);
            var predicate = Expression.Call(propertyAsString, containsMethod, searchExpression);

            combinedExpression = combinedExpression == null
                ? predicate
                : Expression.OrElse(combinedExpression, predicate);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression!, parameter);
        return query.Where(lambda);
    }
}
