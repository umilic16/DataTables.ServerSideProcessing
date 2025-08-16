namespace DataTables.ServerSideProcessing.Data.Enums;
/// <summary>
/// Specifies the filter category of a column.
/// </summary>
public enum FilterCategory
{
    /// <summary>
    /// A text-based filter
    /// </summary>
    Text,
    /// <summary>
    /// A numeric filter
    /// </summary>
    Numeric,
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
