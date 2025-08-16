using System.Linq.Expressions;
using DataTables.ServerSideProcessing.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DataTables.ServerSideProcessing.EFCore;
/// <summary>
/// Provides utility methods for building DataTables-compatible responses for server-side processing using Entity Framework Core.
/// Supports synchronous and asynchronous overloads, with optional global search, column filters, sorting, pagination, and projection.
/// </summary>
public static class ResponseBuilder
{
    /// <summary>
    /// Asynchronously builds a <see cref="Response{TViewModel}"/> for the specified entity type and maps the result to a view model.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TViewModel">The view model type to map the entity to.</typeparam>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="projection">An expression to map the entity to the view model.</param>
    /// <param name="genericFilterFields">Optional list of property names on which to apply filtering using the search value from the request.</param>
    /// <param name="applySort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="applyColumnFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <param name="options">Configuration options for parsing filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="Response{TViewModel}"/>
    /// containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="form"/>, <paramref name="query"/>, or <paramref name="projection"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="form"/> contains no entries.</exception>
    public static async Task<Response<TViewModel>> BuildAsync<TEntity, TViewModel>(
        IFormCollection form,
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TViewModel>> projection,
        string[]? genericFilterFields = null,
        bool applySort = true,
        bool applyColumnFilters = true,
        FilterParsingOptions? options = default,
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
            return new Response<TViewModel>() { Draw = form["draw"] };

        var response = new Response<TViewModel>() { Draw = form["draw"], RecordsTotal = totalCount };

        Request request = RequestParser.ParseRequest(form, applySort, applyColumnFilters, options);

        IQueryable<TViewModel> baseQuery = query.HandleGenericFilter(genericFilterFields, request.Search)
                                                .Select(projection)
                                                .HandleColumnFilters(request.Filters);

        int filteredCount = await baseQuery.CountAsync(ct);

        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = await baseQuery.HandleSorting(request.SortOrder)
                                       .ToListAsync(request.Skip, request.PageSize, ct);
        return response;
    }

    /// <summary>
    /// Asynchronously builds a <see cref="Response{TEntity}"/> for the specified entity type.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="genericFilterFields">Optional list of property names on which to apply filtering using the search value from the request.</param>
    /// <param name="applySort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="applyColumnFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <param name="options">Configuration options for parsing filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="Response{TEntity}"/>
    /// containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="form"/> or <paramref name="query"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="form"/> contains no entries.</exception>
    public static async Task<Response<TEntity>> BuildAsync<TEntity>(
        IFormCollection form,
        IQueryable<TEntity> query,
        string[]? genericFilterFields = null,
        bool applySort = true,
        bool applyColumnFilters = true,
        FilterParsingOptions? options = default,
        CancellationToken ct = default)
        where TEntity : class
    {

        return await BuildAsync(form, query, x => x, genericFilterFields, applySort, applyColumnFilters, options, ct);
    }

    /// <summary>
    /// Asynchronously builds a <see cref="Response{TViewModel}"/> for the specified entity type and maps the result to a view model.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TViewModel">The view model type to map the entity to.</typeparam>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="projection">An expression to map the entity to the view model.</param>
    /// <param name="genericFilterFields">Optional list of property names on which to apply filtering using the search value from the request.</param>
    /// <param name="applySort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="applyColumnFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <param name="options">Configuration options for parsing filters.</param>
    /// <returns>
    /// A <see cref="Response{TViewModel}"/> containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="form"/>, <paramref name="query"/>, or <paramref name="projection"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="form"/> contains no entries.</exception>
    public static Response<TViewModel> Build<TEntity, TViewModel>(
        IFormCollection form,
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TViewModel>> projection,
        string[]? genericFilterFields = null,
        bool applySort = true,
        bool applyColumnFilters = true,
        FilterParsingOptions? options = default)
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
            return new Response<TViewModel>() { Draw = form["draw"] };

        var response = new Response<TViewModel>() { Draw = form["draw"], RecordsTotal = totalCount };

        Request request = RequestParser.ParseRequest(form, applySort, applyColumnFilters, options);

        IQueryable<TViewModel> baseQuery = query.HandleGenericFilter(genericFilterFields, request.Search)
                                                .Select(projection)
                                                .HandleColumnFilters(request.Filters);

        int filteredCount = baseQuery.Count();

        if (filteredCount == 0)
            return response;

        response.RecordsFiltered = filteredCount;

        response.Data = baseQuery.HandleSorting(request.SortOrder)
                                 .ToList(request.Skip, request.PageSize);
        return response;
    }

    /// <summary>
    /// Asynchronously builds a <see cref="Response{TEntity}"/> for the specified entity type.
    /// Applies optional filtering and sorting based on the DataTables request.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="genericFilterFields">Optional list of property names on which to apply filtering using the search value from the request.</param>
    /// <param name="applySort">Indicates whether to parse sorting information from the request form. Default is true.</param>
    /// <param name="applyColumnFilters">Indicates whether to parse column filter information from the request form. Default is true.</param>
    /// <param name="options">Configuration options for parsing filters.</param>
    /// <returns>
    /// A <see cref="Response{TViewModel}"/> containing the requested data and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="form"/> or <paramref name="query"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="form"/> contains no entries.</exception>
    public static Response<TEntity> Build<TEntity>(
        IFormCollection form,
        IQueryable<TEntity> query,
        string[]? genericFilterFields = null,
        bool applySort = true,
        bool applyColumnFilters = true,
        FilterParsingOptions? options = default)
        where TEntity : class
    {
        return Build(form, query, x => x, genericFilterFields, applySort, applyColumnFilters, options);
    }
}
