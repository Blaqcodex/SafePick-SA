using SafePick.Application.Common.Interfaces;

namespace SafePick.Infrastructure.Identity;

/// <summary>
/// BCrypt-based password hasher.
///
/// BCrypt is deliberately slow — that is the point. It makes brute-force
/// attacks infeasible. We do not roll our own cryptography.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password must not be empty.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}