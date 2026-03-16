# Backend Playback Module — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 18 |
| Done | 0 |
| Remaining | 18 |
| Est. premium requests | ~40 |
| Est. tokens | ~700 K |

## Phase 1A — Hub & List Management

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| PLAY-001 | Track entity (provider, external_id, title, artist, duration, album_art_url, hotness) | S | 1 | 20K | 1A | — | backend-module | 📋 Backlog |
| PLAY-002 | Track EF configuration + TrackRepository | S | 1 | 20K | 1A | PLAY-001 | database | 📋 Backlog |
| PLAY-003 | QueueEntry entity (track_id, hub_id, position, status, source, proposer_id, cost_paid) | M | 2 | 30K | 1A | PLAY-001 | backend-module | 📋 Backlog |
| PLAY-004 | QueueEntry EF configuration + QueueEntryRepository | S | 1 | 20K | 1A | PLAY-003 | database | 📋 Backlog |
| PLAY-005 | PlaybackDbContext (schema "playback") + DI registration | M | 2 | 35K | 1A | PLAY-002, PLAY-004 | database | 📋 Backlog |
| PLAY-006 | IMusicProvider interface (Search, GetMetadata, GetPlaybackUrl) | S | 1 | 15K | 1A | — | backend-module | 📋 Backlog |
| PLAY-007 | SpotifyProvider implementation (Spotify Web API: search, track metadata, playback URL) | L | 4 | 80K | 1A | PLAY-006 | backend-module | 📋 Backlog |
| PLAY-008 | YouTubeMusicProvider implementation (YouTube Data API: search, video metadata) | L | 4 | 80K | 1A | PLAY-006 | backend-module | 📋 Backlog |
| PLAY-009 | NowPlaying query (current track info + progress + queue position) | S | 1 | 20K | 1A | PLAY-003 | backend-module | 📋 Backlog |
| PLAY-010 | PlaybackOrchestrator service (pick next from queue, advance, handle end-of-track) | L | 5 | 90K | 1A | PLAY-003, HUB-020 | backend-module | 📋 Backlog |

## Phase 1B — Queue & Proposals

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| PLAY-011 | Add-to-queue command (from list track or proposal, position calculation) | M | 2 | 35K | 1B | PLAY-010 | backend-module | 📋 Backlog |
| PLAY-012 | Queue interleaving service (alternate list tracks + proposals at configurable ratio) | M | 3 | 50K | 1B | PLAY-011 | backend-module | 📋 Backlog |
| PLAY-013 | Track proposal — coin-paid flow (validate wallet → debit → add to queue) | L | 4 | 70K | 1B | PLAY-011, ECON-005 | backend-module | 📋 Backlog |
| PLAY-014 | Track proposal — collective vote flow (create pre-play vote, 2min window, 50% threshold) | L | 4 | 70K | 1B | PLAY-011, VOTE-004 | backend-module | 📋 Backlog |
| PLAY-015 | Proposal approval/rejection by owner (override collective vote result) | M | 2 | 35K | 1B | PLAY-014 | backend-module | 📋 Backlog |
| PLAY-016 | Queue WebSocket broadcasting (NOW_PLAYING, QUEUE_UPDATED, TRACK_ENDED, TRACK_SKIPPED) | L | 4 | 70K | 1B | PLAY-010 | backend-module | 📋 Backlog |
| PLAY-017 | Playback API endpoints (get queue, get now-playing, propose track, skip) | M | 2 | 40K | 1B | PLAY-010 | backend-module | 📋 Backlog |
| PLAY-018 | Dapr event handlers (hub_activated → start queue, hub_deactivated → stop queue) | M | 2 | 35K | 1B | PLAY-010 | backend-module | 📋 Backlog |

## Dependency Graph

```
PLAY-001 ──► PLAY-002
         └─► PLAY-003 ──► PLAY-004 ──► PLAY-005
                      └─► PLAY-009
                      └─► PLAY-010 ──► PLAY-011 ──► PLAY-012
                                   │            ├─► PLAY-013 (also needs ECON-005)
                                   │            └─► PLAY-014 (also needs VOTE-004)
                                   │                     └─► PLAY-015
                                   ├─► PLAY-016
                                   ├─► PLAY-017
                                   └─► PLAY-018
PLAY-006 ──► PLAY-007
         └─► PLAY-008
```
