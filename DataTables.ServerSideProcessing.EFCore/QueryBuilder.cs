using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.EntityFrameworkCore;
using static DataTables.ServerSideProcessing.EFCore.Filtering.ColumnFilterHandler;
using static DataTables.ServerSideProcessing.EFCore.Sorting.SortHandler;

namespace DataTables.ServerSideProcessing.EFCore;
public static class QueryBuilder
{
    public static IQueryable<T> BuildQuery<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel>? filters = null, IEnumerable<SortModel>? sortOrder = null) where T : class
    {
        if (filters != null)
            query = HandleColumnFilters(filters, query);
        if (sortOrder != null)
            query = HandleSorting(sortOrder, query);
        return query;
    }

    public async static Task<List<T>> ExecuteQuery<T>(this IQueryable<T> query, int skip, int pageSize, CancellationToken ct) where T : class
    {
        return pageSize != -1 ? await query.Skip(skip).Take(pageSize).ToListAsync(ct) : await query.ToListAsync(ct);
    }
}
