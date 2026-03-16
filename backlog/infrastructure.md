# Infrastructure & DevOps — Backlog

## Project Summary

| Metric | Value |
|--------|-------|
| Total tasks | 10 |
| Done | 5 |
| Remaining | 5 |
| Est. premium requests | ~12 |
| Est. tokens | ~200 K |

## Phase 0 — Foundation (DONE)

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| INFRA-001 | Podman Compose (10 services: postgres, memcached, etc.) | L | 3 | 60K | 0 | — | devops | ✅ Done |
| INFRA-002 | Caddyfile (reverse proxy routes for BFF web/mobile + static) | S | 1 | 15K | 0 | — | devops | ✅ Done |
| INFRA-003 | Dapr components (pubsub, statestore, bindings) + config | M | 2 | 30K | 0 | — | devops | ✅ Done |
| INFRA-004 | CI/CD workflows (ci.yml, deploy.yml, dependency-audit.yml) | M | 2 | 40K | 0 | — | devops | ✅ Done |
| INFRA-005 | .editorconfig + .gitignore | XS | 1 | 8K | 0 | — | devops | ✅ Done |

## Phase 1A — Hub & List Management

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| INFRA-010 | Authentik configuration (realm, OIDC client, flows for login/register) | L | 4 | 70K | 1A | — | devops | 📋 Backlog |
| INFRA-011 | PostgreSQL schema-per-module initialization (auth, hub, playback, voting, etc.) | M | 2 | 35K | 1A | — | devops | 📋 Backlog |
| INFRA-012 | Caddy route refinement (WebSocket upgrade paths, CORS for web client) | S | 1 | 20K | 1A | — | devops | 📋 Backlog |

## Phase 1D — Economy

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| INFRA-013 | Stripe webhook endpoint configuration (secret rotation, event forwarding) | M | 2 | 35K | 1D | ECON-007 | devops | 📋 Backlog |

## Phase 1E+ — Quality & Monitoring

| ID | Task | Size | PRs | Tokens | Phase | Deps | Agent | Status |
|----|------|------|-----|--------|-------|------|-------|--------|
| INFRA-014 | SonarQube quality gate configuration (coverage thresholds, duplication, ratings) | M | 2 | 40K | 1E | — | devops | 📋 Backlog |

## Dependency Graph

```
INFRA-010 (standalone — Authentik setup)
INFRA-011 (standalone — DB schemas)
INFRA-012 (standalone — Caddy update)
INFRA-013 (depends on ECON-007 — Stripe webhook exists)
INFRA-014 (standalone — SonarQube config)
```
