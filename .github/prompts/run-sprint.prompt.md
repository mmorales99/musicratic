---
description: "Execute an approved sprint: delegate tasks to specialist agents, track progress, verify builds, and update backlogs. Non-blocking — marks unresolvable issues as Waiting Human."
mode: "agent"
---

# Run Sprint

You are `boberto`. Execute the approved sprint for the Musicratic project.

## Inputs

The user will specify which sprint to run (by number or by listing task IDs). If no sprint is specified, read `backlog/supra-project.md` and execute the next logical sprint.

## Non-Blocking Rule

If any specialist agent encounters a blocker (build error after retry, missing credentials, ambiguous spec, unresolvable error), **do not stall**. Instead:

1. Mark the task `⏳ Waiting Human` in the backlog file
2. Log the reason clearly (what the human needs to do)
3. Mark any tasks that depend on the blocked task back to `📋 Backlog`
4. Continue executing remaining tasks

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
      c. If an agent fails or blocks → mark `⏳ Waiting Human`, log reason, continue
      d. Run build verification:
    - Backend: `dotnet build src/Musicratic.slnx`
    - Web: `cd web && npx ng build` (if web tasks were done)
    - Mobile: `cd mobile && flutter analyze` (if mobile tasks were done)
      e. If build fails → attempt one fix. If still fails → `⏳ Waiting Human` for affected tasks
      f. Mark completed tasks as `✅ Done` in backlog files
4. **Defer blocked dependents** — Tasks depending on `⏳ Waiting Human` tasks → back to `📋 Backlog`
5. **Update supra-project** — Recalculate totals, done counts, and phase percentages
6. **Report results**:

```markdown
## Sprint N — Results

### Completed

| Task | Agent | Est. PRs | Actual PRs | Files |
| ---- | ----- | -------- | ---------- | ----- |

### ⏳ Waiting Human

| Task | Reason |
| ---- | ------ |

### Deferred to Next Sprint

| Task | Blocked By |
| ---- | ---------- |

### Metrics

- Tasks completed: X / Y
- Tasks waiting human: Z
- Premium requests used: ~W (est. V)
- Build status: ✅ Clean / ⚠️ Warnings / ❌ Errors
```
