using System.Linq.Expressions;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class MultiSelectExpressionBuilder
{
    internal static Expression<Func<T, bool>> BuildMultiSelect<T>(string propertyName, string[] searchValues) where T : class
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType) = Shared.GetPropertyExpressionParts<T>(propertyName);

        Expression memberAsString = propertyType == typeof(string) ?
            memberAccess : // e.Property for string properties
            Expression.Call(memberAccess, typeof(object).GetMethod(nameof(ToString))!); // e.Property.ToString() for non-string properties

        ConstantExpression constantList = Expression.Constant(searchValues);
        // searchValues.Contains(e.Property.ToString())
        Expression containsCall = Expression.Call(constantList, typeof(List<string>).GetMethod(nameof(List<string>.Contains), [typeof(string)])!, memberAsString);

        // property is reference type or nullable value type, eliminate nulls before checking Contains
        if (!propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) is not null)
        {
            Expression notNull = Expression.NotEqual(memberAccess, Expression.Constant(null, propertyType));
            // e.Property != null && searchValues.Contains(e.Property.ToString())
            containsCall = Expression.AndAlso(notNull, containsCall);
        }

        return Expression.Lambda<Func<T, bool>>(containsCall, parameter);
    }
}
