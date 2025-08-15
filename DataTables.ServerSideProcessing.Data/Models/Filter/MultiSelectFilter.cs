using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.Filter;
/// <summary>
/// Filter model for MultiSelect columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>List&lt;string&gt;</c> as the type parameter.
/// </summary>
public class MultiSelectFilter<T> : FilterModel<T[]> where T : notnull;
/// <summary>
/// Filter model for MultiSelect columns.
/// Inherits from <see cref="FilterModel{T}"/> with <c>List&lt;string&gt;</c> as the type parameter.
/// </summary>
public sealed class MultiSelectFilter : MultiSelectFilter<string>;
