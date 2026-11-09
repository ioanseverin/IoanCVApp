# IoanCVApp

Personal CV and portfolio website. The backend foundation is implemented; the frontend and hosting are planned.

- **Backend:** ASP.NET Core 9, with Domain, Application, and Infrastructure projects
- **Persistence:** Entity Framework Core 9 with SQLite
- **Planned frontend:** React
- **Planned hosting:** Azure free tier

## Repository structure

```text
.agents/PRDs/      Product requirements (PRD.md)
.agents/stories/   User stories derived from the PRD (PRD-stories.md)
.agents/plans/     Implementation plans
.agents/skills/    Copilot agent skills
.claude/commands/  Claude workflow commands
.claude/skills/    Claude skills
backend/           .NET solution, source projects, and tests
mcp.json           MCP server configuration (contains no credentials)
```

Backend layout (`backend/`):

```text
src/IoanCvApp.Api            ASP.NET Core host and composition root
src/IoanCvApp.Application    DTOs, mappings, and repository abstractions
src/IoanCvApp.Domain         Profile, experience, project, and administrator entities
src/IoanCvApp.Infrastructure EF Core SQLite persistence, password hashing, and migrations
tests/IoanCvApp.Api.Tests    xUnit API and persistence integration tests
```

## Requirements

- .NET 9 SDK. The repository's `global.json` selects SDK 9.0.203 and allows compatible later feature-band versions.

## Run and test the backend

From the repository root:

```sh
dotnet restore backend/IoanCvApp.sln
dotnet build backend/IoanCvApp.sln --no-restore
dotnet test backend/IoanCvApp.sln --no-build
```

Run the API:

```sh
dotnet run --project backend/src/IoanCvApp.Api
```

The API is built with ASP.NET Core controllers. It provides the application host, persistence wiring, a liveness endpoint, anonymous public read endpoints, and the administrator login endpoint. Admin content write endpoints have not been implemented yet.

### Public endpoints

| Method | Path | Description |
|---|---|---|
| GET | `/api/public/profile` | About/CV profile; `404` with a `ProblemDetails` body when no profile exists |
| GET | `/api/public/experience` | Experience entries in display order (`SortOrder`, then newest start date); `[]` when empty |
| GET | `/api/public/projects` | Projects with tech stack and optional image URL; `[]` when empty |

All three are anonymous. In `Development`, the API applies pending migrations at startup and uses `TestDataSeeder` from Infrastructure to add sample content to each empty content table. The SQLite database is persistent, so seeded rows remain between runs; remove or edit sample rows before entering real content. This seeding and automatic migration are disabled outside `Development`. In other environments, apply migrations using the commands below.

Check that the API is running (default development URL `http://localhost:5106`):

```bash
curl http://localhost:5106/health
```

It returns `200 OK` with `{"status":"Healthy"}`. The check only confirms the process is serving requests; it does not test the database. `backend/src/IoanCvApp.Api/IoanCvApp.Api.http` contains the same request for the VS Code REST Client.

In the `Development` environment the API also serves its OpenAPI document at `http://localhost:5106/openapi/v1.json` and an interactive [Scalar](https://scalar.com) API reference at `http://localhost:5106/scalar` to browse and call endpoints. Both are disabled in other environments.

## Administrator authentication

The API exposes `POST /api/auth/login`. Valid credentials return a signed JWT that expires after one hour. Invalid credentials return the same generic `401` response. The endpoint allows five requests per client IP in a fixed one-minute window; further requests receive `429`. Tokens are validated for signature, issuer, audience, expiry, and the `Admin` role. Content write endpoints will use the reusable `Admin` authorization policy when added.

Configure the single administrator account and JWT signing key outside source control. The API project has a `UserSecretsId`, so add local development values from the repository root:

```powershell
dotnet user-secrets set "Authentication:Admin:Username" "your-admin-username" --project backend\src\IoanCvApp.Api
dotnet user-secrets set "Authentication:Admin:Password" "<your-admin-password>" --project backend\src\IoanCvApp.Api
dotnet user-secrets set "Authentication:Jwt:Issuer" "IoanCvApp" --project backend\src\IoanCvApp.Api
dotnet user-secrets set "Authentication:Jwt:Audience" "IoanCvApp" --project backend\src\IoanCvApp.Api
$signingKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Authentication:Jwt:SigningKey" $signingKey --project backend\src\IoanCvApp.Api
Remove-Variable signingKey
dotnet user-secrets set "Cors:AllowedOrigins:0" "http://localhost:5173" --project backend\src\IoanCvApp.Api
```

Use an adequately strong password and keep all secret values private. User-secrets are for local development only. In deployment, set the same keys using the platform's secure configuration mechanism and environment variable names such as `Authentication__Admin__Password` and `Authentication__Jwt__SigningKey`. The signing key must contain at least 32 UTF-8 bytes. Set `Cors__AllowedOrigins__0` to the exact HTTPS frontend origin in production. In Development, only explicitly configured `localhost`, `127.0.0.1`, or `::1` origins are accepted; no CORS origins are enabled by default.

On each startup with valid authentication configuration, the API synchronizes the configured username and password hash to its single admin account. Changing the configured credentials takes effect after restart. Only the Identity password hash is persisted; plaintext credentials and signing keys are not stored in the database. If any required authentication setting is missing or invalid, the public API remains available, login returns `503`, and protected routes reject authentication. The API logs which setting names are invalid, never their values. Do not deploy with valid auth configuration until the database migration has been applied.

Login rate limiting uses the remote client IP observed by the API. Forwarded IP headers are not trusted automatically; deployments behind a proxy need explicit trusted-proxy configuration before relying on per-client-IP limits.

## SQLite database and migrations

The connection string is configured as `ConnectionStrings:Default` in `backend/src/IoanCvApp.Api/appsettings.json`, with `Data Source=ioancvapp.db` by default. The database file is local and ignored by Git.

Restore the repository's local EF Core tool and apply migrations:

```sh
dotnet tool restore
dotnet tool run dotnet-ef database update --project backend/src/IoanCvApp.Infrastructure --startup-project backend/src/IoanCvApp.Api
```

Create a migration after changing the persistence model:

```sh
dotnet tool run dotnet-ef migrations add <Name> --project backend/src/IoanCvApp.Infrastructure --startup-project backend/src/IoanCvApp.Api --output-dir Persistence/Migrations
```

See `.agents/PRDs/PRD.md` for product scope and phases, and `.agents/stories/PRD-stories.md` for the story breakdown.
