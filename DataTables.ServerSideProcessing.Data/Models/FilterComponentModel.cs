using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models;

/// <summary>
/// Base model for filter components, providing common properties for all filter types.
/// Supplies metadata for rendering filter UI elements on the frontend,
/// such as input fields and filter type selectors, based on the column's characteristics.
/// </summary>
public abstract class FilterComponentBaseModel
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
    /// Type of filter to apply to the column (e.g., text, number, date, single-select, multi-select).
    /// </summary>
    public required ColumnFilterType ColumnFilterType { get; init; }
}

/// <summary>
/// Represents the configuration for filtering a specific column in a DataTable.
/// Inherits from <see cref="FilterComponentBaseModel"/> and adds column value type information.
/// Used to provide metadata for rendering and processing column filters, including
/// the type of value the column holds (e.g., base, account number, int, decimal).
/// </summary>
public class FilterComponentModel : FilterComponentBaseModel
{
    /// <summary>
    /// Type of the column value (e.g., base, account number, int, decimal).
    /// Determines how the filter input is rendered and validated.
    /// Defaults to <see cref="ColumnValueType.Base"/>.
    /// </summary>
    public ColumnValueType ColumnValueType { get; init; } = ColumnValueType.Base;
}

/// <summary>
/// Represents a filter component for columns with selectable values (e.g., dropdowns, multi-selects).
/// Provides a list of available values for selection.
/// Inherits from <see cref="FilterComponentBaseModel"/>.
/// </summary>
public class FilterComponentSelectModel : FilterComponentBaseModel
{
    /// <summary>
    /// List of available values that can be selected for filtering.
    /// </summary>
    public required List<string> AvailableValues { get; init; }
}
