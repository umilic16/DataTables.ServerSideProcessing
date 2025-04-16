using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using System.Globalization;
using System.Linq.Expressions;
using static DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilder;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;
internal static class ColumnFilterHandler
{
    internal static IQueryable<T> HandleColumnFilters<T>(this IQueryable<T> query, IEnumerable<DataTableFilterBaseModel> filters) where T : class
    {
        if (!filters.Any())
            return query;

        foreach (DataTableFilterBaseModel filterModel in filters)
        {
            if (!ReflectionCache<T>.Properties.TryGetValue(filterModel.PropertyName, out string? propName))
                throw new InvalidOperationException($"Property '{propName}' not found on type '{typeof(T).Name}'.");

            Expression<Func<T, bool>>? predicate = null;

            if (filterModel is DataTableTextFilterModel filterTextModel)
            {
                if (string.IsNullOrEmpty(filterTextModel.SearchValue))
                    continue;

                if (filterTextModel.ColumnType == ColumnValueType.AccNumber)
                    filterTextModel.SearchValue = filterTextModel.SearchValue.Replace("-", "");

                predicate = BuildTextWhereExpression<T>(
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

                predicate = BuildNumericWhereExpression<T>(
                                    propName,
                                    filterNumberModel.FilterType,
                                    filterNumberModel.SearchValue);
            }
            else if (filterModel is DataTableDateTimeFilterModel filterDateModel)
            {
                if (string.IsNullOrEmpty(filterDateModel.SearchValue))
                    continue;

                if (!DateTime.TryParseExact(filterDateModel.SearchValue, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime datumParsed))
                    continue;

                predicate = BuildDateWhereExpression<T>(
                                    propName,
                                    datumParsed);
            }

            if (predicate != null)
                query = query.Where(predicate);

        }
        return query;
    }
}
