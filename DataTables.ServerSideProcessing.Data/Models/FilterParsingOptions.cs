using System.Globalization;

namespace DataTables.ServerSideProcessing.Data.Models;

/// <summary>
/// Represents configuration options for parsing filters from request data.
/// </summary>
public sealed class FilterParsingOptions
{
    /// <summary>
    /// Default instance with all default values.
    /// </summary>
    public static readonly FilterParsingOptions Default = new();

    /// <summary>
    /// Prefix used for filter parameters in the request data. Default is "filter".
    /// </summary>
    public string Prefix { get; init; } = "filter";

    /// <summary>
    /// Key for the filter type in the request data. Default is "filterType".
    /// </summary>
    public string FilterTypeKey { get; init; } = "filterType";

    /// <summary>
    /// Key for the filter category in the request data. Default is "filterCategory".
    /// </summary>
    public string FilterCategoryKey { get; init; } = "filterCategory";

    /// <summary>
    /// Key for the value category in the request data. Default is "valueCategory".
    /// </summary>
    public string ValueCategoryKey { get; init; } = "valueCategory";

    /// <summary>
    /// Separator for "between" filter values. Default is ";".
    /// </summary>
    public string BetweenSeparator { get; init; } = ";";

    /// <summary>
    /// Separator used for multi-select filter values. Default is ",".
    /// </summary>
    public string MultiSelectSeparator { get; init; } = ",";

    /// <summary>
    /// Datetime styles to use when parsing date values. Default is DateTimeStyles.None.
    /// </summary>
    public DateTimeStyles DateTimeStyles { get; init; } = DateTimeStyles.None;
}
