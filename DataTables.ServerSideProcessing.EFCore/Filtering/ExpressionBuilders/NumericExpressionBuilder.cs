using System.Linq.Expressions;
using System.Numerics;
using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class NumericExpressionBuilder
{
    internal static Expression<Func<T, bool>> Build<T, S>(string propertyName, FilterOperations filterType, S searchValue)
        where T : class where S : INumber<S>
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType) = Shared.GetPropertyExpressionParts<T>(propertyName);
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (!underlyingType.IsNumericType()) throw new InvalidOperationException($"Property '{propertyName}' must be of type 'int' but is of type '{propertyType.Name}'.");

        ConstantExpression constantValue = Expression.Constant(searchValue, underlyingType);
        Expression comparison = filterType switch
        {
            FilterOperations.Equals => Expression.Equal(memberAccess, constantValue),
            FilterOperations.NotEqual => Expression.NotEqual(memberAccess, constantValue),
            FilterOperations.GreaterThan => Expression.GreaterThan(memberAccess, constantValue),
            FilterOperations.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, constantValue),
            FilterOperations.LessThan => Expression.LessThan(memberAccess, constantValue),
            FilterOperations.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, constantValue),
            _ => throw new NotImplementedException($"Filter operation '{filterType}' is not implemented for numeric filtering.")
        };
        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static bool IsNumericType(this Type type)
    {
        return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) ||
               type == typeof(long) || type == typeof(short);
    }
}
