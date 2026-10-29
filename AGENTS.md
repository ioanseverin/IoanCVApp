# Agent Instructions

### Workflow

- For planning a feature, story, or change, always use the `plan` skill (`.agents/skills/plan/SKILL.md`); save plans to `.agents/plans/`. This applies in plan mode too: follow the skill and save the plan in the repository, not only in the session.
- For implementing a plan or an approved story, always use the `implement` skill (`.agents/skills/implement/SKILL.md`), including its branch, validation, and Jira steps.
- For validation-only requests, use the `validate` skill (`.agents/skills/validate/SKILL.md`) to select and report repository-appropriate checks. During implementation, follow the validation steps in `implement`.
- For ASP.NET Core Web API work, use the `dotnet-webapi` skill (`.agents/skills/dotnet-webapi/SKILL.md`) when adding or modifying endpoints, API request/response DTOs, OpenAPI documentation, API error handling, or `.http` request tests. Do not use it for general C# refactoring, EF Core data modeling/query optimization, or frontend work.
- Keep `README.md` current with changes to setup, usage, features, behavior, configuration, or project structure. Assess README impact for every change, update it when relevant, and state when no update is needed.
- Load the skill before starting the work, not after.

### Pull Request Creation

- Target `main` unless the Jira story or repository workflow specifies another base branch.
- Use a ticket-linked title: `IOANCVAPP-<n>: <imperative summary>`. Use `NO-TICKET:` only when no Jira ticket applies.
- Keep each pull request focused on one ticket or closely related change; exclude unrelated changes.
- Link the Jira story in the description and summarize how the changes address its description and acceptance criteria.
- Describe the purpose, key implementation details, and relevant design decisions.
- List the exact validation commands run and their results; identify skipped checks and why.
- Call out migrations, configuration changes, compatibility concerns, deployment steps, and other relevant risks. Include screenshots for user-visible UI changes.
- Create a draft pull request when the work or description is not ready for full review; otherwise create a regular pull request.

### Committing

- Favor minimal explanatory comments in code; avoid verbose inline comments.
- Commit message format: `IOANCVAPP-<n>: <imperative summary>` (subject up to 72 characters), followed by an optional body explaining why.
- Take the Jira key from the story being worked on or the branch name; use `NO-TICKET:` when no issue applies.
- Keep commit messages concise but include necessary context; the "minimal comments" preference does not apply to commit message content.
- Do not commit or push changes without explicit user approval.
- Only force-push after approval; warn about history changes.
