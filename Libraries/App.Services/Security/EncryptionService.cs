using App.Core.Configuration;
using App.Core.Domain.Users;
using App.Core.Security;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace App.Services.Security
{
    /// <summary>
    /// Production-grade crypto: PBKDF2 for passwords, AES-256-CBC for encryption.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly SecuritySettings _settings;
        private readonly byte[] _aesKey; // 32 bytes

        public EncryptionService(IOptions<SecuritySettings> settings)
        {
            _settings = settings.Value ?? new SecuritySettings();

            // Derive a 256-bit key from passphrase using SHA256 (fast KDF for key material).
            // Passphrase itself should be stored securely (KeyVault/Secrets).
            using var sha = SHA256.Create();
            _aesKey = sha.ComputeHash(Encoding.UTF8.GetBytes(_settings.EncryptionPassphrase ?? "CHANGE_ME_IN_SECRETS"));
        }

        #region Passwords

        public string CreateSaltKey(int size = 32)
        {
            var bytes = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public string CreatePasswordHash(string password, string saltKey, PasswordFormat format = PasswordFormat.Hashed)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (saltKey == null) throw new ArgumentNullException(nameof(saltKey));

            return format switch
            {
                PasswordFormat.Clear => password,
                PasswordFormat.Encrypted => EncryptText(password),
                PasswordFormat.Hashed => PBKDF2(password, saltKey, _settings.PasswordHashIterations),
                _ => throw new NotSupportedException($"Unsupported password format: {format}")
            };
        }

        public bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt, PasswordFormat format)
        {
            if (enteredPassword == null) return false;

            switch (format)
            {
                case PasswordFormat.Clear:
                    return enteredPassword == storedHash;

                case PasswordFormat.Encrypted:
                    try
                    {
                        var plain = DecryptText(storedHash);
                        return enteredPassword == plain;
                    }
                    catch { return false; }

                case PasswordFormat.Hashed:
                    var computed = PBKDF2(enteredPassword, storedSalt, _settings.PasswordHashIterations);
                    // constant-time compare
                    return FixedTimeEquals(
                        Convert.FromBase64String(computed),
                        Convert.FromBase64String(storedHash));

                default:
                    return false;
            }
        }

        private static string PBKDF2(string password, string saltBase64, int iterations)
        {
            var salt = Convert.FromBase64String(saltBase64);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(32); // 256-bit
            return Convert.ToBase64String(key);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        #endregion

        #region Tokens & Hashes

        public string GenerateSecureToken(int size = 64)
        {
            var bytes = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            // URL-safe Base64 without padding
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            return Convert.ToBase64String(hash);
        }

        public string ComputeHmac(string input, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key ?? string.Empty));
            var mac = hmac.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            return Convert.ToBase64String(mac);
        }

        #endregion

        #region AES-256-CBC

        public string EncryptText(string plainText)
        {
            if (plainText == null) return null;

            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.GenerateIV(); // 16 bytes
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // store IV + cipher as base64
            var payload = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(payload);
        }

        public string DecryptText(string cipherText)
        {
            if (cipherText == null) return null;

            var payload = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[16];
            var cipherBytes = new byte[payload.Length - 16];
            Buffer.BlockCopy(payload, 0, iv, 0, 16);
            Buffer.BlockCopy(payload, 16, cipherBytes, 0, cipherBytes.Length);

            using var decryptor = aes.CreateDecryptor(aes.Key, iv);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        #endregion
    }
}
