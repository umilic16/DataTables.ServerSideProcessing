using System.Linq.Expressions;
using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore;
/// <summary>
/// Provides utility methods for building DataTables-compatible responses for server-side processing using Entity Framework Core.
/// Supports both synchronous and asynchronous operations, with optional filtering, sorting, and mapping.
/// </summary>
public static class EfResponseBuilder
{
    /// <summary>
    /// Asynchronously builds a <see cref="DataTableResponse{TEntity}"/> for the specified entity type.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="requestForm">The form collection containing DataTables request parameters.</param>
    /// <param name="entities">The queryable source of entities.</param>
    /// <param name="filterableFields">Optional list of fields to apply global search filtering.</param>
    /// <param name="parseSort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="parseFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="DataTableResponse{TEntity}"/>
    /// containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the request form is empty.</exception>
    public static async Task<DataTableResponse<TEntity>> BuildDataTableResponseAsync<TEntity>(
        IFormCollection requestForm,
        IQueryable<TEntity> entities,
        IEnumerable<string>? filterableFields = null,
        bool parseSort = true,
        bool parseFilters = true,
        CancellationToken ct = default)
        where TEntity : class
    {
        if (requestForm.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(requestForm));

        int totalCount = await entities.CountAsync(ct);
        if (totalCount == 0)
            return new DataTableResponse<TEntity>() { Draw = requestForm["draw"] };

        var response = new DataTableResponse<TEntity>() { Draw = requestForm["draw"], RecordsTotal = totalCount };

        DataTableRequest request = RequestParser.ParseRequest(requestForm, parseSort, parseFilters);

        int filteredCount = await entities.HandleGenericFilter(filterableFields, request.Search)
                                          .HandleColumnFilters(request.Filters)
                                          .CountAsync(ct);

        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = await entities.HandleGenericFilter(filterableFields, request.Search)
                                      .HandleColumnFilters(request.Filters)
                                      .HandleSorting(request.SortOrder)
                                      .ExecuteQueryAsync(request.Skip, request.PageSize, ct);
        return response;
    }

    /// <summary>
    /// Asynchronously builds a <see cref="DataTableResponse{TViewModel}"/> for the specified entity type and maps the result to a view model.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TViewModel">The view model type to map the entity to.</typeparam>
    /// <param name="requestForm">The form collection containing DataTables request parameters.</param>
    /// <param name="entities">The queryable source of entities.</param>
    /// <param name="selector">An expression to map the entity to the view model.</param>
    /// <param name="filterableFields">Optional list of fields to apply global search filtering.</param>
    /// <param name="parseSort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="parseFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="DataTableResponse{TViewModel}"/>
    /// containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the request form is empty.</exception>
    public static async Task<DataTableResponse<TViewModel>> BuildDataTableResponseAsync<TEntity, TViewModel>(
        IFormCollection requestForm,
        IQueryable<TEntity> entities,
        Expression<Func<TEntity, TViewModel>> selector,
        IEnumerable<string>? filterableFields = null,
        bool parseSort = true,
        bool parseFilters = true,
        CancellationToken ct = default)
        where TEntity : class
        where TViewModel : class
    {
        if (requestForm.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(requestForm));

        int totalCount = await entities.CountAsync(ct);
        if (totalCount == 0)
            return new DataTableResponse<TViewModel>() { Draw = requestForm["draw"] };

        var response = new DataTableResponse<TViewModel>() { Draw = requestForm["draw"], RecordsTotal = totalCount };

        DataTableRequest request = RequestParser.ParseRequest(requestForm, parseSort, parseFilters);

        int filteredCount = await entities.HandleGenericFilter(filterableFields, request.Search)
                                          .Select(selector)
                                          .HandleColumnFilters(request.Filters)
                                          .CountAsync(ct);

        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = await entities.HandleGenericFilter(filterableFields, request.Search)
                                      .Select(selector)
                                      .HandleColumnFilters(request.Filters)
                                      .HandleSorting(request.SortOrder)
                                      .ExecuteQueryAsync(request.Skip, request.PageSize, ct);
        return response;
    }


    /// <summary>
    /// Asynchronously builds a <see cref="DataTableResponse{TEntity}"/> for the specified entity type.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="requestForm">The form collection containing DataTables request parameters.</param>
    /// <param name="entities">The queryable source of entities.</param>
    /// <param name="filterableFields">Optional list of fields to apply global search filtering.</param>
    /// <param name="parseSort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="parseFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <returns>
    /// A <see cref="DataTableResponse{TEntity}"/> containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the request form is empty.</exception>
    public static DataTableResponse<TEntity> BuildDataTableResponse<TEntity>(
        IFormCollection requestForm,
        IQueryable<TEntity> entities,
        IEnumerable<string>? filterableFields = null,
        bool parseSort = true,
        bool parseFilters = true)
        where TEntity : class
    {
        if (requestForm.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(requestForm));

        int totalCount = entities.Count();
        if (totalCount == 0)
            return new DataTableResponse<TEntity>() { Draw = requestForm["draw"] };

        var response = new DataTableResponse<TEntity>() { Draw = requestForm["draw"], RecordsTotal = totalCount };

        DataTableRequest request = RequestParser.ParseRequest(requestForm, parseSort, parseFilters);

        int filteredCount = entities.HandleGenericFilter(filterableFields, request.Search)
                                    .HandleColumnFilters(request.Filters)
                                    .Count();
        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = entities.HandleGenericFilter(filterableFields, request.Search)
                                .HandleColumnFilters(request.Filters)
                                .HandleSorting(request.SortOrder)
                                .ExecuteQuery(request.Skip, request.PageSize);
        return response;
    }

    /// <summary>
    /// Asynchronously builds a <see cref="DataTableResponse{TViewModel}"/> for the specified entity type and maps the result to a view model.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TViewModel">The view model type to map the entity to.</typeparam>
    /// <param name="requestForm">The form collection containing DataTables request parameters.</param>
    /// <param name="entities">The queryable source of entities.</param>
    /// <param name="selector">An expression to map the entity to the view model.</param>
    /// <param name="filterableFields">Optional list of fields to apply global search filtering.</param>
    /// <param name="parseSort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="parseFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <returns>
    /// A <see cref="DataTableResponse{TViewModel}"/> containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the request form is empty.</exception>
    public static DataTableResponse<TViewModel> BuildDataTableResponse<TEntity, TViewModel>(
        IFormCollection requestForm,
        IQueryable<TEntity> entities,
        Expression<Func<TEntity, TViewModel>> selector,
        IEnumerable<string>? filterableFields = null,
        bool parseSort = true,
        bool parseFilters = true)
        where TEntity : class
        where TViewModel : class
    {
        if (requestForm.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(requestForm));

        int totalCount = entities.Count();
        if (totalCount == 0)
            return new DataTableResponse<TViewModel>() { Draw = requestForm["draw"] };

        var response = new DataTableResponse<TViewModel>() { Draw = requestForm["draw"], RecordsTotal = totalCount };

        DataTableRequest request = RequestParser.ParseRequest(requestForm, parseSort, parseFilters);

        int filteredCount = entities.HandleGenericFilter(filterableFields, request.Search)
                                    .Select(selector)
                                    .HandleColumnFilters(request.Filters)
                                    .Count();

        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = entities.HandleGenericFilter(filterableFields, request.Search)
                                .Select(selector)
                                .HandleColumnFilters(request.Filters)
                                .HandleSorting(request.SortOrder)
                                .ExecuteQuery(request.Skip, request.PageSize);
        return response;
    }
}
