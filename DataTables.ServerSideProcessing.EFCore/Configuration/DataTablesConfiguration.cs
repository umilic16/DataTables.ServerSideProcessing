using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.Data.Models.FilterComponents;

namespace DataTables.ServerSideProcessing.EFCore.Configuration;

/// <summary>
/// Represents the configuration entry point for customizing the default behavior and appearance of DataTables within the library.
/// </summary>
public sealed class DataTablesConfiguration
{
    internal FilterParsingOptions FilterParsingOptions { get; } = FilterParsingOptions.Default;
    internal string TableClasses { get; private set; } = DataTableComponent.DefaultTableClasses;

    /// <summary>
    /// Configures the default <see cref="Data.Models.FilterParsingOptions"/> values.
    /// </summary>
    /// <param name="configure">A delegate used to modify the default filter parsing options.</param>
    /// <returns>The current <see cref="DataTablesConfiguration"/> instance for fluent configuration.</returns>
    public DataTablesConfiguration SetDefaultFilterParsingOptions(Action<FilterParsingOptions> configure)
    {
        configure(FilterParsingOptions);
        return this;
    }

    /// <summary>
    /// Sets the default CSS classes applied to all <see cref="DataTableComponent"/> instances.
    /// </summary>
    /// <param name="classes">The CSS class string to use as the default for DataTables.</param>
    /// <returns>The current <see cref="DataTablesConfiguration"/> instance for fluent configuration.</returns>
    public DataTablesConfiguration SetDefaultTableClasses(string classes)
    {
        TableClasses = classes;
        return this;
    }
}
