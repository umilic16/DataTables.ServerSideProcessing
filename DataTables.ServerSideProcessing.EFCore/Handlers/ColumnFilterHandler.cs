using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataTables.ServerSideProcessing.EFCore.Handlers;
internal static class ColumnFilterHandler
{

    internal static IQueryable<T> HandleColumnFilters<T>(IEnumerable<DataTableFilterBaseModel> filters, IQueryable<T> query) where T : class
    {
        foreach (DataTableFilterBaseModel filterModel in filters)
        {
            if (!ReflectionCache<T>.Properties.TryGetValue(filterModel.PropertyName, out string? propName))
                continue;

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

    internal static Expression<Func<T, bool>> BuildDateWhereExpression<T>(string propertyName, DateTime searchValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        (MemberExpression memberAccess, Expression constantValue) = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Date);

        Expression comparison = Expression.Equal(memberAccess, constantValue);

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    internal static Expression<Func<T, bool>> BuildNumericWhereExpression<T>(string propertyName, NumberFilter numberFilterType, string searchValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        (MemberExpression memberAccess, Expression constantValue) = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Number);

        Expression comparison = numberFilterType switch
        {
            NumberFilter.Equals => Expression.Equal(memberAccess, constantValue),
            NumberFilter.NotEqual => Expression.NotEqual(memberAccess, constantValue),
            NumberFilter.GreaterThan => Expression.GreaterThan(memberAccess, constantValue),
            NumberFilter.GreaterThanOrEqual => Expression.GreaterThanOrEqual(memberAccess, constantValue),
            NumberFilter.LessThan => Expression.LessThan(memberAccess, constantValue),
            NumberFilter.LessThanOrEqual => Expression.LessThanOrEqual(memberAccess, constantValue),
            _ => throw new NotImplementedException(),
        };

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    internal static Expression<Func<T, bool>> BuildTextWhereExpression<T>(string propertyName, TextFilter? textFilterType, string searchValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "e"); // "e"
        (MemberExpression memberAccess, Expression constantValue) = PrepareExpressionData<T>(parameter, propertyName, searchValue, ColumnFilterType.Text);

        Expression comparison = textFilterType switch
        {
            TextFilter.Equals => Expression.Equal(memberAccess, constantValue),
            TextFilter.NotEqual => Expression.NotEqual(memberAccess, constantValue),
            TextFilter.Contains => Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue),
            TextFilter.DoesntContain => Expression.Not(Expression.Call(memberAccess, typeof(string).GetMethod("Contains", [typeof(string)])!, constantValue)),
            TextFilter.StartsWith => Expression.Call(memberAccess, typeof(string).GetMethod("StartsWith", [typeof(string)])!, constantValue),
            TextFilter.EndsWith => Expression.Call(memberAccess, typeof(string).GetMethod("EndsWith", [typeof(string)])!, constantValue),
            _ => throw new NotImplementedException(),
        };

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    internal static (MemberExpression memberAccess, Expression constantValue) PrepareExpressionData<T>(ParameterExpression parameter, string propertyName, object searchValue, ColumnFilterType columnType)
    {
        PropertyInfo? propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        MemberExpression memberAccess = Expression.Property(parameter, propertyInfo);

        // Get the actual type of the property (e.g., int, double?, decimal)
        Type propertyType = propertyInfo.PropertyType;
        // Get the underlying type if it's nullable (e.g., int from int?)
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (columnType == ColumnFilterType.Number && !IsNumericType(underlyingType))
            throw new InvalidOperationException($"Property '{propertyName}' is not a numeric type.");

        if (columnType == ColumnFilterType.Date && underlyingType != typeof(DateTime))
            throw new InvalidOperationException($"Property '{propertyName}' is not a DateTime type.");

        if (columnType == ColumnFilterType.Text && underlyingType != typeof(string))
            throw new InvalidOperationException($"Property '{propertyName}' is not a string type.");

        // Convert the input 'searchValue' to the property's underlying type
        object convertedValue = Convert.ChangeType(searchValue, underlyingType);

        // Create a constant expression using the converted value BUT typed as the *original* property type (including Nullable<>)
        Expression constantValue = Expression.Constant(convertedValue, propertyType);
        return (memberAccess, constantValue);
    }

    internal static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) ||
               type == typeof(long) || type == typeof(short);
    }
}
