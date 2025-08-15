using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models;

/// <summary>
/// Represents a request for server-side processing of a DataTable.
/// Contains search, pagination, sorting, and filtering definition.
/// </summary>
public sealed class Request
{
    /// <summary>
    /// The global search term to filter all columns.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// The number of records to skip (used for pagination).
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// The number of records to return (page size).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The collection of sort definitions.
    /// </summary>
    public SortModel[]? SortOrder { get; set; }

    /// <summary>
    /// The collection of column-specific filter definition.
    /// </summary>
    public FilterModel[]? Filters { get; set; }
}
