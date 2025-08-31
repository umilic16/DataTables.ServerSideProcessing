using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
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
            if (!ReflectionCache<T>.s_properties.TryGetValue(sortModel.PropertyName, out string? propName))
                throw new InvalidOperationException($"Property '{propName}' not found on type '{typeof(T).Name}'.");

            if (isFirstFlag)
            {
                query = sortModel.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(e => EF.Property<object>(e, propName))
                    : query.OrderByDescending(e => EF.Property<object>(e, propName));

                isFirstFlag = false;
            }
            else
            {
                query = sortModel.SortDirection == SortDirection.Ascending
                    ? ((IOrderedQueryable<T>)query).ThenBy(e => EF.Property<object>(e, propName))
                    : ((IOrderedQueryable<T>)query).ThenByDescending(e => EF.Property<object>(e, propName));
            }
        }
        return query;
    }
}
