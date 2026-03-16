# Backend Voting Module — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 15 |
| Done | 14 |
| Remaining | 1 |
| Est. premium requests | ~32 |
| Est. tokens | ~550 K |

## Phase 1B — Queue & Proposals

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| VOTE-001 | Vote entity (user_id, queue_entry_id, value [up/down], cast_at) | S | 1 | 15K | 1B | — | backend-module | ✅ Done |
| VOTE-002 | Vote EF configuration + VoteRepository | S | 1 | 20K | 1B | VOTE-001 | database | ✅ Done |
| VOTE-003 | VotingDbContext (schema "voting") + DI registration | M | 2 | 30K | 1B | VOTE-002 | database | ✅ Done |
| VOTE-004 | CastVote command + handler (validate: one vote per user per entry, record vote) | M | 2 | 40K | 1B | VOTE-001 | backend-module | ✅ Done |
| VOTE-005 | GetTally query (upvotes, downvotes, percentage for a queue entry) | S | 1 | 20K | 1B | VOTE-001 | backend-module | ✅ Done |
| VOTE-014 | Collective vote proposal voting (pre-play vote, 2min window, ≥50% to approve) | L | 4 | 70K | 1B | VOTE-004 | backend-module | ✅ Done |
| VOTE-015 | Cooldown enforcement (5min after rejected collective proposal, max 1 pending) | S | 1 | 20K | 1B | VOTE-014 | backend-module | ✅ Done |

## Phase 1C — Voting & Skipping

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| VOTE-006 | Voting window timer service (60s window, opens when proposed track plays) | M | 3 | 50K | 1C | VOTE-004 | backend-module | ✅ Done |
| VOTE-007 | Skip rule engine (≥65% downvotes → skip, configurable threshold + min vote count) | M | 3 | 50K | 1C | VOTE-005 | backend-module | ✅ Done |
| VOTE-008 | Owner priority vote (owner downvote → instant skip, exempt from window, scoped to lists) | M | 2 | 40K | 1C | VOTE-007, AUTH-020 | backend-module | 📋 Backlog |
| VOTE-009 | Vote tally real-time WebSocket broadcasting (tally updates on each vote) | M | 2 | 40K | 1C | VOTE-005 | backend-module | ✅ Done |
| VOTE-010 | Anti-abuse: rate limiting (1 vote/user/entry), device fingerprint check | M | 2 | 40K | 1C | VOTE-004 | backend-module | ✅ Done |
| VOTE-011 | Minimum vote count threshold (configurable per hub, default 1) | S | 1 | 20K | 1C | VOTE-007 | backend-module | ✅ Done |
| VOTE-012 | Voting API endpoints (cast vote, get tally, get voting window status) | M | 2 | 35K | 1C | VOTE-004 | backend-module | ✅ Done |
| VOTE-013 | Dapr events (vote_cast, track_skipped) + handlers for stats integration | M | 2 | 35K | 1C | VOTE-004, PLAY-010 | backend-module | ✅ Done |

## Dependency Graph

```
VOTE-001 ──► VOTE-002 ──► VOTE-003
         └─► VOTE-004 ──► VOTE-005 ──► VOTE-007 ──► VOTE-008 (also needs AUTH-020)
                      │            │            └─► VOTE-011
                      │            └─► VOTE-009
                      ├─► VOTE-006
                      ├─► VOTE-010
                      ├─► VOTE-012
                      ├─► VOTE-013 (also needs PLAY-010)
                      └─► VOTE-014 ──► VOTE-015
```
