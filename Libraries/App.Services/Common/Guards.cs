using System;
using System.Threading.Tasks;

namespace App.Services.Common
{
    public static class Guards
    {
        public static void NotNull(object obj, string paramName, Func<string> localizedMessageFactory = null)
        {
            if (obj is null)
                throw new ArgumentNullException(paramName, localizedMessageFactory?.Invoke());
        }

        public static Task<T> NullToDefaultAsync<T>(Task<T> task, T defaultValue = default) => task ?? Task.FromResult(defaultValue);
    }
}
