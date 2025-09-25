using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.EFCore.ReflectionCache;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class GlobalFilterHandler
{
    internal static IQueryable<T> HandleGlobalFilter<T>(this IQueryable<T> query, string[]? properties, string? search) where T : class
    {
        if (properties is not { Length: > 0 } || string.IsNullOrEmpty(search))
            return query;

        Expression? combinedExpression = null;
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e");

        foreach (string property in properties)
        {
            PropertyInfo propertyInfo = PropertyInfoCache<T>.GetProperty(property);

            Type propertyType = propertyInfo.PropertyType;

            Expression propertyAccess = Expression.Property(parameter, propertyInfo);
            Expression propertyAsString;
            if (propertyType == typeof(string))
            {
                propertyAsString = Expression.Coalesce(propertyAccess, Expression.Constant(string.Empty));
            }
            else if (!propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) is not null)
            {
                // Convert non-strings to string using ToString, adding null check
                Expression nullCheck = Expression.Equal(propertyAccess, Expression.Constant(null, propertyType));
                Expression defaultValue = Expression.Constant(string.Empty);
                Expression toStringCall = Expression.Call(propertyAccess, MethodInfoCache.s_toString);
                propertyAsString = Expression.Condition(nullCheck, defaultValue, toStringCall);
            }
            else
            {
                propertyAsString = Expression.Call(propertyAccess, MethodInfoCache.s_toString);
            }

            MethodInfo containsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
            ConstantExpression searchExpression = Expression.Constant(search);
            MethodCallExpression predicate = Expression.Call(propertyAsString, containsMethod, searchExpression);

            combinedExpression = combinedExpression == null
                ? predicate
                : Expression.OrElse(combinedExpression, predicate);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression!, parameter);
        return query.Where(lambda);
    }
}
