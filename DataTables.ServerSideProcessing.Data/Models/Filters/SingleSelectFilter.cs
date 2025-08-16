using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filters;

/// <summary>
/// Filter model for SingleSelect columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>string?</c> as the type parameter.
/// </summary>
public class SingleSelectFilter<T> : FilterModel<T>;

/// <summary>
/// Filter model for SingleSelect columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>string</c> as the type parameter.
/// </summary>
public sealed class SingleSelectFilter : SingleSelectFilter<string>;

