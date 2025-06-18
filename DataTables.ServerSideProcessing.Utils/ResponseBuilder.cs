using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.AspNetCore.Http;

namespace DataTables.ServerSideProcessing.Utils;
/// <summary>
/// Provides utility method for building DataTables-compatible responses for server-side processing.
/// </summary>
public class ResponseBuilder
{
    /// <summary>
    /// Builds a <see cref="DataTableResponse{TViewModel}"/> asynchronously based on the provided request form and data retrieval functions.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the data items to be returned in the response.</typeparam>
    /// <param name="requestForm">The form collection containing DataTables request parameters.</param>
    /// <param name="totalCountFunc">A function that asynchronously returns the total count of records (before filtering).</param>
    /// <param name="filteredCountFunc">A function that asynchronously returns the count of records after filtering, given a search string.</param>
    /// <param name="dataFunc">A function that asynchronously returns the data items for the current page, based on the parsed request.</param>
    /// <param name="parseSort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="parseFilters">Indicates whether to parse filter information from the request form. Default is true.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="DataTableResponse{TViewModel}"/> containing the requested data and metadata.
    /// </returns>
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
