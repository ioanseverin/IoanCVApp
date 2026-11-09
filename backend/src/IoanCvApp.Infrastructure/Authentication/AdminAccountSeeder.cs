using IoanCvApp.Application.Abstractions;
using IoanCvApp.Domain;

namespace IoanCvApp.Infrastructure.Authentication;

/// <summary>Synchronizes the configured administrator account with persisted credentials.</summary>
public sealed class AdminAccountSeeder(
    IAdminUserRepository users,
    IPasswordHashService passwordHasher)
{
    /// <summary>Hashes and persists the configured administrator credentials.</summary>
    /// <param name="username">The configured administrator username.</param>
    /// <param name="password">The configured plaintext password.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    public async Task SynchronizeAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var user = new AdminUser { Username = username };
        var passwordHash = passwordHasher.HashPassword(user, password);
        await users.SynchronizeConfiguredAdminAsync(username, passwordHash, cancellationToken);
    }
}
