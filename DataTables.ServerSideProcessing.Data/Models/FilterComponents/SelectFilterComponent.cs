using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.FilterComponents;

/// <summary>
/// Represents a filter component for columns with selectable values (e.g., dropdowns, selects).
/// Provides a dictionary of available values for selection, where the key is the value used for filtering
/// and the value is the display label shown to the user.
/// Inherits from <see cref="FilterComponentModel"/>.
/// </summary>
public record SelectFilterComponent<V, T>(string columnName, Dictionary<V, T> availableValues)
    : FilterComponentModel(columnName) where V : notnull
{
    /// <summary>
    /// Dictionary of available values that can be selected for filtering.
    /// The key (<typeparamref name="V"/>) represents the filter value, and the value (<typeparamref name="T"/>) is the display label.
    /// </summary>
    public Dictionary<V, T> AvailableValues { get; } = availableValues;

    /// <inheritdoc cref="FilterComponentModel.FilterCategory"/>
    public override FilterCategory FilterCategory => FilterCategory.SingleSelect;
}

/// <summary>
/// Represents a filter component for columns with selectable values (e.g., dropdowns, selects).
/// Provides a list of available values for selection.
/// Inherits from <see cref="SelectFilterComponent{V,T}"/>.
/// </summary>
public sealed record SelectFilterComponent(string columnName, Dictionary<string, string> availableValues)
    : SelectFilterComponent<string, string>(columnName, availableValues);
