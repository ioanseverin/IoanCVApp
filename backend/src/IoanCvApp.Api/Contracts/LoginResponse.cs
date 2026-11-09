namespace IoanCvApp.Api.Contracts;

/// <summary>Contains the access token issued after successful administrator authentication.</summary>
/// <param name="AccessToken">The signed bearer token.</param>
/// <param name="ExpiresAtUtc">The time at which the bearer token expires.</param>
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);
