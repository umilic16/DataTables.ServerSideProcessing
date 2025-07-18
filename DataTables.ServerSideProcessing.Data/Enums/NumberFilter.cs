namespace DataTables.ServerSideProcessing.Data.Enums;
/// <summary>
/// Specifies the available numeric filter operations.
/// </summary>
public enum NumberFilter
{
    /// <summary>
    /// Checks if the value is equal to the specified number.
    /// </summary>
    Equals,

    /// <summary>
    /// Checks if the value is not equal to the specified number.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Checks if the value is less than the specified number.
    /// </summary>
    LessThan,

    /// <summary>
    /// Checks if the value is greater than the specified number.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Checks if the value is less than or equal to the specified number.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Checks if the value is greater than or equal to the specified number.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Checks if the value is between two specified numbers (inclusive).
    /// </summary>
    Between
}
