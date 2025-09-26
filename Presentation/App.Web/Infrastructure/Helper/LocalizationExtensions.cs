using App.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace App.Web.Infrastructure.Helper
{
    public static class LocalizationExtensions
    {
        /// <summary>
        /// Get localized resource for the current working language.
        /// Usage: @await Html.T("Resource.Edit")
        /// </summary>
        public static async Task<string> T(this IHtmlHelper helper, string resourceKey, string defaultValue = null)
        {
            var localizationService = (ILocalizationService)helper.ViewContext.HttpContext
                .RequestServices.GetService(typeof(ILocalizationService));

            var resource = await localizationService.GetResourceAsync(resourceKey, defaultValue);
            return string.IsNullOrEmpty(resource) ? resourceKey : resource;
        }

        /// <summary>
        /// Get localized resource for a specific languageId.
        /// Usage: @await Html.T("Resource.Edit", 2) // 2 = English مثلاً
        /// </summary>
        public static async Task<string> T(this IHtmlHelper helper, string resourceKey, int languageId, string defaultValue = null)
        {
            var localizationService = (ILocalizationService)helper.ViewContext.HttpContext
                .RequestServices.GetService(typeof(ILocalizationService));

            var resource = await localizationService.GetResourceAsync(resourceKey, languageId, defaultValue);
            return string.IsNullOrEmpty(resource) ? resourceKey : resource;
        }
    }
}
