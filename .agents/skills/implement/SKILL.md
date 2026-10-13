---
name: implement
description: >
  Implements a reviewed .NET plan end-to-end with build, test and integration validation.
  USE FOR: executing or building out a plan file (.agents/plans/*.plan.md) or an approved story; "implement the plan", "implement ST-01 from the plan".
  DO NOT USE FOR: creating or revising a plan (use plan), ad-hoc small edits without a plan, adding a single API endpoint (use dotnet-webapi), committing or pushing, or creating PRDs and Jira stories.
argument-hint: "<path/to/plan.md>"
---

# Implement .NET Plan

**Plan**: the plan file path provided by the user.

## Mission

Execute the approved plan end-to-end, following repository instructions and existing .NET patterns. Validate each coherent change and fix regressions before continuing.

Do not commit or push unless the user explicitly asks. Follow `AGENTS.md` and any nearer repository instructions for commit requirements.

## Phase 1: Load and Verify the Plan

Read the plan and identify:

- Scope, acceptance criteria, affected projects and files
- Architecture and project-reference direction
- Task order and validation commands
- Jira issue key, if explicitly included

If the plan file is missing, stop and report the path; suggest the `plan` skill to create one.

Before implementation, inspect `AGENTS.md`, `CLAUDE.md`, relevant local instructions, `git status`, and the current branch. Re-check plan assumptions against the current solution, project files, package versions, and adjacent code. The repository may have changed since planning.

## Phase 2: Prepare the Worktree Safely

1. If on `main` and the worktree is clean, create a descriptive branch with `git switch -c feature/{plan-name}`.
2. If there are existing changes, do not stash, reset, checkout, restore, overwrite, or delete them. Determine whether they overlap planned files:
   - If overlapping or ownership is unclear, stop and ask how to proceed.
   - If clearly unrelated, leave them untouched and continue without staging them.
3. If already on a feature branch, continue on that branch.

Never use destructive Git commands to make the worktree appear clean.

## Phase 3: Implement Tasks

For each task, in plan order:

### 3.1 Inspect

- Read target files, adjacent code, project files, callers, and tests before editing.
- Verify cited types/APIs exist and project references match the intended dependency direction.
- If the plan is stale, adapt to current code and record the deviation in the completion report.

### 3.2 Implement

- Follow existing namespaces, naming, nullability, DI, configuration, error handling, and test conventions.
- Preserve low coupling: keep domain/core free of infrastructure concerns; keep provider wiring at the infrastructure boundary and composition root as architecture requires.
- Keep public API contracts separate from persistence entities.
- Do not add or upgrade packages without need. If a package is genuinely required, use the existing version-management convention and explain it.
- Do not commit secrets, local settings, database files, build output, or developer-specific configuration.
- For EF Core changes, create migrations in the configured migrations project and verify the model snapshot. Never apply a migration to production or a shared database during implementation.

### 3.3 Validate Each Coherent Change

Run the narrowest relevant .NET check after each task, using the repository's actual solution/project paths:

```powershell
dotnet build <solution-or-project>
dotnet test <test-project-or-solution> --no-build
```

If a task changes only documentation, skip build/test unless the repository has documentation checks. For formatting or analyzers, use configured tooling, for example:

```powershell
dotnet format <solution> --verify-no-changes
```

Do not assume every repository has `dotnet format`, a linter, or a particular test framework. Use existing commands. If validation fails, diagnose and fix the cause; do not proceed with a known regression. Note pre-existing failures separately and avoid unrelated fixes.

Track task progress in the response or session task tracker. Do not write temporary progress files into the repository.

## Phase 4: Final Verification

Run the plan's applicable final checks, normally:

```powershell
dotnet restore <solution>
dotnet build <solution> --no-restore
dotnet test <solution> --no-build
```

Also:

- Run configured analyzers/format checks when available.
- Run relevant integration tests.
- For API endpoint changes, exercise the endpoint via integration tests or the repository's `.http` requests and verify status/body/OpenAPI behavior.
- For EF Core changes, verify migrations against a fresh disposable local/test database and verify persistence behavior. Never point validation at production or shared data.
- Perform a runtime smoke test when a runnable application was changed and the plan requires one.
- Inspect `git diff --check`, `git status`, and the final diff. Confirm generated output, local databases, and secrets are not included.

End-to-end testing is required when the plan changes an end-to-end user or API flow; it is not a blanket requirement for isolated libraries or documentation.

## Phase 5: Report

Create `.agents/reports/{plan-name}-report.md` only if the plan or repository convention calls for a report. Include:

- Plan and branch
- Summary and completed tasks
- Files created/updated
- Build, test, format, and integration results
- Deviations, blockers, and known pre-existing failures

Archive the plan under `.agents/plans/completed/` only if the plan workflow explicitly expects archival. Preserve the original plan content.

## Phase 6: Update Jira (Only if Linked)

If the plan explicitly includes a Jira issue:

1. Resolve the Atlassian cloud/site identifier once for this run.
2. Read the issue and available transitions before writing.
3. After implementation and validation, transition only to an appropriate active/review status. **Do not mark the issue Done automatically**; completion and acceptance are separate.
4. Use the transition ID/name accepted by the Jira transition operation, not an invented payload shape.
5. Add a concise implementation comment with summary, branch, changed-file/test counts, validation, deviations, and report path when available.
6. Do not alter the issue description unless the plan materially changed the agreed scope; preserve existing text when editing.

If Jira access is unavailable, report that clearly; do not silently claim the issue was updated.

## Phase 7: Completion Summary

Summarize:

- Implementation result and branch
- Files/projects changed
- Build/test and other validation outcomes
- Deviations or unresolved issues
- Report/plan paths, if created or archived
- Jira update result, if applicable

Do not claim completion until the applicable validation has passed. Do not commit, push, or create a PR unless the user explicitly requested it.

## Failure Handling

| Failure | Action |
|---|---|
| Build fails | Inspect the exact diagnostics, fix the issue, and rebuild the affected solution/project. |
| Tests fail | Determine whether implementation or test assumptions are wrong; fix the cause and rerun. |
| Restore fails | Report the package/source/network error; do not make speculative dependency changes. |
| Migration fails | Verify startup project, design-time context creation, provider, and migration assembly; use only a disposable local database. |
| Worktree changes overlap | Stop before overwriting; preserve the changes and ask for direction. |
| Jira update fails | Report which read/write operation failed and leave code changes intact. |
