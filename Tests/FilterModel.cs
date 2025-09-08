using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;

namespace Tests;
public class BaseFilterModel
{
    public required FilterCategory FilterCategory { get; init; }
    public required string SearchValue { get; init; }
    public FilterParsingOptions Options { get; init; } = FilterParsingOptions.Default;
}

public class TextFilterModel : BaseFilterModel
{
    public required FilterOperations FilterType { get; init; }
    public required TextColumn TextCategory { get; init; }
}

public class NumericFilterModel : BaseFilterModel
{
    public required FilterOperations FilterType { get; init; }
    public required NumericColumn NumericCategory { get; init; }
}
