using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class TextExpressionBuilder
{
    internal static Expression<Func<T, bool>> Build<T>(string propertyName, FilterOperations filterType, string searchValue) where T : class
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType) = Shared.GetPropertyExpressionParts<T>(propertyName);
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlyingType != typeof(string)) throw new InvalidOperationException($"Property '{propertyName}' is not a string type.");

        object convertedValue = Convert.ChangeType(searchValue, underlyingType);
        // Create a constant expression using the converted value BUT typed as the *original* property type (including Nullable<>)
        ConstantExpression constantValue = Expression.Constant(convertedValue, propertyType);
        Expression comparison = filterType switch
        {
            FilterOperations.Equals => Expression.Equal(memberAccess, constantValue),
            FilterOperations.NotEqual => Expression.NotEqual(memberAccess, constantValue),
            FilterOperations.Contains => Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue),
            FilterOperations.DoesNotContain => Expression.Not(Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue)),
            FilterOperations.StartsWith => Expression.Call(typeof(DbFunctionsExtensions),
                                                           nameof(DbFunctionsExtensions.Like), Type.EmptyTypes,
                                                           Expression.Constant(EF.Functions), memberAccess,
                                                           Expression.Constant($"{searchValue}%")),
            FilterOperations.EndsWith => Expression.Call(typeof(DbFunctionsExtensions),
                                                         nameof(DbFunctionsExtensions.Like), Type.EmptyTypes,
                                                         Expression.Constant(EF.Functions), memberAccess,
                                                         Expression.Constant($"%{searchValue}")),
            _ => throw new NotImplementedException($"Filter operation '{filterType}' is not implemented for text filtering."),
        };

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }
}
