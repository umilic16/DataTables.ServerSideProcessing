using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.AspNetCore.Http;

namespace DataTables.ServerSideProcessing.Utils;
public class ResponseBuilder
{
    public static async Task<DataTableResponse<TViewModel>> BuildDataTableResponseAsync<TViewModel>(
        IFormCollection requestForm,
        Func<Task<int>> totalCountFunc,
        Func<string?, Task<int>> filteredCountFunc,
        Func<DataTableRequest, Task<List<TViewModel>>> dataFunc,
        bool parseSort = true,
        bool parseFilters = true)
    {
        if (requestForm == null || requestForm.Count == 0)
            return new DataTableResponse<TViewModel>();

        var totalCount = await totalCountFunc();
        if (totalCount == 0)
            return new DataTableResponse<TViewModel>() { Draw = requestForm["draw"] };

        var response = new DataTableResponse<TViewModel>() { Draw = requestForm["draw"], RecordsTotal = totalCount };

        var request = RequestParser.ParseRequest(requestForm, parseSort, parseFilters);

        var filteredCount = await filteredCountFunc(request.Search);
        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = await dataFunc(request);
        return response;
    }
}
