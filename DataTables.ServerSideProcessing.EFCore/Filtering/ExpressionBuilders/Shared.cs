using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

internal static class Shared
{
    internal static (ParameterExpression Parameter, MemberExpression MemberAccess, Type PropertyType) GetPropertyExpressionParts<T>(PropertyInfo propertyInfo) where T : class
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);
        Type propertyType = propertyInfo.PropertyType;

        return (parameter, memberAccess, propertyType);
    }

    internal static bool TryConvertStringToType(string searchValue, Type target, [NotNullWhen(true)] out object? result)
    {
        result = null;
        if (searchValue is null) return false;

        try
        {
            if (target.IsEnum)
            {
                result = Enum.Parse(target, searchValue);
                return true;
            }

            if (target == typeof(DateOnly))
            {
                result = DateOnly.Parse(searchValue, CultureInfo.CurrentCulture);
                return true;
            }

            if (target == typeof(DateTime))
            {
                // check if this will mess up other db providers, its needed for postgres
                result = DateTime.Parse(searchValue, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                return true;
            }

            if (target == typeof(DateTimeOffset))
            {
                // check if this will mess up other db providers, its needed for postgres
                result = DateTimeOffset.Parse(searchValue, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                return true;
            }

            TypeConverter conv = TypeDescriptor.GetConverter(target);
            if (conv != null && conv.IsValid(searchValue))
            {
                result = conv.ConvertFromString(null, CultureInfo.CurrentCulture, searchValue);
                return result is not null;
            }

            result = Convert.ChangeType(searchValue, target, CultureInfo.CurrentCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
