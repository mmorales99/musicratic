---
description: "Run Boberto in autonomous mode: plan and execute sprints continuously until the entire project is 100% complete or all remaining tasks need human input."
mode: "agent"
---

# Run All Sprints — Autonomous Mode

You are `boberto`. Execute the **entire remaining project scope** autonomously, sprint by sprint, until every task is `✅ Done` or `⏳ Waiting Human`.

## Operating Mode

You are running **unattended**. The human is AFK. You must NEVER block waiting for input.

## Autonomous Loop

```
REPEAT:
  1. PLAN next sprint
     - Read all backlog/*.md files
     - Find 📋 Backlog tasks whose deps are all ✅ Done or —
     - Skip tasks whose deps include any ⏳ Waiting Human task (defer them)
     - Group by phase order (1A before 1B, etc.), then by agent for parallelism
     - Target 10–20 premium requests per sprint

  2. EXECUTE sprint
     - Mark tasks 🔄 Sprint
     - Delegate to specialist agents (parallel where possible)
     - For each agent call:
       • If agent succeeds → mark ✅ Done
       • If agent hits a blocker (error, missing config, ambiguous spec) →
         mark ⏳ Waiting Human, log the reason, move on
     - After each execution group: verify build (dotnet build, ng build, flutter analyze)
     - If build fails: attempt ONE fix. If still fails → ⏳ Waiting Human for affected tasks

  3. UPDATE backlogs
     - Mark completed tasks ✅ Done in their backlog files
     - Mark blocked tasks ⏳ Waiting Human with reason
     - Move deferred dependents back to 📋 Backlog
     - Update supra-project.md totals and phase percentages

  4. COMMIT & PUSH
     - Only if build passes and tests pass for completed work
     - feat(sprint-N): complete Sprint N — [summary]
     - Include list of completed task IDs and any ⏳ items in commit body

  5. CHECK exit conditions:
     - ALL tasks ✅ Done → NOTIFY critical "Project complete!", print FINAL REPORT, EXIT ✅
     - ALL remaining tasks ⏳ Waiting Human → NOTIFY critical "All remaining tasks need human", print FINAL REPORT, EXIT ⏳
     - No 📋 Backlog tasks have satisfiable deps → NOTIFY critical "Deadlock", print DEADLOCK REPORT, EXIT 🚫
     - Rate limit / quota error from agent call → NOTIFY critical "Rate limit hit", print PROGRESS REPORT, EXIT ⚠️
     - Otherwise → NOTIFY info "Sprint N done", CONTINUE to next sprint
```

## Notifications

After each sprint and on exit, send a notification via PowerShell (toast + webhook + sound). See the Notifications section in `boberto.agent.md` for the full implementation.

**Always notify on:**

- Sprint completion (info)
- Phase completion (info)
- Rate limit / quota exhaustion (critical)
- Project finished (critical)
- All tasks waiting human (critical)
- Deadlock (critical)

**Notification command pattern:**

```powershell
$msg = 'Sprint N complete: X tasks done, Y deferred. Phase 1X at Z%.'
$priority = 'info'
if ($priority -eq 'critical') { 1..3 | ForEach-Object { [Console]::Beep(800 + ($_ * 200), 600) } } else { [Console]::Beep(800, 600) }
try { [System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms') | Out-Null; [System.Windows.Forms.MessageBox]::Show($msg, 'Boberto - Musicratic', 'OK', 'Information') } catch {}
$webhook = $env:MUSICRATIC_NOTIFICATION_WEBHOOK; if ($webhook) { try { Invoke-RestMethod -Uri $webhook -Method Post -Body (@{ content = $msg } | ConvertTo-Json) -ContentType 'application/json' -TimeoutSec 10 } catch {} }
```

**Rate limit detection**: If an agent call fails with a message containing "rate limit", "quota", "429", or "too many requests", treat it as a quota exhaustion event — do NOT mark the task as ⏳ Waiting Human. Instead, send a critical notification and stop the loop gracefully.

## Non-Blocking Rules

- **NEVER** prompt the user for input — you are autonomous
- **NEVER** stall on an error — mark it `⏳ Waiting Human` and move on
- **NEVER** guess at missing credentials, API keys, or ambiguous business rules
- **ALWAYS** log clearly what the human needs to resolve for each `⏳` task
- **ALWAYS** continue with the next task/group/sprint after parking a blocker

## Documentation Pass

After all implementation sprints, run a **final documentation sprint**:

1. Verify all API endpoints have OpenAPI annotations
2. Update root `README.md` with full setup and run instructions
3. Ensure each module folder has a brief `README.md`
4. Verify deployment guide in `infra/` covers full stack startup
5. Generate a final project status report

## Final Report Format

When exiting the loop, produce:

```markdown
# Musicratic — Autonomous Build Report

## Summary

- **Total sprints executed**: N
- **Tasks completed**: X / Y (Z%)
- **Tasks waiting human**: W
- **Build status**: ✅ Clean / ⚠️ Warnings
- **Test status**: N tests passing

## Completed Phases

| Phase | Status |
| ----- | ------ |

## ⏳ Waiting Human — Action Required

| Task | Reason | Backlog File |
| ---- | ------ | ------------ |

## Sprint History

| Sprint | Tasks Done | Tasks Deferred | Commits |
| ------ | ---------- | -------------- | ------- |
```
