using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class SelectExpressionBuilder
{

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
    internal static Expression<Func<T, bool>> BuildMultiSelect<T>(string propertyName, List<string> searchValues) where T : class
    {
        if (searchValues == null || searchValues.Count == 0) throw new ArgumentException("Search values list cannot be null or empty.", nameof(searchValues));

        ParameterExpression parameter = Expression.Parameter(typeof(T), "e");
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        Type propertyType = propertyInfo.PropertyType;

        Expression memberAsString = propertyType == typeof(string) ?
            memberAccess : // e.Property for string properties
            Expression.Call(memberAccess, propertyType.GetMethod("ToString", Type.EmptyTypes)!); // e.Property.ToString() for non-string properties

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

    /// <summary>
    /// Builds a LINQ expression to filter entities by a property using an <c>Equals</c> comparison.
    /// This is typically used for single-select dropdown filters, where the property value must exactly match the provided search value.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="searchValue">The value to filter by.</param>
    /// <returns>An expression representing the filter.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found.</exception>
    internal static Expression<Func<T, bool>> BuildSingleSelect<T>(string propertyName, string searchValue) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        Type propertyType = propertyInfo.PropertyType;

        Expression memberAsString = propertyType == typeof(string) ?
            memberAccess : // e.Property for string properties
            Expression.Call(memberAccess, propertyType.GetMethod("ToString", Type.EmptyTypes)!); // e.Property.ToString() for non-string properties

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
