namespace SafePick.Application.Common.Interfaces;

/// <summary>
/// Hashes and verifies passwords. Defined in Application so the use case
/// depends on an abstraction; implemented in Infrastructure (BCrypt or
/// ASP.NET Core Identity's hasher).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}