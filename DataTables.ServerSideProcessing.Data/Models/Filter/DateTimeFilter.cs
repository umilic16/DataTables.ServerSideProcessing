using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filter;

/// <summary>
/// Filter model for DateTime columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
public sealed class DateTimeFilter : FilterModel<string?>
{
    /// <summary>
    /// Type of filter to apply (e.g., Equals, Between).
    /// </summary>
    public required FilterOperations FilterType { get; init; }
}
