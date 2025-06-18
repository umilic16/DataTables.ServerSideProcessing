namespace DataTables.ServerSideProcessing.Data.Enums;
/// <summary>
/// Specifies the type of filter that can be applied to a column.
/// </summary>
public enum ColumnFilterType
{
    /// <summary>
    /// A text-based filter
    /// </summary>
    Text,
    /// <summary>
    /// A numeric filter
    /// </summary>
    Number,
    /// <summary>
    /// A date filter
    /// </summary>
    Date,
    /// <summary>
    /// A list-based filter (e.g., dropdown selection). Not yet implemented.
    /// </summary>
    List
}
