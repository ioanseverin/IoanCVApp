# Plan: ST-01 Backend domain and SQLite persistence (IOANCVAPP-10)

## Problem / approach
Greenfield backend (no `/backend` exists). Create a .NET 9 solution under `/backend` with onion layering, EF Core 9 + SQLite, an initial migration for Profile, ExperienceEntry and Project, DTOs at the API boundary, and a persistence test. Scope stops before CRUD endpoints, auth and frontend (later stories).

## Decisions (carried over from earlier ST-01 planning)
- Layout `/backend/src` and `/backend/tests` per PRD section 6.
- String lists (skills, tech stack, tags) stored as JSON text columns via EF value converter with a value comparer.
- Application layer added for strict layering; the earlier chain `API -> Infrastructure -> Domain` was rejected.
- Pin `global.json` to the installed 10.0 SDK? No: target `net9.0`, `rollForward: latestFeature` with SDK 9.0.203 (also installed; avoids slnx surprises). Use classic `.sln`.

## Dependency direction
- Domain: no references.
- Application: Domain (DTOs, mapping, repository/service abstractions).
- Infrastructure: Application, Domain (DbContext, configurations, migrations, repository implementations, `AddInfrastructure` DI extension).
- Api: Application and Infrastructure (composition root only).
- Tests: Api, Infrastructure, Domain.

## Files / projects
- `global.json`, `backend/IoanCvApp.sln`
- `backend/src/IoanCvApp.Domain`: `Profile`, `ExperienceEntry`, `Project` (minimal, text-forward; string lists).
- `backend/src/IoanCvApp.Application`: DTOs (`ProfileDto`, `ExperienceEntryDto`, `ProjectDto`), mapping extensions, `IContentRepository`-style abstractions.
- `backend/src/IoanCvApp.Infrastructure`: `AppDbContext`, entity configurations, JSON list converter, `Migrations/InitialCreate`, DI extension.
- `backend/src/IoanCvApp.Api`: `Program.cs` (SQLite connection string from config, no secrets), `appsettings.json` with a local `Data Source`.
- `backend/tests/IoanCvApp.Api.Tests`: xUnit, in-memory (shared-connection) SQLite, migration applied then store/read round-trip of all three entities including string lists; a check that no entity is exposed directly (DTO mapping test).
- `README.md`: fix structure section to `/backend/...`; `.gitignore`: ignore `*.db`, `*.db-shm`, `*.db-wal`.

## Todos (ordered)
1. scaffold-solution: global.json, sln, four src projects, test project, references.
2. domain-entities: entities in Domain.
3. application-dtos: DTOs, mappers, abstractions.
4. infra-persistence: DbContext, configurations, JSON converter, DI extension.
5. api-wiring: Program.cs, config, SQLite path.
6. initial-migration: create `InitialCreate` (local `dotnet-ef` tool via tool manifest, not global).
7. tests: migration plus round-trip plus DTO mapping tests.
8. docs-gitignore: README and `.gitignore` updates.
9. final-validation: `dotnet restore/build/test`, `git diff --check`, verify migration on a disposable SQLite file, confirm API starts.

## Validation
```powershell
dotnet restore backend\IoanCvApp.sln
dotnet build backend\IoanCvApp.sln --no-restore
dotnet test backend\IoanCvApp.sln --no-build
```

## Risks / notes
- Jira lists ST-00 (Done) as a blocker; the stories file says none. Harmless, flagged for a sync.
- SQLite lacks some EF features (e.g. DateTimeOffset ordering); use `DateOnly` for experience dates.
- A local tool manifest (`.config/dotnet-tools.json`) will be committed for `dotnet-ef`.
- No commit or push without explicit approval; commit prefix `IOANCVAPP-10:`.
