using IoanCvApp.Application.Abstractions;

namespace IoanCvApp.Application.Services;

/// <summary>Authenticates administrator credentials using persisted password hashes.</summary>
public sealed class AdminAuthService(
    IAdminUserRepository users,
    IPasswordHashService passwordHasher) : IAdminAuthService
{
    /// <inheritdoc />
    public async Task<string?> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByUsernameAsync(username, cancellationToken);
        if (user is null || !passwordHasher.VerifyPassword(user, user.PasswordHash, password))
        {
            return null;
        }

        return user.Username;
    }
}
