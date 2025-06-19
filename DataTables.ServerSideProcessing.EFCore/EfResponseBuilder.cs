using System.Linq.Expressions;
using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore;
/// <summary>
/// Provides utility methods for building DataTables-compatible responses for server-side processing using Entity Framework Core.
/// Supports synchronous and asynchronous overloads, with optional global search, column filters, sorting, pagination, and projection.
/// </summary>
public static class EfResponseBuilder
{
    /// <summary>
    /// Asynchronously builds a <see cref="DataTableResponse{TViewModel}"/> for the specified entity type and maps the result to a view model.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TViewModel">The view model type to map the entity to.</typeparam>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="projection">An expression to map the entity to the view model.</param>
    /// <param name="genericFilterFields">Optional list of fields to apply global search filtering.</param>
    /// <param name="applySort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="applyColumnFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="DataTableResponse{TViewModel}"/>
    /// containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="form"/>, <paramref name="query"/>, or <paramref name="projection"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="form"/> contains no entries.</exception>
    public static async Task<DataTableResponse<TViewModel>> BuildDataTableResponseAsync<TEntity, TViewModel>(
        IFormCollection form,
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TViewModel>> projection,
        IEnumerable<string>? genericFilterFields = null,
        bool applySort = true,
        bool applyColumnFilters = true,
        CancellationToken ct = default)
        where TEntity : class
        where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(projection);

        if (form.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(form));

        int totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
            return new DataTableResponse<TViewModel>() { Draw = form["draw"] };

        var response = new DataTableResponse<TViewModel>() { Draw = form["draw"], RecordsTotal = totalCount };

        DataTableRequest request = RequestParser.ParseRequest(form, applySort, applyColumnFilters);

        IQueryable<TViewModel> baseQuery = query.HandleGenericFilter(genericFilterFields, request.Search)
                                                .Select(projection)
                                                .HandleColumnFilters(request.Filters);

        int filteredCount = await baseQuery.CountAsync(ct);

        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = await baseQuery.HandleSorting(request.SortOrder)
                                       .ExecuteQueryAsync(request.Skip, request.PageSize, ct);
        return response;
    }

    /// <summary>
    /// Asynchronously builds a <see cref="DataTableResponse{TEntity}"/> for the specified entity type.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="genericFilterFields">Optional list of fields to apply global search filtering.</param>
    /// <param name="applySort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="applyColumnFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="DataTableResponse{TEntity}"/>
    /// containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="form"/> or <paramref name="query"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="form"/> contains no entries.</exception>
    public static async Task<DataTableResponse<TEntity>> BuildDataTableResponseAsync<TEntity>(
        IFormCollection form,
        IQueryable<TEntity> query,
        IEnumerable<string>? genericFilterFields = null,
        bool applySort = true,
        bool applyColumnFilters = true,
        CancellationToken ct = default)
        where TEntity : class
    {

        return await BuildDataTableResponseAsync(form, query, x => x, genericFilterFields, applySort, applyColumnFilters, ct);
    }

    /// <summary>
    /// Asynchronously builds a <see cref="DataTableResponse{TViewModel}"/> for the specified entity type and maps the result to a view model.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TViewModel">The view model type to map the entity to.</typeparam>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="projection">An expression to map the entity to the view model.</param>
    /// <param name="genericFilterFields">Optional list of fields to apply global search filtering.</param>
    /// <param name="applySort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="applyColumnFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <returns>
    /// A <see cref="DataTableResponse{TViewModel}"/> containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="form"/>, <paramref name="query"/>, or <paramref name="projection"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="form"/> contains no entries.</exception>
    public static DataTableResponse<TViewModel> BuildDataTableResponse<TEntity, TViewModel>(
        IFormCollection form,
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TViewModel>> projection,
        IEnumerable<string>? genericFilterFields = null,
        bool applySort = true,
        bool applyColumnFilters = true)
        where TEntity : class
        where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(projection);

        if (form.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(form));

        int totalCount = query.Count();
        if (totalCount == 0)
            return new DataTableResponse<TViewModel>() { Draw = form["draw"] };

        var response = new DataTableResponse<TViewModel>() { Draw = form["draw"], RecordsTotal = totalCount };

        DataTableRequest request = RequestParser.ParseRequest(form, applySort, applyColumnFilters);

        IQueryable<TViewModel> baseQuery = query.HandleGenericFilter(genericFilterFields, request.Search)
                                                .Select(projection)
                                                .HandleColumnFilters(request.Filters);

        int filteredCount = baseQuery.Count();

        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = baseQuery.HandleSorting(request.SortOrder)
                                 .ExecuteQuery(request.Skip, request.PageSize);
        return response;
    }

    /// <summary>
    /// Asynchronously builds a <see cref="DataTableResponse{TEntity}"/> for the specified entity type.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="genericFilterFields">Optional list of fields to apply global search filtering.</param>
    /// <param name="applySort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="applyColumnFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <returns>
    /// A <see cref="DataTableResponse{TViewModel}"/> containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="form"/> or <paramref name="query"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="form"/> contains no entries.</exception>
    public static DataTableResponse<TEntity> BuildDataTableResponse<TEntity>(
        IFormCollection form,
        IQueryable<TEntity> query,
        IEnumerable<string>? genericFilterFields = null,
        bool applySort = true,
        bool applyColumnFilters = true)
        where TEntity : class
    {
        return BuildDataTableResponse(form, query, x => x, genericFilterFields, applySort, applyColumnFilters);
    }
}
