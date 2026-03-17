# Mobile Flutter Client — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 25 |
| Done | 25 |
| Remaining | 0 |
| Est. premium requests | ~55 |
| Est. tokens | ~950 K |

> Note: Scaffold exists (features with bloc/event/state/screen/repository stubs). All tasks below build on that scaffold.

## Phase 1A — Hub & List Management

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| MOB-001 | Auth flow (open Authentik in WebView/browser, handle callback, secure token storage) | L | 4 | 80K | 1A | AUTH-012 | flutter-mobile | ✅ Done |
| MOB-002 | Auth bloc (idle → authenticating → authenticated → refreshing → error, with freezed) | M | 2 | 40K | 1A | MOB-001 | flutter-mobile | ✅ Done |
| MOB-003 | Hub creation screen (form fields, validation, submit to BFF) | M | 3 | 50K | 1A | HUB-013 | flutter-mobile | ✅ Done |
| MOB-004 | Hub detail screen (info, settings, members, lists, QR display) | L | 4 | 70K | 1A | HUB-011 | flutter-mobile | ✅ Done |
| MOB-005 | List management screen (CRUD lists, add/remove/reorder tracks) | L | 4 | 70K | 1A | HUB-018 | flutter-mobile | ✅ Done |
| MOB-006 | QR scanner + hub join flow (camera scan → validate → attach → queue) | L | 4 | 70K | 1A | HUB-015 | flutter-mobile | ✅ Done |
| MOB-007 | Hub discovery screen (search, filter, paginated results, pull-to-refresh) | M | 3 | 50K | 1A | HUB-019 | flutter-mobile | ✅ Done |

## Phase 1B — Queue & Proposals

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| MOB-008 | Live queue screen (WebSocket real-time, animated list transitions) | L | 4 | 70K | 1B | PLAY-016 | flutter-mobile | ✅ Done |
| MOB-009 | Now-playing widget (track info, album art, animated progress, time remaining) | M | 3 | 50K | 1B | PLAY-009 | flutter-mobile | ✅ Done |
| MOB-010 | Track proposal flow (search → choose pay/free → confirm, with coin balance check) | L | 4 | 70K | 1B | PLAY-013 | flutter-mobile | ✅ Done |
| MOB-011 | Track search screen (multi-provider search with provider tabs) | M | 3 | 50K | 1B | PLAY-006 | flutter-mobile | ✅ Done |

## Phase 1C — Voting & Skipping

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| MOB-012 | Vote buttons widget (up/down, haptic feedback, disabled after voting) | M | 2 | 40K | 1C | VOTE-004 | flutter-mobile | ✅ Done |
| MOB-013 | Vote tally display (real-time animated bar via WebSocket) | M | 2 | 40K | 1C | VOTE-009 | flutter-mobile | ✅ Done |
| MOB-014 | Skip notification (snackbar + optional refund confirmation) | S | 1 | 20K | 1C | VOTE-007 | flutter-mobile | ✅ Done |

## Phase 1D — Economy

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| MOB-015 | Wallet screen (balance, transaction history, pull-to-refresh) | M | 3 | 50K | 1D | ECON-006 | flutter-mobile | ✅ Done |
| MOB-016 | Coin purchase flow (Apple IAP / Google Play Billing, in_app_purchase package) | XL | 8 | 150K | 1D | ECON-009, ECON-010 | flutter-mobile | ✅ Done |
| MOB-017 | Subscription management screen (current tier, upgrade/downgrade) | M | 3 | 50K | 1D | ECON-013 | flutter-mobile | ✅ Done |

## Phase 1E — Analytics & Reports

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| MOB-018 | Analytics dashboard (charts via fl_chart, top tracks, vote distribution) | L | 4 | 70K | 1E | ANLT-009 | flutter-mobile | ✅ Done |
| MOB-019 | Reports screen (weekly/monthly summaries, expandable cards) | M | 2 | 40K | 1E | ANLT-006 | flutter-mobile | ✅ Done |

## Phase 1F — Roles & Delegation

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| MOB-020 | Member management screen (list members, swipe-to-remove) | M | 2 | 40K | 1F | HUB-030 | flutter-mobile | ✅ Done |
| MOB-021 | Role assignment UI (promote/demote bottom sheet, tier limit warnings) | M | 2 | 40K | 1F | HUB-031 | flutter-mobile | ✅ Done |
| MOB-022 | Conditional UI by role (Visibility widgets based on user role in hub) | M | 2 | 35K | 1F | AUTH-021 | flutter-mobile | ✅ Done |

## Phase 1G — Social

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| MOB-023 | User profile screen (display name, avatar picker, stats, edit) | M | 2 | 40K | 1G | SOCL-008 | flutter-mobile | ✅ Done |
| MOB-024 | Hub reviews widget (star rating, write review, reviews list) | M | 2 | 40K | 1G | SOCL-005 | flutter-mobile | ✅ Done |
| MOB-025 | Social sharing (platform share sheet, copy link, deep link handling) | S | 1 | 25K | 1G | SOCL-007 | flutter-mobile | ✅ Done |
