---
description: "Use when writing Angular TypeScript code for Musicratic: components, services, XState machines, guards, interceptors, or any web client code."
applyTo: "web/**/*.ts"
---

# Angular Web Standards

## State Management

- XState state machines ONLY — no NgRx, no BehaviorSubjects for state.
- One machine per feature: `{feature}.machine.ts`.
- Components consume state via `@xstate/angular`.

## Architecture

- Standalone components (no NgModules for features).
- Lazy-loaded feature routes.
- Shell architecture: layout, nav, auth in `shell/`.
- Features in `features/{name}/`.

## BFF Communication

- JSON-RPC for commands, REST for CRUD, GraphQL for search.
- WebSocket for real-time (queue, votes, now-playing).
- Web client talks ONLY to BFF.Web — never directly to backend.

## Conventions

- File naming: `kebab-case.ts` (e.g., `hub-discovery.component.ts`).
- Class naming: `PascalCase` (e.g., `HubDiscoveryComponent`).
- Method naming: `camelCase` (e.g., `getActiveHubs()`).
- Max ~300 lines per file.
- Strict TypeScript: no `any`, explicit return types.
- Use Angular signals for reactive UI where appropriate.

## Testing

- Playwright for component + E2E tests.
- 100% coverage enforced.
