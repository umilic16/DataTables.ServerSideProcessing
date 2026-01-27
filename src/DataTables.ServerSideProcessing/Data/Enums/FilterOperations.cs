namespace DataTables.ServerSideProcessing.Data.Enums;
/// <summary>
/// Specifies the available filter operations.
/// </summary>
public enum FilterOperations
{
    /// <summary>
    /// Checks if the value is equal to the specified value.
    /// </summary>
    Equals,

    /// <summary>
    /// Checks if the value is not equal to the specified value.
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
    Between,

    /// <summary>
    /// The value contains the specified substring.
    /// </summary>
    Contains,

    /// <summary>
    /// The value does not contain the specified substring.
    /// </summary>
    DoesNotContain,

    /// <summary>
    /// The value starts with the specified substring.
    /// </summary>
    StartsWith,

    /// <summary>
    /// The value ends with the specified substring.
    /// </summary>
    EndsWith
}
