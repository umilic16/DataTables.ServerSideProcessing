using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class TextExpressionBuilder
{

    /// <summary>
    /// Builds a LINQ expression to filter entities by a text property using the specified filter type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The name of the text property.</param>
    /// <param name="filterType">The text filter type (e.g., Equals, Contains, StartsWith).</param>
    /// <param name="searchValue">The value to filter by.</param>
    /// <returns>An expression representing the filter.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not found or the type does not match the filter type.</exception>
    internal static Expression<Func<T, bool>> Build<T>(string propertyName, TextFilter filterType, string searchValue) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        // Get the actual type of the property (e.g., int, double?, decimal)
        Type propertyType = propertyInfo.PropertyType;
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlyingType != typeof(string))
            throw new InvalidOperationException($"Property '{propertyName}' is not a string type.");

        ConstantExpression constantValue = searchValue.CreateConstant(propertyType, underlyingType);
        Expression comparison = filterType switch
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
}
