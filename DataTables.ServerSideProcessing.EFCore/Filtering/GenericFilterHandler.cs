using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;
internal static class GenericFilterHandler
{
    internal static IQueryable<T> HandleGenericFilter<T>(IQueryable<T> query, IEnumerable<string> properties, string? search = null) where T : class
    {
        if (string.IsNullOrEmpty(search) || !properties.Any())
            return query;

        Expression? combinedExpression = null;
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e");

        foreach (var property in properties)
        {
            PropertyInfo? propertyInfo = typeof(T).GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{property}' not found on type '{typeof(T).Name}'.");

            Type propertyType = propertyInfo.PropertyType;
            var propAsString = propertyType == typeof(string) ? (Expression)Expression.Coalesce(parameter, Expression.Constant(string.Empty)) : Expression.Call(parameter, parameter.Type.GetMethod("ToString", Type.EmptyTypes)!);

            var predicate = Expression.Call(propAsString, typeof(string).GetMethod("Contains", [typeof(string)])!, Expression.Constant(search));

            combinedExpression = combinedExpression == null
                ? predicate
                : Expression.OrElse(combinedExpression, predicate);
        }
        return query;
    }
}
