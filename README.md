# IoanCVApp

Personal CV / portfolio website.

- **Backend:** ASP.NET Core 9 Web API
- **Frontend:** React
- **Database:** SQLite
- **Hosting:** Azure free tier

## Repository structure

```
.agents/PRDs/      Product requirements (PRD.md)
.agents/stories/   User stories derived from the PRD (PRD-stories.md)
.agents/skills/    Copilot/agent skills (plan, implement, dotnet-webapi)
.claude/commands/  AI workflow commands
backend/           ASP.NET Core solution (IoanCvApp.sln)
mcp.json           MCP server configuration (Atlassian, browser OAuth - no secrets)
```

Backend layout (`backend/`):

```
src/IoanCvApp.Domain          Entities (Profile, ExperienceEntry, Project)
src/IoanCvApp.Application     DTOs, mapping, repository abstractions
src/IoanCvApp.Infrastructure  EF Core SQLite DbContext, migrations, repositories
src/IoanCvApp.Api            ASP.NET Core host (composition root)
tests/IoanCvApp.Api.Tests     xUnit tests
```

Planned:

```
frontend/        React app
```

## Backend

```
dotnet restore backend/IoanCvApp.sln
dotnet build backend/IoanCvApp.sln
dotnet test backend/IoanCvApp.sln
```

The API uses a local SQLite file (`ConnectionStrings:Default`, default `ioancvapp.db`). Add a migration with the local tool:

```
dotnet tool restore
dotnet tool run dotnet-ef migrations add <Name> --project backend/src/IoanCvApp.Infrastructure --startup-project backend/src/IoanCvApp.Api --output-dir Persistence/Migrations
```

## Getting started

```
git clone https://github.com/ioanseverin/IoanCVApp.git
```

See `.agents/PRDs/PRD.md` for scope and phases, and `.agents/stories/PRD-stories.md` for the story breakdown.
