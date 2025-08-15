using System.Linq.Expressions;
using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;
using DataTables.ServerSideProcessing.Data.Models.Filter;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class ColumnFilterHandler
{
    internal static IQueryable<T> HandleColumnFilters<T>(this IQueryable<T> query, FilterModel[] filters) where T : class
    {
        if (filters.Length == 0)
            return query;

        foreach (FilterModel filterModel in filters)
        {
            if (!ReflectionCache<T>.s_properties.TryGetValue(filterModel.PropertyName, out string? propName))
                throw new InvalidOperationException($"Property '{propName}' not found on type '{typeof(T).Name}'.");

            Expression<Func<T, bool>>? predicate = null;

            if (filterModel is Data.Models.Filter.TextFilter filterTextModel)
            {
                if (string.IsNullOrEmpty(filterTextModel.SearchValue))
                    continue;

                if (filterTextModel.ColumnType == ColumnValueTextType.AccNumber)
                    filterTextModel.SearchValue = filterTextModel.SearchValue.Replace("-", "");

                predicate = TextExpressionBuilder.Build<T>(
                                    propName,
                                    filterTextModel.FilterType,
                                    filterTextModel.SearchValue);

            }
            else if (filterModel is NumberFilter filterNumberModel)
            {
                if (string.IsNullOrEmpty(filterNumberModel.SearchValue))
                    continue;

                if (filterNumberModel.ColumnType == ColumnValueNumericType.Decimal)
                    filterNumberModel.SearchValue = filterNumberModel.SearchValue.Replace(".", "");

                predicate = NumericExpressionBuilder.Build<T>(
                                    propName,
                                    filterNumberModel.FilterType,
                                    filterNumberModel.SearchValue);
            }
            else if (filterModel is DateTimeFilter filterDateModel)
            {
                if (string.IsNullOrEmpty(filterDateModel.SearchValue))
                    continue;

                predicate = DateExpressionBuilder.Build<T>(
                                    propName,
                                    filterDateModel.FilterType,
                                    filterDateModel.SearchValue);
            }
            else if (filterModel is SingleSelectFilter filterSingleSelectModel)
            {
                if (string.IsNullOrEmpty(filterSingleSelectModel.SearchValue))
                    continue;

                predicate = SelectExpressionBuilder.BuildSingleSelect<T>(
                                    propName,
                                    filterSingleSelectModel.SearchValue);
            }
            else if (filterModel is MultiSelectFilter filterMultiSelectModel)
            {
                if (filterMultiSelectModel.SearchValue.Length == 0)
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
