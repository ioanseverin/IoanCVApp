using IoanCvApp.Domain;

namespace IoanCvApp.Application.Abstractions;

/// <summary>Hashes and verifies administrator passwords.</summary>
public interface IPasswordHashService
{
    /// <summary>Creates a password hash for the specified administrator.</summary>
    /// <param name="user">The administrator associated with the password.</param>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>The encoded password hash.</returns>
    string HashPassword(AdminUser user, string password);

    /// <summary>Verifies a plaintext password against a stored hash.</summary>
    /// <param name="user">The administrator associated with the password hash.</param>
    /// <param name="passwordHash">The stored encoded password hash.</param>
    /// <param name="providedPassword">The plaintext password to verify.</param>
    /// <returns><see langword="true" /> if the password matches; otherwise, <see langword="false" />.</returns>
    bool VerifyPassword(AdminUser user, string passwordHash, string providedPassword);
}
