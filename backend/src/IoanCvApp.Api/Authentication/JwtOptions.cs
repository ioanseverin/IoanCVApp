namespace IoanCvApp.Api.Authentication;

internal sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";
    public const int TokenLifetimeMinutes = 60;
    public const int MinimumSigningKeyBytes = 32;

    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? SigningKey { get; set; }
}
