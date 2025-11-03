namespace DataTables.ServerSideProcessing.Data.Models;

/// <summary>
/// Provides extension methods for the <see cref="Response{T}"/>.
/// </summary>
public static class ResponseExtensions
{
    /// <summary>
    /// Extends a <see cref="Response{T}"/> with footer data.
    /// </summary>
    /// <typeparam name="T">The type of the data in the response.</typeparam>
    /// <typeparam name="F">The type of the footer data.</typeparam>
    /// <param name="response">The response to extend.</param>
    /// <param name="footerData">The footer data to add.</param>
    /// <returns>A <see cref="ResponseWithFooter{T, F}"/> containing the original response data and the footer data.</returns>
    public static ResponseWithFooter<T, F> Extend<T, F>(this Response<T> response, List<F> footerData)
    {
        return new ResponseWithFooter<T, F>
        {
            Data = response.Data,
            Draw = response.Draw,
            RecordsFiltered = response.RecordsFiltered,
            RecordsTotal = response.RecordsTotal,
            FooterData = footerData
        };
    }
}
