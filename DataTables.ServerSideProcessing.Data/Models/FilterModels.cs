using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models;
/// <summary>
/// Base class for all DataTable filter models.
/// Contains the property name to filter on and the search value.
/// </summary>
public abstract class DataTableFilterBaseModel
{
    private protected DataTableFilterBaseModel() { }

    /// <summary>
    /// Name of the property to filter.
    /// </summary>
    public required string PropertyName { get; init; }
}

/// <summary>
/// Generic base class for all DataTable filter models.
/// Contains the property name to filter on and the search value.
/// Inherits from <see cref="DataTableFilterBaseModel"/>.
/// </summary>
/// <typeparam name="T">Type of the value used in the filter (e.g., string, int, DateTime).</typeparam>
public abstract class DataTableFilterBaseModel<T> : DataTableFilterBaseModel
{
    private protected DataTableFilterBaseModel() { }
    /// <summary>
    /// Value to search for in the filter.
    /// </summary>
    public required T SearchValue { get; set; }
}

/// <summary>
/// Filter model for text-based columns.
/// Inherits from <see cref="DataTableFilterBaseModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
public sealed class DataTableTextFilterModel : DataTableFilterBaseModel<string?>
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
/// Inherits from <see cref="DataTableFilterBaseModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
public sealed class DataTableNumberFilterModel : DataTableFilterBaseModel<string?>
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
/// Inherits from <see cref="DataTableFilterBaseModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
public sealed class DataTableDateTimeFilterModel : DataTableFilterBaseModel<string?>
{
}

/// <summary>
/// Filter model for SingleSelect columns.
/// Inherits from <see cref="DataTableFilterBaseModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
public sealed class DataTableSingleSelectFilterModel : DataTableFilterBaseModel<string?>
{
}

/// <summary>
/// Filter model for MultiSelect columns.
/// Inherits from <see cref="DataTableFilterBaseModel{T}"/> with <c>List&lt;string&gt;</c> as the type parameter.
/// </summary>
public sealed class DataTableMultiSelectFilterModel : DataTableFilterBaseModel<List<string>>
{
}
