using System.Linq.Expressions;
using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.EFCore.Filtering;
using DataTables.ServerSideProcessing.EFCore.Sorting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace DataTables.ServerSideProcessing.EFCore;

/// <summary>
/// Provides a fluent API for building DataTables-compatible server-side responses using Entity Framework Core.
/// Supports synchronous and asynchronous execution with features such as global search, column filters,
/// sorting, pagination, and mapping entities to the specified result type.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the query.</typeparam>
/// <typeparam name="TResult">The type to project the entity into.</typeparam>
public class ResponseBuilder<TEntity, TResult>
    where TEntity : class
    where TResult : class
{
    private readonly IFormCollection _form;
    private readonly IQueryable<TEntity> _query;
    private bool _applySorting = true;
    private bool _applyColumnFilters = true;
    private string[]? _globalFilterProperties;
    private bool ApplyGlobalFilter => _globalFilterProperties is { Length: > 0 };
    private Expression<Func<TEntity, TResult>>? _projection;

    private ResponseBuilder(IQueryable<TEntity> query, IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(query);

        if (form.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(form));

        _form = form;
        _query = query;
    }

    /// <summary>
    /// Creates a new <see cref="ResponseBuilder{TEntity, TResult}"/> instance with a form collection and entity query.
    /// </summary>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <returns>A new instance of <see cref="ResponseBuilder{TEntity, TResult}"/>.</returns>
    public static ResponseBuilder<TEntity, TResult> From(IQueryable<TEntity> query, IFormCollection form)
        => new(query, form);

    /// <summary>
    /// Specifies a projection from <typeparamref name="TEntity"/> to <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="projection">An expression that maps entities to the result type.</param>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TResult}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TResult> WithProjection(Expression<Func<TEntity, TResult>> projection)
    {
        _projection = projection;
        return this;
    }

    /// <summary>
    /// Specifies a projection from <typeparamref name="TEntity"/> to <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResultNew">The type to project the entity into.</typeparam>
    /// <param name="projection">An expression that maps entities to the result type.</param>
    /// <returns>
    /// A new <see cref="ResponseBuilder{TEntity, TResultNew}"/> instance configured with the specified projection,
    /// enabling fluent configuration with the new result type.
    /// </returns>
    public ResponseBuilder<TEntity, TResultNew> WithProjection<TResultNew>(Expression<Func<TEntity, TResultNew>> projection)
        where TResultNew : class
    {
        return new ResponseBuilder<TEntity, TResultNew>(_query, _form)
        {
            _projection = projection
        };
    }

    /// <summary>
    /// Disables applying sorting to the query.
    /// </summary>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TResult}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TResult> WithoutSorting()
    {
        _applySorting = false;
        return this;
    }

    /// <summary>
    /// Disables applying column filters to the query.
    /// </summary>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TResult}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TResult> WithoutColumnFilters()
    {
        _applyColumnFilters = false;
        return this;
    }

    /// <summary>
    /// Enables global filtering on the specified properties using the DataTables "search" input.
    /// </summary>
    /// <param name="properties">The names of the entity properties to include in global search.</param>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TResult}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TResult> WithGlobalFilter(params string[] properties)
    {
        _globalFilterProperties = properties;
        return this;
    }

    /// <summary>
    /// Builds a DataTables-compatible response asynchronously.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> for cancelling the operation.</param>
    /// <returns>A <see cref="Task{Response}"/> containing the response data and metadata.</returns>
    public async Task<Response<TResult>> BuildAsync(CancellationToken ct = default)
    {
        int totalCount = await _query.CountAsync(ct);
        if (totalCount == 0) return new Response<TResult>() { Draw = _form["draw"] };

        var response = new Response<TResult>() { Draw = _form["draw"], RecordsTotal = totalCount };

        Request request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, FilterParsingOptions.Default);

        IQueryable<TResult> baseQuery;
        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        if (_projection != null)
        {
            baseQuery = _query.HandleGlobalFilter(_globalFilterProperties, request.Search)
                              .Select(_projection)
                              .HandleColumnFilters(request.Filters);
        }
        else
        {
            baseQuery = _query.HandleGlobalFilter(_globalFilterProperties, request.Search)
                              .Cast<TResult>()
                              .HandleColumnFilters(request.Filters);
        }

        int filteredCount = await baseQuery.CountAsync(ct);

        if (filteredCount == 0) return response;

        response.RecordsFiltered = filteredCount;


        baseQuery = baseQuery.HandleSorting(request.SortOrder);

        response.Data = request.PageSize != -1
            ? await baseQuery.Skip(request.Skip).Take(request.PageSize).ToListAsync(ct)
            : await baseQuery.ToListAsync(ct);

        return response;
    }

    /// <summary>
    /// Builds a DataTables-compatible response synchronously.
    /// </summary>
    /// <returns>A <see cref="Response{TResult}"/> containing the response data and metadata.</returns>
    public Response<TResult> Build()
    {
        int totalCount = _query.Count();
        if (totalCount == 0) return new Response<TResult>() { Draw = _form["draw"] };

        var response = new Response<TResult>() { Draw = _form["draw"], RecordsTotal = totalCount };

        Request request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, FilterParsingOptions.Default);

        IQueryable<TResult> baseQuery;
        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        if (_projection != null)
        {
            baseQuery = _query.HandleGlobalFilter(_globalFilterProperties, request.Search)
                              .Select(_projection)
                              .HandleColumnFilters(request.Filters);
        }
        else
        {
            baseQuery = _query.HandleGlobalFilter(_globalFilterProperties, request.Search)
                              .HandleColumnFilters(request.Filters)
                              .Cast<TResult>();
        }

        int filteredCount = baseQuery.Count();

        if (filteredCount == 0) return response;

        response.RecordsFiltered = filteredCount;

        baseQuery = baseQuery.HandleSorting(request.SortOrder);

        response.Data = request.PageSize != -1
            ? baseQuery.Skip(request.Skip).Take(request.PageSize).ToList()
            : baseQuery.ToList();

        return response;
    }
}
