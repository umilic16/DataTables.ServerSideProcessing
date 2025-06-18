using Microsoft.Extensions.Primitives;

namespace DataTables.ServerSideProcessing.Utils;
/// <summary>
/// Provides helper extension methods for type conversions.
/// </summary>
internal static class Helpers
{
    /// <summary>
    /// Converts the specified string to an integer.
    /// Returns -1 if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The integer value, or -1 if conversion fails.</returns>
    internal static int ToInt(this string value)
    {
        return int.TryParse(value, out int result) ? result : -1;
    }

    /// <summary>
    /// Converts the specified <see cref="StringValues"/> to an integer.
    /// Returns -1 if the conversion fails.
    /// </summary>
    /// <param name="value">The <see cref="StringValues"/> to convert.</param>
    /// <returns>The integer value, or -1 if conversion fails.</returns>
    internal static int ToInt(this StringValues value)
    {
        return int.TryParse(value, out int result) ? result : -1;
    }
}
