using System.Globalization;
using System.Numerics;
using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;
using DataTables.ServerSideProcessing.Data.Models.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DataTables.ServerSideProcessing.EFCore;

internal static class RequestParser
{
    internal static Request ParseRequest(IFormCollection requestFormData, bool parseSearch, bool parseSort, bool parseFilters, FilterParsingOptions options)
        => new()
        {
            Search = parseSearch ? requestFormData["search[value]"].ToString() : null,
            Skip = requestFormData["start"].ToInt(),
            PageSize = requestFormData["length"].ToInt(),
            SortOrder = parseSort ? ParseSortOrder(requestFormData) : [],
            Filters = parseFilters ? ParseFilters(requestFormData, options) : []
        };

    private static SortModel[] ParseSortOrder(IFormCollection requestFormData)
    {
        List<SortModel> sortModels = [];
        foreach (string key in requestFormData.Keys)
        {
            if (key.Contains("order[") && key.Contains("dir"))
            {
                int start = key.IndexOf('[') + 1;
                int end = key.IndexOf(']');
                string indexString = key[start..end];
                int index = indexString.ToInt();
                string dirKey = $"order[{index}][dir]";
                int columnIdx = requestFormData[$"order[{index}][column]"].ToInt();
                string nameKey = $"columns[{columnIdx}][data]";
                if (string.IsNullOrEmpty(requestFormData[nameKey]))
                    continue;

                sortModels.Add(new SortModel
                {
                    SortDirection = requestFormData[dirKey] == "asc" ? SortDirection.Ascending : SortDirection.Descending,
                    PropertyName = requestFormData[nameKey]!
                });
            }
        }
        return [.. sortModels];
    }

    private static FilterModel[] ParseFilters(IFormCollection requestFormData, FilterParsingOptions options)
    {
        List<FilterModel> filters = [];
        foreach (string key in requestFormData.Keys)
        {
            if (!key.StartsWith($"{options.Prefix}[") || !key.EndsWith($"{options.FilterTypeKey}]"))
                continue;

            int start = key.IndexOf('[') + 1;
            int end = key.IndexOf(']');
            string propertyName = key[start..end];
            string filterTypeKey = $"{options.Prefix}[{propertyName}][{options.FilterTypeKey}]";
            string filterCategoryKey = $"{options.Prefix}[{propertyName}][{options.FilterCategoryKey}]";
            string valueCategoryKey = $"{options.Prefix}[{propertyName}][{options.ValueCategoryKey}]";
            string valueKey = $"{options.Prefix}[{propertyName}]";
            var searchValue = requestFormData[valueKey].ToString();
            if (string.IsNullOrEmpty(searchValue))
                continue;

            if (!Enum.TryParse(requestFormData[filterCategoryKey], out FilterCategory filterCategory))
                continue;

            if (filterCategory == FilterCategory.Text)
            {
                if (!Enum.TryParse(requestFormData[valueCategoryKey], out TextColumn valueCategory))
                    continue;
                if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                    continue;

                if (valueCategory == TextColumn.AccNumber)
                    searchValue = searchValue.Replace("-", "");

                filters.Add(new TextFilter
                {
                    SearchValue = searchValue,
                    PropertyName = propertyName,
                    FilterType = filterType,
                    ColumnType = valueCategory
                });
            }
            else if (filterCategory == FilterCategory.Numeric)
            {
                if (!Enum.TryParse(requestFormData[valueCategoryKey], out NumericColumn valueCategory))
                    continue;
                if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                    continue;

                if (valueCategory == NumericColumn.Decimal)
                {
                    AddNumericFilter<decimal>(searchValue, propertyName, filterType, filters, options.BetweenSeparator);
                }
                else if (valueCategory == NumericColumn.Int)
                {
                    AddNumericFilter<int>(searchValue, propertyName, filterType, filters, options.BetweenSeparator);
                }
            }
            else if (filterCategory == FilterCategory.Date)
            {
                if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                    continue;

                AddDateFilter(searchValue, propertyName, filterType, filters, options.BetweenSeparator);
            }
            else if (filterCategory == FilterCategory.SingleSelect)
            {
                filters.Add(new SingleSelectFilter
                {
                    SearchValue = searchValue,
                    PropertyName = propertyName
                });
            }
            else if (filterCategory == FilterCategory.MultiSelect)
            {
                var searchValues = searchValue.Split(options.MultiSelectSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (searchValues.Length == 0)
                    continue;

                filters.Add(new MultiSelectFilter
                {
                    SearchValue = searchValues,
                    PropertyName = propertyName
                });
            }
        }
        return [.. filters];
    }

    private static int ToInt(this string value)
    {
        return int.TryParse(value, out int result) ? result : -1;
    }

    private static int ToInt(this StringValues value)
    {
        return int.TryParse(value, out int result) ? result : -1;
    }

    private static void AddNumericFilter<T>(
        string searchValue,
        string propertyName,
        FilterOperations filterType,
        List<FilterModel> filters,
        string betweenSeparator) where T : INumber<T>
    {
        if (filterType != FilterOperations.Between)
        {
            if (!T.TryParse(searchValue, CultureInfo.CurrentCulture, out var parsedValue))
                return;

            filters.Add(new NumberFilter<T>
            {
                SearchValue = parsedValue,
                PropertyName = propertyName,
                FilterType = filterType
            });
        }
        else
        {
            var searchValues = searchValue.Split(betweenSeparator);
            for (int i = 0; i < 2; i++)
            {
                if (string.IsNullOrEmpty(searchValues[i])
                    || !T.TryParse(searchValues[i], CultureInfo.CurrentCulture, out var parsedValue))
                    continue;

                filters.Add(new NumberFilter<T>
                {
                    SearchValue = parsedValue,
                    PropertyName = propertyName,
                    FilterType = i == 0 ? FilterOperations.GreaterThanOrEqual : FilterOperations.LessThanOrEqual
                });
            }
        }
    }

    private static void AddDateFilter(
        string searchValue,
        string propertyName,
        FilterOperations filterType,
        List<FilterModel> filters,
        string betweenSeparator)
    {
        if (filterType != FilterOperations.Between)
        {
            if (!DateOnly.TryParse(searchValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateOnly parsedValue))
                return;

            filters.Add(new DateFilter
            {
                SearchValue = parsedValue,
                PropertyName = propertyName,
                FilterType = filterType
            });
        }
        else
        {
            var searchValues = searchValue.Split(betweenSeparator);
            for (int i = 0; i < 2; i++)
            {
                if (string.IsNullOrEmpty(searchValues[i])
                    || (!DateOnly.TryParse(searchValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateOnly parsedValue)))
                    continue;

                filters.Add(new DateFilter
                {
                    SearchValue = i == 0 ? parsedValue : parsedValue.AddDays(1), // Add 1 day to the end date to make it inclusive
                    PropertyName = propertyName,
                    FilterType = i == 0 ? FilterOperations.GreaterThanOrEqual : FilterOperations.LessThanOrEqual
                });
            }
        }
    }
}
