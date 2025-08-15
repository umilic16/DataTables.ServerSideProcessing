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
    /// <returns>An array of <see cref="FilterModel"/> representing the column filters.</returns>
    public static FilterModel[] ParseFilters(IFormCollection requestFormData, string multiSelectSeparator = ",")
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
                if (!Enum.TryParse(requestFormData[columnFilterTypeKey], out ColumnFilterType columnFilterType))
                {
                    columnFilterType = ColumnFilterType.Text;
                }
                if (columnFilterType == ColumnFilterType.Text)
                {
                    if (!Enum.TryParse(requestFormData[columnValueTypeKey], out ColumnValueTextType columnValueType))
                    {
                        columnValueType = ColumnValueTextType.Base;
                    }
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                    {
                        filterType = FilterOperations.Contains;
                    }
                    filters.Add(new TextFilter
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName,
                        FilterType = filterType,
                        ColumnType = columnValueType
                    });
                }
                else if (columnFilterType == ColumnFilterType.Number)
                {
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                    {
                        filterType = FilterOperations.Equals;
                    }
                    filters.Add(new NumberFilter
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName,
                        FilterType = filterType
                    });
                }
                else if (columnFilterType == ColumnFilterType.Date)
                {
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out FilterOperations filterType))
                    {
                        filterType = FilterOperations.Equals;
                    }
                    filters.Add(new DateTimeFilter
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName,
                        FilterType = filterType
                    });
                }
                else if (columnFilterType == ColumnFilterType.SingleSelect)
                {
                    filters.Add(new SingleSelectFilter
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName
                    });
                }
                else if (columnFilterType == ColumnFilterType.MultiSelect)
                {
                    filters.Add(new MultiSelectFilter
                    {
                        SearchValue = [.. requestFormData[valueKey].ToString().Split(multiSelectSeparator)],
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
    /// <returns>A <see cref="Request"/> object representing the parsed request.</returns>
    public static Request ParseRequest(IFormCollection requestFormData, bool parseSort = true, bool parseFilters = true)
    {
        return new Request
        {
            Search = requestFormData["search[value]"],
            Skip = requestFormData["start"].ToInt(),
            PageSize = requestFormData["length"].ToInt(),
            SortOrder = parseSort ? ParseSortOrder(requestFormData) : [],
            Filters = parseFilters ? ParseFilters(requestFormData) : []
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
}
