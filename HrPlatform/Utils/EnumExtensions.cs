using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace HrPlatform.Utils;

/// <summary>
/// Utility extension methods for enums.
/// </summary>
public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<(Type, object), string?> DescriptionCache = new();

    /// <summary>
    /// Retrieves the Description value from an enum member using cached reflection.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="enumValue">The specific enum value to check.</param>
    /// <returns>The string description, or null if no description is found.</returns>
    public static string? GetDescriptionValue<T>(this T enumValue) where T : struct, Enum
    {
        return DescriptionCache.GetOrAdd((typeof(T), enumValue), key =>
        {
            var (type, val) = key;
            var name = val.ToString();
            if (string.IsNullOrEmpty(name)) return null;

            var field = type.GetField(name);
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>(inherit: false);
            return attribute?.Description;
        });
    }
}
