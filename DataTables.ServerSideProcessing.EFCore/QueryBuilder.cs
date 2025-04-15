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

    public static IQueryable<T> HandleGenericFilter<T>(this IQueryable<T> query, IEnumerable<string>? properties = null, string? search = null) where T : class
    {
        return GenericFilterHandler.HandleGenericFilter(query, properties, search);
    }

    public static IQueryable<T> HandleColumnFilters<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel>? filters = null) where T : class
    {
        return ColumnFilterHandler.HandleColumnFilters(query, filters);
    }

    public static IQueryable<T> HandleSorting<T>(this IQueryable<T> query, IEnumerable<SortModel>? sortOrder = null) where T : class
    {
        return SortHandler.HandleSorting(query, sortOrder);
    }

    public async static Task<List<T>> ExecuteQuery<T>(this IQueryable<T> query, int skip, int pageSize, CancellationToken ct) where T : class
    {
        return pageSize != -1 ? await query.Skip(skip).Take(pageSize).ToListAsync(ct) : await query.ToListAsync(ct);
    }
}
