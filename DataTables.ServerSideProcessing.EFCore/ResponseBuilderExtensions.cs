using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
namespace DataTables.ServerSideProcessing.EFCore;

/// <summary>
/// Provides extension methods for creating <see cref="ResponseBuilder{TSource, TResult}"/> instances
/// directly from an <see cref="IQueryable{T}"/> source and a DataTables request form.
/// </summary>
public static class ResponseBuilder
{
    /// <summary>
    /// Creates a new <see cref="ResponseBuilder{TSource, TSource}"/> instance with a form collection and entity query.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity in the query.</typeparam>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <returns>A new instance of <see cref="ResponseBuilder{TSource, TSource}"/>.</returns>
    public static ResponseBuilder<TSource, TSource> ForDataTable<TSource>(this IQueryable<TSource> query, IFormCollection form)
        where TSource : class
        => new(query, form);

    /// <summary>
    /// Creates a new <see cref="ResponseBuilder{TSource, TResult}"/> instance with a form collection and entity query.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity in the query.</typeparam>
    /// <typeparam name="TResult">The type to project the entity into.</typeparam>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="projection">An expression that maps entities to the result type.</param>
    /// <returns>A new instance of <see cref="ResponseBuilder{TSource, TSource}"/>.</returns>
    public static ResponseBuilder<TSource, TResult> ForDataTable<TSource, TResult>(this IQueryable<TSource> query, IFormCollection form, Expression<Func<TSource, TResult>> projection)
        where TSource : class
        where TResult : class
        => new(query, form, projection);

    /// <summary>
    /// Creates a new <see cref="ResponseBuilder{TSource, TSource}"/> instance with a form collection and entity query.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity in the query.</typeparam>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <returns>A new instance of <see cref="ResponseBuilder{TSource, TSource}"/>.</returns>
    public static ResponseBuilder<TSource, TSource> From<TSource>(IQueryable<TSource> query, IFormCollection form)
        where TSource : class
        => new(query, form);

    /// <summary>
    /// Creates a new <see cref="ResponseBuilder{TSource, TSource}"/> instance with a form collection and entity query.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity in the query.</typeparam>
    /// <typeparam name="TResult">The type to project the entity into.</typeparam>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <param name="projection">An expression that maps entities to the result type.</param>
    /// <returns>A new instance of <see cref="ResponseBuilder{TSource, TSource}"/>.</returns>
    public static ResponseBuilder<TSource, TResult> From<TSource, TResult>(IQueryable<TSource> query, IFormCollection form, Expression<Func<TSource, TResult>> projection)
        where TSource : class
        where TResult : class
        => new(query, form, projection);
}
