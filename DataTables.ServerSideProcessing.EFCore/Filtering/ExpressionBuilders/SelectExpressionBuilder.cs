using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class SelectExpressionBuilder
{
    internal static Expression<Func<T, bool>> BuildSingleSelect<T>(PropertyInfo propertyInfo, string searchValue) where T : class
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType) = Shared.GetPropertyExpressionParts<T>(propertyInfo);

        Type targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        bool isNullable = Nullable.GetUnderlyingType(propertyType) is not null;

        if (!Shared.TryConvertStringToType(searchValue, targetType, out object? typed))
        {
            // conversion failed -> return true to skip this filter
            ConstantExpression trueConst = Expression.Constant(true);
            return Expression.Lambda<Func<T, bool>>(trueConst, parameter);
        }

        Expression predicate;
        ConstantExpression constant = Expression.Constant(typed, targetType);

        if (isNullable)
        {
            // e.Property != null && e.Property.Value == typed
            BinaryExpression notNull = Expression.NotEqual(memberAccess, Expression.Constant(null, propertyType));
            MemberExpression valueAccess = Expression.Property(memberAccess, "Value"); // underlying value
            BinaryExpression equals = Expression.Equal(valueAccess, constant);
            predicate = Expression.AndAlso(notNull, equals);
        }
        else
        {
            // e.Property == typed
            predicate = Expression.Equal(memberAccess, constant);
        }

        return Expression.Lambda<Func<T, bool>>(predicate, parameter);
    }
}
