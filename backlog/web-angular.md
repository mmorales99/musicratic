# Web Angular Client — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 25 |
| Done | 13 |
| Remaining | 12 |
| Est. premium requests | ~55 |
| Est. tokens | ~950 K |

> Note: Scaffold exists (shell, feature routes, shared services, XState machines as stubs). All tasks below build on that scaffold.

## Phase 1A — Hub & List Management

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| WEB-001 | Auth flow (redirect to Authentik, handle callback, store JWE, auto-refresh) | L | 4 | 80K | 1A | AUTH-012 | angular-web | ✅ Done |
| WEB-002 | Auth XState machine (idle → authenticating → authenticated → refreshing → error) | M | 2 | 40K | 1A | WEB-001 | angular-web | ✅ Done |
| WEB-003 | Hub creation form (name, type, providers, settings, validation) | M | 3 | 50K | 1A | HUB-013 | angular-web | ✅ Done |
| WEB-004 | Hub detail screen (info, settings, members, lists, QR code display) | L | 4 | 70K | 1A | HUB-011 | angular-web | ✅ Done |
| WEB-005 | List management screen (CRUD lists, add/remove/reorder tracks, search tracks) | L | 4 | 70K | 1A | HUB-018 | angular-web | ✅ Done |
| WEB-006 | Hub join flow (enter code or deep link → validate → attach → see queue) | M | 3 | 50K | 1A | HUB-015 | angular-web | ✅ Done |
| WEB-007 | Hub discovery screen (search, filter by name/genre/rating, paginated results) | M | 3 | 50K | 1A | HUB-019 | angular-web | ✅ Done |

## Phase 1B — Queue & Proposals

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| WEB-008 | Live queue screen (WebSocket real-time list of upcoming tracks, current position) | L | 4 | 70K | 1B | PLAY-016 | angular-web | ✅ Done |
| WEB-009 | Now-playing component (track info, album art, progress bar, time remaining) | M | 3 | 50K | 1B | PLAY-009 | angular-web | ✅ Done |
| WEB-010 | Track proposal flow (search multi-provider → choose pay/free → confirm) | L | 4 | 70K | 1B | PLAY-013 | angular-web | 📋 Backlog |
| WEB-011 | Track search component (query → BFF → results from Spotify + YouTube) | M | 3 | 50K | 1B | PLAY-006 | angular-web | ✅ Done |

## Phase 1C — Voting & Skipping

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| WEB-012 | Vote buttons component (up/down on now-playing, disabled after voting) | M | 2 | 40K | 1C | VOTE-004 | angular-web | ✅ Done |
| WEB-013 | Vote tally display (real-time percentages via WebSocket, progress bar) | M | 2 | 40K | 1C | VOTE-009 | angular-web | ✅ Done |
| WEB-014 | Skip notification (toast when track skipped, refund notification if applicable) | S | 1 | 20K | 1C | VOTE-007 | angular-web | ✅ Done |

## Phase 1D — Economy

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| WEB-015 | Wallet screen (balance display, transaction history, paginated list) | M | 3 | 50K | 1D | ECON-006 | angular-web | 📋 Backlog |
| WEB-016 | Coin purchase flow (select package → Stripe Checkout → confirmation) | L | 4 | 70K | 1D | ECON-008 | angular-web | 📋 Backlog |
| WEB-017 | Subscription management screen (current tier, upgrade/downgrade, billing) | M | 3 | 50K | 1D | ECON-013 | angular-web | 📋 Backlog |

## Phase 1E — Analytics & Reports

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| WEB-018 | Analytics dashboard (charts: top tracks, vote distribution, play counts) | L | 4 | 70K | 1E | ANLT-009 | angular-web | 📋 Backlog |
| WEB-019 | Reports screen (weekly/monthly summaries, actionable suggestions) | M | 2 | 40K | 1E | ANLT-006 | angular-web | 📋 Backlog |

## Phase 1F — Roles & Delegation

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| WEB-020 | Member management screen (list members, view roles, remove member) | M | 2 | 40K | 1F | HUB-030 | angular-web | 📋 Backlog |
| WEB-021 | Role assignment UI (promote/demote, tier limit warnings) | M | 2 | 40K | 1F | HUB-031 | angular-web | 📋 Backlog |
| WEB-022 | Conditional UI rendering by role (show/hide actions based on permissions) | M | 2 | 35K | 1F | AUTH-021 | angular-web | 📋 Backlog |

## Phase 1G — Social

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| WEB-023 | User profile screen (display name, avatar upload, stats, edit) | M | 2 | 40K | 1G | SOCL-008 | angular-web | 📋 Backlog |
| WEB-024 | Hub reviews component (star rating, write review, view reviews list) | M | 2 | 40K | 1G | SOCL-005 | angular-web | 📋 Backlog |
| WEB-025 | Social sharing (Web Share API, copy link, generate share card) | S | 1 | 25K | 1G | SOCL-007 | angular-web | 📋 Backlog |
