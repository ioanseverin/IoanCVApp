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
src/IoanCvApp.Domain         Profile, experience, and project entities
src/IoanCvApp.Infrastructure EF Core SQLite persistence and migrations
tests/IoanCvApp.Api.Tests    xUnit persistence tests
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

The API currently provides the application host, persistence wiring, and a liveness endpoint; content CRUD endpoints have not been implemented yet.

Check that the API is running (default development URL `http://localhost:5106`):

```bash
curl http://localhost:5106/health
```

It returns `200 OK` with `{"status":"Healthy"}`. The check only confirms the process is serving requests; it does not test the database. `backend/src/IoanCvApp.Api/IoanCvApp.Api.http` contains the same request for the VS Code REST Client.

In the `Development` environment the API also serves its OpenAPI document at `http://localhost:5106/openapi/v1.json` and an interactive [Scalar](https://scalar.com) API reference at `http://localhost:5106/scalar` to browse and call endpoints. Both are disabled in other environments.

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
