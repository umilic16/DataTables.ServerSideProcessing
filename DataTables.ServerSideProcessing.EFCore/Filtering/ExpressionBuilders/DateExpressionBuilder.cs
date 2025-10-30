using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class DateExpressionBuilder
{
    internal static Expression<Func<T, bool>> Build<T>(PropertyInfo propertyInfo, FilterOperations filterType, DateOnly searchValue) where T : class
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType) = Shared.GetPropertyExpressionParts<T>(propertyInfo);
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlyingType != typeof(DateTime) && underlyingType != typeof(DateOnly) && underlyingType != typeof(DateTimeOffset))
            throw new InvalidOperationException($"Property '{propertyInfo.Name}' is not a DateTime/DateOnly type.");

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
            ConstantExpression dateTimeBase;
            ConstantExpression dateTimeEnd;
            if (underlyingType == typeof(DateTimeOffset))
            {
                dateTimeBase = Expression.Constant(new DateTimeOffset(searchValue.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), propertyType);
                dateTimeEnd = Expression.Constant(new DateTimeOffset(searchValue.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero), propertyType);
            }
            else
            {
                // DateTime
                dateTimeBase = Expression.Constant(searchValue.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), propertyType);
                dateTimeEnd = Expression.Constant(searchValue.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc), propertyType);
            }

            comparison = filterType switch
            {
                FilterOperations.Equals =>
                    Expression.AndAlso(
                        Expression.GreaterThanOrEqual(memberAccess, dateTimeBase),
                        Expression.LessThan(memberAccess, dateTimeEnd)
                    ),
                FilterOperations.NotEqual =>
                    Expression.OrElse(
                        Expression.LessThan(memberAccess, dateTimeBase),
                        Expression.GreaterThanOrEqual(memberAccess, dateTimeEnd)
                    ),
                FilterOperations.GreaterThan => Expression.GreaterThan(memberAccess, dateTimeBase),
                FilterOperations.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, dateTimeBase),
                FilterOperations.LessThan => Expression.LessThan(memberAccess, dateTimeBase),
                FilterOperations.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, dateTimeBase),
                _ => throw new InvalidOperationException($"Filter operation '{filterType}' is not valid for date filtering.")
            };
        }
        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }



}
