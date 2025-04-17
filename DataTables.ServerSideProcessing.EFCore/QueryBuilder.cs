using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.EFCore.Filtering;
using DataTables.ServerSideProcessing.EFCore.Sorting;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore;
public static class QueryBuilder
{
    public static IQueryable<T> BuildQuery<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel>? filters = null, IEnumerable<SortModel>? sortOrder = null, IEnumerable<string>? properties = null, string? search = null) where T : class
    {
        return query.HandleGenericFilter(properties, search)
                    .HandleColumnFilters(filters)
                    .HandleSorting(sortOrder);
    }

    public static List<T> BuildAndExecuteQuery<T>(this IQueryable<T> query, int skip = 0, int pageSize = -1, IEnumerable<DataTableFilterBaseModel>? filters = null, IEnumerable<SortModel>? sortOrder = null, IEnumerable<string>? properties = null, string? search = null) where T : class
    {
        return query.BuildQuery(filters, sortOrder, properties, search)
                    .ExecuteQuery(skip, pageSize);
    }

    public static async Task<List<T>> BuildAndExecuteQueryAsync<T>(this IQueryable<T> query, int skip = 0, int pageSize = -1, IEnumerable<DataTableFilterBaseModel>? filters = null, IEnumerable<SortModel>? sortOrder = null, IEnumerable<string>? properties = null, string? search = null, CancellationToken ct = default) where T : class
    {
        return await query.BuildQuery(filters, sortOrder, properties, search)
                          .ExecuteQueryAsync(skip, pageSize, ct);
    }

    public static IQueryable<T> HandleGenericFilter<T>(this IQueryable<T> query, IEnumerable<string>? properties = null, string? search = null) where T : class
    {
        return properties == null || string.IsNullOrEmpty(search) ? query : GenericFilterHandler.HandleGenericFilter(query, properties, search);
    }

    public static IQueryable<T> HandleColumnFilters<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel>? filters = null) where T : class
    {
        return filters == null ? query : ColumnFilterHandler.HandleColumnFilters(query, filters);
    }

    public static IQueryable<T> HandleSorting<T>(this IQueryable<T> query, IEnumerable<SortModel>? sortOrder = null) where T : class
    {
        return sortOrder == null ? query : SortHandler.HandleSorting(query, sortOrder);
    }

    public static List<T> ExecuteQuery<T>(this IQueryable<T> query, int skip = 0, int pageSize = -1) where T : class
    {
        return pageSize != -1 ? query.Skip(skip).Take(pageSize).ToList() : query.ToList();
    }

    public static async Task<List<T>> ExecuteQueryAsync<T>(this IQueryable<T> query, int skip = 0, int pageSize = -1, CancellationToken ct = default) where T : class
    {
        return pageSize != -1 ? await query.Skip(skip).Take(pageSize).ToListAsync(ct) : await query.ToListAsync(ct);
    }
}
