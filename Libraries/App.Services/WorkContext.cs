using App.Core.Domain.Localization;
using App.Core.Domain.Users;
using App.Services.Localization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.Services
{
    public class WorkContext : IWorkContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILanguageService _languageService;

        private AppUser _cachedUser;
        private Language _cachedLanguage;

        public WorkContext(IHttpContextAccessor httpContextAccessor, ILanguageService languageService)
        {
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;

        }

        /// <summary>
        /// Get current user from HttpContext (based on ClaimsPrincipal)
        /// </summary>
        public async Task<AppUser> GetCurrentUserAsync()
        {
            if (_cachedUser != null)
                return _cachedUser;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
                return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return null;

            var emailClaim = user.FindFirst(ClaimTypes.Email);

            _cachedUser = new AppUser
            {
                Id = int.Parse(userIdClaim.Value),
                Username = user.Identity.Name,
                Email = emailClaim?.Value
            };

            return await Task.FromResult(_cachedUser);
        }


        /// <summary>
        /// Manually set current user (useful for system tasks or testing)
        /// </summary>
        public async Task SetCurrentUserAsync(AppUser user = null)
        {
            _cachedUser = user;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Original user if impersonation is supported (for now just return current)
        /// </summary>
        public AppUser OriginalUserIfImpersonated => _cachedUser;

        /// <summary>
        /// Get working language (could be from cookie, session, or user preference)
        /// </summary>
        public async Task<Language> GetWorkingLanguageAsync()
        {
            if (_cachedLanguage != null)
                return _cachedLanguage;

            var langCookie = _httpContextAccessor.HttpContext?.Request.Cookies["App.WorkingLanguage"];
            if (!string.IsNullOrEmpty(langCookie))
            {
                _cachedLanguage = (await _languageService.GetAllAsync())
                    .FirstOrDefault(l => l.LanguageCulture == langCookie);
                if (_cachedLanguage != null)
                    return _cachedLanguage;
            }

            _cachedLanguage = (await _languageService.GetAllAsync())
                .FirstOrDefault(l => l.Published)
                ?? (await _languageService.GetAllAsync()).FirstOrDefault();

            return _cachedLanguage;
        }

        /// <summary>
        /// Set working language (store in cache/cookie)
        /// </summary>
        public async Task SetWorkingLanguageAsync(Language language)
        {
            _cachedLanguage = language;

            if (language != null)
            {
                _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                    "App.WorkingLanguage",
                    language.LanguageCulture,
                    new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) }
                );
            }

            await Task.CompletedTask;
        }
    }
}
