using System.Linq.Expressions;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;
internal static class Helpers
{
    /// <summary>
    /// Creates a <see cref="ConstantExpression"/> for use in expression trees,
    /// converting a string input into the specified type.
    /// </summary>
    /// <param name="searchValue">The string representation of the value to convert.</param>
    /// <param name="propertyType">
    /// The type to assign to the resulting <see cref="ConstantExpression"/>. This can be a nullable type.
    /// </param>
    /// <param name="underlyingType">
    /// The actual type to convert the <paramref name="searchValue"/> to before wrapping in the expression.
    /// This should be the non-nullable equivalent of <paramref name="propertyType"/> if it's nullable.
    /// </param>
    /// <returns>
    /// A <see cref="ConstantExpression"/> containing the converted value, strongly typed as <paramref name="propertyType"/>.
    /// </returns>
    internal static ConstantExpression CreateConstant(this string searchValue, Type propertyType, Type underlyingType)
    {
        // Convert the input 'searchValue' to the property's underlying type
        object convertedValue = Convert.ChangeType(searchValue, underlyingType);
        // Create a constant expression using the converted value BUT typed as the *original* property type (including Nullable<>)
        ConstantExpression constantValue = Expression.Constant(convertedValue, propertyType);
        return constantValue;
    }
}
