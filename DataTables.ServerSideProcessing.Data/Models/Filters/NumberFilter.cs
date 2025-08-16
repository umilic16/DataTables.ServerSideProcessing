using System.Numerics;
using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filters;

/// <summary>
/// Filter model for numeric columns.
/// Inherits from <see cref="FilterModel{T}"/> where <typeparamref name="T"/> implements <see cref="INumber{T}"/>.
/// </summary>
/// <typeparam name="T">The numeric type of the column (e.g., int, decimal) that implements <see cref="INumber{T}"/>.</typeparam>
public sealed class NumberFilter<T> : FilterModel<T> where T : INumber<T>
{
    /// <summary>
    /// Type of the numeric column (e.g., Int, Decimal).
    /// </summary>
    public NumericColumn ColumnType { get; init; } = NumericColumn.Int;

    /// <summary>
    /// Type of filter to apply (e.g., Equals, Between).
    /// </summary>
    public required FilterOperations FilterType { get; init; }
}
