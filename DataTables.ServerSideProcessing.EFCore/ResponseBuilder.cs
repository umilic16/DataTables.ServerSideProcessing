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
/// <typeparam name="TSource">The type of the entity in the query.</typeparam>
/// <typeparam name="TResult">The type to project the entity into.</typeparam>
public sealed class ResponseBuilder<TSource, TResult>
    where TSource : class
    where TResult : class
{
    private readonly IFormCollection _form;
    private readonly IQueryable<TSource> _query;
    private readonly Expression<Func<TSource, TResult>>? _projection;
    private bool _applySorting = true;
    private bool _applyColumnFilters = true;
    private string[]? _globalFilterProperties;
    private bool ApplyGlobalFilter => _globalFilterProperties is { Length: > 0 };

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

    internal ResponseBuilder(IQueryable<TSource> query, IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(query);

        if (form.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(form));

        _form = form;
        _query = query;
    }

    internal ResponseBuilder(IQueryable<TSource> query, IFormCollection form, Expression<Func<TSource, TResult>> projection)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(query);

        if (form.Count == 0)
            throw new ArgumentException("Request form cannot be empty.", nameof(form));

        _form = form;
        _query = query;
        _projection = projection;
    }

    /// <summary>
    /// Disables applying sorting to the query.
    /// </summary>
    /// <returns>The current <see cref="ResponseBuilder{TSource, TResult}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TSource, TResult> WithoutSorting()
    {
        _applySorting = false;
        return this;
    }

    /// <summary>
    /// Disables applying column filters to the query.
    /// </summary>
    /// <returns>The current <see cref="ResponseBuilder{TSource, TResult}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TSource, TResult> WithoutColumnFilters()
    {
        _applyColumnFilters = false;
        return this;
    }

    /// <summary>
    /// Enables global filtering on the specified properties using the DataTables "search" input.
    /// </summary>
    /// <param name="properties">The names of the properties to include in global search.</param>
    /// <returns>The current <see cref="ResponseBuilder{TSource, TResult}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TSource, TResult> WithGlobalFilter(params string[] properties)
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

    /// <summary>
    /// Processes the DataTables request and executes the configured query pipeline asynchronously,
    /// applying projection, filtering, sorting, and paging as specified.
    /// </summary>
    /// <param name="skip"> The number of records to skip. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="pageSize">The number of records to return. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="List{TResult}"/> containing the requested data after applying filters, sorting, and paging.
    /// </returns>
    public async Task<List<TResult>> GetDataAsync(int? skip = null, int? pageSize = null, CancellationToken ct = default)
    {
        Request request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, FilterParsingOptions.Default, skip, pageSize);

        BaseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        FilteredQuery = BaseQuery.HandleGlobalFilter(_globalFilterProperties, request.Search)
                                 .HandleColumnFilters(request.Filters);

        FinalQuery = FilteredQuery.HandleSorting(request.SortOrder);

        return request.PageSize != -1
            ? await FinalQuery.Skip(request.Skip).Take(request.PageSize).ToListAsync(ct)
            : await FinalQuery.ToListAsync(ct);
    }

    /// <summary>
    /// Processes the DataTables request and executes the configured query pipeline,
    /// applying projection, filtering, sorting, and paging as specified.
    /// </summary>
    /// <param name="skip"> The number of records to skip. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="pageSize">The number of records to return. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <returns>
    /// A <see cref="List{TResult}"/> containing the requested data after applying filters, sorting, and paging.
    /// </returns>
    public List<TResult> GetData(int? skip = null, int? pageSize = null)
    {
        Request request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, FilterParsingOptions.Default, skip, pageSize);

        BaseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        FilteredQuery = BaseQuery.HandleGlobalFilter(_globalFilterProperties, request.Search)
                                 .HandleColumnFilters(request.Filters);

        FinalQuery = FilteredQuery.HandleSorting(request.SortOrder);

        return request.PageSize != -1
            ? FinalQuery.Skip(request.Skip).Take(request.PageSize).ToList()
            : FinalQuery.ToList();
    }
}
