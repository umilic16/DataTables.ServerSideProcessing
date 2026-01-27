using DataTables.ServerSideProcessing.Data.Models.Abstractions;

namespace DataTables.ServerSideProcessing.Data.Models.FilterComponents;

/// <summary>
/// Represents the configuration and definition of a DataTable, including its name, appearance, and column filter components.
/// </summary>
public sealed class DataTableComponent
{
    /// <summary>
    /// Gets the global default CSS class string applied to table elements when no explicit
    /// <see cref="TableClasses"/> value is provided.
    /// </summary>
    public static string DefaultTableClasses { get; private set; } = string.Empty;

    /// <summary>
    /// Sets the global default CSS classes applied to all DataTables.
    /// </summary>
    /// <param name="value">The CSS class string to apply as the default.</param>
    public static void SetDefaultTableClasses(string value) => DefaultTableClasses = value;

    /// <summary>
    /// Gets the CSS class string used for this specific table instance.
    /// Defaults to <see cref="DefaultTableClasses"/> if not explicitly set.
    /// </summary>
    public string TableClasses { get; init; } = DefaultTableClasses;

    /// <summary>
    /// Gets the unique name or identifier of the data table, typically corresponding to
    /// the underlying dataset or logical grouping it represents.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Gets the collection of filter components associated with this table,
    /// used to define and apply filter behavior to its data.
    /// </summary>
    public required List<IFilterCell> FilterComponents { get; init; }
}
