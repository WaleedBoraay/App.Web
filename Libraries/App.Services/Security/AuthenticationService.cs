using App.Core.Domain.Security;
using App.Core.Domain.Users;
using App.Services.Security;
using App.Services.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IEncryptionService _encryptionService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        public AuthenticationService(
            IUserService userService,
            IEncryptionService encryptionService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _userService = userService;
            _encryptionService = encryptionService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

		public async Task<string> GenerateJwtTokenAsync(AppUser user, IList<string> roles)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"] ?? "120"));

			var claims = new List<Claim>
	{
		new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
		new Claim(ClaimTypes.Name, user.Username ?? ""),
		new Claim(ClaimTypes.Email, user.Email ?? ""),
		new Claim("RegistrationId", user.RegistrationId?.ToString() ?? "0")
	};

			foreach (var role in roles)
				claims.Add(new Claim(ClaimTypes.Role, role));

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public async Task<AppUser> ValidateUserAsync(string usernameOrEmail, string password)
        {
            var input = usernameOrEmail.Trim();

            var user = await _userService.GetByUsernameAsync(input)
                       ?? await _userService.GetByEmailAsync(input);

            if (user == null || !user.IsActive)
                return null;

            var valid = _encryptionService.VerifyPassword(
                password,
                user.PasswordHash,
                user.PasswordSalt,
                user.PasswordFormat
            );

            return valid ? user : null;
        }

        public async Task<string> GenerateTokenAsync(AppUser user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtExpiry = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var exp) ? exp : 120;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userService.GetRolesAsync(user.Id);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("userId", user.Id.ToString()),
                new Claim("roles", string.Join(",", roles))
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtIssuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpiry),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task LogoutAsync(int userId)
        {
            return Task.CompletedTask;
        }

        public Task<string> RefreshTokenAsync(string token)
        {
            // Validate old token and issue a new one
            return Task.FromResult(token); // Placeholder, ???? ??????? ?????
        }

        public async Task SignInAsync(AppUser user, bool isPersistent, IList<string> roles)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
    };

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }

        public async Task SignOutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
        }

    }
}
