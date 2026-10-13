using System.Collections.Concurrent;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.ReflectionCache;

internal static class MethodInfoCache
{
    private static readonly MethodInfo s_enumerableContainsDefinition =
        typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                          .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

    private static readonly ConcurrentDictionary<Type, MethodInfo> s_enumerableContainsCache = [];

    internal static MethodInfo GetEnumerableContains(Type elementType) =>
        s_enumerableContainsCache.GetOrAdd(elementType, t => s_enumerableContainsDefinition.MakeGenericMethod(t));

    internal static readonly MethodInfo s_toString = typeof(object).GetMethod(nameof(ToString))!;
    internal static readonly MethodInfo s_stringContains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
}
