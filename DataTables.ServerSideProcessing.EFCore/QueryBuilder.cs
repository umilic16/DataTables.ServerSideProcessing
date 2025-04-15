using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.EntityFrameworkCore;
using static DataTables.ServerSideProcessing.EFCore.Filtering.ColumnFilterHandler;
using static DataTables.ServerSideProcessing.EFCore.Filtering.GenericFilterHandler;
using static DataTables.ServerSideProcessing.EFCore.Sorting.SortHandler;

namespace DataTables.ServerSideProcessing.EFCore;
public static class QueryBuilder
{
    public static IQueryable<T> BuildQuery<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel>? filters = null, IEnumerable<SortModel>? sortOrder = null, IEnumerable<string>? properties = null, string? search = null) where T : class
    {
        return query.HandleGenericFilter(properties, search)
                    .HandleColumnFilters(filters)
                    .HandleSorting(sortOrder);
    }

    public async static Task<List<T>> ExecuteQuery<T>(this IQueryable<T> query, int skip, int pageSize, CancellationToken ct) where T : class
    {
        return pageSize != -1 ? await query.Skip(skip).Take(pageSize).ToListAsync(ct) : await query.ToListAsync(ct);
    }
}
