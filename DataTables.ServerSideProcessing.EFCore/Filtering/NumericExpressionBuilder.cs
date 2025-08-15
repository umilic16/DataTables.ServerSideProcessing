using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class NumericExpressionBuilder
{
    internal static Expression<Func<T, bool>> Build<T>(string propertyName, FilterOperations filterType, string searchValue) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        // Get the actual type of the property (e.g., int, double?, decimal)
        Type propertyType = propertyInfo.PropertyType;
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (!IsNumericType(underlyingType)) throw new InvalidOperationException($"Property '{propertyName}' is not a numeric type.");

        Expression comparison;
        if (filterType != FilterOperations.Between)
        {
            ConstantExpression constantValue = searchValue.CreateConstant(propertyType, underlyingType);
            comparison = filterType switch
            {
                FilterOperations.Equals => Expression.Equal(memberAccess, constantValue),
                FilterOperations.NotEqual => Expression.NotEqual(memberAccess, constantValue),
                FilterOperations.GreaterThan => Expression.GreaterThan(memberAccess, constantValue),
                FilterOperations.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, constantValue),
                FilterOperations.LessThan => Expression.LessThan(memberAccess, constantValue),
                FilterOperations.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, constantValue),
                _ => throw new NotImplementedException()
            };
        }
        else
        {
            comparison = Between(memberAccess, underlyingType, propertyType, searchValue);
        }
        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static Expression Between(MemberExpression memberAccess, Type underlyingType, Type propertyType, string searchValue)
    {
        Expression comparison;

        string[] parts = searchValue.Split(';');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid format for 'Between'. Expected at least 1 and at most 2 numbers separated with ';'.");

        if (string.IsNullOrEmpty(parts[1]))
        {
            ConstantExpression lowerValue = parts[0].CreateConstant(propertyType, underlyingType);
            comparison = Expression.GreaterThanOrEqual(memberAccess, lowerValue);
        }
        else if (string.IsNullOrEmpty(parts[0]))
        {
            ConstantExpression upperValue = parts[1].CreateConstant(propertyType, underlyingType);
            comparison = Expression.LessThanOrEqual(memberAccess, upperValue);
        }
        else
        {
            ConstantExpression lowerValue = parts[0].CreateConstant(propertyType, underlyingType);
            ConstantExpression upperValue = parts[1].CreateConstant(propertyType, underlyingType);

            Expression lowerBound = Expression.GreaterThanOrEqual(memberAccess, lowerValue);
            Expression upperBound = Expression.LessThanOrEqual(memberAccess, upperValue);

            comparison = Expression.AndAlso(lowerBound, upperBound);
        }

        return comparison;
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) ||
               type == typeof(long) || type == typeof(short);
    }
}
