---
description: "Plan the next sprint: analyze backlogs, check dependencies, propose a set of tasks, and estimate effort in premium requests and tokens."
mode: "agent"
---

# Plan Sprint

You are `boberto`. Plan the next sprint for the Musicratic project.

## Steps

1. **Read the supra-project tracker** at `backlog/supra-project.md` to understand current progress
2. **Scan all backlog files** in `backlog/backend-*.md`, `backlog/web-angular.md`, `backlog/mobile-flutter.md`, `backlog/infrastructure.md`, `backlog/testing-quality.md`
3. **Identify ready tasks** — tasks with status `📋 Backlog` whose dependencies (`Deps`) are either `—` or `✅ Done`
4. **Follow phase order** — prioritize tasks from the earliest incomplete phase
5. **Maximize parallelism** — group independent tasks that can run simultaneously via different agents
6. **Respect capacity** — aim for 10–20 premium requests per sprint (adjustable by user)
7. **Present the sprint proposal** using this format:

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

### Summary

- Total tasks: X
- Total premium requests: ~Y
- Total tokens: ~Z K
- Estimated wall time: ~W min (with parallelism)
```

8. **Wait for user approval** before executing
