using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore.Sorting;
/// <summary>
/// Provides an extension method for applying sorting to <see cref="IQueryable{T}"/> source 
/// based on a collection of <see cref="SortModel"/> definitions.
/// </summary>
internal static class SortHandler
{
    /// <summary>
    /// Applies sorting to the given <see cref="IQueryable{T}"/> based on the specified sort order.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="sortOrder">A collection of <see cref="SortModel"/> specifying the sort order.</param>
    /// <returns>An <see cref="IQueryable{T}"/> with the applied sorting.
    /// If <paramref name="sortOrder"/> is empty, the original query is returned.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a property specified in <paramref name="sortOrder"/> does not exist on <typeparamref name="T"/>.
    /// </exception>
    internal static IQueryable<T> HandleSorting<T>(this IQueryable<T> query, IEnumerable<SortModel> sortOrder) where T : class
    {
        if (!sortOrder.Any())
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
