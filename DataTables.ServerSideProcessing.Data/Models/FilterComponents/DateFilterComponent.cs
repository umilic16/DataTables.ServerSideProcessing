using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.FilterComponents;

/// <summary>
/// Represents the configuration for filtering a specific textual column in a DataTable.
/// Inherits from <see cref="FilterComponentModel"/>
/// Used to provide metadata for rendering and processing column filters
/// </summary>
public sealed record DateFilterComponent : FilterComponentModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateFilterComponent"/> class with specified table and column names.
    /// </summary>
    public DateFilterComponent(string tableName, string columnName)
        : base(tableName, columnName) { }

    /// <inheritdoc cref="FilterComponentModel.FilterCategory"/>
    public override FilterCategory FilterCategory => FilterCategory.Date;
}
