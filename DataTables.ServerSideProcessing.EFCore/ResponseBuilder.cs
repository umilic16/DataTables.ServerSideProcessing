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
/// sorting, pagination, and projection of entities to view models.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the query.</typeparam>
/// <typeparam name="TViewModel">The type of the view model to project the entity into.</typeparam>
public class ResponseBuilder<TEntity, TViewModel>
    where TEntity : class
    where TViewModel : class
{
    private readonly IFormCollection _form;
    private readonly IQueryable<TEntity> _query;
    private bool _applySorting = true;
    private bool _applyColumnFilters = true;
    private string[]? _globalFilterProperties;
    private bool ApplyGlobalFilter => _globalFilterProperties is { Length: > 0 };
    private Expression<Func<TEntity, TViewModel>>? _projection;

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
    /// Creates a new <see cref="ResponseBuilder{TEntity, TViewModel}"/> instance with a form collection and entity query.
    /// </summary>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <returns>A new instance of <see cref="ResponseBuilder{TEntity, TViewModel}"/>.</returns>
    public static ResponseBuilder<TEntity, TViewModel> From(IQueryable<TEntity> query, IFormCollection form)
        => new(query, form);

    /// <summary>
    /// Specifies a projection from <typeparamref name="TEntity"/> to <typeparamref name="TViewModel"/>.
    /// </summary>
    /// <param name="projection">An expression that maps an entity to the view model.</param>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TViewModel}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TViewModel> WithProjection(Expression<Func<TEntity, TViewModel>> projection)
    {
        _projection = projection;
        return this;
    }

    /// <summary>
    /// Specifies a projection from <typeparamref name="TEntity"/> to <typeparamref name="TViewModel"/>.
    /// </summary>
    /// <typeparam name="TNewViewModel">The type to project the entity into.</typeparam>
    /// <param name="projection">An expression that maps an entity to the view model.</param>
    /// <returns>
    /// A new <see cref="ResponseBuilder{TEntity, TNewViewModel}"/> instance configured with the specified projection,
    /// enabling fluent configuration with the new view model type.
    /// </returns>
    public ResponseBuilder<TEntity, TNewViewModel> WithProjection<TNewViewModel>(Expression<Func<TEntity, TNewViewModel>> projection)
        where TNewViewModel : class
    {
        return new ResponseBuilder<TEntity, TNewViewModel>(_query, _form)
        {
            _projection = projection
        };
    }

    /// <summary>
    /// Configures whether sorting should be applied to the query.
    /// </summary>
    /// <param name="applySorting">If <c>true</c>, sorting is applied; otherwise, sorting is skipped. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TViewModel}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TViewModel> WithSorting(bool applySorting = true)
    {
        _applySorting = applySorting;
        return this;
    }

    /// <summary>
    /// Enables or disables applying column filters to the query.
    /// </summary>
    /// <param name="applyColumnFilters">If <c>true</c>, column filters are applied; otherwise, they are skipped. Default is <c>true</c>.</param>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TViewModel}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TViewModel> WithColumnFilters(bool applyColumnFilters = true)
    {
        _applyColumnFilters = applyColumnFilters;
        return this;
    }

    /// <summary>
    /// Configures global filtering fields to be used for DataTables "search" input.
    /// </summary>
    /// <param name="properties">The names of the entity properties to include in global search.</param>
    /// <returns>The current <see cref="ResponseBuilder{TEntity, TViewModel}"/> instance for fluent configuration.</returns>
    public ResponseBuilder<TEntity, TViewModel> WithGlobalFilter(params string[] properties)
    {
        _globalFilterProperties = properties;
        return this;
    }

    /// <summary>
    /// Builds a DataTables-compatible response asynchronously.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> for cancelling the operation.</param>
    /// <returns>A <see cref="Task{Response}"/> containing the response data and metadata.</returns>
    public async Task<Response<TViewModel>> BuildAsync(CancellationToken ct = default)
    {
        int totalCount = await _query.CountAsync(ct);
        if (totalCount == 0) return new Response<TViewModel>() { Draw = _form["draw"] };

        var response = new Response<TViewModel>() { Draw = _form["draw"], RecordsTotal = totalCount };

        Request request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, FilterParsingOptions.Default);

        IQueryable<TViewModel> baseQuery;
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
                              .Cast<TViewModel>()
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
    /// <returns>A <see cref="Response{TViewModel}"/> containing the response data and metadata.</returns>
    public Response<TViewModel> Build()
    {
        int totalCount = _query.Count();
        if (totalCount == 0) return new Response<TViewModel>() { Draw = _form["draw"] };

        var response = new Response<TViewModel>() { Draw = _form["draw"], RecordsTotal = totalCount };

        Request request = RequestParser.ParseRequest(_form, ApplyGlobalFilter, _applySorting, _applyColumnFilters, FilterParsingOptions.Default);

        IQueryable<TViewModel> baseQuery;
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
                              .Cast<TViewModel>();
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
