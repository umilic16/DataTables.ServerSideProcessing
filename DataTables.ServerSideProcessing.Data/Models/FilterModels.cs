using DataTables.ServerSideProcessing.Data.Enums;

namespace DataTables.ServerSideProcessing.Data.Models;
public abstract class DataTableFilterBaseModel
{
    public required string PropertyName { get; init; }
    public required string? SearchValue { get; set; }
}

public class DataTableTextFilterModel : DataTableFilterBaseModel
{
    public ColumnValueType ColumnType { get; init; } = ColumnValueType.Base;
    public required TextFilter FilterType { get; init; }
}

public class DataTableNumberFilterModel : DataTableFilterBaseModel
{
    public ColumnValueType ColumnType { get; init; } = ColumnValueType.Int;
    public required NumberFilter FilterType { get; init; }
}

public class DataTableDateTimeFilterModel : DataTableFilterBaseModel
{
}
