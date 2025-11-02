using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using App.Core.Domain.Users;
using App.Services.Security;
using QRCoder;
using OtpNet;
using App.Services.Users;
using Petrsnd.OtpCore;

namespace App.Web.Services.Security
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IUserService _userService;

        public TwoFactorService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<string> GenerateTwoFactorSecretAsync(int userId, int secretByteLength = 20)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found", nameof(userId));

            var secret = KeyGeneration.GenerateRandomKey(secretByteLength);
            var base32Secret = Base32Encoding.ToString(secret);

            user.TwoFactorSecret = base32Secret;
            await _userService.UpdateAsync(user);

            return base32Secret;
        }

        public Task<string> GetOtpAuthUriAsync(string secret, string email, string issuer = "App")
        {
            // Convert secret to byte[] as required by Petrsnd.OtpCore.OtpAuthUri
            var secretBytes = Base32Encoding.ToBytes(secret);

            // Use Petrsnd.OtpCore.OtpType.Totp instead of OtpNet.OtpType.Totp
            // Use Petrsnd.OtpCore.OtpHmacAlgorithm.Sha1 for the algorithm (commonly used for TOTP)
            var otpAuth = new OtpAuthUri(
                Petrsnd.OtpCore.OtpType.Totp,
                secretBytes,
                email,
                issuer,
                6,
                Petrsnd.OtpCore.OtpHmacAlgorithm.HmacSha1,
                30
            );
            return Task.FromResult(otpAuth.ToString());
        }

        public async Task<bool> ValidateTwoFactorTokenAsync(int userId, string token, int allowedDriftSteps = 1)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            var secretBytes = Base32Encoding.ToBytes(user.TwoFactorSecret);
            var totp = new OtpNet.Totp(secretBytes);
            return totp.VerifyTotp(token, out long _, new VerificationWindow(allowedDriftSteps, 0));
        }

        public async Task EnableTwoFactorAsync(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found", nameof(userId));

            user.TwoFactorEnabled = true;
            await _userService.UpdateAsync(user);
        }

        public async Task DisableTwoFactorAsync(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found", nameof(userId));

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _userService.UpdateAsync(user);
        }

        public async Task<bool> IsTwoFactorEnabledAsync(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return false;

            return user.TwoFactorEnabled;
        }
    }
}