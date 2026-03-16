# Musicratic — AI Coding Instructions

You are working on **Musicratic**, a collaborative music playback platform with democratic voting. Read `/docs/` for full specs before implementing anything.

## Project Structure

- `src/` — C# / .NET 8+ backend (modular monolith)
- `src/Modules/` — Backend modules: Auth, Hub, Playback, Voting, Economy, Analytics, Social, Notification
- `src/BFF/` — Stateless BFF layers (BFF.Web, BFF.Mobile) — ASP.NET Core
- `src/Shared/` — Shared kernel: domain events, base entities, tenant context, contracts
- `web/` — Angular web client (Shell + lazy features, XState state machines)
- `mobile/` — Flutter mobile client (bloc/freezed state machines)
- `docs/` — Specification documents (READ THESE FIRST)
- `dapr/` — Dapr component configuration
- `infra/` — Podman Compose, Caddy config, deployment scripts

## Architecture Rules

1. Backend is a **modular monolith**. Each module has 4 layers: Domain, Application, Infrastructure, Api.
2. Modules communicate **only** via Dapr pub/sub (events) or Dapr service invocation (sync). No direct references between modules.
3. Multi-tenancy: every hub is a tenant. Supra-tenant = global platform. `tenant_id` discriminator on all hub-scoped queries.
4. Angular uses **XState** state machines for state management. All state is client-side.
5. Flutter uses **bloc/freezed** state machines. Mirrors Angular patterns.
6. BFF is **stateless** — no session state, no sticky sessions. Exposes JSON-RPC (commands), REST (resources), GraphQL (search). Communicates with backend via gRPC through Dapr.
7. Auth: **Authentik** issues JWE access tokens + opaque refresh tokens. Role resolved per-request from DB, not embedded in token.
8. 5 accumulative roles: anonymous → visitor → user → list_owner → hub_manager. See [docs/07-user-roles.md](docs/07-user-roles.md).

## Coding Standards

- Max **~300 lines per file**. Extract if larger.
- **1 class per file** (exceptionally 2 if tightly coupled).
- All public APIs must have **explicit return types**.
- Business rules come from `/docs/`. When in doubt, read the spec.
- **100% test coverage** enforced in CI (backend xUnit/Moq, web Playwright, mobile flutter_test + Playwright).
- Error handling: **Problem Details (RFC 9457)** for all HTTP errors, including inner exceptions.
- Logging: **Serilog** structured logging with OpenTelemetry TraceId in all entries.
- REST collections use envelope: `{ success, total_items_in_response, has_more_items, items[], audit }`.
- Dapr topics: `{environment}_{feature}_{action}` format. Tenant ID in message metadata, not topic name.
- Config: appsettings for hot-swap, env vars (`MUSICRATIC_*`) for startup, Podman Secrets / AES-256 for secrets.
- All DB values must have a fallback chain: DB → appsettings → compile-time constant. Missing data never crashes.
- WebSocket events: typed message envelopes per [docs/10-platform-and-tech-stack.md](docs/10-platform-and-tech-stack.md).
- Only web/mobile app clients can access action endpoints. Validate origin/attestation.

## Naming Conventions

| Concept          | Convention                 | Example                        |
| ---------------- | -------------------------- | ------------------------------ |
| C# files         | `PascalCase.cs`            | `HubRepository.cs`             |
| Angular/TS files | `kebab-case.ts`            | `hub-repository.service.ts`    |
| Dart files       | `snake_case.dart`          | `hub_repository.dart`          |
| Classes          | `PascalCase`               | `HubRepository`                |
| C# methods       | `PascalCase`               | `GetActiveHubs()`              |
| TS/Dart methods  | `camelCase`                | `getActiveHubs()`              |
| DB tables        | `snake_case` (plural)      | `hub_members`                  |
| DB columns       | `snake_case`               | `created_at`                   |
| API routes       | `kebab-case`               | `/hubs/{id}/queue-entries`     |
| Dapr topics      | `{env}_{feature}_{action}` | `prod_voting_vote-cast`        |
| Env vars         | `MUSICRATIC_{NAME}`        | `MUSICRATIC_DB_HOST`           |
| Secrets          | `MUSICRATIC_SECRET_{NAME}` | `MUSICRATIC_SECRET_STRIPE_KEY` |

## Commit Messages

Follow Conventional Commits: `feat(voting): implement 65% downvote auto-skip rule`

## Key Specs

- Product vision: [docs/01-product-vision.md](docs/01-product-vision.md)
- Architecture: [docs/02-system-architecture.md](docs/02-system-architecture.md)
- Domain model: [docs/03-domain-model.md](docs/03-domain-model.md)
- Hub system: [docs/04-hub-system.md](docs/04-hub-system.md)
- Voting & playback: [docs/05-voting-and-playback.md](docs/05-voting-and-playback.md)
- Monetization: [docs/06-monetization.md](docs/06-monetization.md)
- User roles: [docs/07-user-roles.md](docs/07-user-roles.md)
- Tech stack: [docs/10-platform-and-tech-stack.md](docs/10-platform-and-tech-stack.md)
- Roadmap: [docs/11-development-roadmap.md](docs/11-development-roadmap.md)
