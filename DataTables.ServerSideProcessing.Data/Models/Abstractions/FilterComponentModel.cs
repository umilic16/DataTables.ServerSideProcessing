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
    /// Defines the category of the filter (e.g., text, numeric, date, etc.),
    /// which determines the type of UI control rendered and filtering behavior applied.
    /// </summary>
    public required FilterCategory FilterCategory { get; init; }

}

/// <summary>
/// Generic abstract base class for filter component models, extending <see cref="FilterComponentModel"/>
/// by introducing a strongly-typed value type for the column being filtered. The generic type parameter <typeparamref name="T"/>
/// must be an <see cref="Enum"/>, representing the specific data type or semantic meaning of the column value
/// (e.g., base, account number, integer, decimal). This enables more precise control over filter input rendering,
/// validation, and processing logic based on the column's value type.
/// </summary>
/// <typeparam name="T">
/// An enumeration that specifies the type or semantic category of the column value.
/// </typeparam>
public abstract class FilterComponentModel<T> : FilterComponentModel where T : Enum
{
    private protected FilterComponentModel() { }

    /// <summary>
    /// Specifies the value category of the column (e.g., base, account number, integer, decimal).
    /// This determines the rendering and validation logic for the filter input.
    /// </summary>
    public required T ValueCategory { get; init; }
}
