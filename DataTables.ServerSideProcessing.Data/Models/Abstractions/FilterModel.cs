namespace DataTables.ServerSideProcessing.Data.Models.Abstractions;

/// <summary>
/// Base class for all DataTable filter models.
/// Contains the property name to filter on and the search value.
/// </summary>
public abstract class FilterModel
{
    private protected FilterModel() { }

    /// <summary>
    /// Name of the property to filter.
    /// </summary>
    public required string PropertyName { get; init; }
}

/// <summary>
/// Generic base class for all DataTable filter models.
/// Contains the property name to filter on and the search value.
/// Inherits from <see cref="FilterModel"/>.
/// </summary>
/// <typeparam name="T">Type of the value used in the filter (e.g., string, int, DateTime).</typeparam>
public abstract class FilterModel<T> : FilterModel
{
    private protected FilterModel() { }

    /// <summary>
    /// Value to search for in the filter.
    /// </summary>
    public required T SearchValue { get; set; }
}
