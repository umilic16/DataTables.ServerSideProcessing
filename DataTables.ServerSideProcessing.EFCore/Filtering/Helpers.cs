using System.Linq.Expressions;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class Helpers
{
    internal static ConstantExpression CreateConstant(this string searchValue, Type propertyType, Type underlyingType)
    {
        // Convert the input 'searchValue' to the property's underlying type
        object convertedValue = Convert.ChangeType(searchValue, underlyingType);
        // Create a constant expression using the converted value BUT typed as the *original* property type (including Nullable<>)
        ConstantExpression constantValue = Expression.Constant(convertedValue, propertyType);
        return constantValue;
    }
}
