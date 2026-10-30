namespace IoanCvApp.Api.Contracts;

/// <summary>Represents the liveness status of the API.</summary>
/// <param name="Status">The service status; <c>Healthy</c> when the API is running.</param>
public sealed record HealthResponse(string Status);
