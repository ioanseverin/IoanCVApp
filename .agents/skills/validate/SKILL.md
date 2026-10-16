---
name: validate
description: >
  Runs repository-appropriate validation for .NET changes and reports results without modifying application code.
  USE FOR: validating .NET changes; running restore, build, test, analyzers, or formatting checks; checking a solution before commit or review; "validate", "run the checks", "test the solution".
  DO NOT USE FOR: implementing fixes for failing checks (use implement or the relevant skill), creating or changing an implementation plan (use plan), or running checks unrelated to the changed code.
argument-hint: "[solution-or-project-path]"
---

# Validate .NET Changes

Run the repository's applicable validation checks and report exact outcomes. Do not modify application code or automatically fix failures.

## Process

1. Inspect `AGENTS.md`, any nearer instructions, `git status`, and the diff to identify the changes in scope. Preserve unrelated work.
2. Find the relevant `.sln` / `.slnx` and `.csproj` files. If the user supplied a solution or project path, use it; otherwise select the solution covering the changed projects. Do not assume a solution name or layout.
3. Check repository configuration and existing scripts for required SDK, package, analyzer, formatter, and test commands. Honor `global.json` and central package management.
4. Run the relevant checks from the repository root, in this order:
   - Restore the selected solution/project: `dotnet restore <solution-or-project>`
   - Formatting verification when supported/configured: `dotnet format <solution-or-project> --verify-no-changes`
   - Build: `dotnet build <solution-or-project> --no-restore`
   - Tests for the affected test projects or solution: `dotnet test <test-project-or-solution> --no-build`
5. Run configured analyzers, integration tests, or smoke checks when they cover the change. For API changes, use the existing integration tests or `.http` requests as appropriate. Do not point database checks at production or shared data.
6. If the repository contains a frontend and the change affects it, run its documented lint, type-check, and test commands from the appropriate directory. Do not run Node/Bun commands for backend-only changes.
7. Report each command, exit status, and material output. Distinguish failures in changed code from known or pre-existing failures. Do not claim a check passed if it was skipped or could not run.

## Failure Handling

- Do not fix or suppress failures as part of validation. Report diagnostics with file and line when available, and suggest a likely next step only when clear.
- If restore fails due to package source or network issues, report that and do not change package versions speculatively.
- If no relevant solution, project, or test command can be identified, report what was inspected and what information is missing.
- Avoid broad catches or hiding command output; preserve enough details to diagnose each failure.

## Output

```markdown
## Validation Results

| Check | Result | Details |
|---|---|---|
| Restore | PASS / FAIL / SKIPPED | Command and relevant output |
| Format / analyzers | PASS / FAIL / SKIPPED | Command and relevant output |
| Build | PASS / FAIL / SKIPPED | Command and relevant output |
| Tests | PASS / FAIL / SKIPPED | Passed / failed / skipped counts |
| Other applicable checks | PASS / FAIL / SKIPPED | Command and relevant output |

### Summary
- **Status**: ALL PASSING / FAILURES / PARTIAL
- **Action needed**: None / diagnostics and next steps
```

Include only applicable checks. Clearly state why any expected check was skipped.
