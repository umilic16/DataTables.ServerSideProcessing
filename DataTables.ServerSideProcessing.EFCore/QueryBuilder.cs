using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.EFCore.Filtering;
using DataTables.ServerSideProcessing.EFCore.Sorting;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore;
/// <summary>
/// Provides extension methods for building and executing server-side queries for DataTables using Entity Framework Core.
/// </summary>
public static class QueryBuilder
{
    /// <summary>
    /// Builds an <see cref="IQueryable{T}"/> by conditionally applying generic search, column filters, and sorting if the relevant parameters are provided.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="filters">Optional column filters to apply.</param>
    /// <param name="sortOrder">Optional sort order definitions.</param>
    /// <param name="properties">Optional property names for generic search.</param>
    /// <param name="search">Optional search string for generic filtering.</param>
    /// <returns>The query with filters and sorting applied.</returns>
    public static IQueryable<T> BuildQuery<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel>? filters = null, IEnumerable<SortModel>? sortOrder = null, IEnumerable<string>? properties = null, string? search = null) where T : class
    {
        return query.HandleGenericFilter(properties, search)
                    .HandleColumnFilters(filters)
                    .HandleSorting(sortOrder);
    }

    /// <summary>
    /// Builds a query by conditionally applying generic search, column filters, and sorting if the relevant parameters are provided.
    /// Then executes it by returning a paged list of results.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="pageSize">The number of records to take. If -1, returns all records.</param>
    /// <param name="filters">Optional column filters to apply.</param>
    /// <param name="sortOrder">Optional sort order definitions.</param>
    /// <param name="properties">Optional property names for generic search.</param>
    /// <param name="search">Optional search string for generic filtering.</param>
    /// <returns>A list of results after applying filters, sorting, and pagination.</returns>
    public static List<T> BuildAndExecuteQuery<T>(this IQueryable<T> query, int skip = 0, int pageSize = -1, IEnumerable<DataTableFilterBaseModel>? filters = null, IEnumerable<SortModel>? sortOrder = null, IEnumerable<string>? properties = null, string? search = null) where T : class
    {
        return query.BuildQuery(filters, sortOrder, properties, search)
                    .ExecuteQuery(skip, pageSize);
    }

    /// <summary>
    /// Builds a query by conditionally applying generic search, column filters, and sorting if the relevant parameters are provided.
    /// Then executes it asynchronously by returning a paged list of results.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="pageSize">The number of records to take. If -1, returns all records.</param>
    /// <param name="filters">Optional column filters to apply.</param>
    /// <param name="sortOrder">Optional sort order definitions.</param>
    /// <param name="properties">Optional property names for generic search.</param>
    /// <param name="search">Optional search string for generic filtering.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with a list of results.</returns>
    public static async Task<List<T>> BuildAndExecuteQueryAsync<T>(this IQueryable<T> query, int skip = 0, int pageSize = -1, IEnumerable<DataTableFilterBaseModel>? filters = null, IEnumerable<SortModel>? sortOrder = null, IEnumerable<string>? properties = null, string? search = null, CancellationToken ct = default) where T : class
    {
        return await query.BuildQuery(filters, sortOrder, properties, search)
                          .ExecuteQueryAsync(skip, pageSize, ct);
    }

    /// <summary>
    /// Applies a generic search filter to the query based on the specified properties and search string.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="properties">The property names to search.</param>
    /// <param name="search">The search string.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<T> HandleGenericFilter<T>(this IQueryable<T> query, IEnumerable<string>? properties, string? search) where T : class
    {
        return properties == null || string.IsNullOrEmpty(search) ? query : GenericFilterHandler.HandleGenericFilter(query, properties, search);
    }

    /// <summary>
    /// Applies column-specific filters to the query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="filters">The column filters to apply.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<T> HandleColumnFilters<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel>? filters) where T : class
    {
        return filters == null ? query : ColumnFilterHandler.HandleColumnFilters(query, filters);
    }

    /// <summary>
    /// Applies sorting to the query based on the specified sort order.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="sortOrder">The sort order definitions.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<T> HandleSorting<T>(this IQueryable<T> query, IEnumerable<SortModel>? sortOrder) where T : class
    {
        return sortOrder == null ? query : SortHandler.HandleSorting(query, sortOrder);
    }

    /// <summary>
    /// Executes the query and returns a paged list of results.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="pageSize">The number of records to take. If -1, returns all records.</param>
    /// <returns>A list of results.</returns>
    public static List<T> ExecuteQuery<T>(this IQueryable<T> query, int skip = 0, int pageSize = -1) where T : class
    {
        return pageSize != -1 ? query.Skip(skip).Take(pageSize).ToList() : query.ToList();
    }

    /// <summary>
    /// Asynchronously executes the query and returns a paged list of results.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="pageSize">The number of records to take. If -1, returns all records.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with a list of results.</returns>
    public static async Task<List<T>> ExecuteQueryAsync<T>(this IQueryable<T> query, int skip = 0, int pageSize = -1, CancellationToken ct = default) where T : class
    {
        return pageSize != -1 ? await query.Skip(skip).Take(pageSize).ToListAsync(ct) : await query.ToListAsync(ct);
    }
}
