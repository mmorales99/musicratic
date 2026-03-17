# Backend Social Module — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 10 |
| Done | 10 |
| Remaining | 0 |
| Est. premium requests | ~22 |
| Est. tokens | ~380 K |

## Phase 1G — Social (Minimal)

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| SOCL-001 | HubReview entity (user_id, hub_id, rating 1–5, comment, created_at) | S | 1 | 15K | 1G | — | backend-module | ✅ Done |
| SOCL-002 | HubReview EF configuration + HubReviewRepository | S | 1 | 20K | 1G | SOCL-001 | database | ✅ Done |
| SOCL-003 | SocialDbContext (schema "social") + DI registration | M | 2 | 30K | 1G | SOCL-002 | database | ✅ Done |
| SOCL-004 | Hub discovery service (search by name, genre tags, rating, active status) | M | 2 | 40K | 1G | — | backend-module | ✅ Done |
| SOCL-005 | Create/update/delete review commands + handlers | M | 2 | 40K | 1G | SOCL-001 | backend-module | ✅ Done |
| SOCL-006 | Hub rating aggregation query (average rating, review count per hub) | S | 1 | 20K | 1G | SOCL-005 | backend-module | ✅ Done |
| SOCL-007 | Social sharing service (generate share URL with hub code + OG metadata) | M | 2 | 35K | 1G | HUB-012 | backend-module | ✅ Done |
| SOCL-008 | User public profile query (display name, avatar, total proposals, total votes) | S | 1 | 20K | 1G | — | backend-module | ✅ Done |
| SOCL-009 | Social API endpoints (reviews CRUD, discovery search, user profile, share link) | M | 2 | 40K | 1G | SOCL-004 | backend-module | ✅ Done |
| SOCL-010 | Dapr events (review_created, hub_rated) for cross-module notification | S | 1 | 20K | 1G | SOCL-005 | backend-module | ✅ Done |

## Dependency Graph

```
SOCL-001 ──► SOCL-002 ──► SOCL-003
         └─► SOCL-005 ──► SOCL-006
                      └─► SOCL-010
SOCL-004 ──► SOCL-009
SOCL-007 (depends on HUB-012 for deep links)
SOCL-008 (standalone)
```
