using System.Linq.Expressions;
using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;
/// <summary>
/// Provides an extension method for handling column filters on <see cref="IQueryable{T}"/> source
/// based on a collection of <see cref="DataTableFilterBaseModel"/> definitions.
/// </summary>
internal static class ColumnFilterHandler
{
    /// <summary>
    /// Applies a collection of column filters to the given <see cref="IQueryable{T}"/> source.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The source query to filter.</param>
    /// <param name="filters">A collection of filter models to apply.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> with the filters applied.
    /// If <paramref name="filters"/> is empty, the original query is returned.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a property specified in <paramref name="filters"/> does not exist on <typeparamref name="T"/> or the type does not match the filter type.
    /// </exception>
    internal static IQueryable<T> HandleColumnFilters<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel> filters) where T : class
    {
        if (!filters.Any())
            return query;

        foreach (DataTableFilterBaseModel filterModel in filters)
        {
            if (!ReflectionCache<T>.s_properties.TryGetValue(filterModel.PropertyName, out string? propName))
                throw new InvalidOperationException($"Property '{propName}' not found on type '{typeof(T).Name}'.");

            Expression<Func<T, bool>>? predicate = null;

            if (filterModel is DataTableTextFilterModel filterTextModel)
            {
                if (string.IsNullOrEmpty(filterTextModel.SearchValue))
                    continue;

                if (filterTextModel.ColumnType == ColumnValueType.AccNumber)
                    filterTextModel.SearchValue = filterTextModel.SearchValue.Replace("-", "");

                predicate = TextExpressionBuilder.Build<T>(
                                    propName,
                                    filterTextModel.FilterType,
                                    filterTextModel.SearchValue);

            }
            else if (filterModel is DataTableNumberFilterModel filterNumberModel)
            {
                if (string.IsNullOrEmpty(filterNumberModel.SearchValue))
                    continue;

                if (filterNumberModel.ColumnType == ColumnValueType.Decimal)
                    filterNumberModel.SearchValue = filterNumberModel.SearchValue.Replace(".", "");

                predicate = NumericExpressionBuilder.Build<T>(
                                    propName,
                                    filterNumberModel.FilterType,
                                    filterNumberModel.SearchValue);
            }
            else if (filterModel is DataTableDateTimeFilterModel filterDateModel)
            {
                if (string.IsNullOrEmpty(filterDateModel.SearchValue))
                    continue;

                predicate = DateExpressionBuilder.Build<T>(
                                    propName,
                                    filterDateModel.FilterType,
                                    filterDateModel.SearchValue);
            }
            else if (filterModel is DataTableSingleSelectFilterModel filterSingleSelectModel)
            {
                if (string.IsNullOrEmpty(filterSingleSelectModel.SearchValue))
                    continue;

                predicate = SelectExpressionBuilder.BuildSingleSelect<T>(
                                    propName,
                                    filterSingleSelectModel.SearchValue);
            }
            else if (filterModel is DataTableMultiSelectFilterModel filterMultiSelectModel)
            {
                if (filterMultiSelectModel.SearchValue.Count == 0)
                    continue;

                predicate = SelectExpressionBuilder.BuildMultiSelect<T>(
                                    propName,
                                    filterMultiSelectModel.SearchValue);
            }

            if (predicate != null)
                query = query.Where(predicate);

        }
        return query;
    }
}
