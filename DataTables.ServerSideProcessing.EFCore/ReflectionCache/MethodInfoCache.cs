using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.ReflectionCache;

internal static class MethodInfoCache
{
    internal static readonly MethodInfo s_enumerableContains =
        typeof(Enumerable).GetMethods()
                          .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
                          .MakeGenericMethod(typeof(string));

    internal static readonly MethodInfo s_toString = typeof(object).GetMethod(nameof(ToString))!;
    internal static readonly MethodInfo s_stringContains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
}
