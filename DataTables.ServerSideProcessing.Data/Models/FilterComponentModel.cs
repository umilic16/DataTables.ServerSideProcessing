using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models;
public class FilterComponentModel
{
    public required string TableName { get; init; }
    public required string ColumnName { get; init; }
    public required ColumnFilterType ColumnFilterType { get; init; }
    public ColumnValueType ColumnValueType { get; init; } = ColumnValueType.Base;
}
