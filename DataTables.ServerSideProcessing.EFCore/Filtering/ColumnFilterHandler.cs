using System.Linq.Expressions;
using System.Reflection;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;
using DataTables.ServerSideProcessing.Data.Models.Filters;
using DataTables.ServerSideProcessing.EFCore.Filtering.ExpressionBuilders;
using DataTables.ServerSideProcessing.EFCore.ReflectionCache;

namespace DataTables.ServerSideProcessing.EFCore.Filtering;

internal static class ColumnFilterHandler
{
    internal static IQueryable<T> HandleColumnFilters<T>(this IQueryable<T> query, FilterModel[]? filters) where T : class
    {
        if (filters is not { Length: > 0 })
            return query;

        foreach (FilterModel filterModel in filters)
        {
            PropertyInfo propertyInfo = PropertyInfoCache<T>.GetProperty(filterModel.PropertyName);

            Expression<Func<T, bool>>? predicate = filterModel switch
            {
                TextFilter tf => TextExpressionBuilder.Build<T>(propertyInfo, tf.FilterType, tf.SearchValue),
                NumberFilter<int> nif => NumericExpressionBuilder.Build<T, int>(propertyInfo, nif.FilterType, nif.SearchValue),
                NumberFilter<decimal> ndf => NumericExpressionBuilder.Build<T, decimal>(propertyInfo, ndf.FilterType, ndf.SearchValue),
                DateFilter df => DateExpressionBuilder.Build<T>(propertyInfo, df.FilterType, df.SearchValue),
                SingleSelectFilter ssf => SelectExpressionBuilder.BuildSingleSelect<T>(propertyInfo, ssf.SearchValue),
                MultiSelectFilter msf => MultiSelectExpressionBuilder.BuildMultiSelect<T>(propertyInfo, msf.SearchValue),
                _ => null
            };

            if (predicate != null)
                query = query.Where(predicate);
        }
        return query;
    }
}
