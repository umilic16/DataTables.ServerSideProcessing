using System.Linq.Expressions;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;
using DataTables.ServerSideProcessing.Data.Models.Filters;
using DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class ColumnFilterHandler
{
    internal static IQueryable<T> HandleColumnFilters<T>(this IQueryable<T> query, FilterModel[]? filters) where T : class
    {
        if (filters is not { Length: > 0 })
            return query;

        foreach (FilterModel filterModel in filters)
        {
            if (!ReflectionCache<T>.s_properties.TryGetValue(filterModel.PropertyName, out string? propName))
                throw new InvalidOperationException($"Property '{propName}' not found on type '{typeof(T).Name}'.");

            Expression<Func<T, bool>>? predicate = filterModel switch
            {
                TextFilter tf => TextExpressionBuilder.Build<T>(propName, tf.FilterType, tf.SearchValue),
                NumberFilter<int> nif => NumericExpressionBuilder.Build<T, int>(propName, nif.FilterType, nif.SearchValue),
                NumberFilter<decimal> ndf => NumericExpressionBuilder.Build<T, decimal>(propName, ndf.FilterType, ndf.SearchValue),
                DateFilter df => DateExpressionBuilder.Build<T>(propName, df.FilterType, df.SearchValue),
                SingleSelectFilter ssf => SelectExpressionBuilder.BuildSingleSelect<T>(propName, ssf.SearchValue),
                MultiSelectFilter msf => MultiSelectExpressionBuilder.BuildMultiSelect<T>(propName, msf.SearchValue),
                _ => null
            };

            if (predicate != null)
                query = query.Where(predicate);
        }
        return query;
    }
}
