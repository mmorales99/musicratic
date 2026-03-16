# Musicratic — Scrum Backlog System

## How It Works

This folder contains the **task backlogs** for every area of the Musicratic project. An AI agent called **boberto** orchestrates work by reading these files, planning sprints, and delegating tasks to specialist agents.

## Structure

```
backlog/
├── README.md                      # This file — estimation model & conventions
├── supra-project.md               # Master tracker — all sub-projects + totals
├── backend-auth.md                # Auth module backlog
├── backend-hub.md                 # Hub module backlog
├── backend-playback.md            # Playback module backlog
├── backend-voting.md              # Voting module backlog
├── backend-economy.md             # Economy module backlog
├── backend-analytics.md           # Analytics module backlog
├── backend-social.md              # Social module backlog
├── backend-notification.md        # Notification module backlog
├── web-angular.md                 # Angular web client backlog
├── mobile-flutter.md              # Flutter mobile client backlog
├── infrastructure.md              # DevOps & infrastructure backlog
└── testing-quality.md             # Cross-cutting test & quality backlog
```

## Estimation Model

Every task is estimated across three dimensions that map to Copilot's resource usage.

### Size Tiers

| Size | Files | Tokens (in+out) | Premium Requests | Wall Time | Description |
|------|-------|------------------|------------------|-----------|-------------|
| **XS** | 1 | ~8–15 K | 1 | 2–5 min | Single file edit, config change, simple rename |
| **S** | 1–2 | ~15–25 K | 1 | 5–10 min | One entity, one DTO, one simple handler |
| **M** | 2–4 | ~25–50 K | 2–3 | 10–20 min | Handler + validation + tests for one use case |
| **L** | 4–8 | ~50–100 K | 3–5 | 20–40 min | Full vertical slice across layers |
| **XL** | 8+ | ~100–200 K | 5–10 | 40–90 min | Multi-module feature with integration |

### Cost Estimation

Premium requests are the unit Copilot uses for quota. Each agent invocation ≈ 1 premium request. Complex tasks that require multiple sub-agent calls consume more.

**Rough cost per premium request** (Claude Sonnet 4, March 2026):
- ~20 K tokens average (input + output) per request
- At Copilot Pro pricing: included in monthly quota (300–500 PRs/month depending on plan)

### Capacity Planning

| Sprint Size | Premium Requests | Tokens | Wall Time |
|-------------|------------------|--------|-----------|
| Small sprint (5–8 tasks) | 10–20 PRs | ~200–400 K | 1–3 hours |
| Medium sprint (10–15 tasks) | 20–40 PRs | ~400–800 K | 3–6 hours |
| Large sprint (15–25 tasks) | 40–80 PRs | ~800 K–1.5 M | 6–12 hours |

## Task Format

Each backlog uses this table format:

```markdown
| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
```

- **ID**: `{MODULE}-{###}` (e.g., `AUTH-010`, `WEB-003`)
- **Size**: XS / S / M / L / XL
- **PRs**: Estimated premium requests consumed
- **Tokens**: Estimated total tokens (K = thousands)
- **Phase**: Roadmap phase (0, 1A, 1B, 1C, 1D, 1E, 1F, 1G)
- **Deps**: Other task IDs this depends on (or `—`)
- **Agent**: Which specialist agent handles it
- **Status**: `✅ Done` / `🔄 Sprint` / `📋 Backlog` / `🚫 Blocked`

## Task Lifecycle

```
📋 Backlog → 🔄 Sprint (planned) → 🏃 In Progress → ✅ Done
                                  ↘ 🚫 Blocked (if dependency not met)
```

## Sprint Workflow

1. **Plan**: User invokes `@boberto` → reads backlogs → proposes sprint
2. **Review**: User approves/adjusts sprint scope
3. **Execute**: Boberto delegates tasks to specialist agents (parallelizable tasks run together)
4. **Track**: Boberto updates backlog statuses after each task completes
5. **Retro**: Actual vs estimated effort recorded for future calibration

## Definition of Done

A task is `✅ Done` when:
- Code compiles with 0 errors, 0 warnings
- Unit tests written and passing (backend: xUnit, web: Playwright, mobile: flutter_test)
- Follows naming conventions and architecture rules from `/docs/`
- Files stay under ~300 lines
- No new SonarQube issues introduced

## Agent → Backlog Mapping

| Agent | Backlog(s) |
|-------|-----------|
| `backend-module` | `backend-*.md` (all 8 modules) |
| `database` | `backend-*.md` (EF configs, migrations, DbContexts) |
| `angular-web` | `web-angular.md` |
| `flutter-mobile` | `mobile-flutter.md` |
| `devops` | `infrastructure.md` |
| `testing` | `testing-quality.md` + inline tests in module backlogs |
| `backend-architect` | Structural tasks across any backlog |
| `boberto` | Orchestrates all backlogs |
