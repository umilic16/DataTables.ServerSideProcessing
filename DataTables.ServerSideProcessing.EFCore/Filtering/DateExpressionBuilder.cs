using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class DateExpressionBuilder
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

        if (underlyingType != typeof(DateTime) && underlyingType != typeof(DateOnly))
            throw new InvalidOperationException($"Property '{propertyName}' is not a DateTime/DateOnly type.");

        Expression comparison;
        if (filterType != FilterOperations.Between)
        {
            ConstantExpression constantValue = DateConstant(searchValue, underlyingType, propertyType);
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
            ConstantExpression lowerValue = DateConstant(parts[0], underlyingType, propertyType);
            comparison = Expression.GreaterThanOrEqual(memberAccess, lowerValue);
        }
        else if (string.IsNullOrEmpty(parts[0]))
        {
            ConstantExpression upperValue = DateConstant(parts[1], underlyingType, propertyType);
            comparison = Expression.LessThanOrEqual(memberAccess, upperValue);
        }
        else
        {
            ConstantExpression lowerValue = DateConstant(parts[0], underlyingType, propertyType);
            ConstantExpression upperValue = DateConstant(parts[1], underlyingType, propertyType);

            Expression lowerBound = Expression.GreaterThanOrEqual(memberAccess, lowerValue);
            Expression upperBound = Expression.LessThanOrEqual(memberAccess, upperValue);

            comparison = Expression.AndAlso(lowerBound, upperBound);
        }

        return comparison;
    }

    private static ConstantExpression DateConstant(string searchValue, Type underlyingType, Type propertyType)
    {
        if (underlyingType == typeof(DateTime))
        {
            if (!DateTime.TryParse(searchValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateParsed))
                throw new ArgumentException($"Invalid date format: {searchValue}. Expected format is based on current culture ({CultureInfo.CurrentCulture.Name}).");

            return Expression.Constant(dateParsed, propertyType);
        }
        else if (underlyingType == typeof(DateOnly))
        {
            if (!DateOnly.TryParse(searchValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateOnly dateParsed))
                throw new ArgumentException($"Invalid date format: {searchValue}. Expected format is based on current culture ({CultureInfo.CurrentCulture.Name}).");
            return Expression.Constant(dateParsed, propertyType);
        }
        else
        {
            // should never happen due to earlier checks
            throw new InvalidOperationException($"Property type '{underlyingType.Name}' is not supported for date filtering.");
        }
    }
}
