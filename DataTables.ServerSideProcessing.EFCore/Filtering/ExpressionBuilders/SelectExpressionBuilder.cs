using System.Linq.Expressions;
using DataTables.ServerSideProcessing.EFCore.ReflectionCache;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class SelectExpressionBuilder
{
    internal static Expression<Func<T, bool>> BuildSingleSelect<T>(string propertyName, string searchValue) where T : class
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType) = Shared.GetPropertyExpressionParts<T>(propertyName);

        Expression memberAsString = propertyType == typeof(string)
            ? memberAccess // e.Property for string properties
            : Expression.Call(memberAccess, MethodInfoCache.s_toString); // e.Property.ToString() for non-string properties

        ConstantExpression constantValue = Expression.Constant(searchValue);
        Expression comparison = Expression.Equal(memberAsString, constantValue);

        // property is reference type or nullable value type, check for null before comparing
        if (!propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) is not null)
        {
            Expression notNull = Expression.NotEqual(memberAccess, Expression.Constant(null, propertyType));
            // e.Property != null && e.Property.ToString() == searchValue
            comparison = Expression.AndAlso(notNull, comparison);
        }

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }
}
