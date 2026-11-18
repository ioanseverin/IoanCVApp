namespace IoanCvApp.Application.Abstractions;

/// <summary>Authenticates credentials for the configured administrator.</summary>
public interface IAdminAuthService
{
    /// <summary>Authenticates the supplied administrator credentials.</summary>
    /// <param name="username">The administrator username.</param>
    /// <param name="password">The administrator password.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>The authenticated username, or <see langword="null" /> when the credentials are invalid.</returns>
    Task<string?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}
