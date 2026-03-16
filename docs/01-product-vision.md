# 01 — Product Vision

## Mission Statement

Musicratic democratizes shared music experiences. We give every listener a voice — and every venue a smarter soundtrack.

## Problem Statement

In shared spaces (cafés, bars, gyms, road trips, parties), music selection is typically controlled by one person. This creates friction:

- **For venue owners**: static playlists grow stale; there is no feedback loop on what customers actually enjoy.
- **For visitors**: no way to influence the soundtrack; feeling disconnected from the atmosphere.
- **For social gatherings**: the "phone DJ" bottleneck — one person controls the speaker, everyone else tolerates it.

Existing jukebox apps are either:
- Locked to a single music provider.
- Missing the social/voting layer.
- Overly complex for casual use.
- Not viable as a business for venue owners.

## Solution

Musicratic introduces a **hub-based, democratic music queue** where:

1. A **hub** represents a physical or virtual shared listening space.
2. A **list owner** curates a base playlist for the hub.
3. **Visitors** can propose songs — either by spending virtual coins or by collective vote.
4. **Everyone votes** on proposed tracks; songs that the crowd dislikes get skipped automatically.
5. The system **learns over time** — promoting well-received tracks and flagging poorly received ones for replacement.

## Target Audience

### Primary — Venue Owners (B2B)
- Coffee shops, bars, restaurants, co-working spaces, gyms.
- Want ambient music that keeps customers happy and engaged.
- Value low-effort curation with data-driven feedback.

### Secondary — Social Groups (B2C)
- Friend groups on road trips, at house parties, or casual hangouts.
- Want a fair, fun way to share music control.
- Willing to pay small amounts for premium features or ad-free experiences.

### Tertiary — Music Enthusiasts (B2C)
- Users who want to discover live music spots, curated playlists, and trending tracks across hubs.
- Social-first users who enjoy public profiles and community interaction.

## Core Value Propositions

| Stakeholder | Value |
|---|---|
| Venue owner | Self-improving playlist, customer engagement, revenue from virtual coins |
| Visitor | Influence the music, discover new tracks, social interaction |
| Music industry | Organic discovery channel, play-count data, promotional opportunities |

## Product Principles

1. **Simplicity over completeness** — A visitor should go from "scan QR" to "propose a song" in under 30 seconds.
2. **Fair by default** — Voting rules are transparent and deterministic. No hidden algorithms, no pay-to-override democracy.
3. **Offline-resilient** — Core playback must survive intermittent connectivity (especially for driving mood).
4. **AI-native development** — The codebase is designed for AI-assisted generation, review, and iteration from day one.
5. **Incremental monetization** — Free tiers are generous enough to prove value before asking for money.
6. **Minimal cost, minimal dependencies** — Every external service is a recurring bill and a vendor lock-in risk. If we can build it or self-host it with reasonable effort, we do. The only paid third-party integrations allowed are those where self-hosting is impossible or legally required (app store IAP, music provider APIs, payment processing). Everything else runs on our own infrastructure.

## Success Metrics (North Stars)

| Metric | Target (Year 1) |
|---|---|
| Active hubs | 500 |
| Monthly active visitors | 50,000 |
| Visitor-to-proposer conversion | > 25% |
| Hub owner retention (month-over-month) | > 70% |
| Virtual coin purchase rate | > 10% of active visitors |

## Competitive Landscape

| Competitor | Strength | Musicratic's Edge |
|---|---|---|
| Rockbot | Venue jukebox | Voting + social layer, multi-provider |
| Jukebox.io | Browser-based | Native mobile, portable moods, offline |
| Spotify Blend | Collaborative playlist | Real-time democracy, hub model for venues |
| AUX / Queueup | Queue sharing | Full business model, venue subscriptions |

https://www.touchtunes.com/business/bars-restaurants
https://www.thejkbxapp.com/index.html
https://jukebox.today/

## Constraints & Assumptions

- **Music licensing**: Musicratic does **not** host or stream music directly. It orchestrates playback through authorized providers (Spotify, YouTube Music, etc.). The venue/user must hold valid streaming accounts.
- **MVP scope**: First release targets the "hub for shops" scenario only. Portable moods (driving, party) come in Phase 2.
- **Location services**: GPS-based auto-attachment is deferred to Phase 2. Phase 1 uses time-limited (1 hour) manual attachment via QR/link.
- **Platform**: Flutter/Dart for mobile (Nativescript is in evaluation for a replacement for flutter/dart), Angular for web. Backend C#.
