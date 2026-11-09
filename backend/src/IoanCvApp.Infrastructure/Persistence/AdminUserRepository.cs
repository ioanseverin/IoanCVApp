using IoanCvApp.Application.Abstractions;
using IoanCvApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace IoanCvApp.Infrastructure.Persistence;

/// <summary>Persists the configured administrator using Entity Framework Core.</summary>
public sealed class AdminUserRepository(AppDbContext db) : IAdminUserRepository
{
    /// <inheritdoc />
    public Task<AdminUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        db.AdminUsers.AsNoTracking().SingleOrDefaultAsync(
            user => user.Username == username,
            cancellationToken);

    /// <inheritdoc />
    public async Task SynchronizeConfiguredAdminAsync(
        string username,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        var user = await db.AdminUsers.SingleOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            db.AdminUsers.Add(new AdminUser
            {
                Username = username,
                PasswordHash = passwordHash
            });
        }
        else
        {
            user.Username = username;
            user.PasswordHash = passwordHash;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
