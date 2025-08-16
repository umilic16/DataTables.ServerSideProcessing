using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filters;

/// <summary>
/// Filter model for Date columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>DateOnly</c> as the type parameter.
/// </summary>
public sealed class DateFilter : FilterModel<DateOnly>
{
    /// <summary>
    /// Type of filter to apply (e.g., Equals, Between).
    /// </summary>
    public required FilterOperations FilterType { get; init; }
}
