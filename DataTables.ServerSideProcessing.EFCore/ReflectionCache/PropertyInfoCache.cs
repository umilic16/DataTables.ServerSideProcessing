using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DataTables.ServerSideProcessing.EFCore.ReflectionCache;

internal static class PropertyInfoCache<T> where T : class
{
    // Cache of property name → PropertyInfo
    private static readonly ConcurrentDictionary<string, PropertyInfo> s_propertyMap = new(
        typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .ToDictionary(p => p.Name, p => p, StringComparer.InvariantCultureIgnoreCase));

    internal static bool TryGetProperty(string propertyName, [NotNullWhen(true)] out PropertyInfo? propertyInfo)
    {
        return s_propertyMap.TryGetValue(propertyName, out propertyInfo);
    }

    internal static PropertyInfo GetProperty(string propertyName)
    {
        if (!TryGetProperty(propertyName, out PropertyInfo? propertyInfo))
            throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        return propertyInfo;
    }

    internal static bool PropertyExists(string propertyName)
    {
        return s_propertyMap.ContainsKey(propertyName);
    }

    internal static void EnsurePropertyExists(string propertyName)
    {
        if (!PropertyExists(propertyName))
            throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'."); ;
    }

}
