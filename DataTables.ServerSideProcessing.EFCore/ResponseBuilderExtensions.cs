using Microsoft.AspNetCore.Http;
namespace DataTables.ServerSideProcessing.EFCore;

/// <summary>
/// Provides extension methods for creating <see cref="ResponseBuilder{TEntity, TResult}"/> instances
/// directly from an <see cref="IQueryable{T}"/> source and a DataTables request form.
/// </summary>
public static class ResponseBuilderExtensions
{

    /// <summary>
    /// Creates a new <see cref="ResponseBuilder{TEntity, TResult}"/> instance with a form collection and entity query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the query.</typeparam>
    /// <typeparam name="TResult">The type of the view model to project to.</typeparam>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <returns>A new instance of <see cref="ResponseBuilder{TEntity, TResult}"/>.</returns>
    public static ResponseBuilder<TEntity, TResult> ForDataTable<TEntity, TResult>(this IQueryable<TEntity> query, IFormCollection form)
        where TEntity : class
        where TResult : class
        => ResponseBuilder<TEntity, TResult>.From(query, form);

    /// <summary>
    /// Creates a new <see cref="ResponseBuilder{TEntity, TResult}"/> instance with a form collection and entity query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the query.</typeparam>
    /// <param name="query">The queryable source of entities.</param>
    /// <param name="form">The form collection containing DataTables request parameters.</param>
    /// <returns>A new instance of <see cref="ResponseBuilder{TEntity, TEntity}"/>.</returns>
    public static ResponseBuilder<TEntity, TEntity> ForDataTable<TEntity>(this IQueryable<TEntity> query, IFormCollection form)
        where TEntity : class
        => ResponseBuilder<TEntity, TEntity>.From(query, form);
}
