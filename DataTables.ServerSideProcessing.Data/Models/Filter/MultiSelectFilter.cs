using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filter;

/// <summary>
/// Filter model for MultiSelect columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>T[]</c> as the type parameter.
/// </summary>
/// <typeparam name="T">The type of the items in the multi-select filter. Must be non-nullable.</typeparam>
public class MultiSelectFilter<T> : FilterModel<T[]> where T : notnull;

/// <summary>
/// Filter model for MultiSelect columns with <c>string</c> as the item type.
/// Inherits from <see cref="MultiSelectFilter{T}"/> with <c>string</c> as the type parameter.
/// </summary>
public sealed class MultiSelectFilter : MultiSelectFilter<string>;
