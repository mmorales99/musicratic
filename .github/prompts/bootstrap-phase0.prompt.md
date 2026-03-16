---
description: "Bootstrap Phase 0 foundation using parallel agent waves: Wave 1 scaffolds all projects simultaneously, Wave 2 fills in module code and DB, Wave 3 writes tests"
agent: "agent"
---

Execute Phase 0 (Foundation) from [docs/11-development-roadmap.md](docs/11-development-roadmap.md) using **parallel agent waves**.

## Parallel Execution Plan

Open one VS Code Copilot Chat session per agent. Run each wave's agents simultaneously.

```
Wave 1 — PARALLEL (no dependencies)
├── Chat 1 → @backend-architect   "Scaffold the .NET solution"
├── Chat 2 → @angular-web         "Scaffold the Angular project"
├── Chat 3 → @flutter-mobile      "Scaffold the Flutter project"
└── Chat 4 → @devops              "Set up infra, CI/CD, Dapr"

     ⏳ Wait for all Wave 1 chats to finish

Wave 2 — PARALLEL (depends on backend-architect)
├── Chat 5 → @backend-module      "Implement Auth + Hub module entities and use cases"
└── Chat 6 → @database            "Create EF Core configs and initial migration"

     ⏳ Wait for all Wave 2 chats to finish

Wave 3 — SEQUENTIAL (depends on all code existing)
└── Chat 7 → @testing             "Write tests for all modules"
```

---

## Wave 1 — Project Scaffolds (4 agents in parallel)

### Chat 1: @backend-architect

Create the .NET solution with modular monolith structure:

- `src/Musicratic.sln` with `Directory.Build.props` + `Directory.Packages.props`
- `src/Shared/` (Domain, Application, Infrastructure, Contracts) with base classes
- `src/Modules/` (Auth, Hub, Playback, Voting, Economy, Analytics, Social, Notification — `.csproj` + `DependencyInjection.cs` for each)
- `src/BFF/` (BFF.Web, BFF.Mobile)
- `src/Host/Musicratic.Host/` (composition root, health endpoint, Serilog, OpenTelemetry, Dapr)
- `tests/` (one `.Tests.csproj` per module)

### Chat 2: @angular-web

Create the Angular project scaffold in `web/`:

- `ng new` with standalone components, strict mode
- Shell architecture (layout, nav, auth guard)
- Feature route stubs (hub, playback, voting, economy, profile, analytics)
- XState integration (`@xstate/angular`)
- Shared services (BFF API client, WebSocket service)
- Environment configs

### Chat 3: @flutter-mobile

Create the Flutter project scaffold in `mobile/`:

- `flutter create` with bloc/freezed setup
- Feature folder structure mirroring Angular
- Bloc stubs per feature
- Shared API client, WebSocket service
- `analysis_options.yaml` with strict rules
- `pubspec.yaml` with all dependencies

### Chat 4: @devops

Create infrastructure and CI/CD:

- `infra/podman-compose.yml` (all services: backend, BFF, Postgres, Memcached, Event Hubs emulator, Azurite, Authentik, Caddy, SonarQube)
- `infra/caddy/Caddyfile`
- `infra/dapr/components/` (pubsub, statestore, bindings)
- `.github/workflows/ci.yml` (build, test, lint, coverage, SonarQube)
- `.github/workflows/deploy.yml`
- `.editorconfig`
- `.gitignore`

---

## Wave 2 — Module Code + Database (2 agents in parallel)

### Chat 5: @backend-module

Implement core module logic (Auth + Hub modules first):

- Auth: JWE token validation, role resolution middleware, Authentik integration contracts
- Hub: Hub entity, HubSettings, HubAttachment, Hub CRUD use cases
- Shared contracts: domain events for cross-module communication

### Chat 6: @database

Create database schema:

- EF Core entity configurations for all core entities (User, Hub, List, Track, QueueEntry, Vote, TrackStats)
- Multi-tenant query filters on hub-scoped tables
- Initial migration (`dotnet ef migrations add InitialSchema`)
- Indexes on frequently queried columns

---

## Wave 3 — Tests

### Chat 7: @testing

Write tests for everything created in Waves 1–2:

- xUnit backend tests for Auth + Hub modules
- Playwright config for Angular
- flutter_test + Playwright config for Flutter
- Integration test setup with `WebApplicationFactory`

---

## Exit Criteria

- A user can register, log in, and see a blank home screen.
- Backend responds to authenticated API calls.
- CI is green.
- All test infrastructure is operational.
