using System.Threading.Tasks;
using App.Core.Domain.Security;
using App.Core.Domain.Users;

namespace App.Services.Authentication
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Validate username & password, return user if success.
        /// </summary>
        Task<AppUser> ValidateUserAsync(string username, string password);

        /// <summary>
        /// Generate JWT or session token for authenticated user.
        /// </summary>
        Task<string> GenerateTokenAsync(AppUser user);

        /// <summary>
        /// Invalidate user session / token.
        /// </summary>
        Task LogoutAsync(int userId);

        /// <summary>
        /// Refresh JWT token (for long-running sessions).
        /// </summary>
        Task<string> RefreshTokenAsync(string token);

        Task SignInAsync(AppUser user, bool isPersistent, IList<string> roles);
        Task SignOutAsync();


    }
}
