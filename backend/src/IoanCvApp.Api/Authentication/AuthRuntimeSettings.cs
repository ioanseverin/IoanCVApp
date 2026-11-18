using System.Text;
using Microsoft.Extensions.Configuration;

namespace IoanCvApp.Api.Authentication;

/// <summary>Represents validated administrator authentication configuration.</summary>
public sealed class AuthRuntimeSettings
{
    public const string AdminUsernameKey = "Authentication:Admin:Username";
    public const string AdminPasswordKey = "Authentication:Admin:Password";

    private AuthRuntimeSettings(
        JwtOptions jwt,
        string? adminUsername,
        string? adminPassword,
        string? disabledReason)
    {
        Jwt = jwt;
        AdminUsername = adminUsername;
        AdminPassword = adminPassword;
        DisabledReason = disabledReason;
    }

    internal JwtOptions Jwt { get; }
    internal string? AdminUsername { get; }
    internal string? AdminPassword { get; }
    internal string? DisabledReason { get; }
    internal bool IsConfigured => DisabledReason is null;

    internal static AuthRuntimeSettings Load(IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var username = configuration[AdminUsernameKey];
        var password = configuration[AdminPasswordKey];
        var invalidSettings = new List<string>();

        if (string.IsNullOrWhiteSpace(username)
            || username.Length > 100
            || !string.Equals(username, username.Trim(), StringComparison.Ordinal))
        {
            invalidSettings.Add(AdminUsernameKey);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            invalidSettings.Add(AdminPasswordKey);
        }

        if (string.IsNullOrWhiteSpace(jwt.Issuer))
        {
            invalidSettings.Add($"{JwtOptions.SectionName}:Issuer");
        }

        if (string.IsNullOrWhiteSpace(jwt.Audience))
        {
            invalidSettings.Add($"{JwtOptions.SectionName}:Audience");
        }

        if (string.IsNullOrWhiteSpace(jwt.SigningKey)
            || Encoding.UTF8.GetByteCount(jwt.SigningKey) < JwtOptions.MinimumSigningKeyBytes)
        {
            invalidSettings.Add($"{JwtOptions.SectionName}:SigningKey");
        }

        var reason = invalidSettings.Count == 0
            ? null
            : $"Authentication settings are missing or invalid: {string.Join(", ", invalidSettings)}.";
        return new AuthRuntimeSettings(jwt, username, password, reason);
    }
}
