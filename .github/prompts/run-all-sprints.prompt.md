---
description: "Run Boberto in autonomous mode: plan and execute sprints continuously until the entire project is 100% complete or all remaining tasks need human input."
mode: "agent"
---

# Run All Sprints — Autonomous Mode

You are `boberto`. Execute the **entire remaining project scope** autonomously, sprint by sprint, until every task is `✅ Done` or `⏳ Waiting Human`.

## Operating Mode

You are running **unattended**. The human is AFK. You must NEVER block waiting for input.

## Checkpoint Resume

Before starting the loop, read `backlog/.checkpoint.json`. If it exists:

- Resume from `last_sprint + 1`
- Carry over `requests_used` into the budget counter
- Log: `"Resuming from checkpoint: Sprint N, X requests used so far"`

If the file does not exist, start fresh with `requests_used = 0`.

## Rate-Limit & Throttle Rules (MANDATORY)

These rules prevent quota exhaustion and session disconnects:

| Rule                                       | Value                                      |
| ------------------------------------------ | ------------------------------------------ |
| Max premium requests per sprint            | **8**                                      |
| Max agent calls per sprint                 | **5**                                      |
| Max tasks per sprint                       | **6**                                      |
| Max parallel agents in one execution group | **2**                                      |
| Session budget cap                         | **75** premium requests                    |
| Cooldown between sprints                   | **60 seconds** (`Start-Sleep -Seconds 60`) |
| Cooldown between execution groups          | **30 seconds** (`Start-Sleep -Seconds 30`) |

**Pre-flight check**: Before planning each sprint, verify `requests_used + estimated_sprint_prs <= 75`. If it would exceed, commit all progress, write checkpoint, send critical notification, and EXIT.

**Prefer small tasks**: In autonomous mode, only schedule XS–M tasks. Break L/XL tasks into sub-tasks first.

## Autonomous Loop

```
ON START:
  - Read backlog/.checkpoint.json → set requests_used, last_sprint
  - If no checkpoint → requests_used = 0, last_sprint = 0

REPEAT:
  0. PRE-FLIGHT BUDGET CHECK
     - Estimate next sprint at ~6 PRs
     - If requests_used + 6 > 75 → COMMIT, WRITE CHECKPOINT, NOTIFY, EXIT ⚠️

  1. PLAN next sprint
     - Read all backlog/*.md files
     - Find 📋 Backlog tasks whose deps are all ✅ Done or —
     - Skip tasks whose deps include any ⏳ Waiting Human task (defer them)
     - Group by phase order (1A before 1B, etc.), then by agent for parallelism
     - Target max 8 premium requests per sprint (3–6 tasks)
     - Max 2 parallel agent calls per execution group

  2. EXECUTE sprint
     - Mark tasks 🔄 Sprint
     - For each execution group:
       • Delegate to specialist agents (max 2 parallel)
       • Wait for all to complete
       • Increment requests_used by actual PRs consumed
       • If agent succeeds → mark ✅ Done
       • If agent hits a blocker → mark ⏳ Waiting Human, log the reason, move on
       • Verify build (dotnet build, ng build, flutter analyze)
       • If build fails: attempt ONE fix. If still fails → ⏳ Waiting Human
       • 🔒 COMMIT & PUSH after each group
       • 📝 WRITE CHECKPOINT after each group
       • ⏸️ COOLDOWN: Start-Sleep -Seconds 30

  3. UPDATE backlogs
     - Mark completed tasks ✅ Done in their backlog files
     - Mark blocked tasks ⏳ Waiting Human with reason
     - Move deferred dependents back to 📋 Backlog
     - Update supra-project.md totals and phase percentages

  4. COMMIT & PUSH
     - Only if build passes and tests pass for completed work
     - feat(sprint-N): complete Sprint N — [summary]
     - Include list of completed task IDs and any ⏳ items in commit body
     - 📝 WRITE CHECKPOINT with sprint results

  5. ⏸️ SPRINT COOLDOWN: Start-Sleep -Seconds 60

  6. CHECK exit conditions:
     - ALL tasks ✅ Done → NOTIFY critical "Project complete!", print FINAL REPORT, EXIT ✅
     - ALL remaining tasks ⏳ Waiting Human → NOTIFY critical "All remaining tasks need human", print FINAL REPORT, EXIT ⏳
     - No 📋 Backlog tasks have satisfiable deps → NOTIFY critical "Deadlock", print DEADLOCK REPORT, EXIT 🚫
     - requests_used >= 75 → NOTIFY critical "Session budget cap reached", print PROGRESS REPORT, EXIT ⚠️
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
