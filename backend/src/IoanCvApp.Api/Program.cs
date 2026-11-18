using System.Text;
using System.Text.Json;
using IoanCvApp.Application;
using IoanCvApp.Api.Authentication;
using IoanCvApp.Infrastructure;
using IoanCvApp.Infrastructure.Authentication;
using IoanCvApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(services =>
    AuthRuntimeSettings.Load(services.GetRequiredService<IConfiguration>()));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<AuthRuntimeSettings>((options, settings) =>
    {
        options.MapInboundClaims = false;
        if (!settings.IsConfigured)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = "authentication-disabled",
                ValidateAudience = true,
                ValidAudience = "authentication-disabled",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    context.Fail("Bearer authentication is not configured.");
                    return Task.CompletedTask;
                }
            };
            return;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.SigningKey!)),
            ValidateIssuer = true,
            ValidIssuer = settings.Jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Jwt.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser().RequireRole("Admin")));

builder.Services.AddCors();
builder.Services.AddSingleton<IConfigureOptions<CorsOptions>>(services =>
    new ConfigureOptions<CorsOptions>(options =>
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var environment = services.GetRequiredService<IWebHostEnvironment>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("CorsConfiguration");
        var cors = GetAllowedOrigins(configuration, environment.IsDevelopment());
        options.AddPolicy("Frontend", policy =>
        {
            if (cors.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(cors.AllowedOrigins).AllowAnyHeader().AllowAnyMethod();
            }
        });

        if (cors.InvalidCount > 0)
        {
            logger.LogWarning("Ignored {Count} invalid or disallowed CORS origin(s).", cors.InvalidCount);
        }

        if (cors.AllowedOrigins.Length == 0)
        {
            logger.LogWarning("No valid CORS origins are configured; cross-origin requests will not be allowed.");
        }
    }));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "The login request limit has been exceeded."
            },
            contentType: "application/problem+json",
            options: new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken: cancellationToken);
    };
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();
var authSettings = app.Services.GetRequiredService<AuthRuntimeSettings>();

if (!authSettings.IsConfigured)
{
    app.Logger.LogWarning("Administrator authentication is disabled. {Reason}", authSettings.DisabledReason);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await TestDataSeeder.SeedAsync(db);
}

if (authSettings.IsConfigured)
{
    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<AdminAccountSeeder>();
    await seeder.SynchronizeAsync(authSettings.AdminUsername!, authSettings.AdminPassword!);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();

static (string[] AllowedOrigins, int InvalidCount) GetAllowedOrigins(
    IConfiguration configuration,
    bool isDevelopment)
{
    var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    var allowedOrigins = new List<string>();
    var invalidCount = 0;
    foreach (var configuredOrigin in configuredOrigins)
    {
        if (string.IsNullOrWhiteSpace(configuredOrigin))
        {
            invalidCount++;
            continue;
        }

        var origin = configuredOrigin.Trim().TrimEnd('/');
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https")
            || uri.UserInfo.Length > 0
            || uri.AbsolutePath != "/"
            || uri.Query.Length > 0
            || uri.Fragment.Length > 0
            || isDevelopment && !IsLoopbackHost(uri.Host)
            || !isDevelopment && uri.Scheme != Uri.UriSchemeHttps)
        {
            invalidCount++;
            continue;
        }

        allowedOrigins.Add(uri.GetLeftPart(UriPartial.Authority));
    }

    return (allowedOrigins.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(), invalidCount);
}

static bool IsLoopbackHost(string host) =>
    host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
    || host.Equals("127.0.0.1", StringComparison.Ordinal)
    || host.Equals("::1", StringComparison.Ordinal);

public partial class Program;
