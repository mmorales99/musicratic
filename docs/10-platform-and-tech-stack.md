# 10 — Platform & Tech Stack

## Design Philosophy

This project is built for **AI-assisted development** from the ground up. Every technology choice optimizes for:

1. **Code generation quality** — Frameworks with strong typing, clear conventions, and large training-data presence in LLMs.
2. **Minimal cost** — Self-host everything possible. The only paid dependencies are the VPS (~€5/mo) and transaction fees on actual revenue. No Firebase, no SaaS analytics, no managed databases.
3. **Minimal external dependencies** — If we can build it or self-host it with reasonable effort, we do. Fewer dependencies = fewer bills, fewer breaking changes, fewer vendor lock-ins.
4. **Deployable as one or many** — The backend is a modular monolith that runs as a single process in dev/MVP and can be split into independent services when needed.
5. **Offline-first architecture** — Every hub must be able to operate in isolation. Multi-tenancy and local data fallback are built into every layer.
6. **Fast iteration** — Hot reload (Flutter/Angular), rapid prototyping, minimal boilerplate.

---

## Stack Overview

| Layer | Technology | Justification | Monthly Cost |
|---|---|---|---|
| **Mobile client** | Flutter 3.x / Dart | Cross-platform iOS + Android from one codebase | €0 |
| **Web client** | Angular | Shell architecture with navigation, rich experience for all user types | €0 |
| **Backend** | C# / .NET 8+ | Modular monolith, event-driven, strong typing, excellent perf, massive AI training data | €0 |
| **BFF (Backend for Frontend)** | ASP.NET Core (stateless) | Thin API layer per client type; state lives in client-side state machine | €0 |
| **Service mesh / sidecar** | Dapr | Pub/sub, state management, service invocation — abstracts infra so same code runs as monolith or microservices | €0 |
| **Database (primary)** | PostgreSQL 16+ | ACID, relational, self-hosted. Used by the supra-hub and all connected hubs. | included in VPS |
| **Database (fallback/offline)** | SQLite | Embedded in each hub's runtime for offline operation. Syncs to PostgreSQL on reconnect. | €0 |
| **Cache** | Memcached | Simple, fast key-value cache for sessions, vote tallies, hot data. Self-hosted. | included in VPS |
| **Event bus** | Azure Event Hubs emulator (via Dapr) | Dapr pub/sub backed by the MS Event Hubs emulator. Zero cost, runs locally in a container. | €0 |
| **Blob / file storage** | Azurite (Azure Storage emulator) | S3-compatible blob storage for QR images, avatars, local tracks. Self-hosted, zero cost. | €0 |
| **Identity provider** | Authentik | Self-hosted IdP. SSO, OAuth2/OIDC, passkeys, MFA. Supports Google/Apple/Spotify/email login. | €0 |
| **Auth** | JWE tokens via Authentik + refresh tokens | Authentik issues JWE-encrypted access tokens + refresh tokens; backend validates them. Role resolved per-request. | €0 |
| **Payments** | Stripe (web) + Apple IAP / Google Play Billing (mobile, direct) | Only unavoidable external costs. No middleware like RevenueCat. | pay-per-transaction |
| **Push Notifications** | WebSocket (in-app) + APNs/FCM direct (OS-level) | No Firebase dependency. We send push directly to Apple/Google endpoints. | €0 |
| **CI/CD** | GitHub Actions | Free for public/private repos (2,000 min/mo free tier) | €0 |
| **Containers** | Podman + Podman Compose | Rootless containers. Drop-in Docker replacement, no daemon, no licensing concerns. | €0 |
| **Hosting (MVP)** | Single VPS (Hetzner / OVH) | All containers on one box: backend, PostgreSQL, Memcached, Azurite, Event Hubs emulator, Authentik, Caddy. | ~€5/mo |
| **Reverse Proxy / TLS** | Caddy | Auto-TLS via Let's Encrypt. Serves web frontend + BFF. | €0 |
| **Logging** | Serilog | Structured logging with sinks (console, file, DB). Configured per-environment. | €0 |
| **Observability** | OpenTelemetry | Distributed tracing, metrics collection, correlation IDs. Integrates with Serilog. | €0 |
| **API Documentation** | OpenAPI + Scalar | OpenAPI spec auto-generated from code. Scalar as interactive API explorer GUI. | €0 |
| **Frontend → BFF** | JSON-RPC + REST + GraphQL | JSON-RPC for commands, REST for resource CRUD, GraphQL for search/queries | €0 |
| **BFF → Backend** | gRPC (via Dapr) | High-performance binary protocol between BFF and backend modules | €0 |
| **Secrets** | Podman Secrets / AES-256 env vars | Podman keyvault preferred; fallback to `{APPNAME}_SECRET_{name}` with AES-256 encoded values | €0 |
| **ORM** | Entity Framework Core (Code First) | Database schema defined in C#, managed via EF migrations. 3-layer pattern: Entity → DataDto → ApiDto. | €0 |
| **Code quality** | SonarQube (Community Edition) | Static analysis, code smells, security hotspots, duplication, coverage tracking. Self-hosted. | €0 |
| **Testing (backend)** | xUnit + Moq | 100% coverage. Unit + integration tests for all services, repositories, entities, use cases. | €0 |
| **Testing (web)** | Playwright | 100% coverage. Component unit tests + full E2E user flows for Angular. | €0 |
| **Testing (mobile)** | Playwright + flutter_test | 100% coverage. Unit + widget + E2E tests for Flutter. | €0 |

> **Total MVP infrastructure cost: ~€5/month** (one VPS). The only variable costs are Stripe transaction fees and app store commissions on actual revenue.

---

## Backend Architecture — Modular Monolith

### Core Principle: One Binary, Many Modules

The backend is a **single .NET solution** composed of independent modules that communicate exclusively through **Dapr pub/sub events** and **Dapr service invocation**. Because Dapr abstracts the communication layer:

- **In development / MVP**: All modules run in a single process. Dapr sidecar handles pub/sub in-memory or via the Event Hubs emulator locally.
- **At scale**: Any module can be extracted into its own container with its own Dapr sidecar. Zero code changes — only deployment configuration changes.

```
+---------------------------------------------------------------------+
|                     MODULAR MONOLITH (.NET)                          |
|                                                                     |
|  +-----------+ +-----------+ +-----------+ +-------------------+    |
|  |   Auth    | |    Hub    | | Playback  | |      Voting       |    |
|  |  Module   | |  Module   | | Module    | |      Module       |    |
|  +-----+-----+ +-----+-----+ +-----+-----+ +--------+----------+   |
|        |              |             |                |               |
|  +-----+--------------+-------------+----------------+-----------+  |
|  |                    Shared Kernel                              |   |
|  |        (Domain events, base entities, tenant context)        |   |
|  +---------------------------+----------------------------------+   |
|                              |                                      |
|  +-----------+ +-----------+ | +-----------+ +-------------------+  |
|  | Economy   | | Analytics | | |  Social   | |   Notification    |  |
|  | Module    | | Module    | | | Module    | |   Module          |  |
|  +-----------+ +-----------+ | +-----------+ +-------------------+  |
|                              |                                      |
+------------------------------+--------------------------------------+
                               |
                    +----------+----------+
                    |    Dapr Sidecar     |
                    |                     |
                    |  - Pub/Sub (events) |
                    |  - State store      |
                    |  - Service invoke   |
                    |  - Bindings         |
                    +----------+----------+
                               |
              +----------------+----------------+
              v                v                v
     +--------------+ +--------------+ +--------------+
     | Event Hubs   | |  Memcached   | |   Azurite    |
     | emulator     | |              | | (blob store) |
     +--------------+ +--------------+ +--------------+
```

### Module Boundaries

Each module owns its data, its domain logic, and its API surface. Modules never share database tables.

| Module | Responsibility | Key Events Published |
|---|---|---|
| **Auth** | Token validation (delegates to Authentik), user profile CRUD, tenant membership | `UserRegistered`, `UserProfileUpdated` |
| **Hub** | Hub CRUD, QR/link generation, attachment, hub hierarchy, tenant lifecycle | `HubCreated`, `HubActivated`, `UserAttached`, `UserDetached` |
| **Playback** | Queue management, track ordering, provider bridging, skip execution | `TrackStarted`, `TrackEnded`, `TrackSkipped`, `QueueUpdated` |
| **Voting** | Vote casting, tally computation, rule enforcement | `VoteCast`, `SkipThresholdReached`, `OwnerDownvoted` |
| **Economy** | Coin wallets, track pricing, IAP/Stripe integration, refund logic, subscriptions | `CoinsSpent`, `CoinsRefunded`, `CoinsPurchased`, `SubscriptionChanged` |
| **Analytics** | Track scoring, weekly/monthly reports, shuffle weighting | `WeeklyReportGenerated`, `MonthlyReportGenerated` |
| **Social** | User profiles, hub reviews, public lists, hub discovery | `ReviewPosted`, `ListPublished` |
| **Notification** | WebSocket push, OS-level push (APNs/FCM direct), in-app prompts | (Consumer only — no events published) |

### Solution Structure

```
src/
+-- Musicratic.sln
|
+-- Shared/
|   +-- Musicratic.SharedKernel/        # Domain events, base entity, tenant context, Result<T>
|   +-- Musicratic.Contracts/           # Public DTOs, event contracts (shared across modules)
|   +-- Musicratic.Infrastructure/      # Dapr client wrappers, PostgreSQL/SQLite abstractions, Memcached helpers
|
+-- Modules/
|   +-- Musicratic.Auth/
|   |   +-- Domain/                     # Entities (User, PasskeyCredential)
|   |   +-- Application/               # Use cases, event handlers
|   |   +-- Infrastructure/            # Authentik client, token validator
|   |   +-- Api/                       # ASP.NET controllers / minimal API endpoints
|   |
|   +-- Musicratic.Hub/
|   |   +-- Domain/                     # Entities (Hub, HubSettings, HubAttachment, HubMember)
|   |   +-- Application/
|   |   +-- Infrastructure/
|   |   +-- Api/
|   |
|   +-- Musicratic.Playback/
|   |   +-- Domain/                     # Entities (QueueEntry, Track, PlaybackSession)
|   |   +-- Application/
|   |   +-- Infrastructure/            # Spotify/YouTube provider adapters
|   |   +-- Api/
|   |
|   +-- Musicratic.Voting/
|   +-- Musicratic.Economy/
|   +-- Musicratic.Analytics/
|   +-- Musicratic.Social/
|   +-- Musicratic.Notification/
|
+-- Host/
|   +-- Musicratic.Monolith/           # Single host — registers all modules
|   +-- Musicratic.Service.{Name}/     # Per-module host (for microservice deployment)
|
+-- BFF/
|   +-- Musicratic.BFF.Web/            # Stateless BFF for Angular client
|   +-- Musicratic.BFF.Mobile/         # Stateless BFF for Flutter client
|
+-- Tests/
    +-- Musicratic.Auth.Tests/
    +-- Musicratic.Hub.Tests/
    +-- Musicratic.Integration.Tests/
    +-- ...
```

### Monolith to Microservice Deployment

```
MODE: MONOLITH (dev / MVP)
+------------------------------+
|  Musicratic.Monolith (host)  |  <-- Single process, all modules loaded
|  + Dapr sidecar              |  <-- Dapr routes events in-memory
+------------------------------+

MODE: MICROSERVICES (scale)
+---------------------+  +---------------------+  +---------------------+
| Musicratic.Service.  |  | Musicratic.Service.  |  | Musicratic.Service.  |
| Hub + Dapr sidecar   |  | Playback + Dapr      |  | Voting + Dapr        |
+---------------------+  +---------------------+  +---------------------+
        ... one container per module that needs independent scaling ...
```

**No code changes between modes.** Only the `Host` project and Podman Compose config differ.

---

## Multi-Tenancy — Hub as Tenant

### Architecture

Every hub is a **tenant**. The system always has a **supra-tenant** that represents the global Musicratic platform.

```
+-------------------------------------------------------------+
|                    SUPRA TENANT                              |
|         (global users, global track catalog,                 |
|          cross-hub analytics, discovery,                     |
|          subscriptions, coin economy)                        |
|                                                              |
|   +----------+  +----------+  +----------+  +----------+    |
|   | Hub A    |  | Hub B    |  | Hub C    |  | Hub D    |    |
|   | (tenant) |  | (tenant) |  | (tenant) |  | (tenant) |    |
|   |          |  |          |  |          |  |          |    |
|   | Own queue|  | Own queue|  | Own queue|  | Own queue|    |
|   | Own list |  | Own list |  | Own list |  | Own list |    |
|   | Own votes|  | Own votes|  | Own votes|  | Own votes|    |
|   | Own stats|  | Own stats|  | Own stats|  | Own stats|    |
|   +----------+  +----------+  +----------+  +----------+    |
|                                                              |
+--------------------------------------------------------------+
```

### Tenant Context

Every request carries a **tenant context** (resolved from the hub the user is attached to). This context:

1. **Routes data** — Each hub's queue, votes, and stats are isolated in their own schema or via a `tenant_id` discriminator column.
2. **Routes events** — Dapr pub/sub topics include the tenant ID, so hubs never receive each other's events.
3. **Scopes permissions** — A user's role is always evaluated within a specific tenant.

```csharp
// Simplified — actual implementation uses middleware
public record TenantContext(Guid TenantId, bool IsSupraTenant);

// Injected per-request via middleware
public class TenantMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantId = ResolveTenantFromRequest(context); // from JWT claim, route, or header
        context.Items["Tenant"] = new TenantContext(tenantId, tenantId == SupraTenantId);
        await next(context);
    }
}
```

### Supra-Tenant Responsibilities

| Concern | Where it lives |
|---|---|
| User accounts & authentication | Supra-tenant (Authentik) |
| Coin wallets & purchases | Supra-tenant (cross-hub economy) |
| Hub discovery & search | Supra-tenant |
| Global track catalog & hotness | Supra-tenant |
| Subscriptions & billing | Supra-tenant |

### Hub-Tenant Responsibilities

| Concern | Where it lives |
|---|---|
| Playback queue | Hub tenant |
| Voting (per track) | Hub tenant |
| List management | Hub tenant |
| Track stats (within hub) | Hub tenant |
| Hub member roles | Hub tenant |
| Attachments | Hub tenant |

### Offline Hub Isolation

When a hub loses connectivity to the supra-tenant:

1. The hub continues operating on **SQLite** as its local data store.
2. Queue, votes, playback, and local stats all function normally.
3. Coin transactions are **deferred** — proposals are allowed but marked as "pending settlement."
4. When connectivity resumes, the hub **syncs** events to the supra-tenant:
   - Event bus replays pending events.
   - SQLite data is reconciled with PostgreSQL.
   - Deferred coin transactions are settled.

This is critical for the **driving mood** (Phase 2) where a car may lose connectivity entirely.

---

## Authentication — Authentik

### Identity Provider

**Authentik** is a self-hosted, open-source identity provider that handles all authentication complexity.

```
User clicks "Login"
       |
       v
  +--------------+     OIDC redirect      +------------------+
  |  Client App  | ---------------------> |    Authentik     |
  |  (Angular /  |                        |  (self-hosted)   |
  |   Flutter)   | <--------------------- |                  |
  +--------------+    ID token + access   |  +------------+  |
                       token returned      |  | Google     |  |
                                          |  | Apple      |  |
                                          |  | Spotify    |  |
                                          |  | Email/Pass |  |
                                          |  | Passkeys   |  |
                                          |  | TOTP/MFA   |  |
                                          |  +------------+  |
                                          +------------------+
```

### Supported Login Methods

| Method | Provider | Notes |
|---|---|---|
| **Google** | OAuth2 via Authentik | Social login — "Sign in with Google" |
| **Apple** | OAuth2 via Authentik | Social login — "Sign in with Apple" (required for iOS) |
| **Spotify** | OAuth2 via Authentik | Social login + implicit provider account linking for playback |
| **Email + Password** | Authentik native | Classic registration and login |

### Post-Login Security (MFA / Passkeys)

Once logged in, users can enroll in additional security:

| Method | Description |
|---|---|
| **Passkeys** (WebAuthn/FIDO2) | Biometric / hardware key authentication. Preferred option. |
| **TOTP** | Time-based one-time passwords (Google Authenticator, Authy). |
| **Recovery codes** | One-time fallback codes for account recovery. |

- MFA is **optional** for visitors. Encouraged via UI but not required.
- MFA is **required** for hub owners and managers (they control billing and playlists).

### Token Flow

1. User authenticates via Authentik -> receives OIDC `id_token` + `access_token`.
2. Client sends `access_token` as `Bearer` token on every API request.
3. Backend validates the token against Authentik's JWKS endpoint (cached).
4. Tenant context + user role resolved per-request from the token claims + `HubMember` table.

### Spotify Account Linking

When a user logs in with Spotify, Authentik preserves the OAuth tokens. The backend uses these tokens to:
- Search tracks via Spotify API.
- Control playback on the user's Spotify account.
- No separate "connect Spotify" step needed — it's inherent in the login.

For users who log in via Google/Apple/email and want Spotify playback, a separate Spotify OAuth linking flow is available in settings.

---

## Frontend Architecture — Angular (Web)

### Shell Architecture

The Angular web app uses a **shell + lazy-loaded feature modules** pattern:

```
+----------------------------------------------------------+
|                      APP SHELL                            |
|  +-------------+  +----------------------------------+   |
|  |  Sidebar /  |  |         ROUTER OUTLET            |   |
|  |  Navigation |  |                                  |   |
|  |             |  |  +----------------------------+  |   |
|  |  . Now      |  |  |    Feature Module          |  |   |
|  |    Playing  |  |  |    (lazy loaded)            |  |   |
|  |  . Queue    |  |  |                             |  |   |
|  |  . Propose  |  |  |                             |  |   |
|  |  . My Lists |  |  |                             |  |   |
|  |  . Hub Info |  |  |                             |  |   |
|  |  . Profile  |  |  |                             |  |   |
|  |  . Settings |  |  +----------------------------+  |   |
|  |             |  |                                  |   |
|  +-------------+  +----------------------------------+   |
|  +------------------------------------------------------+|
|  |               NOW PLAYING BAR (persistent)           ||
|  +------------------------------------------------------+|
+----------------------------------------------------------+
```

**Key shell sections:**
- **Now Playing bar** — Always visible at bottom; shows current track, vote buttons, progress.
- **Navigation sidebar** — Context-aware: shows different items for visitors vs. owners vs. managers.
- **Router outlet** — Lazy-loaded feature modules for each section.

### State Management — Client-Side State Machine

The Angular app uses **XState** (or NgRx with state machine patterns) for predictable, locally-managed state:

```
+------------------------------------------------------+
|              CLIENT STATE MACHINE                     |
|                                                       |
|  +----------+  event   +----------+  event            |
|  |  idle    | -------> | attached | -------> ...      |
|  +----------+          +----------+                   |
|                                                       |
|  States: idle -> scanning -> attaching -> attached ->  |
|          proposing -> voting -> detaching -> idle      |
|                                                       |
|  Each state defines:                                  |
|  . Which API calls are valid                          |
|  . Which UI is shown                                  |
|  . Which transitions are allowed                      |
|  . Which local data is cached                         |
|                                                       |
+------------------------------------------------------+
         |
         | HTTP/WS (stateless)
         v
+------------------------------------------------------+
|              STATELESS BFF                            |
|                                                       |
|  . No session state                                   |
|  . Validates token -> forwards to module API          |
|  . Transforms responses for client needs              |
|  . Manages WebSocket fan-out per client type          |
|                                                       |
+------------------------------------------------------+
```

**Why state machine + stateless BFF:**
- **State machine locally** -> All UI logic is deterministic, testable, and works offline. No "impossible state" bugs.
- **Stateless BFF** -> Backend scales trivially (no session affinity). Any instance can serve any request.

### Project Structure (Angular)

```
web/
+-- src/
|   +-- app/
|   |   +-- shell/                    # App shell (sidebar, now-playing bar, layout)
|   |   +-- core/                     # Singleton services, guards, interceptors
|   |   |   +-- auth/                 # OIDC client, token interceptor
|   |   |   +-- state/                # Global state machine (XState service)
|   |   |   +-- websocket/            # WebSocket service for real-time events
|   |   |   +-- api/                  # BFF HTTP client
|   |   |
|   |   +-- features/                 # Lazy-loaded feature modules
|   |   |   +-- hub/                  # Hub connection, QR scanning, hub info
|   |   |   +-- queue/                # Live queue, now-playing, propose track
|   |   |   +-- voting/               # Vote UI, live tallies
|   |   |   +-- lists/                # Playlist management (owner)
|   |   |   +-- economy/              # Wallet, coin purchase, pricing
|   |   |   +-- analytics/            # Track stats, reports (owner)
|   |   |   +-- social/               # Profiles, reviews, discovery
|   |   |   +-- admin/                # Hub management, role delegation
|   |   |   +-- settings/             # User settings, MFA enrollment, provider linking
|   |   |
|   |   +-- shared/                   # Shared components, pipes, directives
|   |
|   +-- assets/
|   +-- environments/
|   +-- styles/
|
+-- angular.json
+-- tsconfig.json
+-- package.json
```

---

## Frontend Architecture — Flutter (Mobile)

### State Management

**State machine pattern** (using `bloc` + `freezed` or a dedicated state machine package) — mirrors the Angular approach for consistency. All transitions are explicit and predictable.

### Project Structure

```
mobile/
+-- lib/
|   +-- core/
|   |   +-- config/           # Environment config, constants
|   |   +-- di/               # Dependency injection setup
|   |   +-- network/          # HTTP client, WebSocket client, interceptors
|   |   +-- storage/          # SQLite local database, secure storage
|   |   +-- auth/             # OIDC client (Authentik), token management
|   |   +-- state/            # Root state machine
|   |   +-- theme/            # App theme, colors, typography
|   |   +-- utils/            # Extensions, formatters, validators
|   |
|   +-- features/
|   |   +-- auth/             # Login, registration, MFA enrollment
|   |   +-- hub/              # Hub connection, QR scanning
|   |   +-- queue/            # Live queue, now-playing, propose track
|   |   +-- voting/           # Vote UI, live tallies
|   |   +-- lists/            # Playlist management (owner)
|   |   +-- economy/          # Wallet, coin purchase, pricing display
|   |   +-- analytics/        # Track stats, weekly/monthly reports (owner)
|   |   +-- social/           # Profiles, reviews, discovery, public lists
|   |   +-- mood/             # Portable hub modes (Phase 2)
|   |   +-- admin/            # Hub management, role delegation
|   |
|   +-- services/
|   |   +-- music_provider/   # Abstract interface + Spotify/YouTube impls
|   |   +-- qr_service/       # QR generation and scanning
|   |   +-- notification/     # Push notification handling
|   |   +-- connectivity/     # Network status, offline queue management
|   |
|   +-- shared/
|   |   +-- widgets/          # Reusable UI components
|   |   +-- models/           # Shared DTOs / value objects
|   |
|   +-- app.dart              # Root widget, routing, state machine scope
|
+-- pubspec.yaml
+-- test/
```

### Platform-Specific Builds

| Target | Build Command | Notes |
|---|---|---|
| Android | `flutter build apk` / `appbundle` | Min SDK 24 (Android 7.0) |
| iOS | `flutter build ipa` | Min iOS 15 |

---

## BFF (Backend for Frontend)

Two thin, **stateless** BFF layers sit between clients and the backend modules:

| BFF | Client | Responsibilities |
|---|---|---|
| `Musicratic.BFF.Web` | Angular | Aggregates module APIs for web views, SSR hints, WebSocket fan-out |
| `Musicratic.BFF.Mobile` | Flutter | Aggregates module APIs for mobile views, push token registration, bandwidth-optimized payloads |

**Stateless means:**
- No server-side sessions. Token validation on every request.
- No sticky sessions. Any BFF instance serves any request.
- All state lives in the client's state machine or in the backend modules (via Dapr state store / PostgreSQL).

### Communication Protocols

The BFF exposes **three protocols** to clients, each optimized for a specific interaction pattern:

| Protocol | Direction | Use Case | Example |
|---|---|---|---|
| **JSON-RPC 2.0** | Client → BFF | Commands (fire-and-forget or request/response) | `cast_vote`, `propose_track`, `skip_track`, `purchase_coins` |
| **REST** | Client ↔ BFF | Resource CRUD (create, read, update, delete) | `GET /hubs/{id}`, `POST /hubs`, `PUT /hubs/{id}/settings` |
| **GraphQL** | Client → BFF | Search, filtering, complex queries, partial field selection | `query { hubs(genre: "rock", active: true) { name, code, memberCount } }` |

**Why three protocols:**
- **JSON-RPC** for commands: lightweight, explicit method names, no resource-oriented overhead. Commands are actions (verbs), not resources (nouns).
- **REST** for resources: standard HTTP semantics (GET/POST/PUT/DELETE), cacheable, well-understood.
- **GraphQL** for searches: clients request exactly the fields they need, avoids over-fetching in discovery/listing screens.

```
Client (Angular / Flutter)
       |
       |-- JSON-RPC 2.0 --> BFF (commands: vote, propose, skip, buy)
       |-- REST ----------> BFF (resources: hubs, users, lists, tracks)
       |-- GraphQL -------> BFF (search: hub discovery, track search, leaderboards)
       |-- WebSocket -----> BFF (real-time: events, live tallies)
       |
       v
BFF (ASP.NET Core)
       |
       |-- gRPC (via Dapr service invocation) --> Backend Modules
       |
       v
Modular Monolith
```

#### JSON-RPC Request/Response Format

```json
// Request
{
  "jsonrpc": "2.0",
  "method": "voting.cast_vote",
  "params": { "queue_entry_id": "uuid", "direction": "up" },
  "id": 1
}

// Success response
{
  "jsonrpc": "2.0",
  "result": { "tally": { "up": 12, "down": 3 } },
  "id": 1
}

// Error response (Problem Details inside JSON-RPC error)
{
  "jsonrpc": "2.0",
  "error": {
    "code": -32000,
    "message": "Vote rejected",
    "data": {
      "type": "https://musicratic.com/errors/already-voted",
      "title": "Already Voted",
      "status": 409,
      "detail": "User has already voted on this queue entry."
    }
  },
  "id": 1
}
```

#### REST Response Envelope

All REST endpoints returning collections use a standard response envelope:

```json
{
  "success": true,
  "total_items_in_response": 10,
  "has_more_items": true,
  "items": [ ... ],
  "audit": {
    "request_id": "correlation-uuid",
    "timestamp": "2026-03-16T12:00:00Z",
    "server_version": "1.0.0"
  }
}
```

Single-resource responses use the same pattern with `items` containing one element.

#### BFF → Backend: gRPC

All BFF-to-backend communication uses **gRPC** via Dapr service invocation. Benefits:
- Binary protocol = faster serialization, smaller payloads.
- Strongly-typed contracts (`.proto` files) shared between BFF and backend.
- Dapr handles service discovery, retries, and load balancing.
- Same gRPC contracts work whether modules run as monolith or microservices.

---

## Event-Driven Architecture with Dapr

### Why Dapr

Dapr provides a sidecar-based runtime that abstracts infrastructure concerns:

| Dapr Building Block | Used For | Backing Component (Dev) | Backing Component (Prod) |
|---|---|---|---|
| **Pub/Sub** | Inter-module events | Event Hubs emulator | Event Hubs emulator (or swap to Kafka/RabbitMQ without code changes) |
| **State Store** | Ephemeral state (votes, sessions) | Memcached | Memcached |
| **Service Invocation** | Synchronous module-to-module calls + BFF→backend | In-process (monolith) | gRPC (microservices) |
| **Bindings** | External triggers (scheduled reports) | Cron binding | Cron binding |
| **Secrets** | API keys, connection strings | Local file / env vars | Podman Secrets keyvault / AES-256 env vars |

### Event Flow Example

```
User casts vote
       |
       v
Voting Module: validates, persists, publishes event
       |
       +--> Dapr Pub/Sub: "VoteCast" on topic "{env}_voting_vote-cast"
       |
       +--> Notification Module (subscriber): broadcasts VOTE_TALLY_UPDATED via WebSocket
       |
       +--> Playback Module (subscriber): checks skip threshold
       |        |
       |        +-- threshold met -> publishes "SkipThresholdReached"
       |        |        |
       |        |        +--> Economy Module (subscriber): processes 50% refund
       |        |
       |        +-- threshold not met -> no action
       |
       +--> Analytics Module (subscriber): updates track stats
```

### Dapr Configuration (Dev)

```yaml
# dapr/components/pubsub.yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub
spec:
  type: pubsub.azure.eventhubs
  version: v1
  metadata:
    - name: connectionString
      value: "Endpoint=sb://localhost;..." # Event Hubs emulator

# dapr/components/statestore.yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.memcached
  version: v1
  metadata:
    - name: hosts
      value: "localhost:11211"

# dapr/components/blobstore.yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: blobstore
spec:
  type: bindings.azure.blobstorage
  version: v1
  metadata:
    - name: storageAccount
      value: "devstoreaccount1" # Azurite default
    - name: storageAccessKey
      value: "..." # Azurite default key
    - name: container
      value: "musicratic-assets"
```

---

## Music Provider Integration

### Abstract Interface

```csharp
public interface IMusicProvider
{
    Task<IReadOnlyList<Track>> SearchAsync(string query, int limit = 20, CancellationToken ct = default);
    Task<Track> GetTrackAsync(string externalId, CancellationToken ct = default);
    Task<PlaybackSession> StartPlaybackAsync(string externalId, CancellationToken ct = default);
    Task PausePlaybackAsync(PlaybackSession session, CancellationToken ct = default);
    Task ResumePlaybackAsync(PlaybackSession session, CancellationToken ct = default);
    Task StopPlaybackAsync(PlaybackSession session, CancellationToken ct = default);
    IAsyncEnumerable<PlaybackState> PlaybackStateStreamAsync(PlaybackSession session, CancellationToken ct = default);
}
```

### Provider Implementations

| Provider | Phase | SDK / API |
|---|---|---|
| Spotify | Phase 1 | Spotify Web API + Spotify SDK (mobile) |
| YouTube Music | Phase 1 | YouTube Data API v3 + YouTube IFrame API (web) |
| Local Storage | Phase 1 (paid) | Device file system / Azurite blob storage |

### Provider Requirements

- The **hub owner** must authenticate with their provider account (Spotify login via Authentik handles this automatically).
- Playback happens through the **provider's official SDK** — Musicratic does not stream audio directly.
- Track metadata (title, artist, duration, cover art) is fetched from the provider and cached locally.

---

## Data Layer — Entity Framework Code First

The backend uses **Entity Framework Core (Code First)**. The database schema is defined entirely in C# and managed via EF migrations. No raw SQL schema files.

### 3-Layer Data Pattern

Data flows through three distinct layers. Each layer has a clear responsibility and strict boundaries:

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│   Entity     │     │   DataDto    │     │   ApiDto    │
│ (Business)   │────>│ (Database)   │     │ (API I/O)   │
│              │     │              │     │             │
│ Pure domain  │     │ Entity +     │     │ Entity +    │
│ data only    │     │ soft delete  │     │ visibility  │
│              │     │ audit fields │     │ control     │
│              │     │ timestamps   │     │             │
└─────────────┘     └──────────────┘     └─────────────┘
       ▲                    │                    ▲
       │            Used only inside             │
  Business logic    repositories          API controllers
  & use cases       (never leaks)         & BFF endpoints
```

#### 1. Entity (Pure Business Data)

Entities contain **only** the data that matters to the business domain. No database concerns, no audit fields, no soft-delete flags.

```csharp
// Domain/Entities/Hub.cs
public class Hub
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public HubType Type { get; set; }
    public Mood? Mood { get; set; }
    public Guid OwnerId { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
}
```

#### 2. DataDto (Database Concerns)

DataDtos extend the entity with database-specific fields. They are **only used inside the repository/infrastructure layer** and never leak into business logic.

```csharp
// Infrastructure/Data/DataDtos/HubDataDto.cs
public class HubDataDto : Hub
{
    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // EF navigation properties
    public UserDataDto Owner { get; set; } = null!;
    public ICollection<HubMemberDataDto> Members { get; set; } = new List<HubMemberDataDto>();
}
```

EF `DbContext` maps **DataDtos**, not Entities:

```csharp
// Infrastructure/Data/MusicraticDbContext.cs
public class MusicraticDbContext : DbContext
{
    public DbSet<HubDataDto> Hubs => Set<HubDataDto>();
    public DbSet<UserDataDto> Users => Set<UserDataDto>();
    // ...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global query filter for soft delete
        modelBuilder.Entity<HubDataDto>().HasQueryFilter(h => !h.IsDeleted);

        // Tenant-scoped index
        modelBuilder.Entity<HubDataDto>().HasIndex(h => h.TenantId);
    }
}
```

#### 3. ApiDto (Visibility-Controlled)

ApiDtos inherit from the entity and control which fields are visible in API requests/responses. Different endpoints may use different ApiDtos for the same entity.

```csharp
// Api/Dtos/HubSummaryApiDto.cs — for list views
public class HubSummaryApiDto : Hub
{
    // Excludes: Settings, OwnerId (not needed in list views)
    [JsonIgnore] public new Dictionary<string, object> Settings { get; set; } = new();
    [JsonIgnore] public new Guid OwnerId { get; set; }
}

// Api/Dtos/HubDetailApiDto.cs — for detail views
public class HubDetailApiDto : Hub
{
    // Includes everything from Hub + extra computed fields
    public int MemberCount { get; set; }
    public string OwnerDisplayName { get; set; } = string.Empty;
}
```

#### Repository Pattern

Repositories work internally with DataDtos but expose only Entities (or accept Entities) to the application layer. Database concerns are handled as **actions**, never as data fields visible to business logic.

```csharp
// Application/Interfaces/IHubRepository.cs
public interface IHubRepository
{
    Task<Hub?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IReadOnlyList<Hub>> GetByTenantAsync(Guid tenantId);
    Task CreateAsync(Hub hub);
    Task UpdateAsync(Hub hub);
    Task DeleteAsync(Guid id, Guid tenantId, bool soft = true); // soft delete by default
}

// Infrastructure/Repositories/HubRepository.cs
public class HubRepository : IHubRepository
{
    public async Task DeleteAsync(Guid id, Guid tenantId, bool soft = true)
    {
        var dataDto = await _context.Hubs
            .FirstOrDefaultAsync(h => h.Id == id && h.TenantId == tenantId);
        if (dataDto is null) return;

        if (soft)
        {
            dataDto.IsDeleted = true;
            dataDto.DeletedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Hubs.Remove(dataDto);
        }
        await _context.SaveChangesAsync();
    }
}
```

### Core Tables (EF-Generated)

These are the key tables generated from the DataDtos via EF migrations. All tenant-scoped tables include `TenantId` for row-level isolation.

| Table | Key Fields | Notes |
|---|---|---|
| `Users` | Id, AuthentikSub, DisplayName, Email, AvatarUrl, WalletBalance | + audit + soft delete |
| `Hubs` | Id, TenantId, Name, Code, Type, Mood, OwnerId, IsActive, Settings (JSONB) | + audit + soft delete |
| `HubMembers` | Id, HubId, UserId, Role | + audit, unique(HubId, UserId) |
| `HubAttachments` | Id, UserId, HubId, AttachedAt, ExpiresAt, IsActive | + audit |
| `QueueEntries` | Id, HubId, TrackId, ProposedBy, Status, Position | + audit + soft delete |
| `Votes` | Id, QueueEntryId, UserId, Direction | + audit |

### SQLite Fallback

The SQLite schema **mirrors** the PostgreSQL schema for tenant-scoped tables (QueueEntries, Votes, ListTracks, TrackStats). A separate lightweight `DbContext` targets SQLite. On reconnect, a sync process:

1. Replays local events to the supra-tenant's Event Hub.
2. Merges any conflicts (last-write-wins for stats, append-only for votes/queue).
3. Clears the local SQLite once confirmed synced.

---

## WebSocket Protocol

Real-time events use a simple JSON protocol over WebSocket:

```json
{
  "event": "QUEUE_UPDATED",
  "hub_id": "uuid",
  "tenant_id": "uuid",
  "payload": { ... }
}
```

**Event Types:**

| Event | Direction | Payload |
|---|---|---|
| `NOW_PLAYING` | Server -> Client | Current track, start time, voting window |
| `QUEUE_UPDATED` | Server -> Client | Updated queue entries |
| `VOTE_TALLY_UPDATED` | Server -> Client | Current up/down counts for playing track |
| `TRACK_SKIPPED` | Server -> Client | Skipped track info, reason, refund amount |
| `TRACK_ENDED` | Server -> Client | Completed track info |
| `USER_ATTACHED` | Server -> Client | New user joined hub |
| `USER_DETACHED` | Server -> Client | User left hub |
| `CAST_VOTE` | Client -> Server | Vote payload (up/down, entry_id) |
| `PROPOSE_TRACK` | Client -> Server | Track proposal payload |

---

## Error Handling — Problem Details (RFC 9457)

All error responses across the entire system use **Problem Details** (RFC 9457, successor to RFC 7807) as the standard format. This applies to:

- REST API error responses
- JSON-RPC error `data` payloads
- gRPC error details (mapped to Problem Details)
- Inner/chained exceptions (wrapped as nested Problem Details)
- WebSocket error events

### Standard Error Response

```json
{
  "type": "https://musicratic.com/errors/insufficient-coins",
  "title": "Insufficient Coins",
  "status": 400,
  "detail": "User has 3 coins but needs 5 to propose this track.",
  "instance": "/hubs/{hubId}/queue/propose",
  "traceId": "00-abc123-def456-01",
  "errors": {
    "wallet_balance": ["Current balance (3) is below required amount (5)."]
  }
}
```

### Inner Exceptions (Chained Problem Details)

When an error is caused by another error, inner exceptions are represented as nested Problem Details:

```json
{
  "type": "https://musicratic.com/errors/playback-failed",
  "title": "Playback Failed",
  "status": 502,
  "detail": "Could not start playback for the requested track.",
  "inner": {
    "type": "https://musicratic.com/errors/provider-unavailable",
    "title": "Spotify API Unavailable",
    "status": 503,
    "detail": "Spotify returned HTTP 429 Too Many Requests."
  }
}
```

### HTTP Status Code Mapping

| Scenario | Status Code | Error Type |
|---|---|---|
| Missing/invalid field | 400 | `validation-error` |
| Not authenticated | 401 | `authentication-required` |
| Insufficient permissions | 403 | `forbidden` |
| Resource not found | 404 | `not-found` |
| Conflict (already voted, already attached) | 409 | `conflict` |
| Insufficient coins | 400 | `insufficient-coins` |
| Provider API failure (Spotify/YouTube) | 502 | `provider-unavailable` |
| Unhandled exception | 500 | `internal-error` (generic message, details in logs) |

### Implementation

```csharp
// Shared exception base — all business exceptions extend this
public class MusicraticException : Exception
{
    public string Type { get; }
    public string Title { get; }
    public int StatusCode { get; }
    public MusicraticException? Inner { get; }

    public MusicraticException(string type, string title, int statusCode, string detail, MusicraticException? inner = null)
        : base(detail)
    {
        Type = $"https://musicratic.com/errors/{type}";
        Title = title;
        StatusCode = statusCode;
        Inner = inner;
    }
}

// Global exception handler middleware
// Catches MusicraticException -> converts to Problem Details JSON
// Catches unhandled exceptions -> logs stack trace, returns generic 500
```

---

## Logging & Observability — Serilog + OpenTelemetry

### Logging with Serilog

All backend services use **Serilog** for structured logging.

```csharp
// Program.cs
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration) // hot-swappable via appsettings
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Musicratic")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(new RenderedCompactJsonFormatter())
        .WriteTo.File("logs/musicratic-.log", rollingInterval: RollingInterval.Day);
});
```

#### Structured Log Format

```json
{
  "@t": "2026-03-16T12:00:00.000Z",
  "@mt": "Vote cast by {UserId} on entry {QueueEntryId} direction={Direction}",
  "@l": "Information",
  "UserId": "uuid",
  "QueueEntryId": "uuid",
  "Direction": "up",
  "HubId": "uuid",
  "TenantId": "uuid",
  "TraceId": "00-abc123-def456-01",
  "SpanId": "abc123",
  "Application": "Musicratic",
  "Environment": "Production"
}
```

#### Log Levels

| Level | When to Use |
|---|---|
| `Verbose` | Method entry/exit tracing (dev only) |
| `Debug` | Internal state details (dev only) |
| `Information` | Business events: vote cast, track proposed, hub created |
| `Warning` | Recoverable failures: Spotify rate limit hit, cache miss, retry |
| `Error` | Failed operations: payment failed, DB query error, auth rejected |
| `Fatal` | Application crash: unrecoverable state, startup failure |

#### Serilog Configuration via appsettings (Hot-Swappable)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Musicratic.Voting": "Debug"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/musicratic-.log", "rollingInterval": "Day" } }
    ]
  }
}
```

Log levels can be changed at runtime by updating `appsettings.json` — no restart required.

### OpenTelemetry Integration

OpenTelemetry provides **distributed tracing** and **metrics** across all services:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddGrpcClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("Musicratic.*")
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Musicratic.*")
        .AddOtlpExporter());
```

#### Correlation IDs

Every request gets a `TraceId` (from OpenTelemetry). This ID:
- Flows through all service calls (BFF → backend → Dapr events)
- Is included in all Serilog log entries
- Is returned in error responses (`traceId` field in Problem Details)
- Is included in WebSocket events
- Can be used to trace a single user action across the entire system

### API Documentation — OpenAPI + Scalar

- **OpenAPI 3.1** spec auto-generated from ASP.NET Core endpoints via `Swashbuckle` or `NSwag`.
- **Scalar** replaces Swagger UI as the interactive API explorer.
- Available at `/scalar` in development and staging environments.
- Disabled in production (configurable via appsettings).

---

## Event Topic Naming Convention

All Dapr pub/sub topics follow a strict naming convention:

```
{environment}_{feature}_{action}
```

### Examples

| Event | Topic Name (Dev) | Topic Name (Prod) |
|---|---|---|
| Vote cast | `dev_voting_vote-cast` | `prod_voting_vote-cast` |
| Track skipped | `dev_playback_track-skipped` | `prod_playback_track-skipped` |
| Hub created | `dev_hub_hub-created` | `prod_hub_hub-created` |
| User attached | `dev_hub_user-attached` | `prod_hub_user-attached` |
| Coins purchased | `dev_economy_coins-purchased` | `prod_economy_coins-purchased` |
| Skip threshold reached | `dev_voting_skip-threshold-reached` | `prod_voting_skip-threshold-reached` |
| Weekly report generated | `dev_analytics_weekly-report-generated` | `prod_analytics_weekly-report-generated` |

### Rules

- `{environment}`: `dev`, `staging`, `prod` — resolved from `ASPNETCORE_ENVIRONMENT`.
- `{feature}`: Module name in `kebab-case` — matches the module's Dapr app-id.
- `{action}`: Event name in `kebab-case` — describes what happened (past tense).
- **Tenant ID** is included as a **message metadata header**, not in the topic name. Subscribers filter by tenant within their handler logic.
- This allows multiple environments to share the same Event Hubs emulator in dev without cross-contamination.

### Dapr Subscription Configuration

```yaml
# dapr/components/subscription.yaml
apiVersion: dapr.io/v2alpha1
kind: Subscription
metadata:
  name: voting-vote-cast
spec:
  topic: "{env}_voting_vote-cast"
  routes:
    default: /events/voting/vote-cast
  pubsubname: pubsub
```

---

## Configuration Management

### Three-Layer Configuration Strategy

| Layer | Source | Mutability | Use Case |
|---|---|---|---|
| **appsettings.json** | File on disk | **Hot-swappable** (reloaded without restart) | Feature flags, log levels, UI config, module settings, rate limits |
| **Environment variables** | OS / container env | **Before-run only** (read at startup) | Port, database connection, environment name, Dapr endpoints |
| **Secrets** | Podman Secrets / AES-256 env vars | **Before-run only** | API keys, client secrets, encryption keys |

### Environment Variable Naming

All environment variables follow the prefix pattern:

```
MUSICRATIC_{VARIABLE_NAME}
```

| Variable | Description | Example |
|---|---|---|
| `MUSICRATIC_ENVIRONMENT` | Runtime environment | `Development`, `Staging`, `Production` |
| `MUSICRATIC_DB_HOST` | PostgreSQL host | `postgres` |
| `MUSICRATIC_DB_PORT` | PostgreSQL port | `5432` |
| `MUSICRATIC_DB_NAME` | Database name | `musicratic` |
| `MUSICRATIC_DAPR_HTTP_PORT` | Dapr sidecar HTTP port | `3500` |
| `MUSICRATIC_DAPR_GRPC_PORT` | Dapr sidecar gRPC port | `50001` |
| `MUSICRATIC_AUTHENTIK_URL` | Authentik base URL | `https://auth.musicratic.com` |
| `MUSICRATIC_AUTHENTIK_CLIENT_ID` | OIDC client ID | `musicratic-backend` |
| `MUSICRATIC_BFF_PORT` | BFF listener port | `5000` |
| `MUSICRATIC_SCALAR_ENABLED` | Enable Scalar API docs | `true` / `false` |

### Secrets Management

**Preferred: Podman Secrets keyvault** — Secrets are stored in Podman's built-in secret store and mounted as files in containers.

```bash
# Create secret
echo "sk_live_..." | podman secret create musicratic_stripe_key -

# Use in podman-compose.yml
services:
  backend:
    secrets:
      - musicratic_stripe_key
    environment:
      MUSICRATIC_STRIPE_KEY_FILE: /run/secrets/musicratic_stripe_key
```

**Fallback: AES-256 encoded environment variables** — When Podman Secrets is not available (e.g., CI, local dev without Podman):

```
MUSICRATIC_SECRET_STRIPE_KEY=ENC[AES256:base64encodedvalue]
MUSICRATIC_SECRET_SPOTIFY_CLIENT_SECRET=ENC[AES256:base64encodedvalue]
MUSICRATIC_SECRET_AUTHENTIK_CLIENT_SECRET=ENC[AES256:base64encodedvalue]
```

The application decrypts these at startup using a master key provided via `MUSICRATIC_SECRET_MASTER_KEY` (which itself is never AES-encoded — it comes from a secure channel: Podman Secret, CI variable, or manual input).

### appsettings.json Structure

```json
{
  "Musicratic": {
    "Environment": "Development",
    "ScalarEnabled": true,
    "Hub": {
      "MaxAttachmentsPerUser": 1,
      "DefaultAttachmentDurationMinutes": 120,
      "QueueMaxSize": 50
    },
    "Voting": {
      "SkipThresholdPercent": 65,
      "ProposedTrackVoteWindowSeconds": 60,
      "CollectiveVoteWindowSeconds": 120
    },
    "Economy": {
      "FreeDailyCoins": 5,
      "HotnessMultiplierEnabled": true
    },
    "Playback": {
      "DefaultProvider": "spotify"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Musicratic.Voting": "Debug"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=musicratic;Username=musicratic;Password=..."
  }
}
```

**Hot-swap behavior**: Serilog levels, feature flags, business rule thresholds (e.g., `SkipThresholdPercent`) can all be changed by updating `appsettings.json` at runtime. The .NET configuration system reloads automatically.

---

## Authentication & Authorization

### Token Strategy — JWE + Refresh Tokens

Musicratic uses **JWE (JSON Web Encryption)** for access tokens and **opaque refresh tokens**:

| Token Type | Format | Lifetime | Purpose |
|---|---|---|---|
| **Access token** | JWE (encrypted JWT) | Short (15 min) | Bearer token on every API request. Encrypted so contents are not readable by clients. |
| **Refresh token** | Opaque (random string, stored in DB) | Long (30 days) | Used to obtain a new access token without re-authentication. |

**Why JWE instead of JWS?**
- JWS (signed JWT) exposes claims in plaintext — any client can read `sub`, `email`, `iat`, etc.
- JWE encrypts the payload — only the backend can decrypt and read claims.
- Prevents information leakage to browser extensions, intercepting proxies, or compromised client code.

### Token Flow

```
1. User authenticates via Authentik → receives authorization code
2. BFF exchanges code for JWE access_token + refresh_token
3. Client stores tokens securely (HttpOnly cookie / secure storage)
4. Every request: client sends access_token as Bearer
5. BFF decrypts JWE → validates claims → forwards to backend via gRPC
6. On 401 (expired): client sends refresh_token → BFF issues new access_token
7. On refresh_token expiry: full re-authentication required
```

### Endpoint Access Control

All endpoints must resolve access for the current user. The 5-tier role system (see [doc 07](07-user-roles.md)) determines what each user can do.

**Anonymous-accessible actions** (no token required):
- Listing active hubs (`GET /hubs?active=true`)
- Listing featured hubs (`GET /hubs/featured`)

**Visitor-accessible actions** (token required, but no hub membership):
- Joining a hub
- Upvote / downvote (within an active hub session)

**Important restriction**: Only web app and mobile app clients can interact with action endpoints. Non-browser / non-app HTTP requests are rejected via device fingerprinting and origin validation headers.

### Tenant Scoping

Tenants in Musicratic serve a **narrow purpose**: isolating hub sessions and current hub activities. Tenants do NOT scope user accounts, wallets, or global data.

| Scoped by Tenant | NOT Scoped by Tenant |
|---|---|
| Active playback session | User accounts |
| Live queue | Coin wallets |
| Vote tallies | Purchase history |
| Hub member roles & permissions | Global track catalog |
| WebSocket event broadcasting | Hub metadata (search, discovery) |
| Real-time analytics | Subscriptions & billing |

---

## Database Seeding & Fallback Strategy

### Seeding via EF Migrations

All seed data is delivered through **EF migrations** — seed before application data. Every migration that adds new reference data includes it directly:

```csharp
// Migrations/20260101000000_SeedDefaultRoles.cs
public partial class SeedDefaultRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Roles",
            columns: new[] { "Id", "Name", "Level" },
            values: new object[,]
            {
                { Guid.Parse("..."), "anonymous", 0 },
                { Guid.Parse("..."), "visitor", 1 },
                { Guid.Parse("..."), "user", 2 },
                { Guid.Parse("..."), "list_owner", 3 },
                { Guid.Parse("..."), "hub_manager", 4 }
            });
    }
}
```

**Rules**:
- Seed migrations run **before** application starts accepting requests.
- CI always runs `dotnet ef database update` before tests.
- Seed data uses deterministic GUIDs so migrations are idempotent.

### Fallback Mechanism

**Every value retrieved from the database must have a fallback** so a missing row never crashes the system:

```csharp
// BAD — crashes if config row missing
var threshold = await _context.HubConfigs
    .FirstAsync(c => c.Key == "skip_threshold");

// GOOD — fallback to appsettings default
var configRow = await _context.HubConfigs
    .FirstOrDefaultAsync(c => c.Key == "skip_threshold");
var threshold = configRow?.Value ?? _options.Value.Voting.SkipThresholdPercent;
```

**Pattern**:
1. Try to load from database (most specific, per-hub if applicable).
2. Fall back to `appsettings.json` (global default, hot-swappable).
3. Fall back to compile-time constant (last resort, never changes).

This three-level fallback ensures the system stays operational even with incomplete data.

---

## Testing Strategy

**Every feature must pass 100% test coverage.** Coverage gates are enforced in CI — a PR cannot merge if any layer falls below 100%.

### Tooling

| Level | Tool | Scope | Target |
|---|---|---|---|
| **Unit tests (backend)** | xUnit + Moq | Services, repositories, domain entities, use cases, validators, helpers | **100%** |
| **Integration tests (backend)** | xUnit + TestContainers | Module API + database + Dapr interactions, entity/DTO separation | **100%** |
| **Unit tests (Angular)** | Playwright component tests | Components, services, pipes, directives, guards, interceptors, resolvers | **100%** |
| **E2E tests (Angular)** | Playwright E2E | Full web user flows, every route, every interaction | **100%** |
| **Unit tests (Flutter)** | `package:flutter_test` + Playwright (via Flutter web target) | Domain logic, state machines, widgets | **100%** |
| **E2E tests (Flutter)** | Playwright (via Flutter web target) + `package:integration_test` | Full mobile user flows | **100%** |
| **Code quality** | SonarQube | Static analysis, code smells, security hotspots, duplication | Quality gate must pass |

### Test Scope Requirements

Every test suite must verify **all six dimensions**:

| Dimension | What It Validates | Example |
|---|---|---|
| **Configuration** | App bootstraps correctly with all required settings, DI registrations, Dapr components, EF migrations | Backend starts, Angular `AppModule` compiles, Flutter `main()` initializes |
| **Feature correctness** | Each feature produces expected results for all inputs (happy path + edge cases) | Voting calculates tallies correctly, economy debits/credits coins accurately |
| **Feature integration** | Features interact correctly (voting triggers playback, economy debits on queue proposal) | Proposing a track debits coins AND adds to queue AND emits event |
| **Service integration** | Modules communicate correctly via Dapr pub/sub and service invocation | Vote event triggers Analytics update, Hub creation triggers tenant provisioning |
| **Front-back integration** | Client requests produce correct server responses, WebSocket events arrive and update UI state | Angular component calls BFF -> backend returns expected DTO, real-time vote tally updates |
| **Code quality** | SonarQube quality gate passes — no critical/blocker issues, no new bugs, coverage thresholds met | Zero code smells in new code, no security hotspots, no duplications above threshold |

### Entity / DTO Separation Tests

Integration tests must explicitly verify the 3-layer data pattern (see Data Layer section below):

- **Entity** objects never contain audit or soft-delete fields.
- **DataDto** objects include all database concerns but are never exposed outside the repository layer.
- **ApiDto** objects inherit entity data with correct visibility (fields included/excluded per use case).
- Repository `Delete(soft: true)` sets the soft-delete flag in the DataDto without exposing it to the caller.

---

## Infrastructure — Podman Compose

### Development Environment

```yaml
# podman-compose.yml (simplified)
version: "3.8"
services:
  postgres:
    image: postgres:16-alpine
    ports: ["5432:5432"]
    volumes: ["pgdata:/var/lib/postgresql/data"]

  memcached:
    image: memcached:1.6-alpine
    ports: ["11211:11211"]

  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    ports: ["10000:10000", "10001:10001", "10002:10002"]

  eventhubs-emulator:
    image: mcr.microsoft.com/azure-messaging/eventhubs-emulator:latest
    ports: ["5672:5672"]

  sonarqube:
    image: sonarqube:community
    ports: ["9100:9000"]
    volumes: ["sonardata:/opt/sonarqube/data", "sonarlogs:/opt/sonarqube/logs"]

  authentik-server:
    image: ghcr.io/goauthentik/server:latest
    ports: ["9000:9000", "9443:9443"]
    depends_on: [postgres]

  backend:
    build: ./src/Host/Musicratic.Monolith
    ports: ["5000:5000"]
    depends_on: [postgres, memcached, azurite, eventhubs-emulator, authentik-server]

  backend-dapr:
    image: daprio/daprd:latest
    command: ["./daprd", "--app-id", "musicratic", "--app-port", "5000", "--components-path", "/components"]
    network_mode: "service:backend"
    volumes: ["./dapr/components:/components"]

  caddy:
    image: caddy:2-alpine
    ports: ["80:80", "443:443"]
    volumes: ["./Caddyfile:/etc/caddy/Caddyfile", "./web/dist:/srv/web"]

volumes:
  pgdata:
  sonardata:
  sonarlogs:
```

### Production (Single VPS)

Same Podman Compose file with production environment variables, resource limits, and Caddy configured for the production domain.

**Total running containers:**

| Container | Purpose | RAM (approx) |
|---|---|---|
| PostgreSQL | Primary database | ~256 MB |
| Memcached | Cache | ~64 MB |
| Azurite | Blob storage | ~128 MB |
| Event Hubs emulator | Pub/sub | ~256 MB |
| Authentik | Identity provider | ~512 MB |
| Backend (monolith) + Dapr | Application + sidecar | ~256 MB |
| SonarQube | Code quality analysis | ~512 MB |
| Caddy | Reverse proxy | ~32 MB |
| **Total** | | **~2 GB** |

A EUR5/mo Hetzner VPS with 4 GB RAM handles this comfortably.

---

## DevOps & CI/CD

### CI Pipeline (GitHub Actions)

```yaml
# Simplified
on: [push, pull_request]
jobs:
  backend:
    - dotnet format --verify-no-changes
    - dotnet build /warnaserror
    - dotnet test --collect:"XPlat Code Coverage"
    - sonar-scanner (SonarQube analysis + coverage upload)
    - fail if coverage < 100%
  web:
    - npm ci
    - ng lint
    - npx playwright test --project=unit     # component tests (100%)
    - npx playwright test --project=e2e      # E2E tests (100%)
    - ng build --configuration production
    - sonar-scanner (SonarQube analysis)
  mobile:
    - flutter analyze
    - flutter test --coverage                # unit + widget tests (100%)
    - npx playwright test --project=flutter  # E2E via Flutter web (100%)
    - flutter build apk
  quality-gate:
    - sonarqube-quality-gate --fail-on-error # blocks merge if gate fails
  containers:
    - podman build (backend)
    - podman build (web)
```

### Deployment

| Target | Method |
|---|---|
| Backend (.NET) | Podman container -> VPS (Podman Compose) |
| Web app (Angular) | `ng build --configuration production` -> served from VPS via Caddy |
| Android | GitHub Actions -> Play Store (Fastlane) |
| iOS | GitHub Actions -> App Store (Fastlane) |
