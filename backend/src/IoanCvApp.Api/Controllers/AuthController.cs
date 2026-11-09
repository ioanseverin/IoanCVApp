using IoanCvApp.Api.Authentication;
using IoanCvApp.Api.Contracts;
using IoanCvApp.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IoanCvApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Authentication")]
public sealed class AuthController(
    AuthRuntimeSettings settings,
    IAdminAuthService adminAuth,
    IJwtTokenService tokens) : ControllerBase
{
    /// <summary>Authenticates the configured administrator and issues a JWT.</summary>
    /// <param name="request">The administrator credentials.</param>
    /// <returns>The signed access token and its expiration time.</returns>
    /// <response code="200">The credentials are valid.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="401">The credentials are invalid.</response>
    /// <response code="429">The login rate limit was exceeded.</response>
    /// <response code="503">Authentication is not configured.</response>
    [AllowAnonymous]
    [HttpPost("login", Name = "AdminLogin")]
    [EnableRateLimiting("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!settings.IsConfigured)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Authentication unavailable",
                detail: "Administrator authentication is not configured.");
        }

        var username = await adminAuth.AuthenticateAsync(
            request.Username,
            request.Password,
            cancellationToken);
        if (username is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication failed",
                Detail = "The username or password is incorrect."
            });
        }

        return Ok(tokens.CreateToken(username));
    }
}
