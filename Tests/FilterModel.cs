using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;

namespace Tests;

public abstract class BaseFilterModel
{
    public abstract FilterCategory FilterCategory { get; }
    public required string SearchValue { get; init; }
    public FilterParsingOptions Options { get; init; } = FilterParsingOptions.Default;
}

public class TextFilterModel : BaseFilterModel
{
    public override FilterCategory FilterCategory => FilterCategory.Text;
    public FilterOperations FilterType { get; init; } = FilterOperations.Contains;
    public TextColumn TextCategory { get; init; } = TextColumn.Base;
}

public class NumericFilterModel : BaseFilterModel
{
    public override FilterCategory FilterCategory => FilterCategory.Numeric;
    public FilterOperations FilterType { get; init; } = FilterOperations.Equals;
    public NumericColumn NumericCategory { get; init; } = NumericColumn.Decimal;
}
