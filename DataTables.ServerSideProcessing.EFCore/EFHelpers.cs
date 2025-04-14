using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore;
public static class EFHelpers
{
    public static IQueryable<T> ParseFiltersIntoQuery<T>(IEnumerable<DataTableFilterBaseModel> filters, IQueryable<T> query) where T : class
    {
        foreach (DataTableFilterBaseModel filterModel in filters)
        {
            if (!ReflectionCache<T>.Properties.TryGetValue(filterModel.PropertyName, out string? propName))
                continue;

            if (filterModel is DataTableTextFilterModel filterTextModel)
            {
                if (string.IsNullOrEmpty(filterTextModel.SearchValue))
                    continue;

                if (filterTextModel.ColumnType == ColumnValueType.AccNumber)
                    filterTextModel.SearchValue = filterTextModel.SearchValue.Replace("-", "");

                var predicate = BuildTextWhereExpression<T>(
                                    propName,
                                    filterTextModel.FilterType,
                                    filterTextModel.SearchValue);

                if (predicate != null)
                    query = query.Where(predicate);
            }
            else if (filterModel is DataTableNumberFilterModel filterNumberModel)
            {
                if (string.IsNullOrEmpty(filterNumberModel.SearchValue))
                    continue;

                if (filterNumberModel.ColumnType == ColumnValueType.Decimal)
                    filterNumberModel.SearchValue = filterNumberModel.SearchValue.Replace(".", "");

                var predicate = BuildNumericWhereExpression<T>(
                                    propName,
                                    filterNumberModel.FilterType,
                                    filterNumberModel.SearchValue);

                if (predicate != null)
                    query = query.Where(predicate);
            }
            else if (filterModel is DataTableDateTimeFilterModel filterDateModel)
            {
                if (string.IsNullOrEmpty(filterDateModel.SearchValue))
                    continue;

                if (!DateTime.TryParseExact(filterDateModel.SearchValue, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime datumParsed))
                    continue;

                var predicate = BuildDateWhereExpression<T>(
                                    propName,
                                    datumParsed);

                if (predicate != null)
                    query = query.Where(predicate);
            }
        }
        return query;
    }

    public static IQueryable<T> HandleSorting<T>(IEnumerable<SortModel> sortOrder, IQueryable<T> query) where T : class
    {
        bool isFirstFlag = true;
        foreach (var sortModel in sortOrder)
        {
            if (ReflectionCache<T>.Properties.TryGetValue(sortModel.PropertyName, out string? propName))
            {
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
        }
        return query;
    }

    public async static Task<List<T>> ExecuteQuery<T>(IQueryable<T> query, int skip, int pageSize, CancellationToken ct) where T : class
    {
        return pageSize != -1 ? await query.Skip(skip).Take(pageSize).ToListAsync(ct) : await query.ToListAsync(ct);
    }

    private static Expression<Func<T, bool>>? BuildDateWhereExpression<T>(string propertyName, DateTime searchValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        var result = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Date);
        if (!result.HasValue)
            return null;

        (MemberExpression memberAccess, Expression constantValue) = result.Value;
        Expression comparison;
        try
        {
            comparison = Expression.Equal(memberAccess, constantValue);
        }
        catch (Exception)
        {
            return null;
        }

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static Expression<Func<T, bool>>? BuildNumericWhereExpression<T>(string propertyName, NumberFilter? numberFilterType, string searchValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        var result = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Number);
        if (!result.HasValue)
            return null;

        (MemberExpression memberAccess, Expression constantValue) = result.Value;
        Expression? comparison;
        try
        {
            comparison = numberFilterType switch
            {
                NumberFilter.Equals => Expression.Equal(memberAccess, constantValue),
                NumberFilter.NotEqual => Expression.NotEqual(memberAccess, constantValue),
                NumberFilter.GreaterThan => Expression.GreaterThan(memberAccess, constantValue),
                NumberFilter.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, constantValue),
                NumberFilter.LessThan => Expression.LessThan(memberAccess, constantValue),
                NumberFilter.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, constantValue),
                _ => null
            };
        }
        catch (Exception)
        {
            return null;
        }

        return comparison != null ? Expression.Lambda<Func<T, bool>>(comparison, parameter) : null;
    }

    private static Expression<Func<T, bool>>? BuildTextWhereExpression<T>(string propertyName, TextFilter? textFilterType, string searchValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        var result = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Text);
        if (!result.HasValue)
            return null;

        (MemberExpression memberAccess, Expression constantValue) = result.Value;
        Expression? comparison;
        try
        {
            comparison = textFilterType switch
            {
                TextFilter.Equals => Expression.Equal(memberAccess, constantValue),
                TextFilter.NotEqual => Expression.NotEqual(memberAccess, constantValue),
                TextFilter.Contains => Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue),
                TextFilter.DoesntContain => Expression.Not(Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue)),
                TextFilter.StartsWith => Expression.Call(memberAccess, typeof(string).GetMethod("StartsWith", [typeof(string)])!, constantValue),
                TextFilter.EndsWith => Expression.Call(memberAccess, typeof(string).GetMethod("EndsWith", [typeof(string)])!, constantValue),
                _ => null
            };
        }
        catch (Exception)
        {
            return null;
        }

        return comparison != null ? Expression.Lambda<Func<T, bool>>(comparison, parameter) : null;
    }

    private static (MemberExpression memberAccess, Expression constantValue)? PrepareExpressionData<T>(ParameterExpression parameter, string propertyName, object searchValue, ColumnFilterType columnType)
    {
        MemberExpression memberAccess;
        PropertyInfo? propertyInfo;

        // --- Get Property Info ---
        try
        {
            propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
                return null;

            memberAccess = Expression.Property(parameter, propertyInfo);
        }
        catch (Exception)
        {
            return null;
        }

        // --- Prepare the Constant Value (Matching Property Type) ---
        Expression constantValue;
        try
        {
            // Get the actual type of the property (e.g., int, double?, decimal)
            Type propertyType = propertyInfo.PropertyType;
            // Get the underlying type if it's nullable (e.g., int from int?)
            Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            // Check if the underlying type is numeric (you might want to refine this check)
            if (columnType == ColumnFilterType.Number && !IsNumericType(underlyingType))
                return null;

            if (columnType == ColumnFilterType.Date && underlyingType != typeof(DateTime))
                return null;

            if (columnType == ColumnFilterType.Text && underlyingType != typeof(string))
                return null;

            // Convert the input 'searchValue' (decimal) to the property's underlying type
            object convertedValue = Convert.ChangeType(searchValue, underlyingType);

            // Create a constant expression using the converted value BUT typed as the *original* property type (including Nullable<>)
            constantValue = Expression.Constant(convertedValue, propertyType);
        }
        catch (Exception)
        {
            return null;
        }

        return (memberAccess, constantValue);
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) ||
               type == typeof(long) || type == typeof(short);
    }
}
