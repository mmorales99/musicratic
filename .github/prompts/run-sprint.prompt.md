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

1. **Read checkpoint** — Load `backlog/.checkpoint.json` if it exists, carry over `requests_used`
2. **Read the sprint plan** — Identify the tasks, their execution groups, and target agents
3. **Mark tasks as sprint** — Update status from `📋 Backlog` to `🔄 Sprint` in each backlog file
4. **Execute each group sequentially** (max **2** parallel agents per group):
   a. For each task in the group, invoke the appropriate specialist agent with:
    - Task ID and full description
    - Spec document references from `/docs/`
    - Files to read for context
    - Acceptance criteria
    - File ownership constraints
      b. Wait for all agents in the group to complete
      c. Increment `requests_used` by PRs consumed
      d. If an agent fails or blocks → mark `⏳ Waiting Human`, log reason, continue
      e. Run build verification:
    - Backend: `dotnet build src/Musicratic.slnx`
    - Web: `cd web && npx ng build` (if web tasks were done)
    - Mobile: `cd mobile && flutter analyze` (if mobile tasks were done)
      f. If build fails → attempt one fix. If still fails → `⏳ Waiting Human` for affected tasks
      g. Mark completed tasks as `✅ Done` in backlog files
      h. **🔒 COMMIT & PUSH** after each group
      i. **📝 WRITE CHECKPOINT** — Update `backlog/.checkpoint.json`
      j. **⏸️ COOLDOWN** — `Start-Sleep -Seconds 30` before next group
5. **Defer blocked dependents** — Tasks depending on `⏳ Waiting Human` tasks → back to `📋 Backlog`
6. **Update supra-project** — Recalculate totals, done counts, and phase percentages
7. **🔒 FINAL COMMIT & PUSH** — Commit backlog updates
8. **📝 FINAL CHECKPOINT** — Write updated checkpoint
9. **Report results**:

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
