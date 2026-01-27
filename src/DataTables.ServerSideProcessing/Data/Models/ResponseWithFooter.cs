namespace DataTables.ServerSideProcessing.Data.Models;

/// <summary>
/// Represents a generic response model for DataTables server-side processing,
/// including additional footer data.
/// </summary>
/// <typeparam name="T">The type of main data contained in the response.</typeparam>
/// <typeparam name="F">The type of data for the footer.</typeparam>
public sealed record ResponseWithFooter<T, F> : Response<T>
{
    /// <summary>
    /// Data for the footer to be returned to the DataTable.
    /// </summary>
    public List<F> FooterData { get; set; } = [];
}
