using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using IoanCvApp.Application.Abstractions;
using IoanCvApp.Api.Contracts;
using IoanCvApp.Infrastructure.Authentication;
using IoanCvApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IoanCvApp.Api.Tests;

public sealed class AuthEndpointTests
{
    [Fact]
    public async Task Valid_credentials_return_short_lived_token_accepted_by_admin_policy()
    {
        using var factory = new TestApiFactory(authConfigured: true);
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = TestApiFactory.AdminUsername,
            Password = TestApiFactory.AdminPassword
        });
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(login);
        Assert.InRange(
            login.ExpiresAtUtc,
            DateTimeOffset.UtcNow.AddMinutes(59),
            DateTimeOffset.UtcNow.AddMinutes(61));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(login.AccessToken);
        Assert.Equal(TestApiFactory.AdminUsername, jwt.Subject);
        Assert.Equal("Admin", jwt.Claims.Single(claim => claim.Type == "role").Value);
        Assert.Equal(TestApiFactory.Issuer, jwt.Issuer);
        Assert.Contains(TestApiFactory.Audience, jwt.Audiences);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var protectedResponse = await client.GetAsync("/test/protected");
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }

    [Theory]
    [InlineData("unknown-user", TestApiFactory.AdminPassword)]
    [InlineData(TestApiFactory.AdminUsername, "wrong-password")]
    public async Task Invalid_credentials_return_the_same_structured_unauthorized_response(
        string username,
        string password)
    {
        using var factory = new TestApiFactory(authConfigured: true);
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = username,
            Password = password
        });
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsPayload>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Authentication failed", problem?.Title);
        Assert.Equal("The username or password is incorrect.", problem?.Detail);
        Assert.Equal((int)HttpStatusCode.Unauthorized, problem?.Status);
    }

    [Fact]
    public async Task Missing_credentials_return_validation_problem_details()
    {
        using var factory = new TestApiFactory(authConfigured: true);
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/auth/login", new { username = TestApiFactory.AdminUsername });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Missing_or_invalid_auth_configuration_keeps_login_unavailable_and_protected_routes_closed(
        bool authConfigured,
        bool invalidAuthConfiguration)
    {
        using var factory = new TestApiFactory(
            authConfigured: authConfigured,
            invalidAuthConfiguration: invalidAuthConfiguration);
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = TestApiFactory.AdminUsername,
            Password = TestApiFactory.AdminPassword
        });
        var unauthenticatedResponse = await client.GetAsync("/test/protected");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken("valid"));
        var signedTokenResponse = await client.GetAsync("/test/protected");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, login.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticatedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, signedTokenResponse.StatusCode);
    }

    [Theory]
    [InlineData("signature")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("expired")]
    [InlineData("missing-role")]
    [InlineData("malformed")]
    public async Task Invalid_bearer_tokens_are_rejected_by_admin_policy(string tokenType)
    {
        using var factory = new TestApiFactory(authConfigured: true);
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenType == "malformed" ? "not-a-jwt" : CreateToken(tokenType));

        var response = await client.GetAsync("/test/protected");

        Assert.Equal(
            tokenType == "missing-role" ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Configured_password_is_only_persisted_as_a_hash_and_is_synchronized()
    {
        using var factory = new TestApiFactory(authConfigured: true);
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();
        await client.GetAsync("/health");
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminAccountSeeder>();
        var admin = await db.AdminUsers.SingleAsync();
        var originalHash = admin.PasswordHash;

        Assert.NotEqual(TestApiFactory.AdminPassword, originalHash);
        Assert.True(hasher.VerifyPassword(admin, originalHash, TestApiFactory.AdminPassword));

        await seeder.SynchronizeAsync("rotated-admin", "rotated-test-password");

        Assert.Equal("rotated-admin", admin.Username);
        Assert.NotEqual(originalHash, admin.PasswordHash);
        Assert.NotEqual("rotated-test-password", admin.PasswordHash);
        Assert.True(hasher.VerifyPassword(admin, admin.PasswordHash, "rotated-test-password"));
    }

    [Fact]
    public async Task Sixth_login_attempt_from_one_ip_is_throttled()
    {
        using var factory = new TestApiFactory(authConfigured: true);
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();
        var statuses = new List<HttpStatusCode>();
        ProblemDetailsPayload? rejection = null;
        string? rejectionMediaType = null;

        for (var attempt = 0; attempt < 6; attempt++)
        {
            using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Username = TestApiFactory.AdminUsername,
                Password = "wrong-password"
            });
            statuses.Add(response.StatusCode);
            if (attempt == 5)
            {
                rejectionMediaType = response.Content.Headers.ContentType?.MediaType;
                rejection = await response.Content.ReadFromJsonAsync<ProblemDetailsPayload>();
            }
        }

        Assert.Equal(Enumerable.Repeat(HttpStatusCode.Unauthorized, 5), statuses.Take(5));
        Assert.Equal(HttpStatusCode.TooManyRequests, statuses[5]);
        Assert.Equal("application/problem+json", rejectionMediaType);
        Assert.Equal((int)HttpStatusCode.TooManyRequests, rejection?.Status);
        Assert.Equal("Too many requests", rejection?.Title);
    }

    [Theory]
    [InlineData("Development", "http://localhost:5173", true)]
    [InlineData("Development", "https://frontend.example.test", false)]
    [InlineData("Production", "https://frontend.example.test", true)]
    [InlineData("Production", "https://unapproved.example.test", false)]
    [InlineData("Production", "http://frontend.example.test", false)]
    public async Task Cors_only_allows_configured_origins(
        string environment,
        string origin,
        bool allowed)
    {
        using var factory = new TestApiFactory(environment);
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/public/experience");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        using var response = await client.SendAsync(request);
        var allowOrigin = response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values)
            ? values.Single()
            : null;

        Assert.Equal(allowed ? origin : null, allowOrigin);
    }

    private static string CreateToken(string tokenType)
    {
        var key = tokenType == "signature"
            ? "different-test-signing-key-with-at-least-32-bytes"
            : TestApiFactory.SigningKey;
        var issuer = tokenType == "issuer" ? "WrongIssuer" : TestApiFactory.Issuer;
        var audience = tokenType == "audience" ? "WrongAudience" : TestApiFactory.Audience;
        var expires = tokenType == "expired"
            ? DateTime.UtcNow.AddMinutes(-1)
            : DateTime.UtcNow.AddMinutes(10);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, TestApiFactory.AdminUsername),
            new("name", TestApiFactory.AdminUsername)
        };
        if (tokenType != "missing-role")
        {
            claims.Add(new Claim("role", "Admin"));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record ProblemDetailsPayload(int? Status, string? Title, string? Detail);
}
