# Plan: ST-03 Authenticate the Single Site Administrator

## Summary
Implement Jira story IOANCVAPP-6: add a single-admin login endpoint that verifies configured credentials and returns a short-lived signed JWT. Persist only the admin username and a standard password hash, validate bearer tokens for a reusable Admin policy, and protect the API with a per-IP login limit and restrictive configurable CORS. Keep production migrations explicit.

Confirmed behavior:
- JWT lifetime is one hour.
- Allow five login attempts per client IP per fixed one-minute window; further attempts receive 429.
- Synchronize the configured admin username and password hash on every startup, after the database schema is available.
- If auth configuration is absent, incomplete, or invalid, keep the public API online; login responds 503 and bearer authorization fails closed. Report configuration errors without exposing secret values.
- Production migrations are applied explicitly before deploying the schema-dependent application.

## User Story
As the site owner, I want to authenticate with a seeded admin account and receive a short-lived JWT, so that only I can access content-changing operations.

## Metadata
| Field | Value |
|---|---|
| Type | NEW_CAPABILITY |
| Complexity | HIGH |
| Projects / Systems | Domain, Application, Infrastructure, API, API.Tests |
| Jira Issue | IOANCVAPP-6 (ST-03; High priority; currently To Do; blocked by ST-01, which is complete) |

## Scope
### In
- Configure credentials through .NET user-secrets in Development and environment/deployment configuration outside Development.
- Add one `AdminUser` record with a unique username and password hash, repository, and EF Core migration.
- On startup, synchronize the one configured admin username and hashed password after schema availability; do not persist or log plaintext passwords.
- Add anonymous `POST /api/auth/login` with validated request data, a generic structured 401 for either unknown username or wrong password, and a 503 when auth is unavailable.
- Issue and validate a signed one-hour JWT with configured issuer/audience, expiry, subject, and Admin role; provide a reusable Admin authorization policy for later CRUD work.
- Throttle login to five requests per client IP per fixed one-minute window.
- Configure restrictive CORS with explicit configured origins; only allow configured localhost development origins in Development. Keep HTTPS redirection.
- Add integration tests, safe `.http` examples, and README setup/configuration documentation.

### Out
- Admin content CRUD endpoints (ST-04 and later), frontend login, public registration, multiple admin accounts, refresh tokens, MFA, audit logging, and Azure deployment changes.
- Production auto-migrations. Apply the migration explicitly before deploying the schema-dependent application.
- Password rotation UI/API; configuration changes take effect after restart.

## Repository Findings and Patterns
| Category | File:Lines | Existing pattern |
|---|---|---|
| SDK | `global.json:1-5` | SDK 9.0.203 with `latestFeature` roll-forward. |
| Project references | `backend/src/IoanCvApp.Api/IoanCvApp.Api.csproj:1-23` | `net9.0` API host references Application and Infrastructure; built-in OpenAPI and Scalar are already configured. |
| Project references | `backend/src/IoanCvApp.Application/IoanCvApp.Application.csproj:1-16` | Application references Domain only. |
| Project references | `backend/src/IoanCvApp.Infrastructure/IoanCvApp.Infrastructure.csproj:1-21` | Infrastructure references Application and Domain; EF Core SQLite 9.0.* is here. |
| Test dependencies | `backend/tests/IoanCvApp.Api.Tests/IoanCvApp.Api.Tests.csproj:1-27` | `net9.0`, xUnit 2.9.2, `WebApplicationFactory` 9.0.*, and references to solution projects. |
| API composition | `backend/src/IoanCvApp.Api/Program.cs:7-31` | Controllers, DI and OpenAPI; only Development migrates and seeds; HTTPS redirection precedes controller mapping. |
| Persistence | `backend/src/IoanCvApp.Infrastructure/Persistence/AppDbContext.cs:9-39` | DbContext owns entity sets and fluent model configuration. |
| Infrastructure DI | `backend/src/IoanCvApp.Infrastructure/DependencyInjection.cs:13-20` | SQLite and scoped repositories registered through `AddInfrastructure`. |
| Controller | `backend/src/IoanCvApp.Api/Controllers/PublicController.cs:7-43` | Attribute-routed controller, response metadata, `ActionResult<T>`, and cancellation tokens. |
| Integration test host | `backend/tests/IoanCvApp.Api.Tests/TestApiFactory.cs:10-58` | `WebApplicationFactory`, isolated open in-memory SQLite, test service override, explicit migration. |
| Tests | `backend/tests/IoanCvApp.Api.Tests/PublicEndpointTests.cs:9-97` | xUnit integration tests verify status codes, response bodies, ordering, and Development startup. |
| Configuration/docs | `backend/src/IoanCvApp.Api/appsettings.json:1-13`, `.gitignore:1-28`, `README.md:11-90` | SQLite settings are documented; local secrets and DB files are ignored; auth setup is not yet documented. |
| Migration workflow | `README.md:79-90`, `.config/dotnet-tools.json:1-13` | Pinned local `dotnet-ef` 9.0.20; documented create/update commands target Infrastructure with API startup. |

There is no existing authentication, authorization, CORS, or rate-limit implementation. Use built-in ASP.NET Core middleware, `PasswordHasher<TUser>` without a full Identity store, and `Microsoft.AspNetCore.Authentication.JwtBearer` 9.0.*. If needed for the Infrastructure class library, add only `Microsoft.Extensions.Identity.Core` 9.0.*. Use the existing EF Core migration and WebApplicationFactory patterns.

## Architecture and Dependency Direction
- `Domain` remains framework-independent and owns `AdminUser` (`Id`, `Username`, `PasswordHash`).
- `Application` references Domain only. Define repository, password-hash, and auth-service abstractions; credential verification returns an authenticated identity/failure, not a JWT.
- `Infrastructure` references Application and Domain. Implement EF persistence, Identity password hashing, startup credential synchronization, and migration.
- `Api` references Application and Infrastructure. Own HTTP contracts/controller, JWT options and issuance/validation, authorization policy, login rate limiting, CORS, and middleware ordering.
- `Api.Tests` uses its existing API test host and in-memory SQLite. Exercise real bearer middleware via a test-only protected controller; do not add a temporary production endpoint.

## Files and Projects
| Path | Action | Purpose |
|---|---|---|
| `backend/src/IoanCvApp.Domain/AdminUser.cs` | CREATE | Single administrator entity storing username/hash only. |
| `backend/src/IoanCvApp.Application/Abstractions/IAdminUserRepository.cs` | CREATE | Load and persist the administrator through an abstraction. |
| `backend/src/IoanCvApp.Application/Abstractions/IPasswordHashService.cs` | CREATE | Framework-independent hash/verify contract. |
| `backend/src/IoanCvApp.Application/Abstractions/IAdminAuthService.cs` | CREATE | Credential verification interface. |
| `backend/src/IoanCvApp.Application/Services/AdminAuthService.cs` | CREATE | Return the same authentication failure for unknown user and wrong password. |
| `backend/src/IoanCvApp.Application/DependencyInjection.cs` | UPDATE | Register the auth service. |
| `backend/src/IoanCvApp.Infrastructure/Persistence/AppDbContext.cs` | UPDATE | Add AdminUsers and unique username constraint. |
| `backend/src/IoanCvApp.Infrastructure/Persistence/AdminUserRepository.cs` | CREATE | Admin persistence. |
| `backend/src/IoanCvApp.Infrastructure/Authentication/IdentityPasswordHashService.cs` | CREATE | Adapter around Identity's standard password hasher. |
| `backend/src/IoanCvApp.Infrastructure/Authentication/AdminAccountSeeder.cs` | CREATE | Synchronize configured username/password hash after schema is available. |
| `backend/src/IoanCvApp.Infrastructure/DependencyInjection.cs` | UPDATE | Register repository and password hasher. |
| `backend/src/IoanCvApp.Infrastructure/IoanCvApp.Infrastructure.csproj` | UPDATE if required | Reference Identity Core 9.0.* if required for the hasher. |
| `backend/src/IoanCvApp.Infrastructure/Persistence/Migrations/*AddAdminUser*.cs`, `AppDbContextModelSnapshot.cs` | CREATE / UPDATE | Schema migration and model snapshot. |
| `backend/src/IoanCvApp.Api/Contracts/LoginRequest.cs` | CREATE | Validated username/password input; never logged. |
| `backend/src/IoanCvApp.Api/Contracts/LoginResponse.cs` | CREATE | Signed token and UTC expiry. |
| `backend/src/IoanCvApp.Api/Controllers/AuthController.cs` | CREATE | Anonymous `POST /api/auth/login`. |
| `backend/src/IoanCvApp.Api/Authentication/JwtOptions.cs` | CREATE | Issuer, audience, key and token lifetime configuration. |
| `backend/src/IoanCvApp.Api/Program.cs` | UPDATE | Auth, authorization, CORS/rate-limit middleware and account provisioning; keep migration behavior Development-only. |
| `backend/src/IoanCvApp.Api/IoanCvApp.Api.csproj` | UPDATE | Add JwtBearer 9.0.*. |
| `backend/tests/IoanCvApp.Api.Tests/TestApiFactory.cs` | UPDATE | Inject test auth configuration and register a test-only protected controller. |
| `backend/tests/IoanCvApp.Api.Tests/AuthEndpointTests.cs` | CREATE | Login, hashing, JWT validation, authorization, rate-limit, CORS, and disabled-auth tests. |
| `backend/src/IoanCvApp.Api/IoanCvApp.Api.http` | UPDATE | Successful and invalid login examples with placeholders only. |
| `README.md` | UPDATE | User-secrets/deployment setup, login, token lifetime, rate limit, CORS, and explicit migration prerequisite. |

## Ordered Tasks
### 1. Admin persistence and provisioning
- **Changes**: Add `AdminUser`, repository/hasher abstractions and Infrastructure implementations. Add a unique username, EF migration, and startup provisioner that synchronizes the configured username and hashed password after the schema is available. Reject blank credentials and rely on the standard password hasher rather than inventing another password policy. Missing, incomplete, or invalid auth configuration disables login without stopping the API. Never log plaintext or secret values.
- **Dependency**: Existing EF Core model/repository patterns. Production schema migration stays explicit.
- **Validation**: Build; apply the migration to fresh disposable SQLite via the test factory; assert only a password hash is persisted and a configured password change updates the hash on startup.

### 2. Application credential validation and API contracts
- **Changes**: Add the login service and sealed request/response records with XML documentation and request validation. Use RFC 7807 `ProblemDetails` for the generic 401; unknown username and wrong password produce indistinguishable auth failures.
- **Validation**: Test valid, invalid, and malformed credentials; verify equal 401 response shape for invalid cases.

### 3. JWT issuance and bearer authorization
- **Changes**: Issue HMAC-SHA256 signed tokens with subject, Admin role, configured issuer/audience, and UTC expiry. Require a cryptographically strong key of at least 256 bits; never use a default or fallback signing key. Configure bearer validation and a reusable Admin policy. Missing, incomplete, or invalid settings keep the host online, disable login with 503, and make protected routes fail closed.
- **Validation**: Use a test-only protected controller to exercise actual bearer middleware. Reject absent, malformed, expired, wrong-signature, wrong-issuer/audience, and missing-Admin-role tokens; accept a valid Admin token.

### 4. Login rate limit, CORS, and HTTPS
- **Changes**: Use a named built-in limiter partitioned by `RemoteIpAddress`, allowing five requests per IP per fixed one-minute window and returning 429 ProblemDetails thereafter. Configure explicit allowed origins; allow only configured localhost origins in Development and read production frontend origins from configuration. Do not hard-code a production origin or use a wildcard. Place CORS before authentication/authorization and endpoint rate limiting after routing. Preserve HTTPS redirection.
- **Validation**: Integration-test limit threshold, CORS allow/deny behavior, and auth-disabled behavior. Do not trust forwarded IP headers unless proxy configuration is explicitly added.

### 5. End-to-end docs and validation
- **Changes**: Update the test factory, `.http` examples, and README with secret setup and security policies.
- **Validation**: Run all solution checks; test Development and Production configuration with in-memory SQLite; test migration only against fresh disposable SQLite; confirm no secrets or database files are tracked.

## Risks and Mitigations
| Risk | Mitigation |
|---|---|
| Plaintext credential or signing key leaks | Use user-secrets/deployment environment; never put real values in source or committed settings, or log them. |
| Production schema missing after deploy | Require explicit migration before deploying the schema-dependent app; never auto-migrate production. |
| Startup synchronization changes the admin password hash | This is intentional: changing secure configured credentials takes effect on restart. |
| Weak/missing signing key | Require at least 256 bits for HMAC-SHA256; never fall back to a known key. Disable auth safely for absent/invalid configuration. |
| Token-generation tests miss validation defects | Exercise configured bearer middleware and Admin policy with an in-memory protected test controller. |
| Proxy IP mismatch affects throttling | Use direct `RemoteIpAddress`; trust forwarded headers only after explicit proxy configuration. |
| CORS blocks the future frontend or overexposes the API | Require explicit configured origins, allow localhost only in Development, and test accepted/rejected origins. |

## Validation
Run from the repository root:
```powershell
dotnet restore backend\IoanCvApp.sln
dotnet format backend\IoanCvApp.sln --verify-no-changes
dotnet build backend\IoanCvApp.sln --no-restore
dotnet test backend\IoanCvApp.sln --no-build
```
Also exercise the documented `.http` login requests with throwaway local secrets, actual bearer authorization, rate limiting, CORS, disabled auth, and migration against fresh disposable SQLite only. Never apply migrations to production or a shared database during validation.

## Acceptance Criteria
- [ ] Valid configured credentials at `POST /api/auth/login` return a signed JWT and UTC expiry.
- [ ] Invalid credentials return the same structured 401 and reveal no credential-specific detail.
- [ ] Only a password hash is persisted; plaintext credentials and signing key are not logged or committed.
- [ ] Bearer authorization rejects invalid signature, issuer/audience, expiry, and missing Admin role; it accepts a valid Admin token.
- [ ] The sixth login attempt from the same IP in a fixed one-minute window returns 429.
- [ ] Missing, incomplete, or invalid auth configuration leaves the API running; login returns 503 and protected requests fail closed.
- [ ] CORS permits only configured frontend origins and approved configured Development localhost origins; HTTPS redirection remains enabled.
- [ ] Migration, format, build, and tests pass; README and `.http` examples contain no secrets.
