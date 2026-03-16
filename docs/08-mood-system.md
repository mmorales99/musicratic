# 08 — Mood System (Portable Hubs)

> **Phase 2 feature** — Built after core venue hub functionality is complete and revenue-generating.

## Concept

A **mood** is a portable, temporary hub designed for social listening sessions outside of venues. Instead of a fixed business location, a mood is created ad-hoc by a regular user for a specific context.

The mood determines the **behavioral rules** of the hub — who controls what, how skipping works, and how the session is managed.

## Available Moods

| Mood | Context | Key Behavior |
|---|---|---|
| **Home Party** | House party, gathering | Democratic — composer sets list but votes like everyone else |
| **Driving** | Road trip, car ride | Driver-controlled — driver's list is king, passengers propose |
| *(Future)* | Gym, Study, Chill | Additional moods can be added as behavioral rule presets |

---

## Home Party Mood

### Roles

| Role | Assigned To | Privileges |
|---|---|---|
| **Composer** | The user who creates the mood | Sets default playlist, manages hub settings. **Votes and counts as a normal user** — no owner skip priority. |
| **Participant** | Everyone else | Propose tracks, vote (same as regular visitor). |

### Rules

- Composer selects a default playlist that plays in the background.
- Any participant can propose a track (coins or collective vote — **coins cost 0 during paid mood events**).
- Voting works identically to the venue model, including the 65% skip threshold.
- **Key difference**: The composer has **no owner vote priority**. Their downvote does not trigger instant skip. They vote as equals.
- The composer can manually skip tracks via the hub management UI (but this counts the same as a manual owner skip for refund purposes).

### Session Management

- The composer sets a **duration** when creating the mood (e.g., 4 hours).
- 15 minutes before the end, all users receive a notification.
- Composer can extend the session (additional cost applies).
- At session end, the hub becomes inactive. No data is lost — the session is archived for review.

---

## Driving Mood

### Roles

| Role | Assigned To | Privileges |
|---|---|---|
| **Driver** | The user who creates the mood | Selects a personal playlist. Can only vote (downvote). |
| **Passenger** | Everyone else | Can propose tracks and vote normally. |

### Rules

#### Driver's Playlist

- The driver selects a playlist from their personal library or creates one.
- **Driver's list tracks are not skippable** by passengers — only the driver can skip their own tracks (via downvote or manual skip).
- Driver's list tracks do **not** open a voting window.
- The driver can only **downvote** — no upvoting allowed. This simplifies the driver UX to a single "skip" action.

#### Passenger Proposals

- Passengers propose tracks normally (all proposals are **free** during paid mood events).
- Proposed tracks are interleaved with driver's list tracks.
- Standard voting rules apply to proposed tracks (65% downvote threshold for auto-skip).

#### Driver Skip Abuse Prevention

To prevent the driver from silently vetoing all passenger proposals:

```
Driver downvotes passenger-proposed track?
    │
    ▼
Has the driver downvoted 5 consecutive passenger tracks?
├── NO → Track is skipped. Counter increments.
│
└── YES → This track CANNOT be skipped by driver.
           Track must play to completion.
           Counter resets to 0.
           Next track: driver can downvote again normally.
```

**Result**: After rejecting 5 passenger tracks in a row, at least 1 passenger track must play. This prevents the driver from running a dictatorship while still giving them significant control.

#### Driver Rotation

- Every **2 hours**, the mood prompts all participants: "Time for a driver change!"
- The current driver can:
  - **Continue** driving (extends by 2 hours).
  - **Transfer** driver role to another participant.
- If the driver transfers, the new driver selects their own playlist.
- If no action is taken within 5 minutes, a random participant is suggested as the new driver + the old driver continues.

### Driving Safety Considerations

- **Minimal driver UI**: The driver's interface is stripped to essentials — now playing, big thumbs-down/skip button, nothing else.
- **Voice commands** (stretch goal): "Hey Musicratic, skip" / "Hey Musicratic, next".
- **No browsing while driving**: The driver cannot browse tracks or search. Only the pre-selected playlist plays.
- **Large touch targets**: All driver-accessible buttons are minimum 48x48dp, spaced for easy tapping without looking.

---

## Connectivity Options (Driving Mood)

Driving mode must work in scenarios with limited or no internet connectivity.

### Connection Strategies (Priority Order)

| Method | Use Case | Latency | Battery Impact |
|---|---|---|---|
| **Internet** (cellular/WiFi) | Default when available | Low | Normal |
| **Wi-Fi Hotspot** | Driver shares phone as hotspot | Low | High (driver) |
| **Bluetooth** | Local sync for small groups | Medium | Low |
| **NFC** | Initial pairing / hub join | One-shot | Minimal |
| **Offline + Sync** | No connectivity areas | N/A | Minimal |

### Hotspot Mode

1. Driver enables "Hotspot Hub" in driving mood settings.
2. Driver's phone creates a Wi-Fi hotspot (uses OS APIs).
3. Passengers connect to the hotspot.
4. The hub operates as a local network service — all communication stays on the local network.
5. **Music playback**: The driver's phone is the master playback device. Passengers see the queue and vote, but audio comes from the driver's phone (connected to car speakers).

### Bluetooth Mode

1. Driver enables "Bluetooth Hub" in driving mood settings.
2. Passengers pair via Bluetooth.
3. Limited bandwidth: only queue state, votes, and track metadata are synced.
4. Playback is on the driver's device only.

### Offline Queue

When no connectivity is available:

- Passengers can **queue proposals offline**. When connectivity resumes, proposals are synced and inserted.
- Driver's playlist plays from locally cached tracks.
- Votes are stored locally and synced when connectivity resumes.

---

## Battery Optimization (Driving Mood)

Driving sessions can last hours. Musicratic must minimize battery drain.

| Strategy | Detail |
|---|---|
| **Reduce sync frequency** | Vote tallies sync every 5 seconds instead of real-time. Queue updates batch every 10 seconds. |
| **Dark mode enforced** | UI defaults to dark theme in driving mood to reduce OLED power consumption. |
| **Minimal animations** | No particle effects, no smooth scrolling for background UI. Driver UI is static. |
| **Background audio** | Use native audio session (Android `MediaSession`, iOS `AVAudioSession`) for efficient background playback. |
| **Screen-off capable** | Passengers can lock their phone; votes and proposals queue locally and sync when screen wakes. |
| **Location services OFF** | No geolocation needed in driving mood — connection is via hotspot/Bluetooth/internet, not GPS. |
| **Lazy image loading** | Album art loads only when scrolled into view. Cached aggressively. |

---

## Mood Pricing (Recap)

> Full details in [Monetization](06-monetization.md#3-mood-events-portable-hubs).

| Factor | Rule |
|---|---|
| **Base price** | €5.00 for 5 users, 24 hours |
| **+2 users** | Price increases by ~33% |
| **+24 hours** | Price increases by 35% |
| **Free tier** | 1 event/month, 3 users, 2 hours, ads |
| **During paid mood** | All track proposals cost 0 coins |

---

## Mood Creation Flow

```
1. User taps "Create Mood" from home screen
       │
       ▼
2. Select mood type (Home Party / Driving)
       │
       ▼
3. Configure session:
   ├── Duration (hours)
   ├── Max users
   ├── Default playlist selection
   └── Connection method (driving only)
       │
       ▼
4. Pricing shown → Confirm & Pay
   (or select free tier)
       │
       ▼
5. Hub created → Share link / QR / Bluetooth pairing
       │
       ▼
6. Participants join → Session starts
       │
       ▼
7. Session timer runs
   ├── 15 min warning
   ├── Extend option
   └── Session ends → Hub archived
```

---

## Future Moods (Conceptual)

| Mood | Concept |
|---|---|
| **Gym** | High-energy preset. Tempo-based auto-filtering. No slow tracks allowed. |
| **Study** | Lo-fi / ambient only. No vocals filter. Auto-skip anything over 100 BPM. |
| **Chill** | Acoustic / mellow preset. Community vibe, all votes equal. |

These would reuse the same portable hub infrastructure with different behavioral rule sets.
