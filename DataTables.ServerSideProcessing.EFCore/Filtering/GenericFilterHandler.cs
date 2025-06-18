using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;
/// <summary>
/// Provides an extension method for applying generic filtering to <see cref="IQueryable{T}"/> source
/// based on a set of property names and a search string.
/// </summary>
internal static class GenericFilterHandler
{
    /// <summary>
    /// Filters the query by checking if any of the specified properties of <typeparamref name="T"/> contain the given search string.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query to filter.</param>
    /// <param name="properties">A collection of property names to search within.</param>
    /// <param name="search">The search string to look for in the property values.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> filtered such that any of the specified properties contain the search string.
    /// If <paramref name="properties"/> is empty, the original query is returned.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a property name in <paramref name="properties"/> does not exist on <typeparamref name="T"/>.
    /// </exception>
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
            if (propertyType == typeof(string))
            {
                propertyAsString = Expression.Coalesce(propertyAccess, Expression.Constant(string.Empty));
            }
            else if (!propertyType.IsValueType
                     || Nullable.GetUnderlyingType(propertyType) != null)
            {
                // Convert non-strings to string using ToString, adding null check
                Expression nullCheck = Expression.Equal(propertyAccess, Expression.Constant(null, propertyType));
                Expression defaultValue = Expression.Constant(string.Empty);
                Expression toStringCall = Expression.Call(propertyAccess, propertyType.GetMethod("ToString", Type.EmptyTypes)!);
                propertyAsString = Expression.Condition(nullCheck, defaultValue, toStringCall);
            }
            else
            {
                propertyAsString = Expression.Call(propertyAccess, propertyType.GetMethod("ToString", Type.EmptyTypes)!);
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
