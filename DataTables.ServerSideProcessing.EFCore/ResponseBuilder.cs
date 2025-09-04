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
public sealed class ResponseBuilder<TEntity, TResult>
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

    /// <summary>
    /// Gets the query with projection applied, but before any filtering or sorting.
    /// </summary>
    public IQueryable<TResult>? BaseQuery { get; private set; }

    /// <summary>
    /// Gets the query with projection, global filters, and column filters applied, but without sorting.
    /// </summary>
    public IQueryable<TResult>? FilteredQuery { get; private set; }

    /// <summary>
    /// Gets the final query with projection, filters and sorting applied.
    /// </summary>
    public IQueryable<TResult>? FinalQuery { get; private set; }

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
    /// <param name="properties">The names of the properties to include in global search.</param>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TResult}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TResult> WithGlobalFilter(params string[] properties)
    {
        _globalFilterProperties = properties;
        return this;
    }

    /// <summary>
    /// Processes the DataTables request and executes the configured query pipeline asynchronously,
    /// applying projection, filtering, sorting, and paging as specified.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Response{TResult}"/> containing the data and metadata required by DataTables.
    /// </returns>
    public async Task<Response<TResult>> BuildAsync(CancellationToken ct = default)
    {
        int totalCount = await _query.CountAsync(ct);
        if (totalCount == 0) return new Response<TResult>() { Draw = _form["draw"] };

        var response = new Response<TResult>() { Draw = _form["draw"], RecordsTotal = totalCount };

        Request request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, FilterParsingOptions.Default);

        BaseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        FilteredQuery = BaseQuery.HandleGlobalFilter(_globalFilterProperties, request.Search)
                                 .HandleColumnFilters(request.Filters);

        int filteredCount = await FilteredQuery.CountAsync(ct);

        if (filteredCount == 0) return response;

        response.RecordsFiltered = filteredCount;

        FinalQuery = FilteredQuery.HandleSorting(request.SortOrder);

        response.Data = request.PageSize != -1
            ? await FinalQuery.Skip(request.Skip).Take(request.PageSize).ToListAsync(ct)
            : await FinalQuery.ToListAsync(ct);

        return response;
    }

    /// <summary>
    /// Processes the DataTables request and executes the configured query pipeline,
    /// applying projection, filtering, sorting, and paging as specified.
    /// </summary>
    /// <returns>
    /// A <see cref="Response{TResult}"/> containing the data and metadata required by DataTables.
    /// </returns>
    public Response<TResult> Build()
    {
        int totalCount = _query.Count();
        if (totalCount == 0) return new Response<TResult>() { Draw = _form["draw"] };

        var response = new Response<TResult>() { Draw = _form["draw"], RecordsTotal = totalCount };

        Request request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, FilterParsingOptions.Default);

        BaseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        FilteredQuery = BaseQuery.HandleGlobalFilter(_globalFilterProperties, request.Search)
                                 .HandleColumnFilters(request.Filters);

        int filteredCount = FilteredQuery.Count();

        if (filteredCount == 0) return response;

        response.RecordsFiltered = filteredCount;

        FinalQuery = FilteredQuery.HandleSorting(request.SortOrder);

        response.Data = request.PageSize != -1
            ? FinalQuery.Skip(request.Skip).Take(request.PageSize).ToList()
            : FinalQuery.ToList();

        return response;
    }
}
