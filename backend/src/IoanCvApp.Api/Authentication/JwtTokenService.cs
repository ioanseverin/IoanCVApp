using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IoanCvApp.Api.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace IoanCvApp.Api.Authentication;

internal sealed class JwtTokenService(AuthRuntimeSettings settings) : IJwtTokenService
{
    public LoginResponse CreateToken(string username)
    {
        if (!settings.IsConfigured)
        {
            throw new InvalidOperationException("JWT authentication is not configured.");
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(JwtOptions.TokenLifetimeMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("name", username),
            new Claim("role", "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.SigningKey!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings.Jwt.Issuer,
            audience: settings.Jwt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
