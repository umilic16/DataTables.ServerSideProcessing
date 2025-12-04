using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.FilterComponents;

/// <summary>
/// Represents the configuration for filtering a specific textual column in a DataTable.
/// Inherits from <see cref="FilterComponentModelWithInitialOperation"/>
/// Used to provide metadata for rendering and processing column filters
/// </summary>
public sealed record DateFilterComponent(string columnName, FilterOperations initialFilterOperation = FilterOperations.Equals)
    : FilterComponentModelWithInitialOperation(columnName)
{

    /// <inheritdoc cref="FilterComponentModel.FilterCategory"/>
    public override FilterCategory FilterCategory => FilterCategory.Date;

    /// <inheritdoc cref="FilterComponentModelWithInitialOperation.InitialFilterOperation"/>
    public override FilterOperations InitialFilterOperation { get; } = initialFilterOperation;
}
