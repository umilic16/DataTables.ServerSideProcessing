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

        if (underlyingType != typeof(DateTime) && underlyingType != typeof(DateOnly) && underlyingType != typeof(DateTimeOffset))
            throw new InvalidOperationException($"Property '{propertyName}' is not a DateTime/DateOnly type.");

        Expression comparison;
        if (underlyingType == typeof(DateOnly))
        {
            ConstantExpression constantValue = Expression.Constant(searchValue, propertyType);

            comparison = filterType switch
            {
                FilterOperations.Equals => Expression.Equal(memberAccess, constantValue),
                FilterOperations.NotEqual => Expression.NotEqual(memberAccess, constantValue),
                FilterOperations.GreaterThan => Expression.GreaterThan(memberAccess, constantValue),
                FilterOperations.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, constantValue),
                FilterOperations.LessThan => Expression.LessThan(memberAccess, constantValue),
                FilterOperations.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, constantValue),
                _ => throw new InvalidOperationException($"Filter operation '{filterType}' is not valid for date filtering.")
            };
        }
        else
        {
            // For DateTime or DateTimeOffset, create values with time component set to start and end of the day
            ConstantExpression start;
            ConstantExpression end;
            if (underlyingType == typeof(DateTimeOffset))
            {
                start = Expression.Constant(new DateTimeOffset(searchValue.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), propertyType);
                end = Expression.Constant(new DateTimeOffset(searchValue.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), propertyType);
            }
            else
            {
                // DateTime
                start = Expression.Constant(searchValue.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), propertyType);
                end = Expression.Constant(searchValue.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), propertyType);
            }

            comparison = filterType switch
            {
                FilterOperations.Equals =>
                    Expression.AndAlso(
                        Expression.GreaterThanOrEqual(memberAccess, start),
                        Expression.LessThan(memberAccess, end)
                    ),
                FilterOperations.NotEqual =>
                    Expression.OrElse(
                        Expression.LessThan(memberAccess, start),
                        Expression.GreaterThanOrEqual(memberAccess, end)
                    ),
                FilterOperations.GreaterThan => Expression.GreaterThan(memberAccess, start),
                FilterOperations.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, start),
                FilterOperations.LessThan => Expression.LessThan(memberAccess, end),
                FilterOperations.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, end),
                _ => throw new InvalidOperationException($"Filter operation '{filterType}' is not valid for date filtering.")
            };
        }
        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }



}
