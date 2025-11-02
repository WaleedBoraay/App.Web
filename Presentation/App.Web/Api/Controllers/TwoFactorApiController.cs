using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Security;
using App.Services.Users;
using App.Services;
using App.Core.Domain.Users;

namespace App.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/2fa")]
    public class TwoFactorApiController : ControllerBase
    {
        private readonly ITwoFactorService _twoFactorService;
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;

        public TwoFactorApiController(
            ITwoFactorService twoFactorService,
            IUserService userService,
            IWorkContext workContext)
        {
            _twoFactorService = twoFactorService;
            _userService = userService;
            _workContext = workContext;
        }

        // GET: api/Account/2fa/generate
        // Generates a new secret for the current user and returns the secret + otpauth URI.
        [HttpGet("generate")]
        [Authorize]
        public async Task<IActionResult> GenerateSecret()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null) return Unauthorized();

            var secret = await _twoFactorService.GenerateTwoFactorSecretAsync(currentUser.Id);
            var otpAuthUri = await _twoFactorService.GetOtpAuthUriAsync(secret, currentUser.Email, issuer: "App");

            return Ok(new
            {
                Secret = secret,
                OtpAuthUri = otpAuthUri
                // Frontend can render QR from the URI (no QR image generated here to keep server lightweight)
            });
        }

        // POST: api/Account/2fa/enable
        // Body: { "token": "123456" }
        [HttpPost("enable")]
        [Authorize]
        public async Task<IActionResult> Enable([FromBody] TwoFactorTokenModel model)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null) return Unauthorized();

            var valid = await _twoFactorService.ValidateTwoFactorTokenAsync(currentUser.Id, model.Token);
            if (!valid) return BadRequest(new { message = "Invalid two-factor token." });

            await _twoFactorService.EnableTwoFactorAsync(currentUser.Id);
            return Ok(new { message = "Two-factor authentication enabled." });
        }

        // POST: api/Account/2fa/disable
        [HttpPost("disable")]
        [Authorize]
        public async Task<IActionResult> Disable()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null) return Unauthorized();

            await _twoFactorService.DisableTwoFactorAsync(currentUser.Id);
            return Ok(new { message = "Two-factor authentication disabled." });
        }


        [AllowAnonymous]
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] TwoFactorValidateModel model)
        {
            var ok = await _twoFactorService.ValidateTwoFactorTokenAsync(model.UserId, model.Token);
            if (!ok) return BadRequest(new { message = "Invalid token." });
            return Ok(new { message = "Valid token." });
        }
    }

    public class TwoFactorTokenModel
    {
        public string Token { get; set; } = string.Empty;
    }

    public class TwoFactorValidateModel
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}