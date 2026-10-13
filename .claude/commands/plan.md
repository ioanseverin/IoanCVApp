---
description: Create a .NET implementation plan grounded in the repository's architecture and conventions
argument-hint: <feature description | path/to/prd.md>
---

# .NET Implementation Plan Generator

**Input**: $ARGUMENTS

## Objective

Create a context-rich implementation plan that fits the existing .NET solution, layering, dependency versions, coding conventions, and tests.

**Plan only**: do not change application code. Save the plan under `.agents/plans/`.

## Phase 1: Parse the Request

Determine whether the input is a PRD path, another Markdown document, or a feature description. For a PRD, extract only the requested or next relevant scope; do not silently expand a story into adjacent phases.

Capture:
- Problem and user story
- Type: `NEW_CAPABILITY`, `ENHANCEMENT`, `REFACTOR`, or `BUG_FIX`
- Complexity: `LOW`, `MEDIUM`, or `HIGH`
- Relevant Jira issue key when known
- Acceptance criteria and explicit scope boundaries

## Phase 2: Explore the .NET Repository

Inspect the workspace directly. Do not assume a particular frontend stack or project structure. For non-trivial exploration, use a read-only Explore agent only when it materially helps; do not delegate a small, straightforward inspection.

Inspect the relevant subset of:

1. Repository instructions: `AGENTS.md`, `CLAUDE.md`, and any nearer instructions.
2. Solution/build configuration: `global.json`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `NuGet.config`, `.editorconfig`, and solution files (`.sln` / `.slnx`).
3. Relevant `.csproj` files, project references, target frameworks, package versions, and existing project boundaries.
4. Analogous implementation and tests: naming, namespaces, DI, error handling, validation, persistence, API style, and test framework.
5. Existing build/test/format/analyzer configuration and commands.
6. Recent Git history only if it helps identify conventions.

Do not report a pattern as established if the repository has no example; mark it as a proposed convention instead. Do not invent files, APIs, fields, package versions, or validation commands.

For database work, inspect the current provider, `DbContext`, entity configurations, migrations, design-time factory/startup wiring, and test database strategy. For API work, inspect endpoint style and the `dotnet-webapi` skill if it is available and relevant.

### Record Patterns

Use real C# and test file references with line numbers:

| Category | File:Lines | Pattern |
|---|---|---|
| PROJECT / DEPENDENCIES | `path/to/Project.csproj:1-20` | Target framework and reference direction |
| NAMING / TYPES | `path/to/Type.cs:10-25` | Actual namespace, type, and nullability conventions |
| ERRORS / API | `path/to/Endpoint.cs:20-40` | Existing result and error handling |
| TESTS | `path/to/FeatureTests.cs:1-30` | Framework, fixture, assertions, and test naming |

If no relevant example exists, say so and propose the simplest consistent pattern.

## Phase 3: Design the Change

- List files/projects to create, update, or remove, with a reason for each.
- Order tasks by project dependency and implementation dependency.
- Specify project-reference direction explicitly when multiple .NET projects are involved. Keep domain/core projects independent of infrastructure and frameworks unless repository architecture requires otherwise.
- Prefer existing packages and frameworks. Do not add a dependency when the framework or current solution already provides the needed capability.
- For EF Core changes, state how migrations are created and verified. Never plan to apply migrations to production or a shared database as part of local validation.
- Identify risks and mitigations, including compatibility, data migration, configuration/secrets, and test isolation where relevant.
- Ask a focused clarification if a significant product or architecture decision cannot be determined from the request or repository. Otherwise make a reasonable, explicit assumption and proceed.

## Phase 4: Write the Plan

**Output**: `.agents/plans/{kebab-case-name}.plan.md`

Create `.agents/plans/` if needed. Use this format:

```markdown
# Plan: {Feature}

## Summary
{Approach and bounded scope}

## User Story
As a {user}
I want to {action}
So that {benefit}

## Metadata
| Field | Value |
|---|---|
| Type | {type} |
| Complexity | {level} |
| Projects / Systems | {list} |
| Jira Issue | {key or N/A} |

## Scope
### In
- ...
### Out
- ...

## Repository Findings and Patterns
{Relevant project structure, references, versions, and actual file:line examples.
Clearly mark proposed patterns where no precedent exists.}

## Architecture and Dependency Direction
{For multi-project work, list the intended project-reference graph and explain
where external framework/provider dependencies belong.}

## Files and Projects
| Path | Action | Purpose |
|---|---|---|
| `path/...` | CREATE / UPDATE / DELETE | ... |

## Ordered Tasks
### 1. {Task}
- **Changes**: ...
- **Pattern / dependency**: ...
- **Validation**: `dotnet ...`

## Risks and Mitigations
| Risk | Mitigation |
|---|---|
| ... | ... |

## Validation
```powershell
dotnet restore {solution}
dotnet build {solution} --no-restore
dotnet test {solution} --no-build
```
Replace these examples with commands actually supported by the repository.
Include formatting/analyzer, integration, migration, or smoke checks only when applicable.

## Acceptance Criteria
- [ ] ...
```

Task validation must use commands appropriate to the repository, such as `dotnet build <solution-or-project>`, `dotnet test <solution-or-test-project>`, or `dotnet format <solution-or-project> --verify-no-changes`. Do not include `pnpm`, `npm`, or other non-.NET commands unless the planned change touches those projects.

## Phase 5: Report

Summarize the plan path, scope, affected projects, important architecture choices, and key validation commands. State that no implementation code was changed.
