namespace App.Core.Domain.Users
{
    /// <summary>
    /// Storage format of user passwords.
    /// </summary>
    public enum PasswordFormat
    {
        Clear = 0,
        Hashed = 1,
        Encrypted = 2
    }
}
