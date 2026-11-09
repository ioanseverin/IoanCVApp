using IoanCvApp.Api.Contracts;

namespace IoanCvApp.Api.Authentication;

/// <summary>Creates signed administrator access tokens.</summary>
public interface IJwtTokenService
{
    /// <summary>Creates a signed access token for the authenticated administrator.</summary>
    /// <param name="username">The administrator username.</param>
    /// <returns>The access token and its UTC expiration time.</returns>
    LoginResponse CreateToken(string username);
}
