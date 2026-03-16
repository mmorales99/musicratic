# 11 — Development Roadmap

## Phasing Strategy

The roadmap follows a **"prove and grow"** model: each phase validates the previous phase's assumptions before adding complexity.

```
Phase 0          Phase 1              Phase 2              Phase 3
Foundation ────▶ Core Venue Hub ────▶ Portable Moods ────▶ Social & Scale
(this doc)       (revenue start)      (B2C growth)         (network effects)
```

---

## Phase 0 — Foundation

**Goal**: Set up the project, validate architecture, and build the skeleton.

| # | Task | Deliverable |
|---|---|---|
| 0.1 | Foundation documentation (this) | `/docs/` — 12 documents |
| 0.2 | Flutter project scaffold | Monorepo: `app/`, `server/`, `shared/` |
| 0.3 | Backend scaffold (Serverpod or Shelf) | Running server with health endpoint |
| 0.4 | Database schema v1 | PostgreSQL migrations for core entities |
| 0.5 | Auth system | Registration, login, JWT, OAuth (Google/Apple) |
| 0.6 | CI/CD pipeline | GitHub Actions: lint, test, build, coverage gates |
| 0.7 | Dev environment setup | Docker Compose (Postgres, Redis, server, web) |
| 0.8 | Backend test infrastructure | xUnit + Moq project, `WebApplicationFactory` setup, coverage enforcement at 100% |
| 0.9 | Web frontend test infrastructure | Playwright configured for Angular: unit + E2E projects, coverage enforcement at 100% |
| 0.10 | Mobile test infrastructure | Playwright configured for Flutter web target + `flutter_test`, coverage enforcement at 100% |
| 0.11 | SonarQube setup | Self-hosted SonarQube on VPS, quality gate configured (100% coverage, 0 blockers, A ratings) |
| 0.12 | SonarQube CI integration | sonar-scanner in GitHub Actions, pipeline fails if quality gate not passed |

**Exit criteria**: A user can register, log in, and see a blank home screen. Backend responds to authenticated API calls. CI is green. **All test infrastructure is operational**: xUnit/Moq backend tests run and enforce 100% coverage, Playwright unit + E2E tests run for both Angular and Flutter, SonarQube quality gate passes on every PR.

---

## Phase 1 — Core Venue Hub

**Goal**: Deliver the full "coffee shop jukebox" experience. First revenue.

### 1A — Hub & List Management

| # | Task | Deliverable |
|---|---|---|
| 1A.1 | Hub CRUD (create, edit, activate, deactivate) | Hub management screens + API |
| 1A.2 | QR code generation + direct link | QR image + deep link per hub |
| 1A.3 | Hub attachment (scan QR → join, 1h expiry) | Visitor join flow |
| 1A.4 | List CRUD (create, add/remove tracks) | List management screens + API |
| 1A.5 | Music provider integration — Spotify | Search, metadata, playback via Spotify SDK |
| 1A.6 | Music provider integration — YouTube Music | Search, metadata, playback via YouTube API |
| 1A.7 | Play mode (ordered + shuffled) | Queue populates from list in correct order |

**Exit criteria**: A hub owner can create a hub, add a playlist, activate it, and music plays. A visitor can scan the QR and see the live queue. **100% test coverage** for all backend services (xUnit/Moq), Angular components (Playwright unit), and Flutter widgets. SonarQube quality gate passes.

### 1B — Queue & Proposals

| # | Task | Deliverable |
|---|---|---|
| 1B.1 | Live queue display (WebSocket) | Real-time queue screen for all attached users |
| 1B.2 | Now-playing display | Track info, progress bar, album art |
| 1B.3 | Track proposal (coin-paid) | Visitor proposes track → deducted from wallet → added to queue |
| 1B.4 | Track proposal (collective vote) | Free proposal flow with approval voting |
| 1B.5 | Queue interleaving logic | Proposals inserted between list tracks (configurable ratio) |
| 1B.6 | Track pricing engine | Duration-based + hotness multiplier pricing |

**Exit criteria**: Visitors can propose tracks via coins or collective vote. Proposals appear interleaved in the queue. **100% test coverage** on proposal, pricing, and queue interleave logic (backend + frontend). Playwright E2E covers full proposal flow.

### 1C — Voting & Skipping

| # | Task | Deliverable |
|---|---|---|
| 1C.1 | Vote casting (up/down) | Vote buttons on now-playing screen |
| 1C.2 | Live vote tally (WebSocket) | Real-time vote count display |
| 1C.3 | Auto-skip rule (≥65% downvotes, first minute) | Playback orchestrator enforces threshold |
| 1C.4 | Owner instant-skip (owner downvote) | Owner downvote triggers immediate skip |
| 1C.5 | Manual skip (owner/manager) | Skip button in management UI |
| 1C.6 | Refund logic (50% on skip) | Economy service processes refunds |

**Exit criteria**: Full voting flow works end-to-end. Proposed tracks get skipped when voted down. Refunds are processed. **100% test coverage** on vote casting, tally computation, skip rules, refund logic. Playwright E2E covers attach → propose → vote → skip → refund.

### 1D — Economy

| # | Task | Deliverable |
|---|---|---|
| 1D.1 | Virtual coin wallet | Wallet UI, balance display, transaction history |
| 1D.2 | Coin purchase (IAP) | Stripe (web) + Apple IAP / Google Play Billing direct (mobile) |
| 1D.3 | Hub subscription (free trial + paid) | Subscription management UI, Stripe billing |
| 1D.4 | Ad integration (free tier) | Deferred to Phase 2 — self-served or open-source ad solution |

**Exit criteria**: Users can buy coins and spend them. Hub owners can subscribe. Free tier shows ads. **100% test coverage** on wallet operations, IAP verification, subscription lifecycle. Playwright E2E covers coin purchase → spend → balance update.

### 1E — Analytics & Reports

| # | Task | Deliverable |
|---|---|---|
| 1E.1 | Track stats collection | Upvotes, downvotes, plays, skips per track per hub |
| 1E.2 | Shuffle weighting by vote score | Weighted random selection in shuffle mode |
| 1E.3 | Weekly downvoted tracks report | Automated weekly prompt to list owner |
| 1E.4 | Monthly popular proposals report | Automated monthly prompt to incorporate tracks |

**Exit criteria**: List owners receive automated reports. Shuffle mode biases toward well-received tracks. **100% test coverage** on stats aggregation, weighted shuffle algorithm, report generation. Integration tests verify stats flow from votes to reports.

### 1F — Roles & Delegation

| # | Task | Deliverable |
|---|---|---|
| 1F.1 | Hub member management | List members, view roles |
| 1F.2 | Role promotion (sub-manager, sub-list-owner) | Super owner can assign roles |
| 1F.3 | Role-based UI | Conditional UI based on role |
| 1F.4 | Permission enforcement at API level | Middleware checks role for every action |

**Exit criteria**: Hub hierarchy works. Sub-owners manage their lists with proper permissions. **100% test coverage** on role assignment, permission checks (API middleware), and conditional UI rendering. Integration tests verify role-based API access denial.

### 1G — Social (Minimal)

| # | Task | Deliverable |
|---|---|---|
| 1G.1 | User profiles (basic) | Display name, avatar, stats |
| 1G.2 | Hub discovery (search by name, genre) | Discovery screen with filters |
| 1G.3 | Hub reviews & ratings | Star ratings with comments |
| 1G.4 | Social sharing (hub link, now-playing) | Share sheets with deep links |

**Exit criteria**: Users can find and review hubs. Shared links open the app or web. **100% test coverage** on discovery search, review submission, deep link resolution. Playwright E2E covers discovery → review → share flow.

### Phase 1 — Launch Checklist

- [ ] All 1A–1G exit criteria met
- [ ] **100% backend test coverage** (xUnit + Moq) — all services, repositories, endpoints
- [ ] **100% web frontend unit test coverage** (Playwright) — all Angular components, services, pipes, guards
- [ ] **100% web frontend E2E coverage** (Playwright) — all user flows covered
- [ ] **100% mobile frontend unit test coverage** (Playwright + flutter_test) — all widgets, providers, use cases
- [ ] **100% mobile frontend E2E coverage** (Playwright) — all user flows mirroring web
- [ ] **SonarQube quality gate passed** — 0 critical/blocker issues, A ratings, < 3% duplication
- [ ] Integration tests passing (service ↔ service, feature ↔ feature, front ↔ back)
- [ ] End-to-end tests passing (attach → propose → vote → skip → refund)
- [ ] App Store / Play Store submissions prepared
- [ ] Web app deployed to production
- [ ] Landing page with marketing copy
- [ ] 5 beta venue partners onboarded
- [ ] Privacy policy and Terms of Service published
- [ ] GDPR compliance verified

---

## Phase 2 — Portable Moods & Location

**Goal**: Expand to B2C with portable hubs. Add location-based features.

| # | Task | Deliverable |
|---|---|---|
| 2.1 | Mood system infrastructure | Portable hub creation, role variants (composer, driver) |
| 2.2 | Home Party mood | Full implementation per [Mood System](08-mood-system.md) |
| 2.3 | Driving mood | Driver controls, skip abuse prevention, rotation |
| 2.4 | Mood pricing & event payment | Time + user-count based billing |
| 2.5 | Hotspot / Bluetooth connectivity | Local network hub for driving mood |
| 2.6 | Offline queue & sync | Proposal and vote queuing without connectivity |
| 2.7 | Battery optimization (driving mood) | Reduced sync, dark mode, minimal UI |
| 2.8 | Location-based hub attachment | GPS geofence, auto-attach/detach |
| 2.9 | Location-based hub discovery | "Near me" in discovery screen |
| 2.10 | Live music features | Live indicator, schedule, push notifications |
| 2.11 | Global leaderboards | Musicratic Charts (most played, most loved) |
| 2.12 | Desktop apps (hub manager, list owner) | macOS, Windows, Linux builds |

**Exit criteria**: Users can create mood events. Driving mood works offline via hotspot. Hubs appear on a map. **100% test coverage** maintained across all new features. SonarQube quality gate passes for all mood, location, and offline-sync code.

---

## Phase 3 — Social & Scale

**Goal**: Network effects. User-generated content flywheel.

| # | Task | Deliverable |
|---|---|---|
| 3.1 | Public lists (browse, clone, follow) | List marketplace |
| 3.2 | User following & social feed | "Friends are listening to..." |
| 3.3 | Hub owner analytics dashboard | Rich data visualizations, CSV export |
| 3.4 | API for hub owners | REST API for custom integrations |
| 3.5 | Additional music providers | Apple Music, SoundCloud, Tidal |
| 3.6 | Additional moods (Gym, Study, Chill) | New behavioral rule presets |
| 3.7 | Service extraction (Playback, Voting) | Microservice decomposition for scale |
| 3.8 | Multi-region deployment | Low-latency real-time globally |
| 3.9 | AI-powered playlist suggestions | "Based on your hub's vibe, try these tracks" |
| 3.10 | Partnerships with venues/promoters | B2B growth channel |

---

## Development Velocity Targets

These are directional, not commitments:

| Phase | Scope | Team Size |
|---|---|---|
| Phase 0 | Foundation | 1 developer + AI |
| Phase 1A-1C | Core playback loop | 1 developer + AI |
| Phase 1D-1G | Economy + social | 1-2 developers + AI |
| Phase 2 | Moods + location | 2-3 developers |
| Phase 3 | Scale | 3-5 developers |

> The AI-assisted development strategy (see [next document](12-ai-assisted-development.md)) is designed to make a single developer productive enough to deliver Phase 0 and the core of Phase 1 solo.
