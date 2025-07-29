using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.AspNetCore.Http;

namespace DataTables.ServerSideProcessing.Utils;
/// <summary>
/// Provides utility methods for parsing DataTables server-side processing requests.
/// </summary>
public static class RequestParser
{
    /// <summary>
    /// Parses the sort order information from the DataTables request form data.
    /// </summary>
    /// <param name="requestFormData">The form data from the DataTables request.</param>
    /// <returns>An enumerable of <see cref="SortModel"/> representing the sort order.</returns>
    public static IEnumerable<SortModel> ParseSortOrder(IFormCollection requestFormData)
    {
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

                yield return new SortModel
                {
                    SortDirection = requestFormData[dirKey] == "asc" ? SortDirection.Ascending : SortDirection.Descending,
                    PropertyName = requestFormData[nameKey]!
                };
            }
        }
    }

    /// <summary>
    /// Parses filter information from the DataTables request form data.
    /// </summary>
    /// <param name="requestFormData">The form data from the DataTables request.</param>
    /// <param name="multiSelectSeparator">Separator to be used to split values from multi-select filters. Defaults to ",".</param>
    /// <returns>An enumerable of <see cref="DataTableFilterBaseModel"/> representing the column filters.</returns>
    public static IEnumerable<DataTableFilterBaseModel> ParseFilters(IFormCollection requestFormData, string multiSelectSeparator = ",")
    {
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
                    if (!Enum.TryParse(requestFormData[columnValueTypeKey], out ColumnValueType columnValueType))
                    {
                        columnValueType = ColumnValueType.Base;
                    }
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out TextFilter filterType))
                    {
                        filterType = TextFilter.Contains;
                    }
                    yield return new DataTableTextFilterModel
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName,
                        FilterType = filterType,
                        ColumnType = columnValueType
                    };
                }
                else if (columnFilterType == ColumnFilterType.Number)
                {
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out NumberFilter filterType))
                    {
                        filterType = NumberFilter.Equals;
                    }
                    yield return new DataTableNumberFilterModel
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName,
                        FilterType = filterType
                    };
                }
                else if (columnFilterType == ColumnFilterType.Date)
                {
                    if (!Enum.TryParse(requestFormData[filterTypeKey], out NumberFilter filterType))
                    {
                        filterType = NumberFilter.Equals;
                    }
                    yield return new DataTableDateTimeFilterModel
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName,
                        FilterType = filterType
                    };
                }
                else if (columnFilterType == ColumnFilterType.SingleSelect)
                {
                    yield return new DataTableSingleSelectFilterModel
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName
                    };
                }
                else if (columnFilterType == ColumnFilterType.MultiSelect)
                {
                    yield return new DataTableMultiSelectFilterModel
                    {
                        SearchValue = [.. requestFormData[valueKey].ToString().Split(multiSelectSeparator)],
                        PropertyName = propertyName
                    };
                }
            }
        }
    }

    /// <summary>
    /// Parses a DataTables request from form data, including search, pagination, and optionally sorting and filters.
    /// </summary>
    /// <param name="requestFormData">The form data from the DataTables request.</param>
    /// <param name="parseSort">Whether to parse sort order information.</param>
    /// <param name="parseFilters">Whether to parse column filter information.</param>
    /// <returns>A <see cref="DataTableRequest"/> object representing the parsed request.</returns>
    public static DataTableRequest ParseRequest(IFormCollection requestFormData, bool parseSort = true, bool parseFilters = true)
    {
        return new DataTableRequest
        {
            Search = requestFormData["search[value]"],
            Skip = requestFormData["start"].ToInt(),
            PageSize = requestFormData["length"].ToInt(),
            SortOrder = parseSort ? ParseSortOrder(requestFormData) : [],
            Filters = parseFilters ? ParseFilters(requestFormData) : []
        };
    }
}
