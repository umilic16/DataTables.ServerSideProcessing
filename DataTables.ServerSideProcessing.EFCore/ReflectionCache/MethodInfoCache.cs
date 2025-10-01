using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.ReflectionCache;

internal static class MethodInfoCache
{
    private static readonly MethodInfo s_enumerableContainsDefinition =
        typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                          .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

    private static readonly Dictionary<Type, MethodInfo> s_enumerableContainsCache = [];

    internal static MethodInfo GetEnumerableContains(Type elementType)
    {
        if (!s_enumerableContainsCache.TryGetValue(elementType, out MethodInfo? method))
        {
            method = s_enumerableContainsDefinition.MakeGenericMethod(elementType);
            s_enumerableContainsCache[elementType] = method;
        }

        return method;
    }

    internal static readonly MethodInfo s_toString = typeof(object).GetMethod(nameof(ToString))!;
    internal static readonly MethodInfo s_stringContains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
}
