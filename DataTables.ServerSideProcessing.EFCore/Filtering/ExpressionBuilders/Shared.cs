using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class Shared
{
    internal static (ParameterExpression Parameter, MemberExpression MemberAccess, Type PropertyType) GetPropertyExpressionParts<T>(PropertyInfo propertyInfo) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);
        Type propertyType = propertyInfo.PropertyType;

        return (parameter, memberAccess, propertyType);
    }
}
