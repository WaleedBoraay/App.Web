using App.Services.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace App.Services.Infrastructure.Helper
{
    public static class LocalizationRazorHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static async Task<string> T(string resourceKey, string defaultValue = null)
        {
            if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
                return defaultValue ?? resourceKey;

            var localizationService = _httpContextAccessor.HttpContext.RequestServices
                .GetService(typeof(ILocalizationService)) as ILocalizationService;

            if (localizationService == null)
                return defaultValue ?? resourceKey;

            var resource = await localizationService.GetResourceAsync(resourceKey, defaultValue);
            return string.IsNullOrEmpty(resource) ? defaultValue ?? resourceKey : resource;
        }
    }
}
