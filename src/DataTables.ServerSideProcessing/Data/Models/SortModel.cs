using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models;
/// <summary>
/// Represents sorting definition for a DataTable column.
/// </summary>
public sealed record SortModel
{
    /// <summary>
    /// Property name to sort by.
    /// </summary>
    public required string PropertyName { get; set; }

    /// <summary>
    /// Direction of the sort.
    /// </summary>
    public SortDirection SortDirection { get; set; }
}
