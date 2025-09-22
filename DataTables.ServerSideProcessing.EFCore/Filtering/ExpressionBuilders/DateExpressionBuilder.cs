using System.Linq.Expressions;
using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class DateExpressionBuilder
{
    internal static Expression<Func<T, bool>> Build<T>(string propertyName, FilterOperations filterType, DateOnly searchValue) where T : class
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType) = Shared.GetPropertyExpressionParts<T>(propertyName);
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlyingType != typeof(DateTime) && underlyingType != typeof(DateOnly))
            throw new InvalidOperationException($"Property '{propertyName}' is not a DateTime/DateOnly type.");

        ConstantExpression constantValue = underlyingType == typeof(DateTime)
            ? Expression.Constant(searchValue.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), propertyType)
            : Expression.Constant(searchValue, propertyType);

        Expression comparison = filterType switch
        {
            FilterOperations.Equals => Expression.Equal(memberAccess, constantValue),
            FilterOperations.NotEqual => Expression.NotEqual(memberAccess, constantValue),
            FilterOperations.GreaterThan => Expression.GreaterThan(memberAccess, constantValue),
            FilterOperations.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, constantValue),
            FilterOperations.LessThan => Expression.LessThan(memberAccess, constantValue),
            FilterOperations.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, constantValue),
            _ => throw new InvalidOperationException($"Filter operation '{filterType}' is not valid for date filtering.")
        };
        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }
}
