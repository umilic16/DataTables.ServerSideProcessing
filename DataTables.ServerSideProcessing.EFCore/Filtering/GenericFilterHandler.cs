using DataTables.ServerSideProcessing.Data.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Reflection;
using static DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilder;


namespace DataTables.ServerSideProcessing.EFCore.Filtering;
internal class GenericFilterHandler
{
    internal static IQueryable<T> HandleGenericFilter<T>(List<string> properties, string search, IQueryable<T> query) where T : class
    {
        if (string.IsNullOrEmpty(search) || properties.Count == 0)
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
