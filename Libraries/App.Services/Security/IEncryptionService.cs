using App.Core.Domain.Users;

namespace App.Services.Security
{
    public interface IEncryptionService
    {
        /// <summary>
        /// Create a hash for a given password with salt & format.
        /// </summary>
        string CreatePasswordHash(string password, string saltKey, PasswordFormat format = PasswordFormat.Hashed);

        /// <summary>
        /// Generate a new random salt.
        /// </summary>
        string CreateSaltKey(int size = 32);

        /// <summary>
        /// Verify a password against hash+salt.
        /// </summary>
        bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt, PasswordFormat format);

        /// <summary>
        /// Generate a random secure token (for password reset, MFA).
        /// </summary>
        string GenerateSecureToken(int size = 64);

        /// <summary>
        /// Encrypt plain text with system key.
        /// </summary>
        string EncryptText(string plainText);

        /// <summary>
        /// Decrypt text with system key.
        /// </summary>
        string DecryptText(string cipherText);

        /// <summary>
        /// Compute SHA256 hash of a string.
        /// </summary>
        string ComputeSha256(string input);

        /// <summary>
        /// Compute HMAC (keyed hash).
        /// </summary>
        string ComputeHmac(string input, string key);
    }
}
