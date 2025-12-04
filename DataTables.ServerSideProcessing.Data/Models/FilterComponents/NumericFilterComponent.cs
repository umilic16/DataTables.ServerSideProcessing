using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.FilterComponents;

/// <summary>
/// Represents the configuration for filtering a specific numeric column in a DataTable.
/// Inherits from <see cref="FilterComponentModel{T}"/>
/// Used to provide metadata for rendering and processing column filters, including
/// the type of value the column holds (e.g., int, decimal).
/// </summary>
public sealed record NumericFilterComponent(string columnName, NumericColumn valueCategory = default, FilterOperations initialFilterOperation = FilterOperations.Equals)
    : FilterComponentModel<NumericColumn>(columnName, valueCategory)
{
    /// <inheritdoc cref="FilterComponentModel.FilterCategory"/>
    public override FilterCategory FilterCategory => FilterCategory.Numeric;

    /// <inheritdoc cref="FilterComponentModelWithInitialOperation.InitialFilterOperation"/>
    public override FilterOperations InitialFilterOperation { get; } = initialFilterOperation;
}
