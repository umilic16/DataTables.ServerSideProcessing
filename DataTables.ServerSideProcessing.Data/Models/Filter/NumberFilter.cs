using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filter;

/// <summary>
/// Filter model for numeric columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
public sealed class NumberFilter : FilterModel<string?>
{
    /// <summary>
    /// Type of the column (e.g., Int, Decimal).
    /// </summary>
    public ColumnValueNumericType ColumnType { get; init; } = ColumnValueNumericType.Int;

    /// <summary>
    /// Type of number filter to apply (e.g., Equals, Between).
    /// </summary>
    public required FilterOperations FilterType { get; init; }
}
