using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.FilterComponents;

/// <summary>
/// Represents the configuration for filtering a specific textual column in a DataTable.
/// Inherits from <see cref="FilterComponentModel{T}"/>
/// Used to provide metadata for rendering and processing column filters, including
/// the type of value the column holds (e.g., base, account number).
/// </summary>
public sealed record TextFilterComponent : FilterComponentModel<TextColumn>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextFilterComponent"/> class.
    /// </summary>
    public TextFilterComponent() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextFilterComponent"/> class with specified table and column names, and optionally the value category.
    /// </summary>
    public TextFilterComponent(string tableName, string columnName, TextColumn valueCategory = default)
        : base(tableName, columnName, valueCategory) { }

    /// <inheritdoc cref="FilterComponentModel.FilterCategory"/>
    public override FilterCategory FilterCategory => FilterCategory.Text;
}
