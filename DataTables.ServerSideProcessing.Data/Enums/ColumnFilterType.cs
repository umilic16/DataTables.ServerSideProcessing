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
    /// A single-selection list filter (e.g., dropdown)
    /// </summary>
    SingleSelect,
    /// <summary>
    /// A multi-selection list filter (e.g., multi-select dropdown or checklist)
    /// </summary>
    MultiSelect
}
