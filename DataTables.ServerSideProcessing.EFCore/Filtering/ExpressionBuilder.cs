using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;
/// <summary>
/// Provides static helper methods for <see cref="ColumnFilterHandler"/> to build LINQ expressions for filtering entity properties.
/// Supports building expressions for date, numeric, and text-based filters.
/// </summary>
internal static class ExpressionBuilder
{
    /// <summary>
    /// Builds a LINQ expression to filter entities by a date property.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the date property.</param>
    /// <param name="searchValue">The date value to filter by.</param>
    /// <returns>An expression representing the filter.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found or the type does not match the filter type.</exception>
    internal static Expression<Func<T, bool>> BuildDateWhereExpression<T>(string propertyName, DateTime searchValue) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        (MemberExpression memberAccess, Expression constantValue) = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Date);

        Expression comparison = Expression.Equal(memberAccess, constantValue);

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    /// <summary>
    /// Builds a LINQ expression to filter entities by a numeric property using the specified filter type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the numeric property.</param>
    /// <param name="numberFilterType">The numeric filter type (e.g., Equals, GreaterThan, LessThanOrEqual).</param>
    /// <param name="searchValue">The value to filter by, as a string.</param>
    /// <returns>An expression representing the filter.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found or the type does not match the filter type.</exception>
    internal static Expression<Func<T, bool>> BuildNumericWhereExpression<T>(string propertyName, NumberFilter numberFilterType, string searchValue) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        Expression comparison;
        if (numberFilterType != NumberFilter.Between)
        {
            (MemberExpression memberAccess, Expression constantValue) = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Number);

            comparison = numberFilterType switch
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
            comparison = BuildNumericBetweenExpression<T>(propertyName, searchValue, parameter);
        }
        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static Expression BuildNumericBetweenExpression<T>(string propertyName, string searchValue, ParameterExpression parameter) where T : class
    {
        Expression comparison;
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        // Get the actual type of the property (e.g., int, double?, decimal)
        Type propertyType = propertyInfo.PropertyType;
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (!IsNumericType(underlyingType))
            throw new InvalidOperationException($"Property '{propertyName}' is not a numeric type.");

        string[] parts = searchValue.Split(';');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid format for 'Between'. Expected at least 1 and at most 2 numbers separated with ';'.");

        if (string.IsNullOrEmpty(parts[1]))
        {
            object lowerValue = Convert.ChangeType(parts[0], propertyType);
            comparison = Expression.GreaterThanOrEqual(memberAccess, Expression.Constant(lowerValue));
        }
        else if (string.IsNullOrEmpty(parts[0]))
        {
            object upperValue = Convert.ChangeType(parts[1], propertyType);
            comparison = Expression.LessThanOrEqual(memberAccess, Expression.Constant(upperValue));
        }
        else
        {
            object lowerValue = Convert.ChangeType(parts[0], propertyType);
            object upperValue = Convert.ChangeType(parts[1], propertyType);

            Expression lowerBound = Expression.GreaterThanOrEqual(memberAccess, Expression.Constant(lowerValue));
            Expression upperBound = Expression.LessThanOrEqual(memberAccess, Expression.Constant(upperValue));

            comparison = Expression.AndAlso(lowerBound, upperBound);
        }

        return comparison;
    }

    /// <summary>
    /// Builds a LINQ expression to filter entities by a text property using the specified filter type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the text property.</param>
    /// <param name="textFilterType">The text filter type (e.g., Equals, Contains, StartsWith).</param>
    /// <param name="searchValue">The value to filter by.</param>
    /// <returns>An expression representing the filter.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found or the type does not match the filter type.</exception>
    internal static Expression<Func<T, bool>> BuildTextWhereExpression<T>(string propertyName, TextFilter? textFilterType, string searchValue) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        (MemberExpression memberAccess, Expression constantValue) = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Text);

        Expression comparison = textFilterType switch
        {
            TextFilter.Equals => Expression.Equal(memberAccess, constantValue),
            TextFilter.NotEqual => Expression.NotEqual(memberAccess, constantValue),
            TextFilter.Contains => Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue),
            TextFilter.DoesntContain => Expression.Not(Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue)),
            TextFilter.StartsWith => Expression.Call(typeof(DbFunctionsExtensions), nameof(DbFunctionsExtensions.Like),
                                                     Type.EmptyTypes, Expression.Constant(EF.Functions), memberAccess,
                                                     Expression.Constant($"{searchValue}%")),
            TextFilter.EndsWith => Expression.Call(typeof(DbFunctionsExtensions), nameof(DbFunctionsExtensions.Like),
                                                     Type.EmptyTypes, Expression.Constant(EF.Functions), memberAccess,
                                                     Expression.Constant($"%{searchValue}")),
            _ => throw new NotImplementedException(),
        };

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    /// <summary>
    /// Builds a LINQ expression to filter entities by a property using an <c>Equals</c> comparison.
    /// This is typically used for single-select dropdown filters, where the property value must exactly match the provided search value.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="searchValue">The value to filter by.</param>
    /// <returns>An expression representing the filter.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found or the type does not match the filter type.</exception>
    internal static Expression<Func<T, bool>> BuildSingleSelectWhereExpression<T>(string propertyName, string searchValue) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        (MemberExpression memberAccess, Expression constantValue) = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.SingleSelect);

        Expression comparison = Expression.Equal(memberAccess, constantValue);

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    /// <summary>
    /// Builds a LINQ expression to filter entities by a property using a <c>Contains()</c> comparison.
    /// This is typically used for multi-select dropdown filters.
    /// The filter checks if the entity property's string representation is present in the provided search values list.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="searchValues">The values to filter by.</param>
    /// <returns>An expression representing the filter.</returns>
    /// <exception cref="ArgumentException">Thrown if the search values list is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found on the entity type.</exception>
    internal static Expression<Func<T, bool>> BuildMultiSelectWhereExpression<T>(string propertyName, List<string> searchValues) where T : class
    {
        if (searchValues == null || searchValues.Count == 0)
            throw new ArgumentException("Search values list cannot be null or empty.", nameof(searchValues));

        ParameterExpression parameter = Expression.Parameter(typeof(T), "e");
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        // Call e.Property.ToString()
        MethodInfo toStringMethod = typeof(object).GetMethod(nameof(ToString))!;
        Expression toStringCall = Expression.Call(memberAccess, toStringMethod);

        // Create the constant list (List<string>)
        ConstantExpression constantList = Expression.Constant(searchValues);

        // searchValues.Contains(e.Property.ToString())
        Expression containsCall = Expression.Call(constantList, typeof(List<string>).GetMethod(nameof(List<string>.Contains), [typeof(string)])!, toStringCall);

        return Expression.Lambda<Func<T, bool>>(containsCall, parameter);
    }

    /// <summary>
    /// Prepares the member access and constant value expressions for a property filter.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="parameter">The parameter expression representing the entity.</param>
    /// <param name="propertyName">The property name to access.</param>
    /// <param name="searchValue">The value to compare against.</param>
    /// <param name="columnType">The type of column filter (eg., Text, Number, Date).</param>
    /// <returns>A tuple containing the member access and constant value expressions.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found or the type does not match the filter type.</exception>
    private static (MemberExpression memberAccess, Expression constantValue) PrepareExpressionData<T>(ParameterExpression parameter, string propertyName, object searchValue, ColumnFilterType columnType) where T : class
    {
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        // Get the actual type of the property (e.g., int, double?, decimal)
        Type propertyType = propertyInfo.PropertyType;
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (columnType == ColumnFilterType.Number && !IsNumericType(underlyingType))
            throw new InvalidOperationException($"Property '{propertyName}' is not a numeric type.");

        if (columnType == ColumnFilterType.Date && underlyingType != typeof(DateTime))
            throw new InvalidOperationException($"Property '{propertyName}' is not a DateTime type.");

        if (columnType == ColumnFilterType.Text && underlyingType != typeof(string))
            throw new InvalidOperationException($"Property '{propertyName}' is not a string type.");

        // Convert the input 'searchValue' to the property's underlying type
        object convertedValue = Convert.ChangeType(searchValue, underlyingType);

        // Create a constant expression using the converted value BUT typed as the *original* property type (including Nullable<>)
        Expression constantValue = Expression.Constant(convertedValue, propertyType);
        return (memberAccess, constantValue);
    }

    /// <summary>
    /// Determines if a type is a supported numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is numeric; otherwise, false.</returns>
    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) ||
               type == typeof(long) || type == typeof(short);
    }
}
