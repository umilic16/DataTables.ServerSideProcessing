namespace DataTables.ServerSideProcessing.Data.Enums;
/// <summary>
/// Specifies the available text filter operations.
/// </summary>
public enum TextFilter
{
    /// <summary>
    /// The value contains the specified substring.
    /// </summary>
    Contains,

    /// <summary>
    /// The value does not contain the specified substring.
    /// </summary>
    DoesntContain,

    /// <summary>
    /// The value starts with the specified substring.
    /// </summary>
    StartsWith,

    /// <summary>
    /// The value ends with the specified substring.
    /// </summary>
    EndsWith,

    /// <summary>
    /// The value is equal to the specified string.
    /// </summary>
    Equals,

    /// <summary>
    /// The value is not equal to the specified string.
    /// </summary>
    NotEqual
}
