# 03 — Domain Model

## Entity Relationship Overview

```
┌──────────┐        owns         ┌──────────┐      contains      ┌──────────┐
│   User   │ ──────────────────▶ │   Hub    │ ─────────────────▶ │   List   │
│          │                     │          │                     │          │
│ id       │                     │ id       │                     │ id       │
│ name     │  attached_to (0..1) │ name     │                     │ name     │
│ email    │ ◀──────────────────│ code     │                     │ mode     │
│ role     │                     │ type     │                     │ (ordered/│
│ wallet   │                     │ mood     │                     │  shuffle)│
└────┬─────┘                     └────┬─────┘                     └────┬─────┘
     │                                │                                │
     │ votes                          │ has_queue                      │ has_tracks
     ▼                                ▼                                ▼
┌──────────┐                     ┌──────────┐                     ┌──────────┐
│   Vote   │                     │  Queue   │ ◀── proposed_by ── │  Track   │
│          │                     │  Entry   │                     │          │
│ user_id  │  vote_for           │          │                     │ id       │
│ entry_id │ ──────────────────▶ │ id       │  references        │ provider │
│ value    │                     │ track_id │ ──────────────────▶ │ ext_id   │
│ cast_at  │                     │ position │                     │ title    │
│          │                     │ status   │                     │ artist   │
│          │                     │ source   │                     │ duration │
└──────────┘                     │ paid     │                     │ hotness  │
                                 └──────────┘                     └────┬─────┘
                                                                       │
                                                                       │ stats
                                                                       ▼
                                                                  ┌──────────┐
                                                                  │  Track   │
                                                                  │  Stats   │
                                                                  │          │
                                                                  │ track_id │
                                                                  │ hub_id   │
                                                                  │ upvotes  │
                                                                  │ downvotes│
                                                                  │ plays    │
                                                                  │ skips    │
                                                                  └──────────┘
```

## Core Entities

### User

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `authentik_sub` | String | Authentik subject identifier (OIDC `sub` claim) |
| `display_name` | String | Public display name |
| `email` | String | Login credential |
| `avatar_url` | String? | Profile image |
| `created_at` | DateTime | Registration timestamp |
| `wallet_balance` | Int | Virtual coin balance (atomic integer, no decimals) |
| `current_attachment` | HubAttachment? | Currently attached hub (null if none) |

> **Supra-tenant scoped**: Users exist at the global (supra-tenant) level. A user can be a member of multiple hub-tenants.

### Hub

Every hub is a **tenant**. The `tenant_id` isolates all hub-scoped data (queue, votes, lists, stats).

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `tenant_id` | UUID | Tenant identifier (equals `id` by default; used for data isolation) |
| `name` | String | Display name (e.g., "Café Luna") |
| `code` | String | Unique short code for QR/link (e.g., `CAFELUNA23`) |
| `type` | Enum | `venue` / `portable` |
| `mood` | Enum? | Only for portable: `home_party` / `driving` / ... |
| `owner_id` | UUID | Super list owner (creator) |
| `subscription_tier` | Enum | `free_trial` / `monthly` / `annual` / `event` |
| `subscription_expires_at` | DateTime? | Null for free trial period |
| `is_active` | Bool | Whether hub is currently broadcasting |
| `qr_url` | String | URL to generated QR code image |
| `direct_link` | String | Deep link for hub attachment |
| `created_at` | DateTime | Hub creation timestamp |
| `settings` | HubSettings | Embedded configuration object |

### HubSettings

| Field | Type | Description |
|---|---|---|
| `allow_proposals` | Bool | Whether visitors can propose tracks |
| `auto_skip_threshold` | Double | Downvote percentage for auto-skip (default: 0.65) |
| `voting_window_seconds` | Int | Duration of voting window (default: 60) |
| `max_queue_size` | Int | Maximum pending proposals in queue |
| `allowed_providers` | List\<Provider\> | Which music sources are enabled |
| `enable_local_storage` | Bool | Only available on paid tier |
| `ads_enabled` | Bool | Derived from subscription status |

### HubAttachment

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `user_id` | UUID | Attached user |
| `hub_id` | UUID | Target hub |
| `attached_at` | DateTime | When user attached |
| `expires_at` | DateTime | Phase 1: attached_at + 1 hour. Phase 2: null (location-based) |
| `is_active` | Bool | Computed: now < expires_at and not manually detached |

### List

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `hub_id` | UUID | Parent hub |
| `name` | String | List display name |
| `owner_id` | UUID | List owner (may be sub-owner) |
| `play_mode` | Enum | `ordered` / `shuffled` |
| `created_at` | DateTime | Creation timestamp |

### ListTrack

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `list_id` | UUID | Parent list |
| `track_id` | UUID | Referenced track |
| `position` | Int | Order in list (for ordered mode) |
| `added_at` | DateTime | When added to list |
| `total_upvotes` | Int | Cumulative upvotes while in this list |
| `total_downvotes` | Int | Cumulative downvotes while in this list |
| `total_plays` | Int | Times played while in this list |
| `shuffle_weight` | Double | Computed from vote ratio; higher = more frequent in shuffle |

> **Stat reset rule**: When a user-proposed track is incorporated into the list, its `total_upvotes`, `total_downvotes`, and `total_plays` start at 0. The track's "while proposed" stats are preserved in `TrackStats`. If the track is later removed from the list, all stats restart.

### Track

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key (internal) |
| `provider` | Enum | `spotify` / `youtube_music` / `local` |
| `external_id` | String | Provider-specific track ID |
| `title` | String | Track title |
| `artist` | String | Artist name |
| `album` | String? | Album name |
| `duration_seconds` | Int | Track length in seconds |
| `cover_url` | String? | Album art URL |
| `hotness_score` | Double | Global popularity across all hubs (updated periodically) |

### QueueEntry

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `hub_id` | UUID | Parent hub |
| `track_id` | UUID | Track to play |
| `proposed_by` | UUID? | User who proposed (null if from list) |
| `source` | Enum | `list` / `proposal_coins` / `proposal_vote` |
| `status` | Enum | `pending` / `playing` / `played` / `skipped` |
| `position` | Int | Queue position |
| `coins_spent` | Int | Coins paid (0 if free/from list) |
| `created_at` | DateTime | When entry was created |
| `played_at` | DateTime? | When playback started |
| `voting_ends_at` | DateTime? | End of voting window (played_at + voting_window_seconds) |

### Vote

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `user_id` | UUID | Voter |
| `queue_entry_id` | UUID | Target queue entry |
| `value` | Enum | `up` / `down` |
| `cast_at` | DateTime | When vote was cast |
| `is_owner_vote` | Bool | Whether voter is a list owner (for priority logic) |

### TrackStats

Aggregated statistics for a track within a specific hub context.

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `track_id` | UUID | Target track |
| `hub_id` | UUID | Hub context |
| `context` | Enum | `in_list` / `proposed` |
| `upvotes` | Int | Total upvotes |
| `downvotes` | Int | Total downvotes |
| `plays` | Int | Total plays |
| `skips` | Int | Total skips |
| `last_played_at` | DateTime? | Most recent play |

### Wallet Transaction

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `user_id` | UUID | Wallet owner |
| `type` | Enum | `purchase` / `spend` / `refund` |
| `amount` | Int | Coin amount (positive = credit, negative = debit) |
| `reference_id` | UUID? | Related entity (queue_entry_id, purchase_id) |
| `description` | String | Human-readable description |
| `created_at` | DateTime | Transaction timestamp |

### HubMember

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `hub_id` | UUID | Hub |
| `user_id` | UUID | Member |
| `role` | Enum | `super_owner` / `sub_manager` / `sub_list_owner` / `visitor` |
| `assigned_by` | UUID? | Who assigned this role |
| `assigned_at` | DateTime | When role was assigned |

### Review

| Field | Type | Description |
|---|---|---|
| `id` | UUID | Primary key |
| `hub_id` | UUID | Reviewed hub |
| `user_id` | UUID | Author |
| `rating` | Int | 1-5 stars |
| `comment` | String? | Optional text |
| `created_at` | DateTime | Review timestamp |

## Multi-Tenancy in the Domain Model

The system has two tenant scopes:

| Scope | Entities | Description |
|---|---|---|
| **Supra-tenant** (global) | User, WalletTransaction, Track (global catalog), Review | Shared across all hubs. The supra-tenant always exists. |
| **Hub-tenant** (per hub) | QueueEntry, Vote, List, ListTrack, TrackStats, HubMember, HubAttachment | Isolated per hub via `tenant_id` column. |

All hub-tenant-scoped queries are automatically filtered by `tenant_id` via middleware. When a hub operates **offline** (disconnected from the supra-tenant), hub-tenant data is stored in **SQLite** locally and synced to PostgreSQL on reconnect.

---

## Key Relationships

| Relationship | Cardinality | Notes |
|---|---|---|
| User → Hub (ownership) | 1:N | A user can own multiple hubs |
| User → HubAttachment | 1:0..1 | A user is attached to at most one hub at a time |
| Hub → List | 1:N | A hub has one or more playlists |
| List → ListTrack | 1:N | Each list has ordered tracks |
| Hub → QueueEntry | 1:N | Active queue for the hub |
| QueueEntry → Vote | 1:N | Each entry collects votes |
| Hub → HubMember | 1:N | Role assignments per hub |
| User → WalletTransaction | 1:N | Transaction history |

## State Machines

### QueueEntry Status

```
              ┌──────────────┐
              │   pending    │
              └──────┬───────┘
                     │ playback starts
                     ▼
              ┌──────────────┐
        ┌─────│   playing    │─────┐
        │     └──────────────┘     │
        │ track ends               │ skipped (votes or owner)
        ▼                          ▼
 ┌──────────────┐          ┌──────────────┐
 │    played    │          │   skipped    │
 └──────────────┘          └──────────────┘
```

### HubAttachment Lifecycle

```
  User scans QR / clicks link
              │
              ▼
       ┌─────────────┐
       │   active     │ ◀─── Phase 2: auto-reattach if in range
       └──────┬───────┘
              │ expires (1h) OR manual detach OR attach elsewhere
              ▼
       ┌─────────────┐
       │   expired    │
       └─────────────┘
```
