using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filters;

/// <summary>
/// Filter model for SingleSelect columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
internal record SingleSelectFilter<T> : FilterModel<T>;

/// <summary>
/// Filter model for SingleSelect columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>string</c> as the type parameter.
/// </summary>
internal sealed record SingleSelectFilter : SingleSelectFilter<string>;

