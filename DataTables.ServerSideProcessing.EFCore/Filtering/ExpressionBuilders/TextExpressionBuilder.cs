using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.EFCore.ReflectionCache;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class TextExpressionBuilder
{
    internal static Expression<Func<T, bool>> Build<T>(PropertyInfo propertyInfo, FilterOperations filterType, string searchValue) where T : class
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType) = Shared.GetPropertyExpressionParts<T>(propertyInfo);
        // Get the underlying type if it's nullable (e.g., string from string?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        if (underlyingType != typeof(string)) throw new InvalidOperationException($"Property '{propertyInfo.Name}' is not a string type.");

        ConstantExpression constantValue = Expression.Constant(searchValue);
        Expression comparison = filterType switch
        {
            FilterOperations.Equals => Expression.Equal(memberAccess, constantValue),
            FilterOperations.NotEqual => Expression.NotEqual(memberAccess, constantValue),
            FilterOperations.Contains => Expression.Call(memberAccess, MethodInfoCache.s_stringContains, constantValue),
            FilterOperations.DoesNotContain => Expression.Not(Expression.Call(memberAccess, MethodInfoCache.s_stringContains, constantValue)),
            FilterOperations.StartsWith => Expression.Call(typeof(DbFunctionsExtensions),
                                                           nameof(DbFunctionsExtensions.Like), Type.EmptyTypes,
                                                           Expression.Constant(EF.Functions), memberAccess,
                                                           Expression.Constant($"{searchValue}%")),
            FilterOperations.EndsWith => Expression.Call(typeof(DbFunctionsExtensions),
                                                         nameof(DbFunctionsExtensions.Like), Type.EmptyTypes,
                                                         Expression.Constant(EF.Functions), memberAccess,
                                                         Expression.Constant($"%{searchValue}")),
            _ => throw new InvalidOperationException($"Filter operation '{filterType}' is not valid for text filtering."),
        };

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }
}
