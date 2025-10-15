using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.FilterComponents;

/// <summary>
/// Represents the configuration for filtering a specific numeric column in a DataTable.
/// Inherits from <see cref="FilterComponentModel{T}"/>
/// Used to provide metadata for rendering and processing column filters, including
/// the type of value the column holds (e.g., int, decimal).
/// </summary>
public sealed record NumericFilterComponent : FilterComponentModel<NumericColumn>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumericFilterComponent"/> class.
    /// </summary>
    public NumericFilterComponent() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumericFilterComponent"/> class with specified table and column names, and optionally the value category.
    /// </summary>
    public NumericFilterComponent(string tableName, string columnName, NumericColumn valueCategory = default)
        : base(tableName, columnName, valueCategory) { }

    /// <inheritdoc cref="FilterComponentModel.FilterCategory"/>
    public override FilterCategory FilterCategory => FilterCategory.Numeric;
}
