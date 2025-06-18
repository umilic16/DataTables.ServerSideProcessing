using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models;
/// <summary>
/// Base class for all DataTable filter models.
/// Contains the property name to filter on and the search value.
/// </summary>
public abstract class DataTableFilterBaseModel
{
    /// <summary>
    /// Name of the property to filter.
    /// </summary>
    public required string PropertyName { get; init; }

    /// <summary>
    /// Value to search for in the filter.
    /// </summary>
    public required string? SearchValue { get; set; }
}

/// <summary>
/// Filter model for text-based columns.
/// </summary>
public class DataTableTextFilterModel : DataTableFilterBaseModel
{
    /// <summary>
    /// Type of the column (e.g., Base, AccNumber).
    /// </summary>
    public ColumnValueType ColumnType { get; init; } = ColumnValueType.Base;

    /// <summary>
    /// Type of text filter to apply (e.g., Contains, StartsWith).
    /// </summary>
    public required TextFilter FilterType { get; init; }
}

/// <summary>
/// Filter model for numeric columns.
/// </summary>
public class DataTableNumberFilterModel : DataTableFilterBaseModel
{
    /// <summary>
    /// Type of the column (e.g., Int, Decimal).
    /// </summary>
    public ColumnValueType ColumnType { get; init; } = ColumnValueType.Int;

    /// <summary>
    /// Type of number filter to apply (e.g., Equals, Between).
    /// </summary>
    public required NumberFilter FilterType { get; init; }
}

/// <summary>
/// Filter model for DateTime columns.
/// </summary>
public class DataTableDateTimeFilterModel : DataTableFilterBaseModel
{
}
