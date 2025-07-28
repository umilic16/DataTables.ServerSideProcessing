using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;
/// <summary>
/// Provides static helper methods for <see cref="ColumnFilterHandler"/> to build LINQ expressions for filtering entity properties.
/// Supports building expressions for date, numeric, and text-based filters.
/// </summary>
internal static class DateExpressionBuilder
{
    /// <summary>
    /// Builds a LINQ expression to filter entities by a date property.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the date property.</param>
    /// <param name="filterType">The filter type (e.g., Equals, GreaterThan, LessThanOrEqual).</param>
    /// <param name="searchValue">The date value to filter by.</param>
    /// <returns>An expression representing the filter.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found or the type does not match the filter type.</exception>
    internal static Expression<Func<T, bool>> Build<T>(string propertyName, NumberFilter filterType, string searchValue) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        // Get the actual type of the property (e.g., int, double?, decimal)
        Type propertyType = propertyInfo.PropertyType;
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlyingType != typeof(DateTime) && underlyingType != typeof(DateOnly))
            throw new InvalidOperationException($"Property '{propertyName}' is not a DateTime/DateOnly type.");

        Expression comparison;
        if (filterType != NumberFilter.Between)
        {
            ConstantExpression constantValue = DateConstant(searchValue, underlyingType);
            comparison = filterType switch
            {
                NumberFilter.Equals => Expression.Equal(memberAccess, constantValue),
                NumberFilter.NotEqual => Expression.NotEqual(memberAccess, constantValue),
                NumberFilter.GreaterThan => Expression.GreaterThan(memberAccess, constantValue),
                NumberFilter.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, constantValue),
                NumberFilter.LessThan => Expression.LessThan(memberAccess, constantValue),
                NumberFilter.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, constantValue),
                _ => throw new NotImplementedException()
            };
        }
        else
        {
            comparison = Between(memberAccess, underlyingType, searchValue);
        }

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static Expression Between(MemberExpression memberAccess, Type underlyingType, string searchValue)
    {
        Expression comparison;
        string[] parts = searchValue.Split(';');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid format for 'Between'. Expected at least 1 and at most 2 numbers separated with ';'.");

        var memberAccessNullable = Expression.Convert(memberAccess, underlyingType);
        if (string.IsNullOrEmpty(parts[1]))
        {
            ConstantExpression lowerValue = DateConstant(parts[0], underlyingType);
            comparison = Expression.GreaterThanOrEqual(memberAccessNullable, lowerValue);
        }
        else if (string.IsNullOrEmpty(parts[0]))
        {
            ConstantExpression upperValue = DateConstant(parts[1], underlyingType);
            comparison = Expression.LessThanOrEqual(memberAccessNullable, upperValue);
        }
        else
        {
            ConstantExpression lowerValue = DateConstant(parts[0], underlyingType);
            ConstantExpression upperValue = DateConstant(parts[1], underlyingType);

            Expression lowerBound = Expression.GreaterThanOrEqual(memberAccessNullable, lowerValue);
            Expression upperBound = Expression.LessThanOrEqual(memberAccessNullable, upperValue);

            comparison = Expression.AndAlso(lowerBound, upperBound);
        }

        return comparison;
    }

    private static ConstantExpression DateConstant(string searchValue, Type underlyingType)
    {
        if (underlyingType == typeof(DateTime))
        {
            if (!DateTime.TryParse(searchValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateParsed))
                throw new ArgumentException($"Invalid date format: {searchValue}. Expected format is based on current culture ({CultureInfo.CurrentCulture.Name}).");

            return Expression.Constant(dateParsed);
        }
        else if (underlyingType == typeof(DateOnly))
        {
            if (!DateOnly.TryParse(searchValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateOnly dateParsed))
                throw new ArgumentException($"Invalid date format: {searchValue}. Expected format is based on current culture ({CultureInfo.CurrentCulture.Name}).");
            return Expression.Constant(dateParsed);
        }
        else
        {
            // should never happen due to earlier checks
            throw new InvalidOperationException($"Property type '{underlyingType.Name}' is not supported for date filtering.");
        }
    }
}
