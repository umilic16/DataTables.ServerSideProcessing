using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class TextExpressionBuilder
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

        if (underlyingType != typeof(string)) throw new InvalidOperationException($"Property '{propertyName}' is not a string type.");

        ConstantExpression constantValue = searchValue.CreateConstant(propertyType, underlyingType);
        Expression comparison = filterType switch
        {
            FilterOperations.Equals => Expression.Equal(memberAccess, constantValue),
            FilterOperations.NotEqual => Expression.NotEqual(memberAccess, constantValue),
            FilterOperations.Contains => Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue),
            FilterOperations.DoesNotContain => Expression.Not(Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue)),
            FilterOperations.StartsWith => Expression.Call(typeof(DbFunctionsExtensions), nameof(DbFunctionsExtensions.Like),
                                                     Type.EmptyTypes, Expression.Constant(EF.Functions), memberAccess,
                                                     Expression.Constant($"{searchValue}%")),
            FilterOperations.EndsWith => Expression.Call(typeof(DbFunctionsExtensions), nameof(DbFunctionsExtensions.Like),
                                                     Type.EmptyTypes, Expression.Constant(EF.Functions), memberAccess,
                                                     Expression.Constant($"%{searchValue}")),
            _ => throw new NotImplementedException(),
        };

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }
}
