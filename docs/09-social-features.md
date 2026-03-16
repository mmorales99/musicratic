# 09 — Social Features

## Overview

Social features turn Musicratic from a utility into a community. They drive organic growth, retention, and content-quality feedback loops.

## Feature Map

```
Social Features
├── User Profiles
├── Hub Discovery
├── Hub Reviews & Ratings
├── Public Lists
├── Track Leaderboards
├── Live Music Discovery
└── Social Sharing
```

---

## 1. User Profiles

Every registered user has a public profile.

### Profile Fields

| Field | Visibility | Editable |
|---|---|---|
| Display name | Public | Yes |
| Avatar | Public | Yes |
| Bio (140 chars) | Public | Yes |
| Favorite genres (tags) | Public | Yes |
| Total tracks proposed | Public | No (computed) |
| Total upvotes received | Public | No (computed) |
| Hubs visited count | Public | No (computed) |
| Public lists | Public | Yes (toggle per list) |
| Member since | Public | No |
| Currently attached hub | Visible to hub members only | No |

### Profile Stats

- **Proposer score**: Ratio of proposed tracks that played to completion vs. skipped. Higher score = better taste gauge.
- **Active voter badge**: Awarded when user votes on >80% of proposed tracks in a session.
- **Top contributor**: Awarded when 3+ of a user's proposed tracks are incorporated into a hub's permanent list.

---

## 2. Hub Discovery

Users can browse and search for public hubs.

### Discovery Methods

| Method | Description |
|---|---|
| **Search by name** | Full-text search on hub name and description |
| **Genre tags** | Hubs are tagged with genres (set by owner, auto-suggested from track metadata) |
| **Location** (Phase 2) | Find hubs near current GPS location |
| **"Now Playing" preview** | See what's currently playing on a hub before joining |
| **Rating & reviews** | Sort by average star rating |
| **Trending** | Hubs with the most active users in the last 24h |
| **Live music indicator** | Filter for hubs with live performances happening now |

### Discovery Screen

```
┌─────────────────────────────────────┐
│ 🔍 Search hubs...                   │
├─────────────────────────────────────┤
│ [Jazz] [Rock] [Lo-fi] [Latin] ...  │  ← Genre chips
├─────────────────────────────────────┤
│ 📍 Near You         🔥 Trending     │  ← Tabs
│ 🎵 Live Music       ⭐ Top Rated    │
├─────────────────────────────────────┤
│                                     │
│  ┌─────────────────────────────┐    │
│  │ Café Luna          ⭐ 4.7   │    │
│  │ Jazz / Bossa Nova           │    │
│  │ 🎵 Now: "Girl from Ipanema"│    │
│  │ 👥 12 listeners             │    │
│  └─────────────────────────────┘    │
│                                     │
│  ┌─────────────────────────────┐    │
│  │ The Vinyl Corner    ⭐ 4.3  │    │
│  │ Rock / Indie                │    │
│  │ 🎵 Now: "Creep" - Radiohead│    │
│  │ 👥 8 listeners   🎤 LIVE   │    │
│  └─────────────────────────────┘    │
│                                     │
└─────────────────────────────────────┘
```

### Genre Tagging

- Hub owners set **up to 5 genre tags** from a predefined list.
- The system **auto-suggests** additional tags based on track metadata analysis (e.g., if 70%+ of list tracks are classified as "Jazz" by the provider, suggest "Jazz" tag).
- Tags are used for search, filtering, and recommendation.

---

## 3. Hub Reviews & Ratings

Users who have attached to a hub can leave a review.

### Review Structure

| Field | Type | Rules |
|---|---|---|
| Rating | 1-5 stars | Required |
| Comment | Text (500 chars max) | Optional |
| Timestamp | DateTime | Auto |

### Review Rules

- A user can leave **one review per hub**. They can edit or delete it.
- Reviews are only allowed from users who have been **attached to the hub at least once**.
- Hub owners can **respond** to reviews (one response per review).
- Reviews are public on the hub's discovery page.
- **Moderation**: Reviews containing flagged content are held for manual review. Hub owners can report reviews but cannot delete them.

### Aggregate Rating

$$\text{hub\_rating} = \frac{\sum_{i=1}^{n} r_i}{n}$$

Displayed as stars with one decimal (e.g., 4.3). Minimum 3 reviews before rating is shown publicly.

---

## 4. Public Lists

Users and hub owners can make their playlists public.

### Public List Features

| Feature | Description |
|---|---|
| **Browse** | Explore public lists by genre, popularity, or curator |
| **Preview** | See track listing without attaching to a hub |
| **Clone** | Hub owners can clone a public list into their hub (with attribution) |
| **Follow** | Users can follow a list to get notified of changes |
| **Upvote** | Users can upvote public lists; affects ranking |

### List Visibility Settings

| Setting | Who can see |
|---|---|
| **Private** (default) | Only the list owner |
| **Hub-only** | Only users attached to the hub |
| **Public** | Anyone — listed in discovery |

### Most Voted Lists

A leaderboard of public lists ranked by:

$$\text{score} = \text{upvotes} + (0.5 \times \text{followers}) + (0.1 \times \text{clones})$$

Updated daily. Separate leaderboards per genre.

---

## 5. Track Leaderboards

### Hub-Level Leaderboard

Each hub has a "Top Tracks" view:

| Rank | Track | Plays | Upvote % |
|---|---|---|---|
| 1 | "Bohemian Rhapsody" | 342 | 94% |
| 2 | "Hotel California" | 298 | 91% |
| 3 | "Superstition" | 267 | 89% |

Sorted by: `plays × upvote_ratio`.

### Global Leaderboard ("Musicratic Charts")

Aggregated across all public hubs:

- **Most Played** — Tracks with highest play count across all hubs.
- **Most Loved** — Tracks with highest upvote ratio (minimum 50 plays to qualify).
- **Most Proposed** — Tracks most frequently proposed by visitors.
- **Rising** — Tracks with the fastest growth in plays over the last 7 days.

Genre-filtered views available.

---

## 6. Live Music Discovery

Hub owners can signal that a **live performance** is happening.

### Live Music Features

| Feature | Description |
|---|---|
| **Live indicator** | Hub profile shows a "LIVE" badge when active |
| **Live music filter** | Discovery screen has a dedicated "Live Music" tab |
| **Schedule** (optional) | Hub owners can post upcoming live events on their hub profile |
| **Push notification** | Users who follow a hub get notified when live music starts |
| **Live music pushup** | Hubs with live music get a temporary boost in discovery ranking |

### How It Works

1. Hub owner taps "Start Live Set" in the hub management UI.
2. The Musicratic queue **pauses** — live music takes over.
3. Visitors can still vote (thumbs up/down) — this is feedback for the venue, not for skipping.
4. Hub discovery shows the "LIVE" badge.
5. Owner taps "End Live Set" — queue resumes.

### Live Set Statistics

During a live set, the hub collects:

- Number of listeners (attached users).
- Upvote / downvote sentiment (percentage).
- Peak listener count.
- Duration of set.

These stats are shown to the hub owner post-set as a "Live Set Report."

---

## 7. Social Sharing

| Shareable | Where | Format |
|---|---|---|
| **Hub** | Social media, messaging | Link + preview card ("Listen to Café Luna on Musicratic") |
| **Currently playing track** | Social media, messaging | Link + album art + "Now playing at {hub_name}" |
| **Public list** | Social media, messaging | Link + list preview |
| **User profile** | Social media, messaging | Link + stats summary |
| **QR code** | Print, screen display | Hub QR image download/share |

### Deep Linking

All shared links follow the format:

```
https://musicratic.app/{type}/{id}
```

| Type | Example |
|---|---|
| Hub join | `https://musicratic.app/join/CAFELUNA23` |
| Hub profile | `https://musicratic.app/hub/CAFELUNA23` |
| Public list | `https://musicratic.app/list/{list_id}` |
| User profile | `https://musicratic.app/user/{username}` |
| Track (now playing) | `https://musicratic.app/track/{track_id}?hub={hub_code}` |

Deep links open the app if installed, or the web app if not (universal links / app links).

---

## Social Features by Phase

| Feature | Phase 1 | Phase 2 | Phase 3 |
|---|---|---|---|
| User profiles (basic) | Yes | | |
| Hub discovery (name, genre) | Yes | | |
| Hub reviews & ratings | Yes | | |
| Public lists | Yes | | |
| Hub-level leaderboard | Yes | | |
| Social sharing (hub, track) | Yes | | |
| Location-based discovery | | Yes | |
| Global leaderboard | | Yes | |
| Live music features | | Yes | |
| User following / feed | | | Yes |
| Social feed (what your friends are listening to) | | | Yes |
