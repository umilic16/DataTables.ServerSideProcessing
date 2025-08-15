namespace DataTables.ServerSideProcessing.Data.Models;

/// <summary>
/// Represents a generic response model for DataTables server-side processing.
/// </summary>
/// <typeparam name="T">The type of data contained in the response.</typeparam>
public sealed class Response<T>
{
    /// <summary>
    /// Data to be returned to the DataTable.
    /// </summary>
    public List<T> Data { get; set; } = [];

    /// <summary>
    /// Draw counter that ensures Ajax requests from DataTables are in sequence.
    /// </summary>
    public string? Draw { get; set; }

    /// <summary>
    /// Total number of records after filtering.
    /// </summary>
    public int RecordsFiltered { get; set; }

    /// <summary>
    /// Total number of records before filtering.
    /// </summary>
    public int RecordsTotal { get; set; }
}
