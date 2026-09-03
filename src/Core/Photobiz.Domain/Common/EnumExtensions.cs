using System.ComponentModel;
using System.Reflection;

namespace Photobiz.Domain.Common
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var member = value.GetType().GetField(value.ToString());
            var description = member?.GetCustomAttribute<DescriptionAttribute>();

            return description?.Description ?? value.ToString();
        }
    }
}
