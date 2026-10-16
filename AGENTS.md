# Agent Instructions

### Workflow

- For planning a feature, story, or change, always use the `plan` skill (`.agents/skills/plan/SKILL.md`); save plans to `.agents/plans/`. This applies in plan mode too: follow the skill and save the plan in the repository, not only in the session.
- For implementing a plan or an approved story, always use the `implement` skill (`.agents/skills/implement/SKILL.md`), including its branch, validation, and Jira steps.
- For validation-only requests, use the `validate` skill (`.agents/skills/validate/SKILL.md`) to select and report repository-appropriate checks. During implementation, follow the validation steps in `implement`.
- Load the skill before starting the work, not after.

### Committing

- Favor minimal explanatory comments in code; avoid verbose inline comments.
- Commit message format: `IOANCVAPP-<n>: <imperative summary>` (subject up to 72 characters), followed by an optional body explaining why.
- Take the Jira key from the story being worked on or the branch name; use `NO-TICKET:` when no issue applies.
- Keep commit messages concise but include necessary context; the "minimal comments" preference does not apply to commit message content.
- Do not commit or push changes without explicit user approval.
- Only force-push after approval; warn about history changes.
