# Musicratic — Supra-Project Tracker

## Project Health

| Metric | Value |
|--------|-------|
| **Total tasks** | 176 |
| **Done** | 144 |
| **Remaining** | 32 |
| **Est. premium requests** | ~350 |
| **Est. tokens** | ~7 M |
| **Est. wall time** | ~60–100 h |

## Sub-Project Summary

| Sub-Project | Total | Done | Backlog | Est. PRs | Est. Tokens | Phase Coverage |
|-------------|-------|------|---------|----------|-------------|----------------|
| [Backend Auth](backend-auth.md) | 16 | 16 | 0 | ~22 | ~400 K | 0, 1A, 1F |
| [Backend Hub](backend-hub.md) | 22 | 19 | 3 | ~35 | ~600 K | 0, 1A, 1F |
| [Backend Playback](backend-playback.md) | 18 | 18 | 0 | ~40 | ~700 K | 1A, 1B |
| [Backend Voting](backend-voting.md) | 15 | 15 | 0 | ~32 | ~550 K | 1B, 1C |
| [Backend Economy](backend-economy.md) | 15 | 15 | 0 | ~34 | ~600 K | 1C, 1D |
| [Backend Analytics](backend-analytics.md) | 10 | 10 | 0 | ~22 | ~380 K | 1E |
| [Backend Social](backend-social.md) | 10 | 0 | 10 | ~22 | ~380 K | 1G |
| [Backend Notification](backend-notification.md) | 10 | 9 | 1 | ~22 | ~380 K | 1B–1G |
| [Web Angular](web-angular.md) | 25 | 16 | 9 | ~55 | ~950 K | 1A–1G |
| [Mobile Flutter](mobile-flutter.md) | 25 | 16 | 9 | ~55 | ~950 K | 1A–1G |
| [Infrastructure](infrastructure.md) | 10 | 8 | 2 | ~12 | ~200 K | 0, 1A |
| [Testing & Quality](testing-quality.md) | 10 | 2 | 8 | ~25 | ~430 K | 0–1G |

## Phase Progress

| Phase | Description | Tasks | Done | % |
|-------|-------------|-------|------|---|
| **Phase 0** | Foundation | 28 | 28 | 100% |
| **Phase 1A** | Hub & List Management | 42 | 42 | 100% |
| **Phase 1B** | Queue & Proposals | 28 | 25 | 89% |
| **Phase 1C** | Voting & Skipping | 24 | 20 | 83% |
| **Phase 1D** | Economy | 18 | 18 | 100% |
| **Phase 1E** | Analytics & Reports | 14 | 11 | 79% |
| **Phase 1F** | Roles & Delegation | 12 | 4 | 33% |
| **Phase 1G** | Social (Minimal) | 14 | 0 | 0% |

## Recommended Sprint Order

Sprints should follow feature dependencies:

```
Sprint 1 ── Phase 1A (Hub & Lists)  ─── backend-auth + backend-hub + backend-playback (entities)
Sprint 2 ── Phase 1A (Hub & Lists)  ─── web + mobile (hub screens) + infra (Authentik)
Sprint 3 ── Phase 1B (Queue)        ─── backend-playback (queue, proposals) + backend-voting (entities)
Sprint 4 ── Phase 1B (Queue)        ─── web + mobile (queue screens, now-playing)
Sprint 5 ── Phase 1C (Voting)       ─── backend-voting (rules, skip) + backend-economy (refunds)
Sprint 6 ── Phase 1C (Voting)       ─── web + mobile (vote UI, tally display)
Sprint 7 ── Phase 1D (Economy)      ─── backend-economy (Stripe, IAP, subscriptions)
Sprint 8 ── Phase 1D (Economy)      ─── web + mobile (wallet, purchase flows)
Sprint 9 ── Phase 1E + 1F           ─── backend-analytics + backend-auth (roles)
Sprint 10 ── Phase 1E + 1F          ─── web + mobile (analytics, role UI)
Sprint 11 ── Phase 1G               ─── backend-social + backend-notification
Sprint 12 ── Phase 1G               ─── web + mobile (social screens) + E2E tests
```

## Legend

- **PRs** = Premium Requests (Copilot quota units)
- **Tokens** = Approximate input + output tokens consumed
- **Phase** = Roadmap phase from [docs/11-development-roadmap.md](../docs/11-development-roadmap.md)

## Task Statuses

| Status | Meaning |
|--------|---------|
| ✅ Done | Completed, build passes, tests pass |
| 🔄 Sprint | Assigned to current sprint, in progress |
| 📋 Backlog | Ready to be planned into a sprint |
| 🚫 Blocked | Dependency not met |
| ⏳ Waiting Human | Needs human input — agent cannot proceed autonomously |

### ⏳ Waiting Human Rules

- Agents mark tasks `⏳ Waiting Human` when they encounter unresolvable blockers (missing config, ambiguous specs, credentials needed, persistent build errors)
- Boberto treats `⏳ Waiting Human` as **blocking for sprint completion** — those tasks are deferred to the next sprint
- Tasks **depending on** `⏳ Waiting Human` tasks are also deferred
- Humans resolve the issue and change status back to `📋 Backlog` for the next planning cycle
