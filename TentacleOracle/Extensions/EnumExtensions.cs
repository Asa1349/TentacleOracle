using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TentacleOracle.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value) =>
        value.GetType().GetField(value.ToString())?
            .GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
}
