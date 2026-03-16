# 02 — System Architecture

## Architecture Overview

Musicratic follows a **modular monolith, event-driven, multi-tenant architecture** with two client applications (Angular web + Flutter mobile), a stateless BFF layer, and a shared backend platform orchestrated by **Dapr**.

```
+---------------------------------------------------------------------+
|                        CLIENT LAYER                                  |
|                                                                      |
|  +--------------------+                +--------------------+        |
|  |   Angular (Web)    |                |   Flutter (Mobile) |        |
|  |                    |                |                    |        |
|  |  Shell + XState    |                |  State machine     |        |
|  |  state machine     |                |  (bloc/freezed)    |        |
|  +--------+-----------+                +--------+-----------+        |
|           |                                     |                    |
+-----------+-------------------------------------+--------------------+
            |                                     |
            v                                     v
+---------------------------------------------------------------------+
|                    BFF LAYER (Stateless)                              |
|                                                                      |
|  +--------------------+                +--------------------+        |
|  | BFF.Web            |                | BFF.Mobile         |        |
|  | (ASP.NET Core)     |                | (ASP.NET Core)     |        |
|  +--------+-----------+                +--------+-----------+        |
|           |                                     |                    |
+-----------+-------------------------------------+--------------------+
            |                                     |
            v                                     v
+---------------------------------------------------------------------+
|             MODULAR MONOLITH (.NET) + Dapr Sidecar                   |
|                                                                      |
|  +--------+  +-----+  +----------+  +--------+  +-----------+       |
|  |  Auth  |  | Hub |  | Playback |  | Voting |  | Economy   |       |
|  +--------+  +-----+  +----------+  +--------+  +-----------+       |
|                                                                      |
|  +-----------+  +--------+  +--------------+                         |
|  | Analytics |  | Social |  | Notification |                         |
|  +-----------+  +--------+  +--------------+                         |
|                                                                      |
|  +--------------------------------------------------------------+   |
|  |              Shared Kernel (events, entities, tenant ctx)     |   |
|  +--------------------------------------------------------------+   |
+-----------------------------------+----------------------------------+
                                    |
                 +------------------+-------------------+
                 |           Dapr Sidecar               |
                 |   Pub/Sub | State | Invoke | Bindings|
                 +--+--------+-------+--------+---------+
                    |        |       |        |
     +--------------+--+ +---+----+ +--+-----+--+ +----+------+
     | Event Hubs      | | Memcached| | Azurite  | | PostgreSQL|
     | emulator        | |          | | (blobs)  | |           |
     +-----------------+ +----------+ +----------+ +-----------+
                                                          |
                                                    +-----+-----+
                                                    |   SQLite   |
                                                    | (offline   |
                                                    |  fallback) |
                                                    +-----------+
```

---

## Multi-Tenancy Model

Every **hub is a tenant**. A permanent **supra-tenant** represents the global Musicratic platform.

| Scope | Supra-Tenant | Hub-Tenant |
|---|---|---|
| **User accounts** | yes | — |
| **Authentication (Authentik)** | yes | — |
| **Coin wallets & purchases** | yes | — |
| **Hub discovery & search** | yes | — |
| **Global track catalog** | yes | — |
| **Subscriptions & billing** | yes | — |
| **Playback queue** | — | yes |
| **Voting** | — | yes |
| **Lists** | — | yes |
| **Hub member roles** | — | yes |
| **Attachments** | — | yes |

**Tenant context** flows through every request:
1. Resolved from JWT claims + route/header (middleware).
2. Applied as a filter on all data queries (`tenant_id` discriminator).
3. Included in Dapr pub/sub topic names (`hub-{tenantId}.votes`).

---

## Component Breakdown

### 1. Client Applications

Two distinct client applications share the same BFF contracts but have independent architectures:

| Client | Technology | Architecture | Platforms |
|---|---|---|---|
| **Web app** | Angular | Shell + lazy features, XState state machine, stateless BFF | Browser (Chrome, Safari, Firefox, Edge) |
| **Mobile app** | Flutter | State machine (bloc/freezed), stateless BFF | iOS 15+, Android 7+ |

Both clients:
- Manage **all UI state locally** via state machines. No server-side sessions.
- Authenticate via **OIDC redirect to Authentik**.
- Communicate with the backend exclusively through their **stateless BFF**.
- Receive real-time events via **WebSocket**.

### 2. BFF Layer (Backend for Frontend)

Two thin ASP.NET Core services that sit between clients and the backend:

| BFF | Client | Purpose |
|---|---|---|
| `Musicratic.BFF.Web` | Angular | Aggregates module APIs for web views, SSR hints, WebSocket fan-out |
| `Musicratic.BFF.Mobile` | Flutter | Bandwidth-optimized payloads, push token registration, mobile-specific aggregation |

**Stateless** — no server-side sessions, no sticky routing. Token validation on every request. Any BFF instance can handle any request.

**Protocols exposed to clients:** JSON-RPC 2.0 (commands), REST (resource CRUD), GraphQL (search/queries), WebSocket (real-time events).

**Internal protocol:** gRPC via Dapr service invocation to backend modules.

### 3. Backend Modules (Modular Monolith)

Modules communicate through **Dapr pub/sub** (async events) and **Dapr service invocation** (sync calls). Each module owns its database schema. See [doc 10](10-platform-and-tech-stack.md) for full module breakdown.

| Module | Responsibility |
|---|---|
| **Auth** | Token validation (Authentik), user profile CRUD, tenant membership |
| **Hub** | Hub CRUD, QR/link generation, attachment, hub hierarchy, tenant lifecycle |
| **Playback** | Queue management, track ordering, provider bridging, skip execution |
| **Voting** | Vote casting, tally computation, rule enforcement |
| **Economy** | Coin wallets, track pricing, IAP/Stripe integration, refund logic, subscriptions |
| **Analytics** | Track scoring, weekly/monthly reports, shuffle weighting |
| **Social** | User profiles, hub reviews, public lists, hub discovery |
| **Notification** | WebSocket push, OS-level push (APNs/FCM direct), in-app prompts |

**Deployment flexibility**: All modules run in a single process (MVP) or as independent containers (scale). No code changes — only Podman Compose config differs.

### 4. Dapr Sidecar

Dapr abstracts all infrastructure concerns so the same code runs locally or distributed:

| Building Block | Used For | Backing Service |
|---|---|---|
| **Pub/Sub** | Inter-module events | Azure Event Hubs emulator |
| **State Store** | Ephemeral state (vote tallies, sessions) | Memcached |
| **Service Invocation** | Sync module-to-module calls | In-process (monolith) / HTTP (microservices) |
| **Bindings** | Scheduled triggers, external integrations | Cron, blob storage |
| **Secrets** | API keys, connection strings | Local file / env vars | Podman Secrets / AES-256 env vars |

### 5. Data Layer

| Store | Use Case | Justification |
|---|---|---|
| **PostgreSQL** | Persistent relational data (users, hubs, tracks, votes, economy) | ACID, complex queries, self-hosted |
| **SQLite** | Offline hub data (queue, votes, stats when disconnected from supra-tenant) | Embedded, zero-config, syncs on reconnect |
| **Memcached** | Ephemeral data (live vote tallies, session cache, JWKS cache, hot queue state) | Fast key-value, simple, self-hosted |
| **Azurite** | Binary assets (QR codes, avatars, local track files for paid hubs) | Azure Storage emulator, S3-compatible, self-hosted, zero cost |

**ORM**: Entity Framework Core (Code First). Database schema is defined entirely in C# via a 3-layer pattern:

- **Entity** — pure business data (no DB concerns). Used by business logic and use cases.
- **DataDto** — extends Entity with soft delete, audit fields, timestamps. Used only inside repositories. Never leaks to business logic.
- **ApiDto** — extends Entity with visibility control (field inclusion/exclusion per endpoint). Used for API request/response.

Repositories expose business actions (e.g., `Delete(soft: true)`) — callers never see database-specific fields. See [doc 10](10-platform-and-tech-stack.md) for full pattern with code examples.

### 6. Identity Provider — Authentik

Self-hosted, open-source IdP that handles:
- **Login methods**: Google, Apple, Spotify, Email+Password (all via OAuth2/OIDC).
- **Post-login security**: Passkeys (WebAuthn/FIDO2), TOTP, recovery codes.
- **Token issuance**: JWE-encrypted access tokens + opaque refresh tokens.
- Access tokens are JWE (encrypted) — only the backend can read claims. Prevents information leakage.
- See [doc 07](07-user-roles.md) for role/permission details and [doc 10](10-platform-and-tech-stack.md) for full auth flow.

### 7. External Integrations

| Integration | Purpose | Self-hosted alternative? |
|---|---|---|
| **Spotify Web API / SDK** | Track search, metadata, playback control | No — provider API required |
| **YouTube Music API** | Track search, metadata, playback control | No — provider API required |
| **Stripe** | Coin purchase (web), hub subscriptions | No — payment processing requires licensed gateway |
| **Apple IAP / Google Play Billing** | In-app coin purchase (mobile) | No — required by app store policies |
| **Google Maps / Geolocation** (Phase 2) | Location-based hub attachment | Evaluate OSM-based alternatives |

> **No Firebase, no third-party analytics, no third-party push services.** Push notifications via WebSocket (in-app) + direct APNs/FCM (OS-level). Analytics and error tracking via structured logging + self-hosted dashboard.

---

## Communication Patterns

### Client ↔ BFF Protocol Stack

Clients communicate with the BFF using **three protocols**, each optimized for a different interaction pattern:

| Protocol | Use Case | Direction | Examples |
|---|---|---|---|
| **JSON-RPC 2.0** | Commands (actions/mutations) | Client → BFF | `cast_vote`, `propose_track`, `skip_track`, `purchase_coins` |
| **REST** | Resource CRUD | Client ↔ BFF | `GET /hubs/{id}`, `POST /hubs`, `PUT /hubs/{id}/settings` |
| **GraphQL** | Search, filtering, complex queries | Client → BFF | Hub discovery, track search, leaderboards, partial field selection |
| **WebSocket** | Real-time events | BFF → Client | Vote tallies, queue updates, track changes |

### BFF → Backend: gRPC

All BFF-to-backend communication uses **gRPC** via Dapr service invocation. Binary protocol, strongly-typed `.proto` contracts, automatic service discovery.

### Real-Time Flow (WebSocket via BFF)

```
Visitor proposes track (JSON-RPC via BFF)
        |
        v
  Playback Module --> publishes QueueUpdated via Dapr
        |
        +---> Notification Module (subscriber): broadcasts QUEUE_UPDATED via WebSocket
        |
        v
  Voting window opens (1 min)
        |
        v
  Voting Module tallies --> publishes VoteTallyUpdated via Dapr
        |
        +---> Notification Module: broadcasts VOTE_TALLY_UPDATED
        |
        v
  If >=65% downvotes --> Playback Module executes SKIP
                     --> Economy Module processes 50% REFUND (if coins used)
                     --> Notification Module: broadcasts TRACK_SKIPPED
```

### Event-Driven Flow (Dapr Pub/Sub)

All inter-module communication uses Dapr pub/sub with environment-scoped topics:
- Topic format: `{environment}_{feature}_{action}` (e.g., `prod_voting_vote-cast`, `dev_playback_track-skipped`)
- Tenant ID is included as a **message metadata header**, not in the topic name.
- Delivery guarantee: at-least-once (modules must be idempotent)

### Error Handling

All errors across the system use **Problem Details (RFC 9457)**:
- REST error responses return Problem Details JSON.
- JSON-RPC errors carry Problem Details in the `error.data` field.
- gRPC errors are mapped to Problem Details.
- Inner/chained exceptions are nested as `inner` Problem Details.
- See [doc 10](10-platform-and-tech-stack.md) for full error format and HTTP status code mapping.

### Offline / Low-Connectivity Strategy

- **Hub offline mode**: When a hub loses connectivity to the supra-tenant, it continues on SQLite. Queue, votes, playback, and local stats function normally over the local network.
- **Deferred coin transactions**: Proposals allowed but marked "pending settlement." Settled on reconnect.
- **Event replay**: When connectivity resumes, pending events are replayed to the supra-tenant's Event Hub. SQLite data reconciled with PostgreSQL (last-write-wins for stats, append-only for votes/queue).
- **Driving mood** (Phase 2): Full offline queue with pre-cached tracks. Sync on reconnect.

---

## Deployment Model

### Phase 1 — MVP (Monolith on Single VPS)

All containers run on a single VPS (~4 GB RAM):

| Container | Purpose | RAM (approx) |
|---|---|---|
| PostgreSQL | Primary database | ~256 MB |
| Memcached | Cache | ~64 MB |
| Azurite | Blob storage | ~128 MB |
| Event Hubs emulator | Pub/sub | ~256 MB |
| Authentik | Identity provider | ~512 MB |
| Backend (monolith) + Dapr sidecar | Application | ~256 MB |
| Caddy | Reverse proxy + TLS + web static files | ~32 MB |
| **Total** | | **~1.5 GB** |

- **Container runtime**: Podman + Podman Compose (rootless, no daemon).
- **Reverse proxy**: Caddy with auto-TLS via Let's Encrypt.
- **Web frontend**: Angular build served as static files from Caddy.
- **Total infrastructure cost target: ~EUR5/month.**

### Phase 2 — Scale

- Extract high-traffic modules (Playback, Voting) into independent containers with their own Dapr sidecars.
- Same Podman Compose, just more containers. No code changes.
- Move to a second VPS or small cluster when a single box is no longer enough.
- Multi-region only when revenue justifies it.

---

## Security Considerations

- All API communication over TLS (Caddy auto-TLS).
- OIDC tokens issued by Authentik with short expiry + refresh token rotation.
- MFA required for hub owners and managers.
- Role-based access control (RBAC) enforced at BFF and module level.
- Tenant isolation enforced in every query (middleware + data layer).
- Input validation and sanitization at API boundary.
- Rate limiting per user and per hub to prevent abuse.
- Virtual coin transactions are atomic and idempotent.
- QR codes are signed — hub identity cannot be spoofed.
- No direct music file hosting — playback delegated to licensed providers.
