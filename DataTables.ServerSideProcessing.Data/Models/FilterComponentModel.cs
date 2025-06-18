using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models;
/// <summary>
/// Represents the configuration for filtering a specific column in a DataTable.
/// This model provides the necessary metadata for rendering filter UI elements on the frontend,
/// such as input fields and filter type selectors, based on the column's characteristics.
/// </summary>
public class FilterComponentModel
{
    /// <summary>
    /// Name of the table to which the filter applies.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Name of the column to be filtered.
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// Type of filter to apply to the column (e.g., text, number, date, list).
    /// </summary>
    public required ColumnFilterType ColumnFilterType { get; init; }

    /// <summary>
    /// Type of the column (e.g., base, account number, int, decimal).
    /// Defaults to <see cref="ColumnValueType.Base"/>.
    /// </summary>
    public ColumnValueType ColumnValueType { get; init; } = ColumnValueType.Base;
}
