---
description: "Plan the next sprint: analyze backlogs, check dependencies, handle waiting-human deferrals, propose tasks, and estimate effort."
mode: "agent"
---

# Plan Sprint

You are `boberto`. Plan the next sprint for the Musicratic project.

## Steps

1. **Read the supra-project tracker** at `backlog/supra-project.md` to understand current progress
2. **Scan all backlog files** in `backlog/backend-*.md`, `backlog/web-angular.md`, `backlog/mobile-flutter.md`, `backlog/infrastructure.md`, `backlog/testing-quality.md`
3. **Identify ready tasks** — tasks with status `📋 Backlog` whose dependencies (`Deps`) are either `—` or `✅ Done`
4. **Skip blocked tasks** — if a task depends on a `⏳ Waiting Human` task, it is NOT ready (defer it)
5. **Include deferred tasks** — any `⏳ Waiting Human` tasks from previous sprints that have been resolved (changed back to `📋 Backlog` by the human) should be considered
6. **Follow phase order** — prioritize tasks from the earliest incomplete phase
7. **Maximize parallelism** — group independent tasks that can run simultaneously via different agents
8. **Respect capacity** — aim for 10–20 premium requests per sprint (adjustable by user)
9. **Present the sprint proposal** using this format:

```markdown
## Sprint N — [Phase Focus Title]

**Phase**: 1X — [Description]
**Tasks**: X | **Premium Requests**: ~Y | **Tokens**: ~Z K | **Est. Time**: ~W min

### Group 1 — [parallel]

| Task | Title | Agent | Size | PRs | Tokens |
| ---- | ----- | ----- | ---- | --- | ------ |

### Group 2 — [after Group 1]

| Task | Title | Agent | Size | PRs | Tokens |
| ---- | ----- | ----- | ---- | --- | ------ |

### ⏳ Deferred (blocked by Waiting Human)

| Task | Blocked By | Reason |
| ---- | ---------- | ------ |

### Summary

- Total tasks: X
- Total premium requests: ~Y
- Total tokens: ~Z K
- Estimated wall time: ~W min (with parallelism)
- Deferred tasks: D (waiting human resolution)
```

10. **In autonomous mode** (`/run-all-sprints`): skip user approval, proceed directly to execution
11. **In interactive mode**: wait for user approval before executing
