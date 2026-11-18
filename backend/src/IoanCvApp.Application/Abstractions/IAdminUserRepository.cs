using IoanCvApp.Domain;

namespace IoanCvApp.Application.Abstractions;

/// <summary>Provides persistence operations for the configured administrator.</summary>
public interface IAdminUserRepository
{
    /// <summary>Finds the administrator with the specified username.</summary>
    /// <param name="username">The administrator username.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>The matching administrator, or <see langword="null" /> when none exists.</returns>
    Task<AdminUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the single configured administrator account.</summary>
    /// <param name="username">The configured administrator username.</param>
    /// <param name="passwordHash">The hash of the configured administrator password.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    Task SynchronizeConfiguredAdminAsync(string username, string passwordHash, CancellationToken cancellationToken = default);
}
