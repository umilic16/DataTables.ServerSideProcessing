using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.EFCore.ReflectionCache;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class MultiSelectExpressionBuilder
{
    internal static Expression<Func<T, bool>> BuildMultiSelect<T>(PropertyInfo propertyInfo, string[] searchValues) where T : class
    {
        (ParameterExpression parameter, MemberExpression memberAccess, Type propertyType)
            = Shared.GetPropertyExpressionParts<T>(propertyInfo);

        Type targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        bool isNullable = Nullable.GetUnderlyingType(propertyType) is not null;

        // Convert searchValues from string[] to List<T>
        Type listType = typeof(List<>).MakeGenericType(targetType);
        System.Collections.IList typedList = (System.Collections.IList)Activator.CreateInstance(listType)!;
        foreach (string s in searchValues)
        {
            if (Shared.TryConvertStringToType(s, targetType, out object? typed))
            {
                typedList.Add(typed);
            }
        }

        if (typedList.Count == 0)
        {
            // conversion failed -> return true to skip this filter
            ConstantExpression trueConst = Expression.Constant(true);
            return Expression.Lambda<Func<T, bool>>(trueConst, parameter);
        }

        ConstantExpression listConstant = Expression.Constant(typedList);

        MethodInfo containsMethod = MethodInfoCache.GetEnumerableContains(targetType);

        if (isNullable)
        {
            // e.Property != null && typedValues.Contains(e.Property.Value)
            Expression notNull = Expression.NotEqual(memberAccess, Expression.Constant(null, propertyType));
            MemberExpression valueAccess = Expression.Property(memberAccess, "Value");
            Expression containsCall = Expression.Call(containsMethod, listConstant, valueAccess);
            Expression body = Expression.AndAlso(notNull, containsCall);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
        else
        {
            // typedValues.Contains(e.Property)
            Expression containsCall = Expression.Call(containsMethod, listConstant, memberAccess);
            return Expression.Lambda<Func<T, bool>>(containsCall, parameter);
        }
    }
}
