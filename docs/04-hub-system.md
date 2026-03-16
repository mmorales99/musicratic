# 04 — Hub System

## What Is a Hub?

A **hub** is the central unit of Musicratic. It represents a shared listening space — physical (a café, a gym) or virtual (a road trip, a house party). A hub owns a playback queue, a set of playlists, and a community of attached users.

## Hub Types

| Type | Description | Phase |
|---|---|---|
| **Venue** | Permanent hub tied to a physical business. Managed by a super list owner with optional sub-owners/managers. | Phase 1 |
| **Portable** | Temporary hub for social contexts. Has a designated **mood** that determines behavior rules. | Phase 2 |

## Hub Lifecycle

```
  Owner creates hub
        │
        ▼
  ┌─────────────┐     configure lists,
  │   created    │ ──▶ settings, providers
  └──────┬───────┘
         │ activate
         ▼
  ┌─────────────┐     users can attach,
  │   active     │ ◀── queue plays,
  └──────┬───────┘     votes are counted
         │
    ┌────┴────┐
    │         │
    ▼         ▼
 pause     deactivate
    │         │
    ▼         ▼
 ┌────────┐  ┌──────────────┐
 │ paused │  │ deactivated  │
 └───┬────┘  └──────────────┘
     │ resume
     └──────▶ active
```

## Hub Configuration

When a hub is created, the owner configures:

1. **Name & Description** — Public-facing identity.
2. **Music sources** — Which providers are enabled (Spotify, YouTube Music, local).
3. **Base playlist(s)** — One or more curated lists assigned to the hub.
4. **Play mode** — Ordered or shuffled (per list).
5. **Proposal settings** — Whether visitors can propose tracks, max queue depth.
6. **Voting rules** — Threshold for auto-skip (default 65%), voting window (default 60s).
7. **Economy settings** — Coin cost multiplier (if the hub wants to adjust base pricing).

## Connectivity — How Users Attach

### Phase 1: QR Code + Direct Link (Time-Limited)

Each hub generates:

- **QR Code**: Encodes a signed URL → `https://musicratic.app/join/{hub_code}?sig={signature}`
- **Direct Link**: Same URL, shareable via text/social.
- **Hub Code**: Short alphanumeric code for manual entry (e.g., `CAFELUNA23`).

**Attachment process:**

1. User scans QR / clicks link / enters code.
2. App validates hub exists and is active.
3. If user is already attached to another hub → detach from previous hub.
4. Create `HubAttachment` with `expires_at = now + 1 hour`.
5. User sees the hub's live queue and can interact.

**Attachment duration:** 1 hour fixed. User can manually detach or re-attach at any time (timer resets).

### Phase 2: Location-Based (Persistent)

Requires geolocation permission.

- Hub owner defines a **geofence radius** (e.g., 50 meters).
- Users within range are **auto-attached** when they open the app.
- Users **remain attached** as long as they stay within range.
- When leaving range, attachment persists for a grace period (e.g., 5 min) then expires.
- When returning within range within grace period, attachment resumes seamlessly.
- Location checks happen:
  - On app foreground.
  - Periodically in background (battery-optimized).
  - On significant location change (OS-level trigger).

### QR Code Security

- QR URLs include a **cryptographic signature** derived from the hub's secret key.
- Prevents hub impersonation (you can't create a QR that attaches users to someone else's hub).
- Signatures rotate periodically (configurable by hub owner).
- Short-lived QR codes can be generated for events.

## Hub Hierarchy — Multi-Owner Management

A venue hub supports a role hierarchy:

```
        Super List Owner (creator)
               │
        ┌──────┴──────┐
        ▼              ▼
  Sub Hub Manager   Sub List Owner
        │
        ▼
  (can manage users, settings)
```

| Role | Can do |
|---|---|
| **Super List Owner** | Everything: CRUD hub, assign roles, manage lists, manage subscriptions, override all votes |
| **Sub Hub Manager** | Manage hub settings, manage user attachments, view analytics. Cannot delete hub or manage billing. |
| **Sub List Owner** | Manage assigned lists (add/remove tracks, set play mode). Has owner vote priority on their lists. |

**Delegation rules:**

- Only the super list owner can promote users to sub-manager or sub-list-owner.
- Sub-managers cannot promote other users.
- Sub-list-owners receive owner-level vote priority only for tracks in their assigned lists.
- All roles can be revoked by the super list owner at any time.

## Queue Behavior

The hub's queue is the **central playback pipeline**. It interleaves list tracks and proposed tracks.

### Queue Population

1. **From list**: The Playback Orchestrator continuously draws the next track from the active list (respecting order or shuffle + vote-weighted probability).
2. **From proposals**: When a user proposes a track (via coins or collective vote), it's inserted into the queue at a designated position.

### Queue Ordering Rules

```
List Track → Proposed Track → List Track → List Track → Proposed Track → ...
```

- Proposed tracks are interleaved with list tracks at a configurable ratio (default: 1 proposed per 2 list tracks).
- If no proposals are pending, list tracks play continuously.
- If the queue runs out of list tracks (e.g., empty list), only proposals play.
- Coin-paid proposals get **priority** over vote-approved proposals in the proposal sub-queue.

### Shuffle Weighting

When a list is in shuffle mode, track selection is **weighted by vote score**:

$$w_i = \max\left(0.1,\ \frac{u_i + 1}{u_i + d_i + 2}\right)$$

Where:
- $w_i$ = shuffle weight for track $i$
- $u_i$ = total upvotes for track $i$ in this list
- $d_i$ = total downvotes for track $i$ in this list
- The +1/+2 smoothing prevents new tracks from having zero weight.
- The floor of 0.1 ensures every track has some chance of playing.

### Weekly & Monthly Reports

**Weekly (every Monday at hub's configured timezone):**

The system generates a **"Tracks to Review"** report for the list owner:

- Tracks with the highest downvote ratio over the past 7 days.
- Sorted by `downvotes / (upvotes + downvotes)` descending.
- Top 10 or 10% of list (whichever is smaller).
- Owner can: keep, remove, or replace each flagged track.

**Monthly (1st of each month):**

The system generates a **"Tracks to Incorporate"** report:

- User-proposed tracks that were played the most and received the highest upvote ratio.
- Sorted by `plays * (upvotes / (upvotes + downvotes))` descending.
- Top 10 or 10% of proposals (whichever is smaller).
- Owner can: add to list (stats reset to 0), ignore, or block from future proposals.

## Hub Discovery

Hubs can be discoverable or private:

| Setting | Behavior |
|---|---|
| **Public** | Listed in search/discovery. Findable by name, location, genre tags. |
| **Private** | Only accessible via QR code / direct link / hub code. |
| **Unlisted** | Not searchable, but anyone with the link can join. |

**Discovery filters:**

- Genre tags (set by owner, refined by track metadata).
- Location (Phase 2, with geolocation).
- Rating (from user reviews).
- "Now playing" preview.
- Live music indicator (owner-toggled when live performance is happening).
