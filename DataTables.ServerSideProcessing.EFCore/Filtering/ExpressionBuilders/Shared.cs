using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class Shared
{
    internal static (ParameterExpression Parameter, MemberExpression MemberAccess, Type PropertyType) GetPropertyExpressionParts<T>(string propertyName) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);
        Type propertyType = propertyInfo.PropertyType;

        return (parameter, memberAccess, propertyType);
    }
}
