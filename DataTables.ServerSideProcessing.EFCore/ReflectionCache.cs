namespace DataTables.ServerSideProcessing.EFCore;
/// <summary>
/// Provides a cached, case-insensitive set of property names for the specified type <typeparamref name="T"/>.
/// This is used to optimize reflection-based property lookups.
/// </summary>
/// <typeparam name="T">The class type whose property names are to be cached.</typeparam>
internal static class ReflectionCache<T> where T : class
{
    /// <summary>
    /// A case-insensitive set containing the names of all public properties of <typeparamref name="T"/>.
    /// </summary>
    internal static readonly HashSet<string> s_properties = new(
        typeof(T).GetProperties().Select(p => p.Name),
        StringComparer.InvariantCultureIgnoreCase);
}
