namespace DataTables.ServerSideProcessing.Data.Models;

public class DataTableResponse<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public string? Draw { get; set; }
    public int RecordsFiltered { get; set; }
    public int RecordsTotal { get; set; }
}
