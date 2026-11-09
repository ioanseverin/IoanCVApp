using IoanCvApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoanCvApp.Api.Tests;

public sealed class TestApiFactory(
    string environment = "Production",
    bool authConfigured = false,
    bool invalidAuthConfiguration = false) : WebApplicationFactory<Program>
{
    public const string AdminUsername = "test-admin";
    public const string AdminPassword = "test-only-password";
    public const string SigningKey = "test-only-signing-key-with-at-least-32-bytes";
    public const string Issuer = "IoanCvApp.Tests";
    public const string Audience = "IoanCvApp.Tests.Client";

    private readonly SqliteConnection _connection = OpenConnection();

    private static SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = environment == "Development"
                    ? "http://localhost:5173"
                    : "https://frontend.example.test"
            };

            if (authConfigured)
            {
                settings["Authentication:Admin:Username"] = AdminUsername;
                settings["Authentication:Admin:Password"] = AdminPassword;
                settings["Authentication:Jwt:Issuer"] = Issuer;
                settings["Authentication:Jwt:Audience"] = Audience;
                settings["Authentication:Jwt:SigningKey"] = invalidAuthConfiguration
                    ? "too-short"
                    : SigningKey;
            }

            configuration.AddInMemoryCollection(settings);
        });
        builder.ConfigureServices(services =>
        {
            services
                .AddControllers()
                .AddApplicationPart(typeof(ProtectedTestController).Assembly);

            var toRemove = services
                .Where(d => d.ServiceType == typeof(AppDbContext)
                    || d.ServiceType.IsGenericType && d.ServiceType.GenericTypeArguments.Contains(typeof(AppDbContext)))
                .ToList();
            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));
        });
    }

    public async Task InitializeDatabaseAsync(Func<AppDbContext, Task>? seed = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
        if (seed is not null)
        {
            await seed(db);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
