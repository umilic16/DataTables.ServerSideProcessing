using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;

namespace DataTables.ServerSideProcessing.Utils;
public static class RequestParser
{
    public static IEnumerable<SortModel> ParseSortOrder(Dictionary<string, string> requestFormData)
    {
        foreach (var key in requestFormData.Keys)
        {
            if (key.Contains("order[") && key.Contains("dir"))
            {
                var start = key.IndexOf('[') + 1;
                var end = key.IndexOf(']');
                var indexString = key[start..end];
                var index = indexString.ToInt();
                var dirKey = $"order[{index}][dir]";
                var columnIdx = requestFormData[$"order[{index}][column]"].ToInt();
                var nameKey = $"columns[{columnIdx}][data]";
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

    public static IEnumerable<DataTableFilterBaseModel> ParseFilters(Dictionary<string, string> requestFormData)
    {
        foreach (var key in requestFormData.Keys)
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
                if (!Enum.TryParse(requestFormData[columnFilterTypeKey].ToString(), out ColumnFilterType columnFilterType))
                {
                    columnFilterType = ColumnFilterType.Text;
                }
                if (columnFilterType == ColumnFilterType.Text)
                {
                    if (!Enum.TryParse(requestFormData[columnValueTypeKey].ToString(), out ColumnValueType columnValueType))
                    {
                        columnValueType = ColumnValueType.Base;
                    }
                    if (!Enum.TryParse(requestFormData[filterTypeKey].ToString(), out TextFilter filterType))
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
                    if (!Enum.TryParse(requestFormData[filterTypeKey].ToString(), out NumberFilter filterType))
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
                    yield return new DataTableDateTimeFilterModel
                    {
                        SearchValue = requestFormData[valueKey],
                        PropertyName = propertyName
                    };
                }
            }
        }
    }
}
