using System;
using System.Linq;
using System.Reflection;
using App.Core.Attributes;

namespace App.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetResourceKey(this Enum enumValue)
        {
            var attr = enumValue.GetType()
                                .GetMember(enumValue.ToString())
                                .First()
                                .GetCustomAttribute<AppResourceDisplayNameAttribute>();

            return attr?.ResourceKey ?? enumValue.ToString();
        }
    }
}
