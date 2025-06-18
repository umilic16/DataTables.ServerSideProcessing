namespace DataTables.ServerSideProcessing.Data.Enums;
/// <summary>
/// Specifies the type of filter that can be applied to a column.
/// </summary>
public enum ColumnFilterType
{
    Text,
    Number,
    Date,
    /// <summary>
    /// A list-based filter (e.g., dropdown selection). Not yet implemented.
    /// </summary>
    List
}
