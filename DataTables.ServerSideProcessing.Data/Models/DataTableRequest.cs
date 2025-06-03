namespace DataTables.ServerSideProcessing.Data.Models;

public class DataTableRequest
{
    public string? Search { get; set; }
    public int Skip { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<SortModel>? SortOrder { get; set; }
    public IEnumerable<DataTableFilterBaseModel>? Filters { get; set; }
}
