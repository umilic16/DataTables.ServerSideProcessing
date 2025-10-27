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
    /// Configures DataTables options and registers them in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="configure">An action to configure the <see cref="DataTablesConfiguration"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection ConfigureDataTables(this IServiceCollection services, Action<DataTablesConfiguration> configure)
    {
        var cfg = new DataTablesConfiguration();

        configure(cfg);

        FilterParsingOptions.SetDefault(cfg.FilterParsingOptions);
        DataTableComponent.SetDefaultTableClasses(cfg.TableClasses);

        // optionally register the configuration instance in DI container

        return services;
    }
}
