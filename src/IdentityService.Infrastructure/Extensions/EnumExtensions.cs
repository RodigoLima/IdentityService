using System.ComponentModel;
using System.Reflection;

namespace IdentityService.Infrastructure.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

        return attribute == null ? value.ToString() : attribute.Description;
    }

    public static bool HasAnyFlag<T>(this T source, T target) where T : Enum
    {
        return (Convert.ToInt64(source) & Convert.ToInt64(target)) != 0;
    }
}
