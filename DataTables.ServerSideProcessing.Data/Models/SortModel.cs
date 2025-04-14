using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models;
public class SortModel
{
    public required string PropertyName { get; set; }
    public SortDirection SortDirection { get; set; }
}
