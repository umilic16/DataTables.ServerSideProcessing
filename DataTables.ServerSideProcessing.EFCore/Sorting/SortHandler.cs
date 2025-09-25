using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.EFCore.ReflectionCache;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore.Sorting;

internal static class SortHandler
{
    internal static IQueryable<T> HandleSorting<T>(this IQueryable<T> query, SortModel[]? sortOrder) where T : class
    {
        if (sortOrder is not { Length: > 0 })
            return query;

        bool isFirstFlag = true;
        foreach (SortModel sortModel in sortOrder)
        {
            if (!PropertyInfoCache<T>.PropertyExists(sortModel.PropertyName))
                throw new InvalidOperationException($"Property '{sortModel.PropertyName}' not found on type '{typeof(T).Name}'.");

            if (isFirstFlag)
            {
                query = sortModel.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(e => EF.Property<object>(e, sortModel.PropertyName))
                    : query.OrderByDescending(e => EF.Property<object>(e, sortModel.PropertyName));

                isFirstFlag = false;
            }
            else
            {
                query = sortModel.SortDirection == SortDirection.Ascending
                    ? ((IOrderedQueryable<T>)query).ThenBy(e => EF.Property<object>(e, sortModel.PropertyName))
                    : ((IOrderedQueryable<T>)query).ThenByDescending(e => EF.Property<object>(e, sortModel.PropertyName));
            }
        }
        return query;
    }
}
