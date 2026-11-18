using System.ComponentModel.DataAnnotations;

namespace IoanCvApp.Api.Contracts;

/// <summary>Contains the administrator credentials submitted for authentication.</summary>
public sealed record LoginRequest
{
    /// <summary>Gets the administrator username.</summary>
    [Required, StringLength(100)]
    public required string Username { get; init; }

    /// <summary>Gets the administrator password.</summary>
    [Required, StringLength(1024)]
    public required string Password { get; init; }
}
