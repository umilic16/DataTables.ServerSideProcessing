namespace DataTables.ServerSideProcessing.EFCore;
internal static class ReflectionCache<T>
{
    internal static readonly HashSet<string> Properties = new(
        typeof(T).GetProperties().Select(p => p.Name),
        StringComparer.InvariantCultureIgnoreCase);
}
