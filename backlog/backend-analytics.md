# Backend Analytics Module — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 10 |
| Done | 0 |
| Remaining | 10 |
| Est. premium requests | ~22 |
| Est. tokens | ~380 K |

## Phase 1E — Analytics & Reports

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| ANLT-001 | TrackStats entity (track_id, hub_id, upvotes, downvotes, plays, skips, last_played_at) | S | 1 | 20K | 1E | — | backend-module | 📋 Backlog |
| ANLT-002 | TrackStats EF configuration + TrackStatsRepository | S | 1 | 20K | 1E | ANLT-001 | database | 📋 Backlog |
| ANLT-003 | AnalyticsDbContext (schema "analytics") + DI registration | M | 2 | 30K | 1E | ANLT-002 | database | 📋 Backlog |
| ANLT-004 | Stats aggregation service (increment counters from vote/play/skip events) | M | 3 | 50K | 1E | ANLT-001 | backend-module | 📋 Backlog |
| ANLT-005 | Shuffle weight calculation (score = (upvotes−downvotes)/plays, normalize to 0–1 range) | M | 2 | 40K | 1E | ANLT-004 | backend-module | 📋 Backlog |
| ANLT-006 | Weekly downvoted tracks report (tracks with >40% downvotes → notify list owner) | M | 3 | 50K | 1E | ANLT-004 | backend-module | 📋 Backlog |
| ANLT-007 | Monthly popular proposals report (top proposed tracks → suggest adding to list) | M | 3 | 50K | 1E | ANLT-004 | backend-module | 📋 Backlog |
| ANLT-008 | Hotness calculation service (concurrent_plays / active_hubs → hotness tier) | M | 2 | 40K | 1E | ANLT-004 | backend-module | 📋 Backlog |
| ANLT-009 | Analytics API endpoints (get hub stats, get track stats, get reports) | M | 2 | 35K | 1E | ANLT-004 | backend-module | 📋 Backlog |
| ANLT-010 | Dapr event handlers (vote_cast → +1, track_played → +1, track_skipped → +1) | M | 2 | 40K | 1E | ANLT-004 | backend-module | 📋 Backlog |

## Dependency Graph

```
ANLT-001 ──► ANLT-002 ──► ANLT-003
         └─► ANLT-004 ──► ANLT-005
                      ├─► ANLT-006
                      ├─► ANLT-007
                      ├─► ANLT-008
                      ├─► ANLT-009
                      └─► ANLT-010
```
