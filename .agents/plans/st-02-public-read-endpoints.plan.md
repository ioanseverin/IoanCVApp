# Plan: ST-02 Public read endpoints (IOANCVAPP-5)
After approval this is saved to `.agents/plans/st-02-public-read-endpoints.plan.md` per the repo `plan` skill (blocked in plan mode).

## Summary
Switch the API from minimal APIs to controllers, and add anonymous `GET /api/public/profile`, `/experience` and `/projects` as a `PublicController`. The existing `/health` endpoint becomes a `HealthController` with the same route and response. The public controller goes through a new `IContentService` in Application, which wraps the existing `IContentRepository` and `ToDto()` mapping. Empty content returns `[]` for the lists and `404 ProblemDetails` for the profile. Development-only sample data is seeded. Type: NEW_CAPABILITY plus REFACTOR (minimal APIs to controllers), complexity MEDIUM.

## Scope
In: minimal-API-to-controller switch (`AddControllers`/`MapControllers`, `HealthController`, remove `Endpoints\`), `PublicController`, service layer, OpenAPI metadata (visible in Scalar), Development-only seeder in Infrastructure, `.http` requests, integration tests, README.
Out: auth and admin (ST-03+), frontend, CORS, caching, pagination, entity or migration changes, global exception handler.

## Findings
- Current style is minimal APIs: static `Map<X>(this WebApplication)` with `TypedResults` (`HealthEndpoints.cs`). There is no controller precedent, so controllers are a proposed convention: `[ApiController]`, attribute routing, `ActionResult<T>`, `[ProducesResponseType]`, and automatic `ProblemDetails` for errors.
- `AddOpenApi()`, `MapOpenApi()` and Scalar work with controllers unchanged.
- `HealthEndpointTests` and `ApiDocumentationTests` hit URLs only, so they should pass unchanged after the switch.
- `IContentRepository` already has the three read methods (`AsNoTracking`, ordered by `SortOrder`). DTOs and `ToDto()` exist.
- No Application DI extension yet (propose `AddApplication()`). Tests use xUnit with `WebApplicationFactory<Program>`. There is no seeding precedent.
- The PRD (Section 6) says endpoints depend on services, not the DbContext.

## Dependency direction
Api references Application and Infrastructure. Infrastructure references Application and Domain. Application references Domain only. No new references. The seeder lives in Infrastructure and is called from `Program.cs` only.

## Files
- CREATE `Application\Abstractions\IContentService.cs`, `Application\Services\ContentService.cs`, `Application\DependencyInjection.cs`
- CREATE `Api\Controllers\HealthController.cs` (`[Route("health")]`, returns `HealthResponse`, name `GetHealth`)
- CREATE `Api\Controllers\PublicController.cs` (`[Route("api/public")]`; profile returns `ActionResult<ProfileDto>` with 404 `ProblemDetails`; lists return `ActionResult<IReadOnlyList<…>>`; all take a `CancellationToken`)
- DELETE `Api\Endpoints\HealthEndpoints.cs` (and the `Endpoints` folder)
- UPDATE `Api\Program.cs` (`AddControllers`, `MapControllers`, `AddApplication`, Development migrate and seed; remove `MapHealth`)
- CREATE `Infrastructure\Persistence\TestDataSeeder.cs` (Development-only sample data; no credentials)
- UPDATE `Api\IoanCvApp.Api.http`, `README.md`
- CREATE `tests\IoanCvApp.Api.Tests\PublicEndpointTests.cs`

## Tasks (todos tracked in SQL)
1. Service layer, then build.
2. Switch to controllers: `AddControllers()`/`MapControllers()`, port `/health` to `HealthController`, delete `HealthEndpoints`, and run the existing 5 tests to confirm no behaviour change.
3. `PublicController` actions (`GetPublicProfile`, `GetPublicExperience`, `GetPublicProjects`) with `ProducesResponseType` metadata.
4. Development seeder: in `IsDevelopment()` only, apply pending migrations and seed non-sensitive sample data using Infrastructure's `TestDataSeeder`; it adds content only to empty tables. Tests must use an in-memory SQLite database and verify Production does not seed.
5. Tests with in-memory SQLite override:
   - Empty DB: lists return `[]`, profile returns 404 problem+json.
   - Seeded DB: profile fields, experience order, projects tech stack and nullable image, ISO `DateOnly`, anonymous access.
   - The OpenAPI doc lists the paths.
6. `.http` and README (grep README, `AGENTS.md` and skills for "minimal API" wording and update it).
7. Wrap-up via the `implement` skill: branch `feature/IOANCVAPP-5-public-read-endpoints`, validate, Jira to "Wird überprüft". No commit or push without approval.

## Risks
- The controller switch could change `/health` behaviour or JSON naming: the existing tests guard it, and the System.Text.Json camelCase default is the same.
- `/health` is outside `/api`, so it needs an explicit `[Route("health")]`.
- Development seeding modifies the configured Development database; it must remain gated by `IsDevelopment()` and add sample data only to empty tables.
- Production does not run startup migrations or seed data. Migrations there are applied separately using the documented EF tool commands.
- Test isolation: per-test in-memory SQLite.
- Application DTOs double as the wire contract; revisit if they diverge.

## Validation
```powershell
dotnet restore backend\IoanCvApp.sln
dotnet format backend\IoanCvApp.sln --verify-no-changes
dotnet build backend\IoanCvApp.sln --no-restore
dotnet test backend\IoanCvApp.sln --no-build
```
Smoke: run the API, `curl http://localhost:5106/api/public/projects`, open `/scalar/v1`.

## Acceptance criteria
- [ ] All three endpoints return DTOs anonymously, with experience in display order.
- [ ] Empty content gives `[]` or a 404 ProblemDetails, with no unhandled exception.
- [ ] Documented and callable in Scalar (Development).
- [ ] Development-only sample seeding uses Infrastructure's `TestDataSeeder`; tests override persistence with in-memory SQLite; no credentials are seeded.
- [ ] Build, format and tests pass; README updated.
