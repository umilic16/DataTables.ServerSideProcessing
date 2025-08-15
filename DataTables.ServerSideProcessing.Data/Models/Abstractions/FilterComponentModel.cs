using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models.Abstractions;

/// <summary>
/// Serves as the abstract base class for all filter component models, encapsulating shared properties
/// required for defining filter behavior and metadata. This includes the table and column names to which
/// the filter applies, as well as the type of filter to be rendered and processed. The metadata provided
/// by this class supports dynamic rendering of filter UI elements on the frontend, such as input fields
/// and filter type selectors, tailored to the characteristics of the data column.
/// </summary>
public abstract class FilterComponentModel
{
    private protected FilterComponentModel() { }

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
/// Generic abstract base class for filter component models, extending <see cref="FilterComponentModel"/>
/// by introducing a strongly-typed value type for the column being filtered. The generic type parameter <typeparamref name="T"/>
/// must be an <see cref="Enum"/>, representing the specific data type or semantic meaning of the column value
/// (e.g., base, account number, integer, decimal). This enables more precise control over filter input rendering,
/// validation, and processing logic based on the column's value type.
/// </summary>
/// <typeparam name="T">
/// An enumeration that specifies the type or semantic category of the column value, used to determine
/// appropriate filter UI and validation.
/// </typeparam>
public abstract class FilterComponentModel<T> : FilterComponentModel where T : Enum
{
    /// <summary>
    /// Type of the column value (e.g., base, account number, int, decimal).
    /// Determines how the filter input is rendered and validated.
    /// </summary>
    public required T ColumnValueType { get; init; }
}
