using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.Data.Models.FilterComponents;
using Microsoft.Extensions.DependencyInjection;

namespace DataTables.ServerSideProcessing.EFCore.Configuration;

/// <summary>
/// Provides extension methods for customizing the default behavior and appearance of DataTables within the library.
/// </summary>
public static class DataTablesConfigurationExtensions
{
    /// <summary>
    /// Sets the library-wide default values (filter parsing options and table CSS classes).
    /// Intended to be called once at application startup.
    /// </summary>
    /// <param name="services">The service collection. It is returned unchanged, to allow chaining.</param>
    /// <param name="configure">An action to configure the <see cref="DataTablesConfiguration"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection ConfigureDataTables(this IServiceCollection services, Action<DataTablesConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var cfg = new DataTablesConfiguration();

        configure(cfg);

        FilterParsingOptions.SetDefault(cfg.FilterParsingOptions);
        DataTableComponent.SetDefaultTableClasses(cfg.TableClasses);

        return services;
    }
}
