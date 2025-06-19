using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.AspNetCore.Http;

namespace DataTables.ServerSideProcessing.Utils;
/// <summary>
/// Provides utility methods for building DataTables-compatible responses for server-side processing.
/// </summary>
public static class ResponseBuilder
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
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="DataTableResponse{TViewModel}"/>
    /// containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Throws <see cref="ArgumentException"/> if the request form is empty.
    /// </exception>
    public static async Task<DataTableResponse<TViewModel>> BuildDataTableResponseAsync<TViewModel>(
        IFormCollection requestForm,
        Func<Task<int>> totalCountFunc,
        Func<string?, Task<int>> filteredCountFunc,
        Func<DataTableRequest, Task<List<TViewModel>>> dataFunc,
        bool parseSort = true,
        bool parseFilters = true)
    {
        if (requestForm.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(requestForm));

        int totalCount = await totalCountFunc();
        if (totalCount == 0)
            return new DataTableResponse<TViewModel>() { Draw = requestForm["draw"] };

        var response = new DataTableResponse<TViewModel>() { Draw = requestForm["draw"], RecordsTotal = totalCount };

        DataTableRequest request = RequestParser.ParseRequest(requestForm, parseSort, parseFilters);

        int filteredCount = await filteredCountFunc(request.Search);
        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = await dataFunc(request);
        return response;
    }

    /// <summary>
    /// Builds a <see cref="DataTableResponse{TViewModel}"/> synchronously based on the provided request form and data retrieval functions.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the data items to be returned in the response.</typeparam>
    /// <param name="requestForm">The form collection containing DataTables request parameters.</param>
    /// <param name="totalCountFunc">A function that asynchronously returns the total count of records (before filtering).</param>
    /// <param name="filteredCountFunc">A function that asynchronously returns the count of records after filtering, given a search string.</param>
    /// <param name="dataFunc">A function that asynchronously returns the data items for the current page, based on the parsed request.</param>
    /// <param name="parseSort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="parseFilters">Indicates whether to parse filter information from the request form. Default is true.</param>
    /// <returns>
    /// A <see cref="DataTableResponse{TViewModel}"/> containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Throws <see cref="ArgumentException"/> if the request form is empty.
    /// </exception>
    public static DataTableResponse<TViewModel> BuildDataTableResponse<TViewModel>(
        IFormCollection requestForm,
        Func<int> totalCountFunc,
        Func<string?, int> filteredCountFunc,
        Func<DataTableRequest, List<TViewModel>> dataFunc,
        bool parseSort = true,
        bool parseFilters = true)
    {
        if (requestForm.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(requestForm));

        int totalCount = totalCountFunc();
        if (totalCount == 0)
            return new DataTableResponse<TViewModel>() { Draw = requestForm["draw"] };

        var response = new DataTableResponse<TViewModel>() { Draw = requestForm["draw"], RecordsTotal = totalCount };

        DataTableRequest request = RequestParser.ParseRequest(requestForm, parseSort, parseFilters);

        int filteredCount = filteredCountFunc(request.Search);
        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = dataFunc(request);
        return response;
    }
}
