using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.FilterComponents;

/// <summary>
/// Represents the configuration for filtering a specific textual column in a DataTable.
/// Inherits from <see cref="FilterComponentModel{T}"/>
/// Used to provide metadata for rendering and processing column filters, including
/// the type of value the column holds (e.g., base, account number, int, decimal).
/// </summary>
public sealed class NumericFilterComponent : FilterComponentModel<NumericColumn>;
