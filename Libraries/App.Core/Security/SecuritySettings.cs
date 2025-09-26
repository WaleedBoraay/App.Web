namespace App.Core.Configuration
{
    /// <summary>
    /// Global security settings used across encryption, hashing and authentication.
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Passphrase to derive AES key (store securely in KeyVault/Secrets).
        /// </summary>
        public string EncryptionPassphrase { get; set; } = "CHANGE_ME_IN_SECRETS";

        /// <summary>
        /// Number of iterations used in PBKDF2 for password hashing.
        /// </summary>
        public int PasswordHashIterations { get; set; } = 100_000;

        /// <summary>
        /// Size of salt in bytes for password hashing.
        /// </summary>
        public int PasswordSaltSize { get; set; } = 32;

        /// <summary>
        /// Default password expiration days (0 = never expires).
        /// </summary>
        public int PasswordExpiryDays { get; set; } = 0;

        /// <summary>
        /// Maximum allowed failed login attempts before lockout.
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;

        /// <summary>
        /// Default lockout duration in minutes (after exceeding failed attempts).
        /// </summary>
        public int DefaultLockoutMinutes { get; set; } = 15;

        /// <summary>
        /// Require two-factor authentication globally.
        /// </summary>
        public bool RequireTwoFactor { get; set; } = false;
    }
}
