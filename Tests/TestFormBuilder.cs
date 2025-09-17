using DataTables.ServerSideProcessing.Data.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Tests;

public class TestFormBuilder
{
    private readonly Dictionary<string, StringValues> _data;
    private int _columnIndex = 0;

    private TestFormBuilder()
    {
        _data = [];
        _data["draw"] = "1";
    }

    public static TestFormBuilder Create()
        => new();


    public TestFormBuilder WithPaging(int start = 0, int length = 10)
    {
        _data["start"] = start.ToString();
        _data["length"] = length.ToString();
        return this;
    }

    public TestFormBuilder WithGlobalSearch(string value)
    {
        _data["search[value]"] = value;
        return this;
    }

    public TestFormBuilder AddColumn(string property, BaseFilterModel? filter = null, SortDirection? sortDirection = null)
    {
        var i = _columnIndex++;
        // property
        _data[$"columns[{i}][data]"] = property;

        // column filter
        if (filter is not null)
        {
            _data[$"{filter.Options.Prefix}[{property}][{filter.Options.FilterCategoryKey}]"] = ((int)filter.FilterCategory).ToString();

            if (filter is NumericFilterModel numModel)
            {
                _data[$"{filter.Options.Prefix}[{property}][{filter.Options.FilterTypeKey}]"] = ((int)numModel.FilterType).ToString();
                _data[$"{filter.Options.Prefix}[{property}][{filter.Options.ValueCategoryKey}]"] = ((int)numModel.NumericCategory).ToString();
            }
            else if (filter is TextFilterModel textModel)
            {
                _data[$"{filter.Options.Prefix}[{property}][{filter.Options.FilterTypeKey}]"] = ((int)textModel.FilterType).ToString();
                _data[$"{filter.Options.Prefix}[{property}][{filter.Options.ValueCategoryKey}]"] = ((int)textModel.TextCategory!).ToString();
            }
            else if (filter is DateFilterModel dateModel)
            {
                _data[$"{filter.Options.Prefix}[{property}][{filter.Options.FilterTypeKey}]"] = ((int)dateModel.FilterType).ToString();
            }

            string valueKey = $"{filter.Options.Prefix}[{property}]";
            _data[valueKey] = filter.SearchValue;
        }

        // sorting
        if (sortDirection.HasValue)
        {
            _data[$"order[{i}][column]"] = i.ToString();
            _data[$"order[{i}][dir]"] = sortDirection == SortDirection.Ascending ? "asc" : "desc";
        }
        return this;
    }

    public IFormCollection Build() => new FormCollection(_data);
}
