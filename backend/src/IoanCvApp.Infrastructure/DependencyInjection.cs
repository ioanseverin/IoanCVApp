using IoanCvApp.Application.Abstractions;
using IoanCvApp.Domain;
using IoanCvApp.Infrastructure.Authentication;
using IoanCvApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoanCvApp.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IPasswordHashService, IdentityPasswordHashService>();
        services.AddScoped<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
        services.AddScoped<AdminAccountSeeder>();
        return services;
    }
}
