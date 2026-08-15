using System.ComponentModel;
using HrPlatform.Data.Enums;

namespace HrPlatform.Utils;

public static class EnumExtensions
{
    /// <summary>
    /// Retrieves the Description value from an enum member using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="enumValue">The specific enum value to check.</param>
    /// <returns>The string description, or null if no description is found.</returns>
    public static string? GetDescriptionValue<T>(this T enumValue) where T : struct, Enum
    {
        // 1. Get the Type of the enum (e.g., typeof(EmploymentType))
        var type = typeof(T);

        // 2. Get the FieldInfo for the specific member value passed in (e.g., EmploymentW2)
        var field = type.GetField(enumValue.ToString());

        if (field == null)
        {
            return null; // Should not happen if input is a valid enum
        }

        // 3. Get the custom attribute attached to that field
        var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), true);

        if (attributes.Length > 0)
        {
            // 4. Cast and return the Description property value
            return ((DescriptionAttribute)attributes[0]).Description;
        }

        // If no description attribute is found, return null or a default string
        return null; 
    }
}
