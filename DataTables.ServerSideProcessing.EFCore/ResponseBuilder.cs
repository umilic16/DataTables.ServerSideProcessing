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
    private Request? _request;
    private IQueryable<TResult>? _baseQuery;
    private IQueryable<TResult>? _filteredQuery;
    private IQueryable<TResult>? _finalQuery;

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

    /// <inheritdoc cref="BuildAsync(FilterParsingOptions?, CancellationToken)"/>
    public Task<Response<TResult>> BuildAsync(CancellationToken ct = default)
        => BuildAsync(null, ct);

    /// <summary>
    /// Processes the DataTables request and executes the configured query pipeline asynchronously,
    /// applying projection, filtering, sorting, and paging as specified.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <param name="options">Options for parsing filters from the request. If <c>null</c>, default options are used.</param>
    /// <returns>
    /// A <see cref="Response{TResult}"/> containing the data and metadata required by DataTables.
    /// </returns>
    public async Task<Response<TResult>> BuildAsync(FilterParsingOptions? options = null, CancellationToken ct = default)
    {
        int totalCount = await _query.CountAsync(ct);
        if (totalCount == 0) return new Response<TResult>() { Draw = _form["draw"] };

        var response = new Response<TResult>() { Draw = _form["draw"], RecordsTotal = totalCount };

        options ??= FilterParsingOptions.Default;
        _request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, options);

        _baseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        _filteredQuery = _baseQuery.HandleGlobalFilter(_globalFilterProperties, _request.Search)
                                   .HandleColumnFilters(_request.Filters);

        int filteredCount = await _filteredQuery.CountAsync(ct);

        if (filteredCount == 0) return response;

        response.RecordsFiltered = filteredCount;

        _finalQuery = _filteredQuery.HandleSorting(_request.SortOrder);

        response.Data = _request.PageSize != -1
            ? await _finalQuery.Skip(_request.Skip).Take(_request.PageSize).ToListAsync(ct)
            : await _finalQuery.ToListAsync(ct);

        return response;
    }

    /// <summary>
    /// Processes the DataTables request and executes the configured query pipeline,
    /// applying projection, filtering, sorting, and paging as specified.
    /// </summary>
    /// <param name="options">Options for parsing filters from the request. If <c>null</c>, default options are used.</param>
    /// <returns>
    /// A <see cref="Response{TResult}"/> containing the data and metadata required by DataTables.
    /// </returns>
    public Response<TResult> Build(FilterParsingOptions? options = null)
    {
        int totalCount = _query.Count();
        if (totalCount == 0) return new Response<TResult>() { Draw = _form["draw"] };

        var response = new Response<TResult>() { Draw = _form["draw"], RecordsTotal = totalCount };

        options ??= FilterParsingOptions.Default;
        _request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, options);

        _baseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        _filteredQuery = _baseQuery.HandleGlobalFilter(_globalFilterProperties, _request.Search)
                                   .HandleColumnFilters(_request.Filters);

        int filteredCount = _filteredQuery.Count();

        if (filteredCount == 0) return response;

        response.RecordsFiltered = filteredCount;

        _finalQuery = _filteredQuery.HandleSorting(_request.SortOrder);

        response.Data = _request.PageSize != -1
            ? _finalQuery.Skip(_request.Skip).Take(_request.PageSize).ToList()
            : _finalQuery.ToList();

        return response;
    }

    /// <inheritdoc cref="GetDataAsync(int?, int?, FilterParsingOptions?, CancellationToken)"/>
    public Task<List<TResult>> GetDataAsync(CancellationToken ct = default)
        => GetDataAsync(null, null, null, ct);

    /// <inheritdoc cref="GetDataAsync(int?, int?, FilterParsingOptions?, CancellationToken)"/>
    public Task<List<TResult>> GetDataAsync(FilterParsingOptions? options = null, CancellationToken ct = default)
        => GetDataAsync(null, null, options, ct);

    /// <inheritdoc cref="GetDataAsync(int?, int?, FilterParsingOptions?, CancellationToken)"/>
    public Task<List<TResult>> GetDataAsync(int? skip = null, int? pageSize = null, CancellationToken ct = default)
        => GetDataAsync(skip, pageSize, null, ct);

    /// <summary>
    /// Processes the DataTables request and executes the configured query pipeline asynchronously,
    /// applying projection, filtering, sorting, and paging as specified.
    /// </summary>
    /// <param name="skip">The number of records to skip. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="pageSize">The number of records to return. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="options">Options for parsing filters from the request. If <c>null</c>, default options are used.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="List{TResult}"/> containing the requested data after applying filters, sorting, and paging.
    /// </returns>
    public async Task<List<TResult>> GetDataAsync(int? skip = null, int? pageSize = null, FilterParsingOptions? options = null, CancellationToken ct = default)
    {
        options ??= FilterParsingOptions.Default;
        _request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, options, skip, pageSize);

        _baseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        _filteredQuery = _baseQuery.HandleGlobalFilter(_globalFilterProperties, _request.Search)
                                   .HandleColumnFilters(_request.Filters);

        _finalQuery = _filteredQuery.HandleSorting(_request.SortOrder);

        return _request.PageSize != -1
            ? await _finalQuery.Skip(_request.Skip).Take(_request.PageSize).ToListAsync(ct)
            : await _finalQuery.ToListAsync(ct);
    }

    /// <inheritdoc cref="GetData(int?, int?, FilterParsingOptions?)"/>
    public List<TResult> GetData(FilterParsingOptions? options = null)
        => GetData(null, null, options);

    /// <summary>
    /// Processes the DataTables request and executes the configured query pipeline,
    /// applying projection, filtering, sorting, and paging as specified.
    /// </summary>
    /// <param name="skip">The number of records to skip. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="pageSize">The number of records to return. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="options">Options for parsing filters from the request. If <c>null</c>, default options are used.</param>
    /// <returns>
    /// A <see cref="List{TResult}"/> containing the requested data after applying filters, sorting, and paging.
    /// </returns>
    public List<TResult> GetData(int? skip = null, int? pageSize = null, FilterParsingOptions? options = null)
    {
        options ??= FilterParsingOptions.Default;
        _request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, options, skip, pageSize);

        _baseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        // no need to check if global filter / column filter / sorting should be handled or not here,
        // as RequestParser will return empty arrays / strings if they shouldn't be applied
        // and QueryBuilder extension methods will return the original query in that case
        _filteredQuery = _baseQuery.HandleGlobalFilter(_globalFilterProperties, _request.Search)
                                   .HandleColumnFilters(_request.Filters);

        _finalQuery = _filteredQuery.HandleSorting(_request.SortOrder);

        return _request.PageSize != -1
            ? _finalQuery.Skip(_request.Skip).Take(_request.PageSize).ToList()
            : _finalQuery.ToList();
    }

    /// <summary>
    /// Gets the base query with the projection applied (if a projection was provided).
    /// </summary>
    /// <returns>
    /// An <see cref="IQueryable{TResult}"/> representing the projected source sequence without filters or sorting.
    /// </returns>
    public IQueryable<TResult> GetBaseQuery()
    {
        if (_baseQuery is not null) return _baseQuery;
        return _baseQuery = _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();
    }

    /// <inheritdoc cref="GetFilteredQuery(int?, int?, FilterParsingOptions?)"/>
    public IQueryable<TResult> GetFilteredQuery(FilterParsingOptions options)
        => GetFilteredQuery(null, null, options);

    /// <summary>
    /// Gets the query with global and column filters applied (sorting is not applied). Calling this method will parse the request (if it's not already parsed).
    /// </summary>
    /// <param name="skip">The number of records to skip. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="pageSize">The number of records to return. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="options">Options for parsing filters from the request. If <c>null</c>, default options are used.</param>
    /// <returns>
    /// An <see cref="IQueryable{TResult}"/> representing the projected and filtered sequence without sorting.
    /// </returns>
    public IQueryable<TResult> GetFilteredQuery(int? skip = null, int? pageSize = null, FilterParsingOptions? options = null)
    {
        if (_filteredQuery is not null) return _filteredQuery;

        _baseQuery ??= _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();

        if (_request is null)
        {
            options ??= FilterParsingOptions.Default;
            _request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, options, skip, pageSize);
        }
        return _filteredQuery = _baseQuery.HandleGlobalFilter(_globalFilterProperties, _request.Search)
                                          .HandleColumnFilters(_request.Filters);
    }

    /// <inheritdoc cref="GetFilteredQuery(int?, int?, FilterParsingOptions?)"/>
    public IQueryable<TResult> GetFinalQuery(FilterParsingOptions options)
        => GetFinalQuery(null, null, options);

    /// <summary>
    /// Gets the final query with filters and sorting applied. Calling this method will parse the request (if it's not already parsed).
    /// </summary>
    /// <param name="skip">The number of records to skip. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="pageSize">The number of records to return. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="options">Options for parsing filters from the request. If <c>null</c>, default options are used.</param>
    /// <returns>
    /// An <see cref="IQueryable{TResult}"/> representing the projected, filtered, and sorted sequence.
    /// </returns>
    public IQueryable<TResult> GetFinalQuery(int? skip = null, int? pageSize = null, FilterParsingOptions? options = null)
    {
        if (_finalQuery is not null) return _finalQuery;
        _baseQuery ??= _projection != null ? _query.Select(_projection) : _query.Cast<TResult>();
        if (_request is null)
        {
            options ??= FilterParsingOptions.Default;
            _request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, options, skip, pageSize);
        }

        _filteredQuery ??= _baseQuery.HandleGlobalFilter(_globalFilterProperties, _request.Search)
                                     .HandleColumnFilters(_request.Filters);

        return _finalQuery = _filteredQuery.HandleSorting(_request.SortOrder);
    }

    /// <inheritdoc cref="BuildFromQueryAsync(IQueryable{TResult}, int?, int?, CancellationToken)"/>
    public Task<Response<TResult>> BuildFromQueryAsync(IQueryable<TResult> query, CancellationToken ct = default)
        => BuildFromQueryAsync(query, null, null, ct);

    /// <summary>
    /// Builds a DataTables-compatible response asynchronously from the provided pre-built query.
    /// </summary>
    /// <param name="query">The pre-built query to execute (typically already filtered and sorted).</param>
    /// <param name="skip">The number of records to skip. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="pageSize">The number of records to return. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Response{TResult}"/> containing the data and metadata required by DataTables.
    /// </returns>
    public async Task<Response<TResult>> BuildFromQueryAsync(IQueryable<TResult> query, int? skip = null, int? pageSize = null, CancellationToken ct = default)
    {
        int totalCount = await _query.CountAsync(ct);
        if (totalCount == 0) return new Response<TResult>() { Draw = _form["draw"] };

        var response = new Response<TResult>() { Draw = _form["draw"], RecordsTotal = totalCount };

        int filteredCount = await query.CountAsync(ct);

        if (filteredCount == 0) return response;

        response.RecordsFiltered = filteredCount;

        pageSize = _request?.PageSize ?? pageSize;
        skip = _request?.Skip ?? skip ?? 0;
        response.Data = pageSize > 0
            ? await query.Skip(skip.Value).Take(pageSize.Value).ToListAsync(ct)
            : await query.ToListAsync(ct);

        return response;
    }

    /// <summary>
    /// Builds a DataTables-compatible response from the provided pre-built query.
    /// </summary>
    /// <param name="query">The pre-built query to execute (typically already filtered and sorted).</param>
    /// <param name="skip">The number of records to skip. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <param name="pageSize">The number of records to return. If <c>null</c>, the value is determined from the parsed request.</param>
    /// <returns>
    /// A <see cref="Response{TResult}"/> containing the data and metadata required by DataTables.
    /// </returns>
    public Response<TResult> BuildFromQuery(IQueryable<TResult> query, int? skip = null, int? pageSize = null)
    {
        int totalCount = _query.Count();
        if (totalCount == 0) return new Response<TResult>() { Draw = _form["draw"] };

        var response = new Response<TResult>() { Draw = _form["draw"], RecordsTotal = totalCount };

        int filteredCount = query.Count();

        if (filteredCount == 0) return response;

        response.RecordsFiltered = filteredCount;

        pageSize = _request?.PageSize ?? pageSize;
        skip = _request?.Skip ?? skip ?? 0;
        response.Data = pageSize > 0
            ? query.Skip(skip.Value).Take(pageSize.Value).ToList()
            : query.ToList();

        return response;
    }
}
