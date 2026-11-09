using IoanCvApp.Application.Abstractions;
using IoanCvApp.Domain;
using Microsoft.AspNetCore.Identity;

namespace IoanCvApp.Infrastructure.Authentication;

/// <summary>Hashes and verifies administrator passwords with ASP.NET Core Identity.</summary>
public sealed class IdentityPasswordHashService(IPasswordHasher<AdminUser> passwordHasher) : IPasswordHashService
{
    /// <inheritdoc />
    public string HashPassword(AdminUser user, string password) =>
        passwordHasher.HashPassword(user, password);

    /// <inheritdoc />
    public bool VerifyPassword(AdminUser user, string passwordHash, string providedPassword) =>
        passwordHasher.VerifyHashedPassword(user, passwordHash, providedPassword) != PasswordVerificationResult.Failed;
}
