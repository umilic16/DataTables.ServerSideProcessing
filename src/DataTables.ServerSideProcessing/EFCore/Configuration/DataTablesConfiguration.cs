using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.Data.Models.FilterComponents;

namespace DataTables.ServerSideProcessing.EFCore.Configuration;

/// <summary>
/// Represents the configuration entry point for customizing the default behavior and appearance of DataTables within the library.
/// </summary>
public sealed class DataTablesConfiguration
{
    internal FilterParsingOptions FilterParsingOptions { get; private set; } = FilterParsingOptions.Default;
    internal string TableClasses { get; private set; } = DataTableComponent.DefaultTableClasses;

    /// <summary>
    /// Configures the default <see cref="Data.Models.FilterParsingOptions"/> values.
    /// </summary>
    /// <param name="configure">
    /// A delegate that receives the current default filter parsing options and returns the new ones,
    /// e.g. <c>o => o with { Prefix = "f" }</c>.
    /// </param>
    /// <returns>The current <see cref="DataTablesConfiguration"/> instance for fluent configuration.</returns>
    public DataTablesConfiguration SetDefaultFilterParsingOptions(Func<FilterParsingOptions, FilterParsingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        FilterParsingOptions = configure(FilterParsingOptions)
            ?? throw new InvalidOperationException("The configure delegate must return a FilterParsingOptions instance.");
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
