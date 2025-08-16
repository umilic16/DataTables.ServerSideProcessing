using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filters;

/// <summary>
/// Filter model for text-based columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
public sealed class TextFilter : FilterModel<string>
{
    /// <summary>
    /// Type of the column (e.g., Base, AccNumber).
    /// </summary>
    public TextColumn ColumnType { get; init; } = TextColumn.Base;

    /// <summary>
    /// Type of text filter to apply (e.g., Contains, StartsWith).
    /// </summary>
    public required FilterOperations FilterType { get; init; }
}
