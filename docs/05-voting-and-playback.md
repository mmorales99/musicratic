# 05 — Voting & Playback Rules

## Voting Overview

Voting is the democratic core of Musicratic. Every attached user gets a voice on proposed tracks. The system enforces deterministic, transparent rules — no hidden algorithms.

## Who Can Vote

| Voter | Can vote on list tracks? | Can vote on proposed tracks? |
|---|---|---|
| **Visitor** | Yes | Yes |
| **Sub List Owner** | Yes | Yes (owner priority on their list's tracks) |
| **Sub Hub Manager** | Yes | Yes |
| **Super List Owner** | Yes | Yes (owner priority on all tracks) |

- **One vote per user per queue entry.** Votes are final and cannot be changed.
- Votes are cast as `up` or `down` — no abstain, no score-based voting.

## Voting Windows

### Standard Voting (Proposed Tracks)

A voting window opens when a **proposed track** starts playing:

```
Track starts playing
        │
        ├─── voting window: 60 seconds ───┤
        │                                  │
    votes accumulate                   window closes
        │                                  │
        ▼                                  ▼
  tally evaluated continuously      final tally → decide
```

- Votes cast during the first 60 seconds of playback count.
- The tally is **evaluated in real-time** — a skip can happen before the window closes if the threshold is already met.
- If the voting window closes without reaching the skip threshold, the track continues playing.

### List Track Voting

List tracks (not proposed by a user) are **also votable** but the result only affects **statistics**:

- Upvotes/downvotes on list tracks feed into the shuffle weight and weekly report.
- List tracks are **never auto-skipped** by votes alone.
- Only the list owner (or super owner) can manually skip a list track.

## Skip Rules — Decision Tree

```
Is the track from a list (not proposed)?
├── YES → Track is never auto-skipped.
│         Votes only affect stats.
│         Owner can manually skip at any time.
│
└── NO → Track was proposed by a visitor.
         │
         Did a list owner downvote?
         ├── YES → SKIP IMMEDIATELY. No waiting.
         │         Owner vote = instant override.
         │         If coins were paid → 50% refund.
         │
         └── NO → Is the track within its first 60 seconds?
                   ├── YES → Evaluate visitor votes:
                   │         │
                   │         Are ≥65% of cast votes downvotes?
                   │         ├── YES → SKIP. Track is removed.
                   │         │         If coins paid → 50% refund.
                   │         │
                   │         └── NO → Continue playing.
                   │                  Re-evaluate on each new vote
                   │                  until window closes.
                   │
                   └── NO → Voting window expired.
                            Track plays to completion.
                            No skip possible (except manual owner skip).
```

## Owner Vote Priority — Detailed Rules

List owners (super owner and sub-list-owners) have **elevated voting power**:

| Rule | Detail |
|---|---|
| **Instant skip** | If any owner downvotes a proposed track, it is skipped immediately regardless of other votes or timing. |
| **No first-minute rule** | Owners are exempt from the 60-second window. Their downvote triggers a skip at any point during playback. |
| **Scope** | Super owner has priority on all tracks in all lists. Sub-list-owners have priority only on tracks in their assigned lists. Sub-hub-managers vote as regular users. |
| **Upvote** | Owner upvotes count the same as visitor upvotes for statistics. No special weight. |  
| **Refund** | If an owner skips a coin-paid track, the proposer receives a 50% coin refund (same as vote-based skip). |

## Refund Rules

| Scenario | Refund |
|---|---|
| Track proposed with coins, **skipped by votes** (≥65% downvotes in first minute) | **50%** of coins spent, rounded down |
| Track proposed with coins, **skipped by owner** downvote | **50%** of coins spent, rounded down |
| Track proposed with coins, **played to completion** | **0%** — no refund |
| Track proposed with coins, **skipped manually by owner** (not via downvote but via skip button) | **50%** of coins spent, rounded down |
| Track proposed via collective vote (0 coins), skipped | **No coins to refund** |

**Refund processing**: Refunds are credited to the proposer's wallet immediately and recorded as a `WalletTransaction` of type `refund`.

## Collective Vote Proposal

A visitor who doesn't want to spend coins can **propose a track for free via collective vote**:

1. Visitor selects a track and chooses "Propose for Free".
2. A **pre-play vote** is opened to all currently attached users.
3. The vote runs for **2 minutes** (configurable by hub owner).
4. If ≥50% of votes are upvotes, the track is added to the queue.
5. If the vote fails (<50% upvotes), the track is rejected. The visitor is notified.
6. The list owner can **approve or reject** the proposal at any time during the vote, overriding the result.

**Limits:**
- A visitor can have at most **1 pending collective vote proposal** at a time.
- After a rejected proposal, the visitor must wait **5 minutes** before proposing again via collective vote.
- No limit on coin-paid proposals (beyond wallet balance).

## Playback Flow — Full Sequence

```
1. Playback Orchestrator picks next track from queue
       │
       ├── From list? → play immediately, collect votes for stats only
       │
       └── From proposal? → play and open voting window
                │
                ▼
2. Track starts playing
   ├── WebSocket: broadcast NOW_PLAYING to all attached users
   ├── Start 60-second voting window
   │
3. During playback (first 60 seconds):
   │   ├── User casts vote → Voting Service records it
   │   ├── Voting Service recalculates tally
   │   │       ├── Owner downvote? → SKIP immediately
   │   │       ├── ≥65% downvotes? → SKIP
   │   │       └── Otherwise → continue
   │   │
   │   └── If SKIP:
   │           ├── Playback Orchestrator stops track
   │           ├── Economy Service processes refund (if coins)
   │           ├── QueueEntry status → 'skipped'
   │           ├── Broadcast TRACK_SKIPPED
   │           └── Go to step 1
   │
4. After 60 seconds (if not skipped):
   │   Voting window closes.
   │   Votes still affect stats but no more skips possible.
   │
5. Track finishes naturally:
       ├── QueueEntry status → 'played'
       ├── Update TrackStats (plays, upvotes, downvotes)
       ├── Broadcast TRACK_ENDED
       └── Go to step 1
```

## Vote Tallying

The skip threshold is calculated as:

$$\text{skip} = \frac{d}{u + d} \geq 0.65$$

Where:
- $d$ = number of downvotes (excluding owner votes, which trigger instant skip)
- $u$ = number of upvotes

**Edge cases:**
- If only 1 vote is cast and it's a downvote: $\frac{1}{1} = 1.0 \geq 0.65$ → skip.
- If no votes are cast: No skip. Track plays to completion.
- Minimum vote count to trigger skip: configurable by hub owner (default: 1). Can be set to, e.g., 3 to prevent a single troll from skipping tracks.

## Anti-Abuse Measures

| Threat | Mitigation |
|---|---|
| **Vote brigading** | Rate limit: max 1 vote per queue entry per user. Device fingerprinting to detect multi-account abuse. |
| **Spam proposals** | Max pending proposals per user per hub (default: 3). Collective vote cooldown on rejection. |
| **Troll downvoting** | Hub owner can set minimum vote count for skip. Hub owner can ban users from voting. |
| **Coin-farming** | Coins are purchased, not earned. No way to gain coins through voting. |
