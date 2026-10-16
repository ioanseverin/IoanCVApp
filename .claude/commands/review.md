---
description: Review .NET Jira tickets, pull requests, diffs, files, folders, or implementation scopes for concrete correctness and reliability issues
argument-hint: "[Jira ticket key or PR number|file|folder|scope]"
---

# .NET Code Review

Scope: $ARGUMENTS

Perform a read-only review. Identify concrete, actionable defects introduced by the reviewed changes. Do not edit code, fix findings, commit, push, or post GitHub/Jira comments as part of a review.

## Phase 1: Determine the Scope

Use the scope supplied above. If none is supplied, review the current working-tree changes, including staged and unstaged diffs; inspect untracked files only when they are clearly part of the requested change.

Supported scopes:

| Input | Action |
|---|---|
| Jira ticket key or URL | Retrieve the issue from Jira, identify its implementation scope, and inspect the related code changes. |
| Pull request number or URL | Use `gh` to inspect PR metadata and diff for the current repository. For another repository, confirm its owner/name from the URL before using qualified `gh` commands. |
| Commit, branch, or range | Inspect the requested commit/range and its parent/base. |
| File or folder | Read the requested files and enough callers/tests/context to assess behavior. |
| No argument | Inspect `git status`, staged diff, and unstaged diff; avoid unrelated existing changes. |

Do not assume a PR, branch, or diff base. If the requested scope cannot be resolved, report that limitation instead of reviewing a different scope.

## Phase 2: Load Context

1. Read `AGENTS.md` and relevant nearer instructions.
2. Load the applicable plan explicitly. Search `.agents/plans/` for a plan matching the Jira ticket, PR, or implementation scope, and read it when present. If no matching plan exists, note that in the review report.
3. Load the Jira story explicitly. For a Jira ticket key or URL, retrieve that issue using the Jira issue retrieval tool and read its description and acceptance criteria. For a pull request, inspect its title, description, branch, commits, and linked issues to identify the associated Jira key, then retrieve that Jira issue. If no Jira issue is associated, or Jira cannot be accessed, note that limitation; do not infer requirements or claim to have reviewed an issue that was not loaded.
4. Read the diff and understand intent before evaluating implementation. Compare the implementation with the loaded plan and Jira story, including the description and acceptance criteria. Treat them as requirements context, account for approved changes, and report concrete omissions or deviations. If the plan and story conflict or either appears outdated, call out the discrepancy instead of silently choosing one.
5. Inspect relevant `.sln`/`.slnx`, `.csproj`, package/version configuration, adjacent code, callers, and tests as needed.
6. Follow the architecture already used by the repository. For ASP.NET Core endpoints, apply the `dotnet-webapi` skill when relevant. For validation commands, apply the `validate` skill where available.
7. Keep the review within scope. Do not broaden it into a general audit or report unrelated pre-existing defects.

## Phase 3: Review for Defects

Prioritize issues that would cause incorrect behavior, regressions, data loss, security exposure, crashes, compatibility problems, or missing required behavior. Check relevant concerns such as:

- C# correctness, nullability, async/cancellation, resource lifetime, and exception behavior.
- EF Core model/migration consistency, query behavior, transaction boundaries, and data compatibility.
- ASP.NET Core HTTP semantics, validation, authorization, DTO boundaries, and error handling.
- Dependency direction, configuration, secrets, and environment-specific behavior.
- Tests that fail to cover critical changed behavior or that give misleading confidence.

Only report a finding when there is evidence in the code and a plausible impact. Avoid speculative risks, personal preferences, and generic best-practice advice. Do not report formatting or style issues unless they create a real defect or violate a specific repository requirement.

For each finding, include:

- Severity: **Critical**, **High**, **Medium**, or **Low**.
- Exact file and line(s) in the reviewed change.
- The concrete scenario and impact.
- A concise recommended fix.
- Confidence from 1–10.

Use the smallest useful line range. If no actionable issues are found, say so clearly and mention the scope reviewed; do not invent findings to fill a report.

## Phase 4: Validation

Use the `validate` skill (`.claude/skills/validate/SKILL.md`) as the source of truth for choosing, running, and reporting repository validation checks. This phase is review-specific:

- Select checks that provide meaningful evidence for the changed code; do not run examples blindly.
- Use the actual solution/project paths and repository-configured commands.
- Run frontend checks only when the reviewed scope includes frontend code.
- Record commands, outcomes, and relevant diagnostics in the review report. Distinguish changed-code failures from unrelated baseline failures.
- If a build fails, do not treat `--no-build` test results from existing binaries as validation of the current source.
- Do not modify files to make validation pass. Report unavailable or skipped checks and why.

## Phase 5: Report

Persist every completed review under `.agents/reviews/{kebab-case-name}.review.md`, matching the plan command's `.agents/plans/{kebab-case-name}.plan.md` naming convention. Create `.agents/reviews/` if needed. Use a stable, sanitized, scope-based name, for example:

- PR: `pr-123.review.md`
- File or folder: `{scope-name}.review.md`
- Working-tree changes: `working-tree.review.md`
- Commit or range: `{short-commit-or-range}.review.md`

If a report for that exact scope already exists, update it with the current review rather than creating ambiguous duplicate files. Do not overwrite reports for a different scope. The report must include the review date, scope and base where applicable, findings ordered by severity and confidence, checks run with results, and the assessment. Do not include secrets or unrelated repository content.

Use this report format:

```markdown
# Code Review: {scope}

**Reviewed**: {date}
**Base**: {base or N/A}
**Assessment**: {findings / no actionable issues}

## Findings

| # | Severity | File: line | Finding | Confidence |
|---|---|---|---|---|
| 1 | High | `path/File.cs:42` | Concrete defect, impact, and recommended fix | 8/10 |

## Checks

| Check | Result | Details |
|---|---|---|
| ... | PASS / FAIL / SKIPPED | Command and relevant output |
```

Then return findings first in chat, ordered by severity and then confidence:

| # | Severity | File: line | Finding | Confidence |
|---|---|---|---|---|
| 1 | High | `path/File.cs:42` | Concrete defect, impact, and recommended fix | 8/10 |

Then summarize:

- **Scope reviewed**: PR/diff/files and base where applicable.
- **Checks**: commands run and pass/fail/skipped results.
- **Assessment**: findings, or "No actionable issues found."
- **Report**: link to `.agents/reviews/{kebab-case-name}.review.md`.

Persisting the local report does not authorize posting the review to GitHub or commenting on a PR. Do not submit a GitHub review or comment unless explicitly requested.
