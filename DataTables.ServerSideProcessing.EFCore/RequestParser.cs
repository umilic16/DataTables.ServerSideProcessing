using System.Globalization;
using System.Numerics;
using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.Data.Models.Abstractions;
using DataTables.ServerSideProcessing.Data.Models.Filter;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DataTables.ServerSideProcessing.EFCore;
/// <summary>
/// Provides utility methods for parsing DataTables server-side processing requests.
/// </summary>
public static class RequestParser
{
    /// <summary>
    /// Parses the sort order information from the DataTables request form data.
    /// </summary>
    /// <param name="requestFormData">The form data from the DataTables request.</param>
    /// <returns>An array of <see cref="SortModel"/> representing the sort order.</returns>
    public static SortModel[] ParseSortOrder(IFormCollection requestFormData)
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

    /// <summary>
    /// Parses filter information from the DataTables request form data.
    /// </summary>
    /// <param name="requestFormData">The form data from the DataTables request.</param>
    /// <param name="multiSelectSeparator">Separator to be used to split values from multi-select filters. Defaults to ",".</param>
    /// <param name="betweenSeparator">Separator to be used for 'Between' filters. Defaults to ";".</param>
    /// <returns>An array of <see cref="FilterModel"/> representing the column filters.</returns>
    public static FilterModel[] ParseFilters(IFormCollection requestFormData, string multiSelectSeparator = ",", string betweenSeparator = ";")
    {
        List<FilterModel> filters = [];
        foreach (string key in requestFormData.Keys)
        {
            if (key.Contains("filter[") && key.Contains("columnFilterType"))
            {
                int start = key.IndexOf('[') + 1;
                int end = key.IndexOf(']');
                string propertyName = key[start..end];
                string filterTypeKey = $"filter[{propertyName}][filterType]";
                string columnFilterTypeKey = $"filter[{propertyName}][columnFilterType]";
                string columnValueTypeKey = $"filter[{propertyName}][columnValueType]";
                string valueKey = $"filter[{propertyName}]";
                var searchValue = requestFormData[valueKey].ToString();
                if (string.IsNullOrEmpty(searchValue))
                    continue;

                if (!Enum.TryParse(requestFormData[columnFilterTypeKey], out ColumnFilterType columnFilterType))
                    continue;

                if (columnFilterType == ColumnFilterType.Text)
                {
                    if (!Enum.TryParse(requestFormData[columnValueTypeKey], out ColumnValueTextType columnValueType))
                        continue;
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                        continue;

                    if (columnValueType == ColumnValueTextType.AccNumber)
                        searchValue = searchValue.Replace("-", "");

                    filters.Add(new TextFilter
                    {
                        SearchValue = searchValue,
                        PropertyName = propertyName,
                        FilterType = filterType,
                        ColumnType = columnValueType
                    });
                }
                else if (columnFilterType == ColumnFilterType.Number)
                {
                    if (!Enum.TryParse(requestFormData[columnValueTypeKey], out ColumnValueNumericType columnValueType))
                        continue;
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                        continue;

                    if (columnValueType == ColumnValueNumericType.Decimal)
                    {
                        AddNumericFilter<decimal>(searchValue, propertyName, filterType, filters, betweenSeparator);
                    }
                    else if (columnValueType == ColumnValueNumericType.Int)
                    {
                        AddNumericFilter<int>(searchValue, propertyName, filterType, filters, betweenSeparator);
                    }
                }
                else if (columnFilterType == ColumnFilterType.Date)
                {
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                        continue;

                    AddDateFilter(searchValue, propertyName, filterType, filters, betweenSeparator);
                }
                else if (columnFilterType == ColumnFilterType.SingleSelect)
                {
                    filters.Add(new SingleSelectFilter
                    {
                        SearchValue = searchValue,
                        PropertyName = propertyName
                    });
                }
                else if (columnFilterType == ColumnFilterType.MultiSelect)
                {
                    var searchValues = searchValue.Split(multiSelectSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (searchValues.Length == 0)
                        continue;

                    filters.Add(new MultiSelectFilter
                    {
                        SearchValue = searchValues,
                        PropertyName = propertyName
                    });
                }
            }
        }
        return [.. filters];
    }

    /// <summary>
    /// Parses a DataTables request from form data, including search, pagination, and optionally sorting and filters.
    /// </summary>
    /// <param name="requestFormData">The form data from the DataTables request.</param>
    /// <param name="parseSort">Whether to parse sort order information.</param>
    /// <param name="parseFilters">Whether to parse column filter information.</param>
    /// <param name="multiSelectSeparator">Separator to be used to split values from multi-select filters. Defaults to ",".</param>
    /// <param name="betweenSeparator">Separator to be used for 'Between' filters. Defaults to ";".</param>
    /// <returns>A <see cref="Request"/> object representing the parsed request.</returns>
    public static Request ParseRequest(
        IFormCollection requestFormData,
        bool parseSort = true,
        bool parseFilters = true,
        string multiSelectSeparator = ",",
        string betweenSeparator = ";")
    {
        return new Request
        {
            Search = requestFormData["search[value]"],
            Skip = requestFormData["start"].ToInt(),
            PageSize = requestFormData["length"].ToInt(),
            SortOrder = parseSort ? ParseSortOrder(requestFormData) : [],
            Filters = parseFilters ? ParseFilters(requestFormData, multiSelectSeparator, betweenSeparator) : []
        };
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
