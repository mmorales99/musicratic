---
description: "Execute an approved sprint: delegate tasks to specialist agents, track progress, verify builds, and update backlogs."
mode: "agent"
---

# Run Sprint

You are `boberto`. Execute the approved sprint for the Musicratic project.

## Inputs

The user will specify which sprint to run (by number or by listing task IDs). If no sprint is specified, read `backlog/supra-project.md` and execute the next logical sprint.

## Execution Steps

1. **Read the sprint plan** — Identify the tasks, their execution groups, and target agents
2. **Mark tasks as sprint** — Update status from `📋 Backlog` to `🔄 Sprint` in each backlog file
3. **Execute each group sequentially** (tasks within a group run in parallel):
   a. For each task in the group, invoke the appropriate specialist agent with:
    - Task ID and full description
    - Spec document references from `/docs/`
    - Files to read for context
    - Acceptance criteria
    - File ownership constraints
      b. Wait for all agents in the group to complete
      c. Run build verification:
    - Backend: `dotnet build src/Musicratic.slnx`
    - Web: `cd web && npx ng build` (if web tasks were done)
    - Mobile: `cd mobile && flutter analyze` (if mobile tasks were done)
      d. Fix any build errors before proceeding to the next group
      e. Mark completed tasks as `✅ Done` in backlog files
4. **Update supra-project** — Recalculate totals, done counts, and phase percentages
5. **Report results**:

```markdown
## Sprint N — Results

### Completed

| Task | Agent | Est. PRs | Actual PRs | Files |
| ---- | ----- | -------- | ---------- | ----- |

### Issues

- [Any build errors, blocked tasks, or deviations]

### Metrics

- Tasks completed: X / Y
- Premium requests used: ~Z (est. W)
- Build status: ✅ Clean / ⚠️ Warnings / ❌ Errors
```
