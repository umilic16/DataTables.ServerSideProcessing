namespace DataTables.ServerSideProcessing.EFCore;

internal static class ReflectionCache<T> where T : class
{
    internal static readonly HashSet<string> s_properties = new(
        typeof(T).GetProperties().Select(p => p.Name),
        StringComparer.InvariantCultureIgnoreCase);
}
